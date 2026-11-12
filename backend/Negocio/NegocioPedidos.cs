using Datos;
using DTOs;
using Entidades;
using Npgsql;

namespace Negocio;

public class NegocioPedidos
{
    private readonly DaoPedidos daoPedidos;
    private readonly NegocioClientes negocioClientes;
    private readonly NegocioEstadoPedido negocioEstadoPedido;
    private readonly NegocioMail negocioMail;
    private readonly ILogger<NegocioPedidos> logger;

    public NegocioPedidos(DaoPedidos daoPedidos, NegocioClientes clientes, NegocioEstadoPedido estados, NegocioMail mail, ILogger<NegocioPedidos> logger)
    {
        this.daoPedidos = daoPedidos;
        negocioClientes = clientes;
        negocioEstadoPedido = estados;
        negocioMail = mail;
        this.logger = logger;
    }

    public async Task<(PedidoResponse? pedido, bool mailEnviado)?> cambiarEstado(Guid idPedido, string? codigoEstado, string? seguimiento)
    {
        var estado = await negocioEstadoPedido.obtenerPorCodigo(codigoEstado)
            ?? throw new ArgumentException("Elegí un estado válido.");

        var observacion = NegocioValidaciones.textoOpcional(seguimiento, "La observación", 300);

        var cambioEstado = await daoPedidos.actualizarEstado(idPedido, estado.IdEstadoPedido, observacion);
        if (cambioEstado is null)
        {
            return null;
        }

        try
        {
            var datos = await daoPedidos.obtenerPorId(idPedido);
            if (datos is null)
            {
                return (null, false);
            }

            var (cliente, pedido) = datos.Value;
            var mailEnviado = cambioEstado.Value && await negocioMail.enviarEstadoPedido(cliente, pedido, NegocioAvisoPedido.CambioEstado);
            return (pedido, mailEnviado);
        }
        catch (Exception e)
        {
            logger.LogWarning(e, "El pedido {IdPedido} se actualizó pero no se pudo leer o avisar al cliente.", idPedido);
            return (null, false);
        }
    }

    public async Task<SeguimientoResponse?> buscarPorTelefono(string? telefono)
    {
        var cliente = await negocioClientes.buscarPorTelefono(telefono);
        if (cliente is null)
        {
            return null;
        }

        return new SeguimientoResponse
        {
            cliente = cliente,
            pedidos = await daoPedidos.listarPorTelefono(cliente.Telefono)
        };
    }

    public async Task<(Clientes cliente, PedidoResponse pedido)?> obtenerPorId(Guid idPedido)
    {
        return await daoPedidos.obtenerPorId(idPedido);
    }

    public async Task<SeguimientoPublicoResponse?> obtenerPublico(Guid idPedido)
    {
        var datos = await daoPedidos.obtenerPorId(idPedido);
        if (datos is null)
        {
            return null;
        }

        var (cliente, pedido) = datos.Value;
        return new SeguimientoPublicoResponse
        {
            cliente = new ClientePublicoResponse
            {
                nombre = cliente.Nombre,
                apellido = cliente.Apellido,
                correo = enmascararCorreo(cliente.Correo),
                telefono = enmascararTelefono(cliente.Telefono),
                zona = cliente.Zona
            },
            pedido = pedido
        };
    }

    private static string enmascararTelefono(string telefono) =>
        telefono.Length <= 4 ? telefono : new string('*', telefono.Length - 4) + telefono[^4..];

    private static string? enmascararCorreo(string? correo)
    {
        var arroba = correo?.IndexOf('@') ?? -1;
        return arroba <= 0 ? null : $"{correo![0]}***{correo[arroba..]}";
    }

    public async Task<int> obtenerNumero(Guid idPedido, NpgsqlConnection conexion, NpgsqlTransaction transaccion)
    {
        return await daoPedidos.obtenerNumero(idPedido, conexion, transaccion);
    }

    public async Task<Guid> agregarPedido(Guid idCliente, int idEstadoPedido, string? nota, string? seguimiento, NpgsqlConnection conexion, NpgsqlTransaction transaccion)
    {
        var pedido = new Pedidos
        {
            IdCliente = idCliente,
            IdEstadoPedido = idEstadoPedido,
            Nota = NegocioValidaciones.textoOpcional(nota, "La nota", 300)!,
            Seguimiento = NegocioValidaciones.textoOpcional(seguimiento, "El seguimiento", 300)!
        };

        return await daoPedidos.agregarPedido(pedido, conexion, transaccion);
    }
}
