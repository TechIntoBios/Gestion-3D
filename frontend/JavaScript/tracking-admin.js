import { exigirSesion } from "./sesion.js";

const formContacto = document.getElementById("form-contacto");
const btnEditarContacto = document.getElementById("btn-editar-contacto");
const btnCancelarContacto = document.getElementById("btn-cancelar-contacto");
const accionesEdicion = formContacto.querySelector(".acciones-edicion");

// Cambia los datos de contacto entre texto y controles editables.
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

document.getElementById("form-buscar").addEventListener("submit", (evento) => {
    evento.preventDefault();
    // Pendiente: buscar por teléfono y cargar estado, contacto, productos y nota del pedido.
});

document.addEventListener("DOMContentLoaded", async () => {
    if (!await exigirSesion()) {
        return;
    }
});
