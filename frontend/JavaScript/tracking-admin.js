import { exigirSesion } from "./sesion.js";
import { crearOption } from "./funciones.js";
import { formatoFecha, mostrarCliente as mostrarDatosCliente, mostrarPedido as mostrarDatosPedido } from "./vista-pedido.js";

const URL_API = "/api";

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

const btnGuardarEstado = document.getElementById("btn-guardar-estado");
const txtObservacion = document.getElementById("txt-observacion");
const lblEstadoGuardado = document.getElementById("lbl-estado-guardado");

let pedidos = [];
let pedidoActual = null;

function mostrarBusqueda(texto, encontrado) {
    lblBusqueda.textContent = texto;
    lblBusqueda.className = `mensaje-busqueda ${encontrado ? "encontrado" : "no-encontrado"}`;
}

function mostrarCliente(cliente) {
    mostrarDatosCliente(formContacto, cliente);
}

function mostrarPedido(pedido) {
    pedidoActual = pedido ?? null;
    btnGuardarEstado.disabled = !pedido;
    lblEstadoGuardado.textContent = "";
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

formBuscar.addEventListener("submit", async (evento) => {
    evento.preventDefault();

    const telefono = txtBuscarTelefono.value.trim();
    btnBuscar.disabled = true;

    try {
        const respuesta = await fetch(`${URL_API}/Pedidos/por_telefono/${encodeURIComponent(telefono)}`);
        const datos = await respuesta.json().catch(() => ({}));

        if (!respuesta.ok) {
            limpiarPantalla();
            mostrarBusqueda(datos.mensaje ?? "No se pudo buscar el pedido.", false);
            return;
        }

        pedidos = datos.pedidos;
        mostrarCliente(datos.cliente);

        if (pedidos.length === 0) {
            contenedorPedidos.hidden = true;
            mostrarPedido(null);
            mostrarBusqueda(`${datos.cliente.nombre} ${datos.cliente.apellido} todavía no tiene pedidos.`, false);
            return;
        }

        ddlPedidos.replaceChildren(...pedidos.map((pedido) => crearOption(textoOpcion(pedido), pedido.idpedido)));
        contenedorPedidos.hidden = pedidos.length < 2;
        mostrarPedido(pedidos[0]);

        const cantidad = pedidos.length === 1 ? "1 pedido" : `${pedidos.length} pedidos`;
        mostrarBusqueda(`${datos.cliente.nombre} ${datos.cliente.apellido}: ${cantidad}.`, true);
    }
    catch (error) {
        mostrarBusqueda("No se logro conectar con el servidor.", false);
    }
    finally {
        btnBuscar.disabled = false;
    }
});

ddlPedidos.addEventListener("change", () => {
    mostrarPedido(pedidos.find((pedido) => pedido.idpedido === ddlPedidos.value));
});

function textoOpcion(pedido) {
    return `#${pedido.numero} · ${formatoFecha.format(new Date(pedido.fecha))} · ${pedido.estado}`;
}

btnGuardarEstado.addEventListener("click", async () => {
    const estadoElegido = document.querySelector('input[name="estado"]:checked');

    if (!pedidoActual || !estadoElegido) {
        lblEstadoGuardado.textContent = "Elegí un estado.";
        lblEstadoGuardado.className = "mensaje-busqueda no-encontrado";
        return;
    }

    btnGuardarEstado.disabled = true;

    try {
        const respuesta = await fetch(`${URL_API}/Pedidos/${pedidoActual.idpedido}/estado`, {
            method: "PUT",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify({ codigoestado: estadoElegido.value, seguimiento: txtObservacion.value })
        });
        const datos = await respuesta.json().catch(() => ({}));

        if (!respuesta.ok) {
            lblEstadoGuardado.textContent = datos.mensaje ?? "No se pudieron guardar los cambios.";
            lblEstadoGuardado.className = "mensaje-busqueda no-encontrado";
            return;
        }

        pedidos[pedidos.findIndex((pedido) => pedido.idpedido === datos.pedido.idpedido)] = datos.pedido;
        const opcion = [...ddlPedidos.options].find((opcion) => opcion.value === datos.pedido.idpedido);
        if (opcion) {
            opcion.textContent = textoOpcion(datos.pedido);
        }
        mostrarPedido(datos.pedido);

        lblEstadoGuardado.textContent = datos.mensaje;
        lblEstadoGuardado.className = "mensaje-busqueda encontrado";
    }
    catch (error) {
        lblEstadoGuardado.textContent = "No se logro conectar con el servidor.";
        lblEstadoGuardado.className = "mensaje-busqueda no-encontrado";
    }
    finally {
        btnGuardarEstado.disabled = !pedidoActual;
    }
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

formContacto.addEventListener("submit", (evento) => {
    evento.preventDefault();
    // Pendiente: enviar los datos a la API y, si responde bien, actualizar los textos.
});

document.addEventListener("DOMContentLoaded", async () => {
    if (!await exigirSesion()) {
        return;
    }
});
