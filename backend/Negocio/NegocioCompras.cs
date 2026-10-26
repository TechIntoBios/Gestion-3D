using Datos;
using DTOs;

namespace Negocio;

public class NegocioCompras
{
    private readonly AccesoDatos accesoDatos;
    private readonly NegocioClientes negocioClientes;
    private readonly NegocioPedidos negocioPedidos;
    private readonly NegocioDetallePedido negocioDetallePedido;

    public NegocioCompras(
        AccesoDatos accesoDatos,
        NegocioClientes negocioClientes,
        NegocioPedidos negocioPedidos,
        NegocioDetallePedido negocioDetallePedido)
    {
        this.accesoDatos = accesoDatos;
        this.negocioClientes = negocioClientes;
        this.negocioPedidos = negocioPedidos;
        this.negocioDetallePedido = negocioDetallePedido;
    }

    public async Task<Guid> registrarCompra(CompraRequest request)
    {
        if (request is null)
        {
            throw new ArgumentException("La solicitud de compra es obligatoria.");
        }

        var cliente = request.cliente ?? throw new ArgumentException("Faltan los datos del cliente.");
        var pedido = request.pedido ?? throw new ArgumentException("Faltan los datos del pedido.");
        var detalles = request.detalle;

        validarCompra(cliente, pedido, detalles);

        await using var conexion = await accesoDatos.obtenerConexion();
        await using var transaccion = await conexion.BeginTransactionAsync();

        try
        {
            var idCliente = await negocioClientes.agregarCliente(cliente.nombre!, cliente.apellido!, cliente.correo, cliente.zona!, cliente.telefono!, conexion, transaccion);

            var idPedido = await negocioPedidos.agregarPedido( idCliente, pedido.idestado, pedido.nota, pedido.Seguimiento, conexion, transaccion);

            foreach (var detalle in detalles!)
            {
                await negocioDetallePedido.agregarDetallePedido(idPedido, detalle.idproducto, detalle.nombre!, detalle.cantidad, conexion, transaccion);
            }

            await transaccion.CommitAsync();
            return idPedido;
        }
        catch
        {
            await transaccion.RollbackAsync();
            throw;
        }
    }

    private static void validarCompra(ClientesRequest cliente, PedidoResquest pedido, List<DetalleResquest>? detalles)
    {
        validarTextoRequerido(cliente.nombre, "El nombre es obligatorio.", 100);
        validarTextoRequerido(cliente.apellido, "El apellido es obligatorio.", 100);
        validarTextoRequerido(cliente.zona, "La zona es obligatoria.", 100);
        validarTextoRequerido(cliente.telefono, "El teléfono es obligatorio.", 20);

        if (cliente.telefono!.Length < 10 || cliente.telefono.Any(c => !char.IsDigit(c)))
        {
            throw new ArgumentException("El teléfono debe tener al menos 10 dígitos y contener solo números.");
        }

        if (cliente.correo?.Length > 300)
        {
            throw new ArgumentException("El correo no puede superar los 300 caracteres.");
        }

        if (pedido.idestado <= 0)
        {
            throw new ArgumentException("El estado del pedido debe ser válido.");
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
