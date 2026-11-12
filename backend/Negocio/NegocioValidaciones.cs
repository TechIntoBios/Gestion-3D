using System.Net.Mail;

namespace Negocio;

public static class NegocioValidaciones
{
    public const int MaximoItemsPorCompra = 100;
    public const int CantidadMaxima = 10_000;
    public const decimal PrecioMaximo = 99_999_999.99m;

    public static string textoRequerido(string? valor, string campo, int maximo)
    {
        var limpio = valor?.Trim();

        if (string.IsNullOrEmpty(limpio))
        {
            throw new ArgumentException($"{campo} es obligatorio.");
        }

        if (limpio.Length > maximo)
        {
            throw new ArgumentException($"{campo} no puede superar los {maximo} caracteres.");
        }

        return limpio;
    }

    public static string? textoOpcional(string? valor, string campo, int maximo)
    {
        var limpio = valor?.Trim();

        if (string.IsNullOrEmpty(limpio))
        {
            return null;
        }

        if (limpio.Length > maximo)
        {
            throw new ArgumentException($"{campo} no puede superar los {maximo} caracteres.");
        }

        return limpio;
    }

    public static void sinNumeros(string valor, string campo)
    {
        if (valor.Any(char.IsDigit))
        {
            throw new ArgumentException($"{campo} no puede contener números.");
        }
    }

    public static string? correoOpcional(string? valor)
    {
        var correo = textoOpcional(valor, "El correo", 300);

        if (correo is not null && !MailAddress.TryCreate(correo, out _))
        {
            throw new ArgumentException("El correo no tiene un formato válido.");
        }

        return correo;
    }

    public static string telefono(string? valor)
    {
        var limpio = valor?.Trim();

        if (string.IsNullOrEmpty(limpio) || limpio.Length < 10 || limpio.Length > 20 || limpio.Any(c => !char.IsDigit(c)))
        {
            throw new ArgumentException("El teléfono debe tener entre 10 y 20 dígitos, sin espacios ni guiones.");
        }

        return limpio;
    }

    public static void precio(decimal valor)
    {
        if (valor <= 0 || valor > PrecioMaximo || decimal.Round(valor, 2) != valor)
        {
            throw new ArgumentException($"El precio debe ser mayor a 0, tener hasta 2 decimales y no superar {PrecioMaximo:N2}.");
        }
    }

    public static void cantidad(int valor)
    {
        if (valor <= 0 || valor > CantidadMaxima)
        {
            throw new ArgumentException($"La cantidad debe estar entre 1 y {CantidadMaxima}.");
        }
    }
}
