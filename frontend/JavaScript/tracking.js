import { mostrarCliente, mostrarPedido } from "./vista-pedido.js";

const URL_API = "/api";
const FORMATO_UUID = /^[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}$/i;

const lblSubtitulo = document.getElementById("lbl-subtitulo");
const lblObservacion = document.getElementById("lbl-observacion");
const seccionContacto = document.getElementById("section-contacto");

const TEXTOS_VACIOS = {
    sinPedido: "No pudimos mostrar tu pedido.",
    sinProductos: "Todavía no hay productos para mostrar."
};

function mostrarObservacion(texto) {
    lblObservacion.replaceChildren();

    if (!texto) {
        lblObservacion.textContent = "Todavía no hay novedades sobre tu pedido.";
        return;
    }

    texto.split(/(https?:\/\/[^\s]+)/g).forEach((parte) => {
        if (!/^https?:\/\//.test(parte)) {
            lblObservacion.append(parte);
            return;
        }

        const enlace = document.createElement("a");
        enlace.href = parte;
        enlace.textContent = parte;
        enlace.target = "_blank";
        enlace.rel = "noopener noreferrer";
        lblObservacion.append(enlace);
    });
}

function mostrarError(mensaje) {
    lblSubtitulo.textContent = mensaje;
    mostrarPedido(null, TEXTOS_VACIOS);
    mostrarCliente(seccionContacto, null);
    mostrarObservacion(null);
}

async function cargarSeguimiento() {
    const id = new URLSearchParams(window.location.search).get("id")?.trim() ?? "";

    if (!FORMATO_UUID.test(id)) {
        mostrarError("El link de seguimiento no es válido. Pedinos uno nuevo por WhatsApp.");
        return;
    }

    try {
        const respuesta = await fetch(`${URL_API}/Pedidos/publico/${id}`);
        const datos = await respuesta.json().catch(() => ({}));

        if (!respuesta.ok) {
            mostrarError(respuesta.status === 429
                ? "Hiciste muchas consultas seguidas. Probá de nuevo en un minuto."
                : datos.mensaje ?? "No pudimos cargar tu pedido.");
            return;
        }

        lblSubtitulo.textContent = `Hola ${datos.cliente.nombre}, estos son los datos de tu pedido.`;
        mostrarPedido(datos.pedido, TEXTOS_VACIOS);
        mostrarCliente(seccionContacto, datos.cliente);
        mostrarObservacion(datos.pedido.seguimiento);
    }
    catch (error) {
        mostrarError("No pudimos conectarnos. Revisá tu conexión y volvé a abrir el link.");
    }
}

cargarSeguimiento();
