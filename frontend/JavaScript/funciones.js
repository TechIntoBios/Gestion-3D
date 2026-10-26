export function crearCelda(texto) {
    const td = document.createElement("td");
    td.textContent = texto;
    return td;
}

export function crearCeldaControl(control) {
    const td = document.createElement("td");
    td.appendChild(control);
    return td;
}

export function crearSpan(texto) {
    const span = document.createElement("span");
    span.textContent = texto;
    return span;
}

export function crearCeldaImg(ruta, texto) {
    const img = document.createElement("img");
    img.src = ruta || "";
    img.alt = texto || "Producto";
    img.style.maxWidth = "80px";
    img.style.maxHeight = "80px";
    img.style.objectFit = "cover";
    return crearCeldaControl(img);
}

export function crearButton(texto, modo) {
    const button = document.createElement("button");
    button.textContent = texto;
    button.style.display = modo;
    return button;
}

export function crearInput(texto, modo) {
    const input = document.createElement("input");
    input.type = modo;
    input.value = texto;
    input.min = 1;
    // input.max = max; 
    return crearCeldaControl(input);
}

export function crearOption(texto, valor) {
    const option = document.createElement("option");
    option.textContent = texto;
    option.value = valor;
    return option;
}

export function crearCeldaBadgeEstado(estado) {
    const td = document.createElement("td");
    const span = document.createElement("span");
    const estados = (estado || "").trim().toLowerCase();

    span.textContent = estado || "—";
    span.classList.add("badge_estado");

    if (estados === "comprado" || estados === "completado") {
        span.classList.add("comprado");
    } else if (estados === "cancelado") {
        span.classList.add("cancelado");
    } else {
        span.classList.add("pendiente");
    }

    td.appendChild(span);
    return td;
}