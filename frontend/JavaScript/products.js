import { exigirSesion } from "./sesion.js";
import { crearCelda, crearCeldaImg, crearButton, crearInput, crearOption } from "./funciones.js";

const URL_API = "/api"; 

const mensaje_productos = document.getElementById("lbl_mensaje_productos"); 
const mensaje_categorias = document.getElementById("lbl_mensaje_categorias"); 
const errores = document.getElementById("lbl_errores"); 
const formProducto = document.getElementById("form-productos"); 
const formCategorias = document.getElementById("form-categorias"); 

mensaje_categorias.innerHTML = ""; 
mensaje_productos.innerHTML = ""; 
errores.innerHTML = ""; 

async function cargarCategorias() {
    const ddlcategorias = document.getElementById("ddl_categorias");

    try{
        const respuesta_cat = await fetch(`${URL_API}/Categorias/listar_Categorias`); 
        
        if(!respuesta_cat.ok){
            mensaje_categorias.textContent = "No se encontraron categorias existentes";
            return;
        }

        const categorias = await respuesta_cat.json(); 

        ddlcategorias.length = 1; 
        categorias.forEach(cat => {
            ddlcategorias.appendChild(crearOption(cat.nombre, cat.idcategoria)); 
        });
    }
    catch(error){
        errores.innerHTML = "Ocurrio un error a conectarse a la base de datos"; 
    }
}

formProducto.addEventListener("submit", async (evento) => {
    evento.preventDefault();
    
    try {
        const datosform = new FormData(formProducto);
        const respuesta_products = await fetch(`${URL_API}/Productos/Agregar`, {
            method: "POST",
            headers: {"Content-Type": "application/json"}, 
            body: JSON.stringify(
                {
                    id_categoria: Number(datosform.get("categoria")),
                    nombre: datosform.get("nombreProducto").trim(),
                    precio: Number(datosform.get("precio"))
                }
            )
        });

        const datos = await respuesta_products.json().catch(() => ({}));

        if(!respuesta_products.ok){
            mensaje_productos.textContent = datos.mensaje ?? "No se pudo agregar el producto.";
            return;
        }
        
        mensaje_productos.textContent = datos.mensaje;
        formProducto.reset();
    }
    catch (error) {
        mensaje_productos.textContent = "Ocurrio un error al conectar a la base de datos";
    }
});

formCategorias.addEventListener("submit", async (evento) => {
    evento.preventDefault();
    
    try {
        const datosform = new FormData(formCategorias);
        const respuesta_categorias = await fetch(`${URL_API}/Categorias/agregar_Categorias`, {
            method: "POST",
            headers: {"Content-Type": "application/json"}, 
            body: JSON.stringify({nombre: datosform.get("nombreCategoria").trim()})
        });

        const datos = await respuesta_categorias.json().catch(() => ({}));

        if(!respuesta_categorias.ok){
            mensaje_categorias.textContent = datos.mensaje ?? "No se pudo agregar la categoria.";
            return;
        }
        
        mensaje_categorias.textContent = datos.mensaje;
        formCategorias.reset();
        
        await cargarCategorias(); 
    }
    catch (error) {
        mensaje_categorias.textContent = "Ocurrio un error al conectar a la base de datos";
    }
});

document.addEventListener("DOMContentLoaded", async () => {
    if (!await exigirSesion()) {
        return;
    }

    await cargarCategorias(); 
});
