namespace Negocio;

public static class NegocioCabecerasSeguridad
{
    private const string PoliticaContenido =
        "default-src 'self'; script-src 'self'; style-src 'self'; img-src 'self' data:; font-src 'self'; " +
        "connect-src 'self'; object-src 'none'; base-uri 'self'; form-action 'self'; frame-ancestors 'none'";

    private static readonly string[] RutasSinIndexar =
    [
        "/api", "/admin.html", "/login.html", "/products.html", "/tracking-admin.html", "/tracking.html"
    ];

    public static void aplicar(HttpContext contexto, bool produccion)
    {
        var cabeceras = contexto.Response.Headers;

        cabeceras.ContentSecurityPolicy = PoliticaContenido;
        cabeceras.XContentTypeOptions = "nosniff";
        cabeceras.XFrameOptions = "DENY";
        cabeceras["Referrer-Policy"] = "strict-origin-when-cross-origin";
        cabeceras["Permissions-Policy"] = "camera=(), microphone=(), geolocation=(), payment=()";
        cabeceras["Cross-Origin-Opener-Policy"] = "same-origin";

        if (produccion)
        {
            cabeceras.StrictTransportSecurity = "max-age=63072000; includeSubDomains";
        }

        if (RutasSinIndexar.Any(ruta => contexto.Request.Path.StartsWithSegments(ruta, StringComparison.OrdinalIgnoreCase)))
        {
            cabeceras["X-Robots-Tag"] = "noindex, nofollow";
        }
    }
}
