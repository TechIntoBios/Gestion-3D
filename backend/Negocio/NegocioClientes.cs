using Datos;
using Entidades;
using Npgsql;

namespace Negocio; 

public class NegocioClientes
{
    private readonly DaoClientes daoClientes;

    public NegocioClientes(DaoClientes dao) => daoClientes = dao; 

    
    public async Task<Guid> agregarCliente(String nombre, String apellido, String? correo, String zona, String telefono, NpgsqlConnection conexion, NpgsqlTransaction transaccion)
    {
        var clientes = new Clientes();
        clientes.Nombre = nombre.Trim();
        clientes.Apellido = apellido.Trim();
        clientes.Correo = string.IsNullOrWhiteSpace(correo) ? null! : correo.Trim();
        clientes.Zona = zona.Trim();
        clientes.Telefono = telefono.Trim();

        return await daoClientes.agregarCliente(clientes, conexion, transaccion);
    }

    public async Task<Clientes?> buscarPorTelefono(String? telefono)
    {
        validarTelefono(telefono);
        return await daoClientes.obtenerPorTelefono(telefono!.Trim());
    }

    public static void validarTelefono(String? telefono)
    {
        var limpio = telefono?.Trim();

        if (string.IsNullOrEmpty(limpio) || limpio.Length < 10 || limpio.Length > 20 || limpio.Any(c => !char.IsDigit(c)))
        {
            throw new ArgumentException("El teléfono debe tener entre 10 y 20 dígitos, sin espacios ni guiones.");
        }
    }
}