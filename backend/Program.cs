using System.Threading.RateLimiting;
using Controllers;
using Datos;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.FileProviders;
using Negocio;
using Npgsql;

var builder = WebApplication.CreateBuilder(args);

if (Environment.GetEnvironmentVariable("PORT") is { Length: > 0 } puerto)
{
    builder.WebHost.UseUrls($"http://0.0.0.0:{puerto}");
}

builder.WebHost.ConfigureKestrel(opciones => opciones.Limits.MaxRequestBodySize = 6 * 1024 * 1024);

builder.Services.AddControllers(opciones => opciones.Filters.Add<FiltroErroresNegocio>())
    .ConfigureApiBehaviorOptions(opciones =>
        opciones.InvalidModelStateResponseFactory = _ => new BadRequestObjectResult(new { mensaje = "Los datos enviados no son válidos." }));
builder.Services.AddProblemDetails();

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

builder.Services.AddHttpClient<NegocioMail>(cliente => cliente.Timeout = TimeSpan.FromSeconds(10));

builder.Services.AddMemoryCache(opciones => opciones.SizeLimit = 10_000);
builder.Services.AddSingleton<NegocioProteccionLogin>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var negocioJwt = new NegocioJwt(builder.Configuration);
builder.Services.AddSingleton(negocioJwt);

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(opciones =>
    {
        opciones.MapInboundClaims = false;
        opciones.TokenValidationParameters = negocioJwt.ParametrosValidacion();
        opciones.Events = new JwtBearerEvents
        {
            OnMessageReceived = contexto =>
            {
                contexto.Token = contexto.Request.Cookies[NegocioJwt.NombreCookie];
                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddAuthorizationBuilder()
    .SetFallbackPolicy(new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build());

builder.Services.AddRateLimiter(opciones =>
{
    opciones.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    opciones.AddPolicy(NegocioLimitesApi.Login, contexto => LimitePorIp(contexto, permitidas: 10));
    opciones.AddPolicy(NegocioLimitesApi.Publico, contexto => LimitePorIp(contexto, permitidas: 120));

    opciones.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(contexto =>
        contexto.Request.Path.StartsWithSegments("/api/auth/login")
            ? RateLimitPartition.GetFixedWindowLimiter("login-global", _ => VentanaDeUnMinuto(300))
            : RateLimitPartition.GetNoLimiter("resto"));
});

builder.Services.Configure<ForwardedHeadersOptions>(opciones =>
{
    opciones.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    opciones.ForwardLimit = 1;
    opciones.KnownIPNetworks.Clear();
    opciones.KnownProxies.Clear();
});

var app = builder.Build();

app.UseForwardedHeaders();

app.Use((contexto, siguiente) =>
{
    NegocioCabecerasSeguridad.aplicar(contexto, produccion: !app.Environment.IsDevelopment());
    return siguiente();
});

app.UseExceptionHandler();

app.Use(async (contexto, siguiente) =>
{
    try { await siguiente(); }
    catch (BadHttpRequestException error) { contexto.Response.StatusCode = error.StatusCode; }
});

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseHttpsRedirection();
}

var rutaFrontend = app.Environment.IsDevelopment()
    ? Path.GetFullPath(Path.Combine(app.Environment.ContentRootPath, "..", "frontend"))
    : Path.Combine(AppContext.BaseDirectory, "frontend");

if (Directory.Exists(rutaFrontend))
{
    var archivos = new PhysicalFileProvider(rutaFrontend);

    app.Use((contexto, siguiente) =>
    {
        if (HttpMethods.IsGet(contexto.Request.Method) || HttpMethods.IsHead(contexto.Request.Method))
        {
            var nueva = NegocioRutasPaginas.rutaNuevaDe(contexto.Request.Path);
            if (nueva is not null)
            {
                contexto.Response.Redirect(nueva + contexto.Request.QueryString, permanent: true);
                return Task.CompletedTask;
            }

            var archivo = NegocioRutasPaginas.archivoDe(contexto.Request.Path);
            if (archivo is not null)
            {
                contexto.Request.Path = archivo;
            }
        }
        return siguiente();
    });

    app.UseDefaultFiles(new DefaultFilesOptions { FileProvider = archivos });
    app.UseStaticFiles(new StaticFileOptions
    {
        FileProvider = archivos,
        OnPrepareResponse = contexto => contexto.Context.Response.Headers.CacheControl =
            !app.Environment.IsDevelopment() && contexto.Context.Request.Path.StartsWithSegments("/Iconos")
                ? "public, max-age=86400"
                : "no-cache"
    });
}

app.UseRouting();
app.UseRateLimiter();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapGet("/healthz", () => Results.Ok(new { estado = "ok" })).AllowAnonymous();

app.MapGet("/healthz/db", async (AccesoDatos datos, ILogger<Program> logger) =>
{
    try
    {
        await using var conexion = await datos.obtenerConexion();
        await using var comando = new NpgsqlCommand("SELECT 1", conexion);
        await comando.ExecuteScalarAsync();
        return Results.Ok(new { estado = "ok" });
    }
    catch (Exception error)
    {
        logger.LogError(error, "La base de datos no respondió en /healthz/db.");
        return Results.Json(new { mensaje = "La base de datos no responde." }, statusCode: StatusCodes.Status503ServiceUnavailable);
    }
}).AllowAnonymous().RequireRateLimiting(NegocioLimitesApi.Publico);

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

public partial class Program;
