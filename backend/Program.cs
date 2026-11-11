using System.Threading.RateLimiting;
using Datos;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.FileProviders;
using Negocio;

var builder = WebApplication.CreateBuilder(args);

// Admite una imagen de 5 MB mas el sobrecosto del formulario.
builder.WebHost.ConfigureKestrel(opciones => opciones.Limits.MaxRequestBodySize = 6 * 1024 * 1024);

// 1. Controladores y errores
builder.Services.AddControllers();
builder.Services.AddProblemDetails();

// Dependencias de acceso a datos y negocio usadas por los controllers.
builder.Services.AddScoped<AccesoDatos>();

builder.Services.AddScoped<DaoUsuarios>();
builder.Services.AddScoped<DaoProductos>();
builder.Services.AddScoped<DaoPedidos>();
builder.Services.AddScoped<DaoDetallePedidos>();
builder.Services.AddScoped<DaoEstadoPedido>();
builder.Services.AddScoped<DaoClientes>();
builder.Services.AddScoped<DaoCategorias>();

builder.Services.AddScoped<NegocioUsuarios>();
builder.Services.AddScoped<NegocioProductos>();
builder.Services.AddScoped<NegocioPedidos>();
builder.Services.AddScoped<NegocioDetallePedido>();
builder.Services.AddScoped<NegocioEstadoPedido>();
builder.Services.AddScoped<NegocioCompras>();
builder.Services.AddScoped<NegocioClientes>();
builder.Services.AddScoped<NegocioCategorias>();

// Mails a clientes con Resend. Si tarda más de 10 s se corta: el pedido no espera al mail.
builder.Services.AddHttpClient<ServicioMail>(cliente => cliente.Timeout = TimeSpan.FromSeconds(10));

builder.Services.AddMemoryCache();
builder.Services.AddSingleton<ProteccionLogin>();

// 2. Swagger basico (solo se publica en Development)
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// 3. Autenticacion: el JWT viaja en una cookie HttpOnly. Sin clave valida, la app no arranca.
var servicioJwt = new ServicioJwt(builder.Configuration);
builder.Services.AddSingleton(servicioJwt);

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(opciones =>
    {
        opciones.MapInboundClaims = false;
        opciones.TokenValidationParameters = servicioJwt.ParametrosValidacion();
        opciones.Events = new JwtBearerEvents
        {
            OnMessageReceived = contexto =>
            {
                contexto.Token = contexto.Request.Cookies[ServicioJwt.NombreCookie];
                return Task.CompletedTask;
            }
        };
    });

// 4. Todo exige sesion salvo lo marcado explicitamente con [AllowAnonymous].
builder.Services.AddAuthorizationBuilder()
    .SetFallbackPolicy(new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build());

// 5. Limites de uso
builder.Services.AddRateLimiter(opciones =>
{
    opciones.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    opciones.AddPolicy(LimitesApi.Login, contexto => LimitePorIp(contexto, permitidas: 10));
    opciones.AddPolicy(LimitesApi.Publico, contexto => LimitePorIp(contexto, permitidas: 120));

    // Tope total de logins: BCrypt es caro y la IP del cliente se puede falsificar.
    opciones.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(contexto =>
        contexto.Request.Path.StartsWithSegments("/api/auth/login")
            ? RateLimitPartition.GetFixedWindowLimiter("login-global", _ => VentanaDeUnMinuto(60))
            : RateLimitPartition.GetNoLimiter("resto"));
});

// 6. Render y Vercel van delante de la app: son dos proxies hasta llegar al cliente.
builder.Services.Configure<ForwardedHeadersOptions>(opciones =>
{
    opciones.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    opciones.ForwardLimit = 2;
    opciones.KnownIPNetworks.Clear();
    opciones.KnownProxies.Clear();
});

var app = builder.Build();

// 7. Middleware Pipeline
app.UseForwardedHeaders();
app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseHttpsRedirection(); // En produccion el HTTPS lo terminan Vercel y Render.

    // En local el front se sirve desde la API para compartir origen, igual que con el rewrite de Vercel.
    var rutaFrontend = Path.GetFullPath(Path.Combine(app.Environment.ContentRootPath, "..", "frontend"));
    // no-cache: el navegador revalida cada archivo y no muestra CSS o JS viejos mientras se desarrolla.
    app.UseStaticFiles(new StaticFileOptions
    {
        FileProvider = new PhysicalFileProvider(rutaFrontend),
        OnPrepareResponse = contexto => contexto.Context.Response.Headers.CacheControl = "no-cache"
    });
}

app.UseRouting();
app.UseRateLimiter();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapGet("/healthz", () => Results.Ok(new { estado = "ok" })).AllowAnonymous();

app.Run();

static RateLimitPartition<string> LimitePorIp(HttpContext contexto, int permitidas) =>
    RateLimitPartition.GetFixedWindowLimiter(
        contexto.Connection.RemoteIpAddress?.ToString() ?? "desconocida",
        _ => VentanaDeUnMinuto(permitidas));

static FixedWindowRateLimiterOptions VentanaDeUnMinuto(int permitidas) => new()
{
    PermitLimit = permitidas,
    Window = TimeSpan.FromMinutes(1),
    QueueLimit = 0
};

// Permite referenciar la app desde los tests con WebApplicationFactory.
public partial class Program;
