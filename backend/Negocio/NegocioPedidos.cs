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
    private readonly ServicioMail servicioMail;

    public NegocioPedidos(DaoPedidos DaoPedidos, NegocioClientes clientes, NegocioEstadoPedido estados, ServicioMail mail)
    {
        daoPedidos = DaoPedidos;
        negocioClientes = clientes;
        negocioEstadoPedido = estados;
        servicioMail = mail;
    }
    public async Task<(PedidoResponse pedido, bool mailEnviado)?> cambiarEstado(Guid idPedido, String? codigoEstado, String? seguimiento)
    {
        var estados = await negocioEstadoPedido.obtenerEstadoPedido();
        var estado = estados.Find(e => string.Equals(e.Codigo, codigoEstado?.Trim(), StringComparison.OrdinalIgnoreCase))
            ?? throw new ArgumentException("Elegí un estado válido.");

        if (seguimiento?.Length > 300)
        {
            throw new ArgumentException("La observación no puede superar los 300 caracteres.");
        }

        var observacion = string.IsNullOrWhiteSpace(seguimiento) ? null : seguimiento.Trim();
        var resultado = await daoPedidos.actualizarEstado(idPedido, estado.IdEstadoPedido, observacion);
        if (resultado is null)
        {
            return null;
        }

        var (telefono, cambioEstado, _) = resultado.Value;
        var datos = await buscarPorTelefono(telefono);
        var pedido = datos?.pedidos.Find(p => p.idpedido == idPedido);
        if (datos?.cliente is null || pedido is null)
        {
            return null;
        }

        var mailEnviado = cambioEstado && await servicioMail.enviarEstadoPedido(datos.cliente, pedido, AvisoPedido.CambioEstado);
        return (pedido, mailEnviado);
    }

    public async Task<SeguimientoResponse?> buscarPorTelefono(String? telefono)
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

    // tracking.html: el pedido del link, con los datos del cliente enmascarados. Null si no existe.
    public async Task<SeguimientoPublicoResponse?> obtenerPublico(Guid idPedido)
    {
        var telefono = await daoPedidos.obtenerTelefonoCliente(idPedido);
        if (telefono is null)
        {
            return null;
        }

        var datos = await buscarPorTelefono(telefono);
        var pedido = datos?.pedidos.Find(p => p.idpedido == idPedido);
        if (datos?.cliente is null || pedido is null)
        {
            return null;
        }

        return new SeguimientoPublicoResponse
        {
            cliente = new ClientePublico
            {
                nombre = datos.cliente.Nombre,
                apellido = datos.cliente.Apellido,
                correo = enmascararCorreo(datos.cliente.Correo),
                telefono = enmascararTelefono(datos.cliente.Telefono),
                zona = datos.cliente.Zona
            },
            pedido = pedido
        };
    }

    // "1144445555" -> "******5555"
    private static String enmascararTelefono(String telefono) =>
        telefono.Length <= 4 ? telefono : new String('*', telefono.Length - 4) + telefono[^4..];

    // "martina@correo.com" -> "m***@correo.com"
    private static String? enmascararCorreo(String? correo)
    {
        var arroba = correo?.IndexOf('@') ?? -1;
        return arroba <= 0 ? null : $"{correo![0]}***{correo[arroba..]}";
    }

    public async Task<int> obtenerNumero(Guid idPedido, NpgsqlConnection conexion, NpgsqlTransaction transaccion)
    {
        return await daoPedidos.obtenerNumero(idPedido, conexion, transaccion);
    }

    
    public async Task<Guid> agregarPedido(Guid idCliente, int idEstadoPedido, String? nota, String? seguimiento, NpgsqlConnection conexion, NpgsqlTransaction transaccion)
    {
        var pedido = new Pedidos(); 
        
        pedido.IdCliente = idCliente;
        pedido.IdEstadoPedido = idEstadoPedido;
        pedido.Nota = string.IsNullOrWhiteSpace(nota) ? null! : nota.Trim();
        pedido.Seguimiento = string.IsNullOrWhiteSpace(seguimiento) ? null! : seguimiento.Trim();

        return await daoPedidos.agregarPedido(pedido, conexion, transaccion);
    }
}