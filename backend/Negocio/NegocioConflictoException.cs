namespace Negocio;

public class NegocioConflictoException : Exception
{
    public NegocioConflictoException(string mensaje) : base(mensaje) { }
}
