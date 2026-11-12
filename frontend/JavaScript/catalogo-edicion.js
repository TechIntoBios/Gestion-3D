import { mensajeError, pedirApi } from "./api.js";
import { crearOption, formatoPesos, mostrarMensaje } from "./funciones.js";

const dialogo = document.getElementById("dlg-edicion");
const formulario = document.getElementById("form-edicion");
const titulo = document.getElementById("ttl-edicion");
const ddlItem = document.getElementById("ddl-edicion-item");
const txtNombre = document.getElementById("txt-edicion-nombre");
const camposProducto = document.getElementById("campos-edicion-producto");
const ddlCategoria = document.getElementById("ddl-edicion-categoria");
const txtPrecio = document.getElementById("txt-edicion-precio");
const chkActivo = document.getElementById("chk-edicion-activo");
const lblMensaje = document.getElementById("lbl-edicion");
const btnGuardar = document.getElementById("btn-guardar-edicion");

const TIPOS = {
    producto: { titulo: "Editar producto", ruta: "/Productos", vacio: "Todavía no hay productos para editar." },
    categoria: { titulo: "Editar categoría", ruta: "/Categorias", vacio: "Todavía no hay categorías para editar." }
};

let tipo = "producto";
let catalogo = { categorias: [], productos: [] };

function etiquetaCategoria(categoria) {
    return categoria.activo ? categoria.nombre : `${categoria.nombre} (inactiva)`;
}

function llenarItems() {
    ddlItem.replaceChildren();

    if (tipo === "categoria") {
        catalogo.categorias.forEach((categoria) => {
            ddlItem.appendChild(crearOption(etiquetaCategoria(categoria), categoria.idcategoria));
        });
        return;
    }

    catalogo.categorias.forEach((categoria) => {
        const productos = catalogo.productos.filter((producto) => producto.id_categoria === categoria.idcategoria);

        if (productos.length === 0) {
            return;
        }

        const grupo = document.createElement("optgroup");
        grupo.label = etiquetaCategoria(categoria);
        productos.forEach((producto) => {
            const sufijo = producto.activo ? "" : " (inactivo)";
            grupo.appendChild(crearOption(`${producto.nombre} · ${formatoPesos.format(producto.precio)}${sufijo}`, producto.id_producto));
        });
        ddlItem.appendChild(grupo);
    });
}

function llenarCategorias(idActual) {
    ddlCategoria.replaceChildren();

    catalogo.categorias
        .filter((categoria) => categoria.activo || categoria.idcategoria === idActual)
        .forEach((categoria) => ddlCategoria.appendChild(crearOption(etiquetaCategoria(categoria), categoria.idcategoria)));

    ddlCategoria.value = idActual;
}

function itemElegido() {
    const id = Number(ddlItem.value);

    return tipo === "categoria"
        ? catalogo.categorias.find((categoria) => categoria.idcategoria === id)
        : catalogo.productos.find((producto) => producto.id_producto === id);
}

function mostrarItem() {
    const item = itemElegido();
    mostrarMensaje(lblMensaje, "");
    btnGuardar.disabled = !item;

    if (!item) {
        return;
    }

    txtNombre.value = item.nombre;
    chkActivo.checked = item.activo;

    if (tipo === "producto") {
        llenarCategorias(item.id_categoria);
        txtPrecio.value = item.precio;
    }
}

function cuerpoDeEdicion() {
    const cuerpo = { nombre: txtNombre.value.trim(), activo: chkActivo.checked };

    if (tipo === "producto") {
        cuerpo.id_categoria = Number(ddlCategoria.value);
        cuerpo.precio = Number(txtPrecio.value);
    }

    return cuerpo;
}

export function iniciarEdicion({ cargarCatalogo, alGuardar }) {
    async function abrir(tipoElegido) {
        tipo = tipoElegido;
        titulo.textContent = TIPOS[tipo].titulo;
        camposProducto.hidden = tipo !== "producto";
        txtPrecio.required = tipo === "producto";
        mostrarMensaje(lblMensaje, "");

        catalogo = await cargarCatalogo();
        llenarItems();
        mostrarItem();

        if (!ddlItem.value) {
            mostrarMensaje(lblMensaje, TIPOS[tipo].vacio, false);
        }

        dialogo.showModal();
        ddlItem.focus();
    }

    ddlItem.addEventListener("change", mostrarItem);
    document.getElementById("btn-cancelar-edicion").addEventListener("click", () => dialogo.close());

    formulario.addEventListener("submit", async (evento) => {
        evento.preventDefault();
        btnGuardar.disabled = true;

        const resultado = await pedirApi(`${TIPOS[tipo].ruta}/${ddlItem.value}`, { metodo: "PUT", cuerpo: cuerpoDeEdicion() });
        btnGuardar.disabled = false;

        if (!resultado.ok) {
            mostrarMensaje(lblMensaje, mensajeError(resultado, "No se pudieron guardar los cambios."), false);
            return;
        }

        dialogo.close();
        await alGuardar(tipo, resultado.datos.mensaje);
    });

    return { abrir };
}
