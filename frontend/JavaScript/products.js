import { mensajeError, pedirApi } from "./api.js";
import { iniciarEdicion } from "./catalogo-edicion.js";
import { crearOption, mostrarMensaje } from "./funciones.js";
import { exigirSesion } from "./sesion.js";

const lblMensajeProductos = document.getElementById("lbl-mensaje-productos");
const lblMensajeCategorias = document.getElementById("lbl-mensaje-categorias");
const lblResumenProductos = document.getElementById("lbl-resumen-productos");
const lblResumenCategorias = document.getElementById("lbl-resumen-categorias");
const lblErrores = document.getElementById("lbl-errores");
const formProducto = document.getElementById("form-productos");
const formCategorias = document.getElementById("form-categorias");
const ddlCategorias = document.getElementById("ddl-categorias");

function plural(cantidad, singular, pluralTexto) {
    return `${cantidad} ${cantidad === 1 ? singular : pluralTexto}`;
}

function resumen(items, singular, pluralTexto, inactivo, inactivos) {
    const apagados = items.filter((item) => !item.activo).length;
    const texto = plural(items.length, singular, pluralTexto);

    return apagados === 0 ? texto : `${texto} · ${plural(apagados, inactivo, inactivos)}`;
}

async function cargarCatalogo() {
    const [categorias, productos] = await Promise.all([
        pedirApi("/Categorias/listar_Categorias?todos=true"),
        pedirApi("/Productos/listar_productos?todos=true")
    ]);

    const fallo = [categorias, productos].find((resultado) => !resultado.ok);
    if (fallo) {
        mostrarMensaje(lblErrores, mensajeError(fallo, "No se pudo cargar el catálogo."), false);
        return { categorias: [], productos: [] };
    }

    mostrarMensaje(lblErrores, "");
    lblResumenCategorias.textContent = resumen(categorias.datos, "categoría", "categorías", "inactiva", "inactivas");
    lblResumenProductos.textContent = resumen(productos.datos, "producto", "productos", "inactivo", "inactivos");

    ddlCategorias.length = 1;
    categorias.datos
        .filter((categoria) => categoria.activo)
        .forEach((categoria) => ddlCategorias.appendChild(crearOption(categoria.nombre, categoria.idcategoria)));

    return { categorias: categorias.datos, productos: productos.datos };
}

const edicion = iniciarEdicion({
    cargarCatalogo,
    alGuardar: async (tipo, mensaje) => {
        await cargarCatalogo();
        mostrarMensaje(tipo === "producto" ? lblMensajeProductos : lblMensajeCategorias, mensaje, true);
    }
});

document.getElementById("btn-editar-productos").addEventListener("click", () => edicion.abrir("producto"));
document.getElementById("btn-editar-categorias").addEventListener("click", () => edicion.abrir("categoria"));

async function enviarFormulario(formulario, ruta, cuerpo, lblMensaje, mensajePorDefecto) {
    const boton = formulario.querySelector('button[type="submit"]');
    boton.disabled = true;

    const resultado = await pedirApi(ruta, { metodo: "POST", cuerpo });
    boton.disabled = false;

    if (!resultado.ok) {
        mostrarMensaje(lblMensaje, mensajeError(resultado, mensajePorDefecto), false);
        return false;
    }

    mostrarMensaje(lblMensaje, resultado.datos.mensaje, true);
    formulario.reset();
    await cargarCatalogo();
    return true;
}

formProducto.addEventListener("submit", async (evento) => {
    evento.preventDefault();

    const datos = new FormData(formProducto);
    await enviarFormulario(formProducto, "/Productos/Agregar", {
        id_categoria: Number(datos.get("categoria")),
        nombre: datos.get("nombreProducto").trim(),
        precio: Number(datos.get("precio"))
    }, lblMensajeProductos, "No se pudo agregar el producto.");
});

formCategorias.addEventListener("submit", async (evento) => {
    evento.preventDefault();

    const datos = new FormData(formCategorias);
    await enviarFormulario(formCategorias, "/Categorias/agregar_Categorias", {
        nombre: datos.get("nombreCategoria").trim()
    }, lblMensajeCategorias, "No se pudo agregar la categoría.");
});

document.addEventListener("DOMContentLoaded", async () => {
    if (await exigirSesion()) {
        await cargarCatalogo();
    }
});
