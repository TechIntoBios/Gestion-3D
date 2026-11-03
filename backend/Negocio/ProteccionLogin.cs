using Microsoft.Extensions.Caching.Memory;

namespace Negocio;

// Bloqueo temporal por usuario. Complementa el limite por IP, que se puede eludir
// llamando directo a la URL de Render con un X-Forwarded-For falso.
public sealed class ProteccionLogin
{
    private const int MaximoFallos = 10;
    private static readonly TimeSpan DuracionBloqueo = TimeSpan.FromMinutes(15);

    private readonly IMemoryCache cache;
    private readonly object candado = new();

    public ProteccionLogin(IMemoryCache cache) => this.cache = cache;

    public bool EstaBloqueado(string usuario) =>
        cache.TryGetValue<int>(Clave(usuario), out var fallos) && fallos >= MaximoFallos;

    public void RegistrarFallo(string usuario)
    {
        lock (candado)
        {
            var clave = Clave(usuario);
            cache.TryGetValue<int>(clave, out var fallos);
            cache.Set(clave, fallos + 1, DuracionBloqueo);
        }
    }

    public void Limpiar(string usuario) => cache.Remove(Clave(usuario));

    private static string Clave(string usuario) => $"login:{usuario.Trim().ToLowerInvariant()}";
}
