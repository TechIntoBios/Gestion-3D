import { mensajeError, pedirApi } from "./api.js";
import { crearOption, formatoFecha, mostrarMensaje } from "./funciones.js";
import { exigirSesion } from "./sesion.js";
import { mostrarCliente as mostrarDatosCliente, mostrarPedido as mostrarDatosPedido } from "./vista-pedido.js";

const formBuscar = document.getElementById("form-buscar");
const txtBuscarTelefono = document.getElementById("txt-buscar-telefono");
const btnBuscar = document.getElementById("btn-buscar");
const lblBusqueda = document.getElementById("lbl-busqueda");
const contenedorPedidos = document.getElementById("contenedor-pedidos");
const ddlPedidos = document.getElementById("ddl-pedidos");

const formContacto = document.getElementById("form-contacto");
const btnEditarContacto = document.getElementById("btn-editar-contacto");
const btnCancelarContacto = document.getElementById("btn-cancelar-contacto");
const accionesEdicion = formContacto.querySelector(".acciones-edicion");
const lblContactoGuardado = document.getElementById("lbl-contacto-guardado");

const btnGuardarEstado = document.getElementById("btn-guardar-estado");
const txtObservacion = document.getElementById("txt-observacion");
const lblEstadoGuardado = document.getElementById("lbl-estado-guardado");

let pedidos = [];
let pedidoActual = null;
let clienteActual = null;

function mostrarCliente(cliente) {
    clienteActual = cliente ?? null;
    mostrarDatosCliente(formContacto, cliente);
    modoEdicionContacto(false);
    btnEditarContacto.disabled = !cliente;
    mostrarMensaje(lblContactoGuardado, "");
}

function mostrarPedido(pedido) {
    pedidoActual = pedido ?? null;
    btnGuardarEstado.disabled = !pedido;
    mostrarMensaje(lblEstadoGuardado, "");
    txtObservacion.value = pedido?.seguimiento ?? "";

    document.querySelectorAll('input[name="estado"]').forEach((radio) => {
        radio.checked = radio.value === pedido?.codigoestado?.toLowerCase();
    });

    mostrarDatosPedido(pedido, {
        sinPedido: "Buscá un pedido para ver su estado.",
        sinProductos: "Todavía no hay un pedido cargado."
    });
}

function limpiarPantalla() {
    pedidos = [];
    contenedorPedidos.hidden = true;
    mostrarCliente(null);
    mostrarPedido(null);
}

function textoOpcion(pedido) {
    return `#${pedido.numero} · ${formatoFecha.format(new Date(pedido.fecha))} · ${pedido.estado}`;
}

formBuscar.addEventListener("submit", async (evento) => {
    evento.preventDefault();

    const telefono = txtBuscarTelefono.value.trim();
    btnBuscar.disabled = true;

    const resultado = await pedirApi(`/Pedidos/por_telefono/${encodeURIComponent(telefono)}`);
    btnBuscar.disabled = false;

    if (!resultado.ok) {
        limpiarPantalla();
        mostrarMensaje(lblBusqueda, mensajeError(resultado, "No se pudo buscar el pedido."), false);
        return;
    }

    const { cliente } = resultado.datos;
    pedidos = resultado.datos.pedidos;
    mostrarCliente(cliente);

    if (pedidos.length === 0) {
        contenedorPedidos.hidden = true;
        mostrarPedido(null);
        mostrarMensaje(lblBusqueda, `${cliente.nombre} ${cliente.apellido} todavía no tiene pedidos.`, false);
        return;
    }

    ddlPedidos.replaceChildren(...pedidos.map((pedido) => crearOption(textoOpcion(pedido), pedido.idpedido)));
    contenedorPedidos.hidden = pedidos.length < 2;
    mostrarPedido(pedidos[0]);

    const cantidad = pedidos.length === 1 ? "1 pedido" : `${pedidos.length} pedidos`;
    mostrarMensaje(lblBusqueda, `${cliente.nombre} ${cliente.apellido}: ${cantidad}.`, true);
});

ddlPedidos.addEventListener("change", () => {
    mostrarPedido(pedidos.find((pedido) => pedido.idpedido === ddlPedidos.value));
});

btnGuardarEstado.addEventListener("click", async () => {
    const estadoElegido = document.querySelector('input[name="estado"]:checked');

    if (!pedidoActual || !estadoElegido) {
        mostrarMensaje(lblEstadoGuardado, "Elegí un estado.", false);
        return;
    }

    btnGuardarEstado.disabled = true;

    const resultado = await pedirApi(`/Pedidos/${pedidoActual.idpedido}/estado`, {
        metodo: "PUT",
        cuerpo: { codigoestado: estadoElegido.value, seguimiento: txtObservacion.value }
    });

    if (!resultado.ok) {
        mostrarMensaje(lblEstadoGuardado, mensajeError(resultado, "No se pudieron guardar los cambios."), false);
        btnGuardarEstado.disabled = !pedidoActual;
        return;
    }

    const { pedido, mensaje } = resultado.datos;

    if (pedido) {
        pedidos[pedidos.findIndex((actual) => actual.idpedido === pedido.idpedido)] = pedido;
        const opcion = [...ddlPedidos.options].find((actual) => actual.value === pedido.idpedido);
        if (opcion) {
            opcion.textContent = textoOpcion(pedido);
        }
        mostrarPedido(pedido);
    }

    mostrarMensaje(lblEstadoGuardado, mensaje, true);
    btnGuardarEstado.disabled = !pedidoActual;
});

function modoEdicionContacto(editar) {
    formContacto.querySelectorAll(".dato-item").forEach((item) => {
        const valor = item.querySelector(".dato-valor");
        const input = item.querySelector(".dato-input");

        if (editar) {
            input.value = valor.textContent === "—" ? "" : valor.textContent;
        }

        valor.hidden = editar;
        input.hidden = !editar;
    });

    accionesEdicion.hidden = !editar;
    btnEditarContacto.hidden = editar;
    btnEditarContacto.setAttribute("aria-pressed", String(editar));

    if (editar) {
        formContacto.querySelector(".dato-input").focus();
    }
}

btnEditarContacto.addEventListener("click", () => modoEdicionContacto(true));
btnCancelarContacto.addEventListener("click", () => modoEdicionContacto(false));

formContacto.addEventListener("submit", async (evento) => {
    evento.preventDefault();

    if (!clienteActual) {
        return;
    }

    const datos = new FormData(formContacto);
    const botonGuardar = formContacto.querySelector('button[type="submit"]');
    botonGuardar.disabled = true;

    const resultado = await pedirApi(`/Clientes/${clienteActual.idcliente}`, {
        metodo: "PUT",
        cuerpo: Object.fromEntries(datos)
    });
    botonGuardar.disabled = false;

    if (!resultado.ok) {
        mostrarMensaje(lblContactoGuardado, mensajeError(resultado, "No se pudieron guardar los datos de contacto."), false);
        return;
    }

    const { cliente, mensaje } = resultado.datos;
    mostrarCliente(cliente);
    txtBuscarTelefono.value = cliente.telefono;
    mostrarMensaje(lblContactoGuardado, mensaje, true);
});

document.addEventListener("DOMContentLoaded", exigirSesion);
