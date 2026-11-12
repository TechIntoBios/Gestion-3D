using Microsoft.Extensions.Caching.Memory;

namespace Negocio;

public sealed class NegocioProteccionLogin
{
    public const int LargoMaximoUsuario = 100;
    public const int LargoMaximoContrasena = 200;

    private const int MaximoFallos = 10;
    private static readonly TimeSpan DuracionBloqueo = TimeSpan.FromMinutes(15);

    private readonly IMemoryCache cache;
    private readonly object candado = new();

    public NegocioProteccionLogin(IMemoryCache cache) => this.cache = cache;

    public static bool EsEntradaValida(string? usuario, string? contrasena) =>
        !string.IsNullOrWhiteSpace(usuario) && usuario.Length <= LargoMaximoUsuario
        && !string.IsNullOrEmpty(contrasena) && contrasena.Length <= LargoMaximoContrasena;

    public bool EstaBloqueado(string usuario, string ip) =>
        cache.TryGetValue<int>(Clave(usuario, ip), out var fallos) && fallos >= MaximoFallos;

    public void RegistrarFallo(string usuario, string ip)
    {
        lock (candado)
        {
            var clave = Clave(usuario, ip);
            cache.TryGetValue<int>(clave, out var fallos);
            cache.Set(clave, fallos + 1, new MemoryCacheEntryOptions
            {
                Size = 1,
                AbsoluteExpirationRelativeToNow = DuracionBloqueo
            });
        }
    }

    public void Limpiar(string usuario, string ip) => cache.Remove(Clave(usuario, ip));

    private static string Clave(string usuario, string ip) => $"login:{usuario.Trim().ToLowerInvariant()}|{ip}";
}
