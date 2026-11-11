using System.Net.Mail;
using Datos;
using DTOs;
using Npgsql;

namespace Negocio;

public class NegocioCompras
{
    private readonly AccesoDatos accesoDatos;
    private readonly NegocioClientes negocioClientes;
    private readonly NegocioPedidos negocioPedidos;
    private readonly NegocioDetallePedido negocioDetallePedido;
    private readonly ServicioMail servicioMail;
    private readonly ILogger<NegocioCompras> logger;

    // Id_EstadoPedido_EST de PENDIENTE: es el primer estado que se inserta en EstadoPedido.
    private const int EstadoPendiente = 1;

    public NegocioCompras(
        AccesoDatos accesoDatos,
        NegocioClientes negocioClientes,
        NegocioPedidos negocioPedidos,
        NegocioDetallePedido negocioDetallePedido,
        ServicioMail servicioMail,
        ILogger<NegocioCompras> logger)
    {
        this.accesoDatos = accesoDatos;
        this.negocioClientes = negocioClientes;
        this.negocioPedidos = negocioPedidos;
        this.negocioDetallePedido = negocioDetallePedido;
        this.servicioMail = servicioMail;
        this.logger = logger;
    }

    public async Task<(Guid idPedido, int numero, string? correo, bool mailEnviado)> registrarCompra(CompraRequest request)
    {
        if (request is null)
        {
            throw new ArgumentException("La solicitud de compra es obligatoria.");
        }

        var cliente = request.cliente ?? throw new ArgumentException("Faltan los datos del cliente.");
        var pedido = request.pedido ?? throw new ArgumentException("Faltan los datos del pedido.");
        var detalles = request.detalle;

        validarCompra(cliente, pedido, detalles);

        Guid idPedido;
        int numero;

        await using (var conexion = await accesoDatos.obtenerConexion())
        await using (var transaccion = await conexion.BeginTransactionAsync())
        {
            try
            {
                var idCliente = await negocioClientes.agregarCliente(cliente.nombre!, cliente.apellido!, cliente.correo, cliente.zona!, cliente.telefono!, conexion, transaccion);

                // Todo pedido nuevo arranca Pendiente, mande lo que mande el formulario.
                idPedido = await negocioPedidos.agregarPedido(idCliente, EstadoPendiente, pedido.nota, pedido.Seguimiento, conexion, transaccion);

                foreach (var detalle in detalles!)
                {
                    await negocioDetallePedido.agregarDetallePedido(idPedido, detalle.idproducto, detalle.nombre!, detalle.cantidad, detalle.precio, conexion, transaccion);
                }

                numero = await negocioPedidos.obtenerNumero(idPedido, conexion, transaccion);

                await transaccion.CommitAsync();
            }
            catch (PostgresException e) when (e.SqlState == PostgresErrorCodes.ForeignKeyViolation)
            {
                await transaccion.RollbackAsync();
                throw new ArgumentException("Alguno de los productos o el estado del pedido no existe.", e);
            }
            catch
            {
                await transaccion.RollbackAsync();
                throw;
            }
        }

        var (correo, mailEnviado) = await avisarCliente(cliente.telefono!.Trim(), idPedido);
        return (idPedido, numero, correo, mailEnviado);
    }

    private async Task<(string? correo, bool enviado)> avisarCliente(string telefono, Guid idPedido)
    {
        try
        {
            var seguimiento = await negocioPedidos.buscarPorTelefono(telefono);
            var pedidoGuardado = seguimiento?.pedidos.Find(p => p.idpedido == idPedido);

            if (seguimiento?.cliente is not null && pedidoGuardado is not null)
            {
                var enviado = await servicioMail.enviarEstadoPedido(seguimiento.cliente, pedidoGuardado, AvisoPedido.Nuevo);
                return (seguimiento.cliente.Correo, enviado);
            }
        }
        catch (Exception e)
        {
            logger.LogWarning(e, "El pedido {IdPedido} se guardó pero no se pudo preparar el mail.", idPedido);
        }

        return (null, false);
    }

    private static void validarCompra(ClientesRequest cliente, PedidoResquest pedido, List<DetalleResquest>? detalles)
    {
        validarTextoRequerido(cliente.nombre, "El nombre es obligatorio.", 100);
        validarTextoRequerido(cliente.apellido, "El apellido es obligatorio.", 100);

        if (cliente.nombre!.Any(char.IsDigit) || cliente.apellido!.Any(char.IsDigit))
        {
            throw new ArgumentException("El nombre y el apellido no pueden contener números.");
        }
        validarTextoRequerido(cliente.zona, "La zona es obligatoria.", 100);
        NegocioClientes.validarTelefono(cliente.telefono);

        if (cliente.correo?.Length > 300)
        {
            throw new ArgumentException("El correo no puede superar los 300 caracteres.");
        }

        if (!string.IsNullOrWhiteSpace(cliente.correo) && !MailAddress.TryCreate(cliente.correo.Trim(), out _))
        {
            throw new ArgumentException("El correo no tiene un formato válido.");
        }

        if (pedido.nota?.Length > 300 || pedido.Seguimiento?.Length > 300)
        {
            throw new ArgumentException("La nota y el seguimiento no pueden superar los 300 caracteres.");
        }

        if (detalles is null || detalles.Count == 0)
        {
            throw new ArgumentException("La compra debe incluir al menos un detalle.");
        }

        foreach (var detalle in detalles)
        {
            if (detalle is null)
            {
                throw new ArgumentException("La lista contiene un detalle vacío.");
            }

            validarTextoRequerido(detalle.nombre, "El nombre de cada detalle es obligatorio.", 100);

            if (detalle.idproducto <= 0 || detalle.cantidad <= 0 || !double.IsFinite(detalle.precio) || detalle.precio <= 0)
            {
                throw new ArgumentException("Cada detalle debe tener producto, cantidad y precio válidos.");
            }
        }
    }

    private static void validarTextoRequerido(string? valor, string mensaje, int longitudMaxima)
    {
        if (string.IsNullOrWhiteSpace(valor) || valor.Length > longitudMaxima)
        {
            throw new ArgumentException(mensaje);
        }
    }
}
