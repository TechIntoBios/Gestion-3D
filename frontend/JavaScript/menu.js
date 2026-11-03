import { cerrarSesion } from "./sesion.js";

const botonMenu = document.getElementById("btn-menu");
const panelMenu = document.getElementById("panel-menu");

function mostrarMenu(mostrar) {
    panelMenu.hidden = !mostrar;
    botonMenu.setAttribute("aria-expanded", String(mostrar));
}

botonMenu.addEventListener("click", (evento) => {
    evento.stopPropagation();
    mostrarMenu(panelMenu.hidden);
});

// Se cierra al tocar fuera del panel o con Escape.
document.addEventListener("click", (evento) => {
    if (!panelMenu.hidden && !panelMenu.contains(evento.target)) {
        mostrarMenu(false);
    }
});

document.addEventListener("keydown", (evento) => {
    if (evento.key === "Escape" && !panelMenu.hidden) {
        mostrarMenu(false);
        botonMenu.focus();
    }
});

document.getElementById("btn-cerrar-sesion").addEventListener("click", cerrarSesion);
