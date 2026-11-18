namespace Negocio;

public static class NegocioDominio
{
    public const string Principal = "zunox3d.com.ar";
    private const string ConWww = "www." + Principal;

    // Si el pedido llega por www, devuelve la misma URL en el dominio principal; si no, null.
    public static string? redireccionDe(HttpRequest pedido) =>
        string.Equals(pedido.Host.Host, ConWww, StringComparison.OrdinalIgnoreCase)
            ? $"https://{Principal}{pedido.PathBase}{pedido.Path}{pedido.QueryString}"
            : null;
}
