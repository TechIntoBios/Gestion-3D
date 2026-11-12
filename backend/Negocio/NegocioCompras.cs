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
    private readonly NegocioProductos negocioProductos;
    private readonly NegocioEstadoPedido negocioEstadoPedido;
    private readonly NegocioMail negocioMail;
    private readonly ILogger<NegocioCompras> logger;

    public NegocioCompras(
        AccesoDatos accesoDatos,
        NegocioClientes negocioClientes,
        NegocioPedidos negocioPedidos,
        NegocioDetallePedido negocioDetallePedido,
        NegocioProductos negocioProductos,
        NegocioEstadoPedido negocioEstadoPedido,
        NegocioMail negocioMail,
        ILogger<NegocioCompras> logger)
    {
        this.accesoDatos = accesoDatos;
        this.negocioClientes = negocioClientes;
        this.negocioPedidos = negocioPedidos;
        this.negocioDetallePedido = negocioDetallePedido;
        this.negocioProductos = negocioProductos;
        this.negocioEstadoPedido = negocioEstadoPedido;
        this.negocioMail = negocioMail;
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
        await negocioProductos.validarVendibles(detalles!.Select(d => d.idproducto));

        var pendiente = await negocioEstadoPedido.obtenerPorCodigo(NegocioEstadoPedido.CodigoPendiente)
            ?? throw new InvalidOperationException("Falta el estado PENDIENTE en la base de datos.");

        Guid idPedido;
        int numero;

        await using (var conexion = await accesoDatos.obtenerConexion())
        await using (var transaccion = await conexion.BeginTransactionAsync())
        {
            try
            {
                var idCliente = await negocioClientes.agregarCliente(cliente, conexion, transaccion);

                idPedido = await negocioPedidos.agregarPedido(idCliente, pendiente.IdEstadoPedido, pedido.nota, pedido.Seguimiento, conexion, transaccion);

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
                throw new ArgumentException("Alguno de los productos no existe.", e);
            }
            catch
            {
                await transaccion.RollbackAsync();
                throw;
            }
        }

        var (correo, mailEnviado) = await avisarCliente(idPedido);
        return (idPedido, numero, correo, mailEnviado);
    }

    private async Task<(string? correo, bool enviado)> avisarCliente(Guid idPedido)
    {
        try
        {
            var datos = await negocioPedidos.obtenerPorId(idPedido);

            if (datos is not null)
            {
                var (cliente, pedido) = datos.Value;
                var enviado = await negocioMail.enviarEstadoPedido(cliente, pedido, NegocioAvisoPedido.Nuevo);
                return (cliente.Correo, enviado);
            }
        }
        catch (Exception e)
        {
            logger.LogWarning(e, "El pedido {IdPedido} se guardó pero no se pudo preparar el mail.", idPedido);
        }

        return (null, false);
    }

    private static void validarCompra(ClientesRequest cliente, PedidoRequest pedido, List<DetalleRequest>? detalles)
    {
        NegocioClientes.validar(cliente);
        NegocioValidaciones.textoOpcional(pedido.nota, "La nota", 300);
        NegocioValidaciones.textoOpcional(pedido.Seguimiento, "El seguimiento", 300);

        if (detalles is null || detalles.Count == 0)
        {
            throw new ArgumentException("La compra debe incluir al menos un detalle.");
        }

        if (detalles.Count > NegocioValidaciones.MaximoItemsPorCompra)
        {
            throw new ArgumentException($"Una compra no puede tener más de {NegocioValidaciones.MaximoItemsPorCompra} productos.");
        }

        foreach (var detalle in detalles)
        {
            if (detalle is null)
            {
                throw new ArgumentException("La lista contiene un detalle vacío.");
            }

            NegocioValidaciones.textoRequerido(detalle.nombre, "El nombre de cada detalle", 100);

            if (detalle.idproducto <= 0)
            {
                throw new ArgumentException("Cada detalle debe tener un producto válido.");
            }

            NegocioValidaciones.cantidad(detalle.cantidad);
            NegocioValidaciones.precio(detalle.precio);
        }
    }
}
