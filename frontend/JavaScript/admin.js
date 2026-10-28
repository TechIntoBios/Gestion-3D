const URL_API = "https://localhost:7199/api"; 

import { crearCelda, crearCeldaImg, crearButton, crearInput, crearOption } from "./funciones.js"; 

const label = document.getElementById("lbl-mensaje"); 
label.innerHTML = ""; 
const btnagregar = document.getElementById("btn-agregar"); 
const contenedorProductos = document.getElementById("tarjeta-productos");
const plantillaProducto = document.getElementById("tarjeta-sub-productos");

function agregarFilaProducto() {
    const fila = plantillaProducto.content.cloneNode(true);
    contenedorProductos.appendChild(fila);
}

async function listar_Categorias() {
    try{
        const respuesta_cat = await fetch(`${URL_API}/Categorias/listar_Categorias`); 

        if(!respuesta_cat.ok){
            label.textContent = "No se encontraron categorias"; 
            return; 
        }

        const categorias = await respuesta_cat.json(); 

        const ddl_categorias = document.getElementById("ddl-categorias"); 
        categorias.aforEach(cat => {
            ddl_categorias.appendChild(crearOption(cat.NOMBRE, cat.IDCATEGORIA)); 
        });
    }
    catch(error){
        label.textContent = "No se logro conectar a la base de datos"; 
    }
}

const formulario = document.getElementById("formulario"); 

formulario.addEventListener("submit", async (evento) => {
    evento.preventDefault(); 

    const datos = new FormData(formulario); 

    const detalle = [...contenedorProductos.querySelectorAll(".sub-contendor-productos")].map((fila) => {
        const producto = fila.querySelector('[name="ddl-productos"]');

        return {
            idproducto: Number(producto.value),
            nombre: producto.selectedOptions[0].textContent.trim(),
            cantidad: Number(fila.querySelector('[name="txt-cantidad"]').value),
            precio: Number(fila.querySelector('[name="txt-precio"]').value)
        };
    });

    const compra = {
        cliente: {
            nombre: datos.get("txt-nombre"),
            apellido: datos.get("txt-apellido"),
            correo: datos.get("txt-correo") || null,
            telefono: datos.get("txt-telefono"),
            zona: datos.get("txt-zona")
        },
        pedido: {
            idestado: 1,
            nota: datos.get("txt-nota") || null,
            seguimiento: null
        },
        detalle
    };

    try{
        const respuesta = await fetch(`${URL_API}/Compras/guardar`, {
            method: "POST",
            headers: {
                "Content-Type": "application/json"
            },
            body: JSON.stringify(compra)
        });

        const resultado = await respuesta.json();

        if (!respuesta.ok) {
            throw new Error(resultado.mensaje || "No se pudo guardar la compra.");
        }

        label.textContent = resultado.mensaje;
    }
    catch(error){
        label.textContent = resultado; 
    }
});


btnagregar.addEventListener("click", () => {
    agregarFilaProducto(); 
}); 

document.addEventListener("DOMContentLoaded", async () => {
    agregarFilaProducto(); 
    await listar_Categorias(); 
}); 


//clonar cosas
// const btnagregar = document.getElementById("btn_agregar"); 
// const contendor1 = document.getElementById("tarjeta-productos"); 
// const contenedor2 = document.getElementById("tarjeta-sub-productos"); 


// btnagregar.addEventListener("click", () => {
//     const nuevo = contenedor2.content.cloneNode(true); 
//     contendor1.appendChild(nuevo); 
// });