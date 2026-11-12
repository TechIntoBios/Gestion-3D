const URL_API = "/api";

const MENSAJE_SESION_VENCIDA = "Tu sesión venció. Volvé a iniciar sesión en otra pestaña y probá de nuevo: lo que cargaste acá no se pierde.";
const MENSAJE_SIN_CONEXION = "No se logró conectar con el servidor.";

export async function pedirApi(ruta, { metodo = "GET", cuerpo } = {}) {
    const opciones = { method: metodo };

    if (cuerpo !== undefined) {
        opciones.headers = { "Content-Type": "application/json" };
        opciones.body = JSON.stringify(cuerpo);
    }

    try {
        const respuesta = await fetch(`${URL_API}${ruta}`, opciones);
        const datos = await respuesta.json().catch(() => ({}));
        return { ok: respuesta.ok, status: respuesta.status, datos };
    }
    catch (error) {
        return { ok: false, status: 0, datos: {} };
    }
}

export function mensajeError({ status, datos }, mensajePorDefecto) {
    if (status === 0) {
        return MENSAJE_SIN_CONEXION;
    }

    if (status === 401) {
        return MENSAJE_SESION_VENCIDA;
    }

    return datos.mensaje ?? mensajePorDefecto;
}
