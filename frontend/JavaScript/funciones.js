export const formatoPesos = new Intl.NumberFormat("es-AR", { style: "currency", currency: "ARS" });
export const formatoFecha = new Intl.DateTimeFormat("es-AR", { dateStyle: "short" });

export function crearCelda(texto) {
    const td = document.createElement("td");
    td.textContent = texto;
    return td;
}

export function crearOption(texto, valor) {
    const option = document.createElement("option");
    option.textContent = texto;
    option.value = valor;
    return option;
}

export function mostrarMensaje(elemento, texto, exito) {
    const color = exito === undefined ? "" : exito ? " encontrado" : " no-encontrado";

    elemento.textContent = texto;
    elemento.className = `mensaje-busqueda${color}`;
}
