const RUTA_LOGIN = "login.html";

export async function haySesion() {
    const respuesta = await fetch("/api/auth/sesion");
    return respuesta.ok;
}

// La protección real está en la API; esto solo evita mostrar la pantalla sin sesión.
export async function exigirSesion() {
    try {
        if (await haySesion()) {
            return true;
        }
    }
    catch (error) {
        // Sin conexión tampoco se puede confirmar la sesión.
    }

    window.location.replace(RUTA_LOGIN);
    return false;
}

export async function cerrarSesion() {
    try {
        await fetch("/api/auth/logout", { method: "POST" });
    }
    finally {
        window.location.replace(RUTA_LOGIN);
    }
}
