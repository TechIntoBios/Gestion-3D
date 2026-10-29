using System.Text;
using Datos;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Negocio;

var builder = WebApplication.CreateBuilder(args);

// 1. Controladores
builder.Services.AddControllers();

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

// 2. CORS (para conectar con frontend de Vercel/HTML)
builder.Services.AddCors(options =>
{
    options.AddPolicy("PermitirFrontend", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// 3. Swagger básico (sin choque de namespaces)
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// 4. Configurar Autenticación JWT de forma segura
var jwtSection = builder.Configuration.GetSection("JwtSettings");
var secretKey = jwtSection["SecretKey"] ?? "ClaveDeRespaldoMuySeguraYDeMasDe32Caracteres!";
var keyBytes = Encoding.UTF8.GetBytes(secretKey);

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSection["Issuer"] ?? "Sistema3DBackend",
            ValidAudience = jwtSection["Audience"] ?? "Sistema3DFrontend",
            IssuerSigningKey = new SymmetricSecurityKey(keyBytes),
            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddAuthorization();

var app = builder.Build();

// 5. Middleware Pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors("PermitirFrontend");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();