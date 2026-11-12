namespace Negocio;

public static class NegocioRutasPaginas
{
    private static readonly Dictionary<string, string> ArchivoPorRuta = new(StringComparer.OrdinalIgnoreCase)
    {
        ["/ingresar"] = "/login.html",
        ["/pedidos"] = "/admin.html",
        ["/productos"] = "/products.html",
        ["/seguimientos"] = "/tracking-admin.html",
        ["/seguimiento"] = "/tracking.html",
        ["/privacidad"] = "/privacidad.html",
    };

    private static readonly Dictionary<string, string> RutaPorArchivo =
        ArchivoPorRuta.ToDictionary(par => par.Value, par => par.Key, StringComparer.OrdinalIgnoreCase)
            .Concat([new KeyValuePair<string, string>("/index.html", "/")])
            .ToDictionary(par => par.Key, par => par.Value, StringComparer.OrdinalIgnoreCase);

    public static readonly string[] SinIndexar = ["/ingresar", "/pedidos", "/productos", "/seguimientos", "/seguimiento"];

    public static string? rutaNuevaDe(PathString ruta)
    {
        var texto = ruta.Value ?? "";
        if (RutaPorArchivo.TryGetValue(texto, out var nueva))
        {
            return nueva;
        }

        var sinBarra = texto.TrimEnd('/');
        return texto.Length > sinBarra.Length && ArchivoPorRuta.ContainsKey(sinBarra) ? sinBarra : null;
    }

    public static string? archivoDe(PathString ruta)
    {
        var texto = (ruta.Value ?? "").TrimEnd('/');
        return ArchivoPorRuta.TryGetValue(texto, out var archivo) ? archivo : null;
    }
}
