import { haySesion } from "./sesion.js";

const RUTA_INICIO = "admin.html";

const MENSAJES_ERROR = {
    401: "Usuario o contraseña incorrectos.",
    429: "Demasiados intentos. Esperá unos minutos y probá de nuevo."
};

const formulario = document.getElementById("formulario");
const botonIngresar = document.getElementById("btn-ingresar");
const mensaje = document.getElementById("lbl-mensaje");
const campoPassword = document.getElementById("txt-password");
const botonVerPassword = document.getElementById("btn-ver-password");
const iconoVerPassword = botonVerPassword.querySelector("img");

formulario.addEventListener("submit", async (evento) => {
    evento.preventDefault();

    const datos = new FormData(formulario);

    mensaje.textContent = "";
    botonIngresar.disabled = true;
    botonIngresar.textContent = "Ingresando...";

    try {
        const respuesta = await fetch("/api/auth/login", {
            method: "POST",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify({
                usuario: datos.get("username").trim(),
                contrasena: datos.get("password")
            })
        });

        if (respuesta.ok) {
            window.location.replace(RUTA_INICIO);
            return;
        }

        mensaje.textContent = MENSAJES_ERROR[respuesta.status] ?? "No pudimos iniciar sesión. Probá de nuevo en unos minutos.";
    }
    catch (error) {
        mensaje.textContent = "No pudimos conectarnos. Revisá tu conexión y probá de nuevo.";
    }

    campoPassword.value = "";
    campoPassword.focus();
    botonIngresar.disabled = false;
    botonIngresar.textContent = "Ingresar";
});

botonVerPassword.addEventListener("click", () => {
    const oculta = campoPassword.type === "password";

    campoPassword.type = oculta ? "text" : "password";
    iconoVerPassword.src = oculta ? "Iconos/ojos-cruzados.svg" : "Iconos/ojo.svg";
    botonVerPassword.setAttribute("aria-label", oculta ? "Ocultar contraseña" : "Mostrar contraseña");
    botonVerPassword.setAttribute("aria-pressed", String(oculta));
});

document.addEventListener("DOMContentLoaded", async () => {
    try {
        if (await haySesion()) {
            window.location.replace(RUTA_INICIO);
        }
    }
    catch (error) {
        // Si la API no responde, el formulario sigue disponible y el error se muestra al ingresar.
    }
});
