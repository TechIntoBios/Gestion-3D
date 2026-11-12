import { pedirApi } from "./api.js";

const RUTA_LOGIN = "ingresar";

export async function haySesion() {
    const { ok } = await pedirApi("/auth/sesion");
    return ok;
}

export async function exigirSesion() {
    if (await haySesion()) {
        return true;
    }

    window.location.replace(RUTA_LOGIN);
    return false;
}

export async function cerrarSesion() {
    await pedirApi("/auth/logout", { metodo: "POST" });
    window.location.replace(RUTA_LOGIN);
}
