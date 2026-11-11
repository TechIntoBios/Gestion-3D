import { crearCelda } from "./funciones.js";

export const formatoPesos = new Intl.NumberFormat("es-AR", { style: "currency", currency: "ARS" });
export const formatoFecha = new Intl.DateTimeFormat("es-AR", { dateStyle: "short" });

const ESTADOS = {
    PENDIENTE: { icono: "Iconos/reloj.svg", descripcion: "El pedido está en preparación." },
    REALIZADO: { icono: "Iconos/Check.svg", descripcion: "El pedido ya está terminado." },
    CANCELADO: { icono: "Iconos/Cancelar.svg", descripcion: "El pedido fue cancelado." }
};

export function mostrarCliente(contenedor, cliente) {
    contenedor.querySelectorAll(".dato-valor[data-campo]").forEach((valor) => {
        valor.textContent = cliente?.[valor.dataset.campo] || "—";
    });
}

export function mostrarPedido(pedido, { sinPedido, sinProductos }) {
    const estado = ESTADOS[pedido?.codigoestado];

    document.getElementById("section-estado").dataset.estado = pedido?.codigoestado ?? "PENDIENTE";
    document.getElementById("lbl-numero-pedido").textContent = pedido ? `PEDIDO #${pedido.numero}` : "SEGUIMIENTO";
    document.getElementById("lbl-estado").textContent = pedido?.estado ?? "—";
    document.getElementById("img-estado").src = estado?.icono ?? ESTADOS.PENDIENTE.icono;
    document.getElementById("lbl-estado-desc").textContent = pedido
        ? `${estado?.descripcion ?? ""} Cargado el ${formatoFecha.format(new Date(pedido.fecha))}.`
        : sinPedido;
    document.getElementById("lbl-nota").textContent = pedido?.nota || "—";

    const tbody = document.getElementById("tbody-productos");
    tbody.replaceChildren();

    if (!pedido || pedido.detalle.length === 0) {
        const fila = document.createElement("tr");
        const celda = crearCelda(pedido ? "Este pedido no tiene productos." : sinProductos);
        fila.className = "fila-vacia";
        celda.colSpan = 2;
        fila.appendChild(celda);
        tbody.appendChild(fila);
    }
    else {
        pedido.detalle.forEach((item) => {
            const fila = document.createElement("tr");
            const producto = document.createElement("td");
            const nombre = document.createElement("div");
            const unitario = document.createElement("div");
            const importe = crearCelda(formatoPesos.format(item.importe));

            nombre.className = "prod-nombre";
            nombre.textContent = item.nombre;
            unitario.className = "prod-unitario";
            unitario.textContent = `${item.cantidad} x ${formatoPesos.format(item.precio)}`;
            importe.className = "prod-precio";

            producto.append(nombre, unitario);
            fila.append(producto, importe);
            tbody.appendChild(fila);
        });
    }

    const unidades = pedido ? pedido.detalle.reduce((suma, item) => suma + item.cantidad, 0) : 0;
    document.getElementById("lbl-unidades").textContent = `${unidades} ${unidades === 1 ? "unidad" : "unidades"}`;
    document.getElementById("lbl-total").textContent = formatoPesos.format(pedido?.total ?? 0);
}
