import { pedirApi } from "./api.js";
import { haySesion } from "./sesion.js";

const RUTA_INICIO = "pedidos";

const MENSAJES_ERROR = {
    0: "No pudimos conectarnos. Revisá tu conexión y probá de nuevo.",
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

    const resultado = await pedirApi("/auth/login", {
        metodo: "POST",
        cuerpo: { usuario: datos.get("username").trim(), contrasena: datos.get("password") }
    });

    if (resultado.ok) {
        window.location.replace(RUTA_INICIO);
        return;
    }

    mensaje.textContent = MENSAJES_ERROR[resultado.status] ?? "No pudimos iniciar sesión. Probá de nuevo en unos minutos.";
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
    if (await haySesion()) {
        window.location.replace(RUTA_INICIO);
    }
});
