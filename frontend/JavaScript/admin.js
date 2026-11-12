import { mensajeError, pedirApi } from "./api.js";
import { crearOption, formatoPesos, mostrarMensaje } from "./funciones.js";
import { exigirSesion } from "./sesion.js";

const lblMensaje = document.getElementById("lbl-mensaje");
const btnAgregar = document.getElementById("btn-agregar");
const contenedorProductos = document.getElementById("tarjeta-productos");
const plantillaProducto = document.getElementById("tarjeta-sub-productos");
const lblTotal = document.getElementById("lbl-total");
const formulario = document.getElementById("formulario");

let categorias = [];
let productos = [];

async function cargarCategorias() {
    const resultado = await pedirApi("/Categorias/listar_Categorias");

    if (!resultado.ok) {
        mostrarMensaje(lblMensaje, mensajeError(resultado, "No se encontraron categorías."), false);
        return;
    }

    categorias = resultado.datos;
}

async function cargarProductos() {
    const resultado = await pedirApi("/Productos/listar_productos");

    if (!resultado.ok) {
        mostrarMensaje(lblMensaje, mensajeError(resultado, "No se encontraron productos."), false);
        return;
    }

    productos = resultado.datos;
}

function calcularSubtotal(tarjeta) {
    const cantidad = Number(tarjeta.querySelector('[name="txt-cantidad"]').value) || 0;
    const precio = Number(tarjeta.querySelector('[name="txt-precio"]').value) || 0;

    return Math.round(cantidad * precio * 100) / 100;
}

function actualizarTotal() {
    const tarjetas = contenedorProductos.querySelectorAll(".sub-contenedor-productos");
    const total = [...tarjetas].reduce((suma, tarjeta) => suma + calcularSubtotal(tarjeta), 0);

    lblTotal.textContent = formatoPesos.format(total);
}

function agregarFilaProducto() {
    const fila = plantillaProducto.content.cloneNode(true);
    const ddlCategorias = fila.querySelector('[name="ddl-categorias"]');
    const ddlProductos = fila.querySelector('[name="ddl-productos"]');
    const txtPrecio = fila.querySelector('[name="txt-precio"]');
    const txtCantidad = fila.querySelector('[name="txt-cantidad"]');
    const tarjeta = fila.querySelector(".sub-contenedor-productos");
    const btnEliminar = fila.querySelector(".btn-eliminar");
    const lblSubtotal = fila.querySelector(".lbl-subtotal");

    function actualizarSubtotal() {
        lblSubtotal.textContent = formatoPesos.format(calcularSubtotal(tarjeta));
        actualizarTotal();
    }

    categorias.forEach((categoria) => {
        ddlCategorias.appendChild(crearOption(categoria.nombre, categoria.idcategoria));
    });

    ddlCategorias.addEventListener("change", () => {
        const idCategoria = Number(ddlCategorias.value);

        ddlProductos.length = 1;
        ddlProductos.selectedIndex = 0;

        productos
            .filter((producto) => producto.id_categoria === idCategoria)
            .forEach((producto) => ddlProductos.appendChild(crearOption(producto.nombre, producto.id_producto)));

        txtPrecio.value = "";
        actualizarSubtotal();
    });

    ddlProductos.addEventListener("change", () => {
        const idProducto = Number(ddlProductos.value);
        const producto = productos.find((actual) => actual.id_producto === idProducto);

        txtPrecio.value = producto ? producto.precio : "";

        if (!txtCantidad.value) {
            txtCantidad.value = 1;
        }

        actualizarSubtotal();
    });

    txtCantidad.addEventListener("input", actualizarSubtotal);
    txtPrecio.addEventListener("input", actualizarSubtotal);

    btnEliminar.addEventListener("click", () => {
        const cantidadTarjetas = contenedorProductos.querySelectorAll(".sub-contenedor-productos").length;

        if (cantidadTarjetas > 1) {
            tarjeta.remove();
            actualizarTotal();
            return;
        }

        ddlCategorias.selectedIndex = 0;
        ddlProductos.length = 1;
        ddlProductos.selectedIndex = 0;
        txtPrecio.value = "";
        txtCantidad.value = "";
        actualizarSubtotal();
    });

    contenedorProductos.appendChild(fila);
    actualizarSubtotal();
}

formulario.addEventListener("submit", async (evento) => {
    evento.preventDefault();

    const datos = new FormData(formulario);

    const detalle = [...contenedorProductos.querySelectorAll(".sub-contenedor-productos")].map((fila) => {
        const producto = fila.querySelector('[name="ddl-productos"]');

        return {
            idproducto: Number(producto.value),
            nombre: producto.selectedOptions[0].textContent.trim(),
            cantidad: Number(fila.querySelector('[name="txt-cantidad"]').value),
            precio: Number(fila.querySelector('[name="txt-precio"]').value)
        };
    });

    const compra = {
        cliente: {
            nombre: datos.get("txt-nombre"),
            apellido: datos.get("txt-apellido"),
            correo: datos.get("txt-correo") || null,
            telefono: datos.get("txt-telefono"),
            zona: datos.get("txt-zona")
        },
        pedido: {
            nota: datos.get("txt-nota") || null
        },
        detalle
    };

    const btnGuardar = formulario.querySelector('button[type="submit"]');
    btnGuardar.disabled = true;
    mostrarMensaje(lblMensaje, "Guardando...");

    const resultado = await pedirApi("/Compras/guardar", { metodo: "POST", cuerpo: compra });
    btnGuardar.disabled = false;

    if (!resultado.ok) {
        mostrarMensaje(lblMensaje, mensajeError(resultado, "No se pudo guardar el pedido."), false);
        return;
    }

    limpiarFormulario();
    mostrarMensaje(lblMensaje, resultado.datos.mensaje, true);
    mostrarPedidoGuardado(resultado.datos);
});

const dlgPedidoGuardado = document.getElementById("dlg-pedido-guardado");
const txtLinkSeguimiento = document.getElementById("txt-link-seguimiento");
const lblCopiado = document.getElementById("lbl-copiado");
const lblMailCliente = document.getElementById("lbl-mail-cliente");

function mostrarPedidoGuardado(resultado) {
    const link = new URL(`seguimiento?id=${encodeURIComponent(resultado.idPedido)}`, window.location.href);

    document.getElementById("lbl-numero-guardado").textContent = `PEDIDO #${resultado.numero}`;
    txtLinkSeguimiento.value = link.href;
    lblCopiado.textContent = "";
    if (resultado.mailEnviado) {
        lblMailCliente.textContent = `Le enviamos el pedido por mail a ${resultado.correo}.`;
    }
    else if (resultado.correo) {
        lblMailCliente.textContent = `No se pudo enviar el mail a ${resultado.correo}. Mandale el link por WhatsApp.`;
    }
    else {
        lblMailCliente.textContent = "El cliente no dejó mail: mandale el link por WhatsApp.";
    }

    dlgPedidoGuardado.showModal();
}

document.getElementById("btn-copiar-link").addEventListener("click", async () => {
    try {
        await navigator.clipboard.writeText(txtLinkSeguimiento.value);
    }
    catch (error) {
        txtLinkSeguimiento.select();
        lblCopiado.textContent = "Seleccioné el link: copialo con Ctrl+C o mantené apretado.";
        return;
    }

    lblCopiado.textContent = "Link copiado.";
});

function limpiarFormulario() {
    formulario.reset();
    contenedorProductos.querySelectorAll(".sub-contenedor-productos").forEach((fila) => fila.remove());
    agregarFilaProducto();
    chkClienteExistente.dispatchEvent(new Event("change"));
}

btnAgregar.addEventListener("click", agregarFilaProducto);

const chkClienteExistente = document.getElementById("chk-cliente-existente");
const txtBuscarTelefono = document.getElementById("txt-buscar-telefono");
const btnBuscarCliente = document.getElementById("btn-buscar-cliente");
const lblBusqueda = document.getElementById("lbl-busqueda");

chkClienteExistente.addEventListener("change", () => {
    const habilitado = chkClienteExistente.checked;

    txtBuscarTelefono.disabled = !habilitado;
    btnBuscarCliente.disabled = !habilitado;
    mostrarMensaje(lblBusqueda, "");

    if (habilitado) {
        txtBuscarTelefono.focus();
    }
    else {
        txtBuscarTelefono.value = "";
    }
});

async function buscarCliente() {
    const telefono = txtBuscarTelefono.value.trim();

    if (!txtBuscarTelefono.checkValidity() || telefono === "") {
        mostrarMensaje(lblBusqueda, "Ingresá entre 10 y 20 dígitos, sin espacios ni guiones.", false);
        return;
    }

    btnBuscarCliente.disabled = true;

    const resultado = await pedirApi(`/Clientes/por_telefono/${encodeURIComponent(telefono)}`);
    btnBuscarCliente.disabled = !chkClienteExistente.checked;

    if (!resultado.ok) {
        mostrarMensaje(lblBusqueda, mensajeError(resultado, "No se pudo buscar el cliente."), false);
        return;
    }

    const cliente = resultado.datos;
    formulario.elements["txt-nombre"].value = cliente.nombre;
    formulario.elements["txt-apellido"].value = cliente.apellido;
    formulario.elements["txt-correo"].value = cliente.correo ?? "";
    formulario.elements["txt-telefono"].value = cliente.telefono;
    formulario.elements["txt-zona"].value = cliente.zona;
    mostrarMensaje(lblBusqueda, `Cliente encontrado: ${cliente.nombre} ${cliente.apellido}.`, true);
}

btnBuscarCliente.addEventListener("click", buscarCliente);

txtBuscarTelefono.addEventListener("keydown", (evento) => {
    if (evento.key === "Enter") {
        evento.preventDefault();
        buscarCliente();
    }
});

document.addEventListener("DOMContentLoaded", async () => {
    if (!await exigirSesion()) {
        return;
    }

    await cargarCategorias();
    await cargarProductos();
    agregarFilaProducto();
});
