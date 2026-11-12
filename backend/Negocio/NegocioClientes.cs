using Datos;
using DTOs;
using Entidades;
using Npgsql;

namespace Negocio;

public class NegocioClientes
{
    private readonly DaoClientes daoClientes;

    public NegocioClientes(DaoClientes dao) => daoClientes = dao;

    public static Clientes validar(ClientesRequest datos)
    {
        var nombre = NegocioValidaciones.textoRequerido(datos.nombre, "El nombre", 100);
        var apellido = NegocioValidaciones.textoRequerido(datos.apellido, "El apellido", 100);
        NegocioValidaciones.sinNumeros(nombre, "El nombre");
        NegocioValidaciones.sinNumeros(apellido, "El apellido");

        return new Clientes
        {
            Nombre = nombre,
            Apellido = apellido,
            Correo = NegocioValidaciones.correoOpcional(datos.correo)!,
            Zona = NegocioValidaciones.textoRequerido(datos.zona, "La zona", 100),
            Telefono = NegocioValidaciones.telefono(datos.telefono)
        };
    }

    public async Task<Guid> agregarCliente(ClientesRequest datos, NpgsqlConnection conexion, NpgsqlTransaction transaccion)
    {
        return await daoClientes.agregarCliente(validar(datos), conexion, transaccion);
    }

    public async Task<Clientes?> buscarPorTelefono(string? telefono)
    {
        telefono = NegocioValidaciones.telefono(telefono);
        return await daoClientes.obtenerPorTelefono(telefono);
    }

    public async Task<Clientes?> actualizar(Guid idCliente, ClientesRequest datos)
    {
        var cliente = validar(datos);

        return await daoClientes.actualizar(idCliente, cliente) switch
        {
            "OK" => await daoClientes.obtenerPorTelefono(cliente.Telefono),
            "NO_EXISTE" => null,
            "TELEFONO_DUPLICADO" => throw new NegocioConflictoException("Ese teléfono ya pertenece a otro cliente."),
            var otro => throw new InvalidOperationException($"fn_ActualizarCliente devolvió un resultado inesperado: {otro}.")
        };
    }
}
