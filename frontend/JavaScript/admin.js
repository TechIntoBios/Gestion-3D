const URL_API = "/api";

import { crearCelda, crearCeldaImg, crearButton, crearInput, crearOption } from "./funciones.js";
import { exigirSesion } from "./sesion.js";

const label = document.getElementById("lbl-mensaje"); 
label.innerHTML = ""; 
const btnagregar = document.getElementById("btn-agregar"); 
const contenedorProductos = document.getElementById("tarjeta-productos");
const plantillaProducto = document.getElementById("tarjeta-sub-productos");

let productos_set = new Set(); 
let array_categorias = []; 
let array_productos = [];

async function listar_Categorias() {
    try{
        const respuesta_cat = await fetch(`${URL_API}/Categorias/listar_Categorias`); 

        if(!respuesta_cat.ok){
            label.textContent = "No se encontraron categorias"; 
            return; 
        }

        array_categorias = await respuesta_cat.json(); 
    }
    catch(error){
        label.textContent = "No se logro conectar a la base de datos"; 
    }
}

async function listar_Productos() {
    try{
        const respuesta_prov = await fetch(`${URL_API}/Productos/listar_productos`); 

        if(!respuesta_prov.ok){
            label.textContent = "No se encontraron productos"; 
            return; 
        }

        array_productos = await respuesta_prov.json(); 
    }
    catch(error){
        label.textContent = "No se logro conectar a la base de datos"; 
    }
}

function agregarFilaProducto() {
    const fila = plantillaProducto.content.cloneNode(true);
    const ddl_categorias = fila.querySelector('[name="ddl-categorias"]');
    const ddl_productos = fila.querySelector('[name="ddl-productos"]');

    array_categorias.forEach(cat => {
        ddl_categorias.appendChild(crearOption(cat.nombre, cat.idcategoria)); 
    });

    ddl_categorias.addEventListener("change", () => {
        const idCategoria = Number(ddl_categorias.value); 

        const productosCategoria = array_productos.filter(prod => prod.id_categoria === idCategoria);

        ddl_productos.length = 1; 
        ddl_productos.selectedIndex = 0; 

        productosCategoria.forEach(prod => {
            ddl_productos.appendChild(crearOption(prod.nombre, prod.id_producto))
        }); 
    }); 

    

    contenedorProductos.appendChild(fila);
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

// Buscador de cliente existente: por ahora solo se habilita con el interruptor; falta la búsqueda en la API.
const chkClienteExistente = document.getElementById("chk-cliente-existente");
const txtBuscarTelefono = document.getElementById("txt-buscar-telefono");
const btnBuscarCliente = document.getElementById("btn-buscar-cliente");
const lblBusqueda = document.getElementById("lbl-busqueda");

chkClienteExistente.addEventListener("change", () => {
    const habilitado = chkClienteExistente.checked;

    txtBuscarTelefono.disabled = !habilitado;
    btnBuscarCliente.disabled = !habilitado;
    lblBusqueda.textContent = "";
    lblBusqueda.className = "mensaje-busqueda";

    if (habilitado) {
        txtBuscarTelefono.focus();
    }
    else {
        txtBuscarTelefono.value = "";
    }
});

document.addEventListener("DOMContentLoaded", async () => {
    if(!await exigirSesion()){
        return;
    }

    await listar_Categorias();
    await listar_Productos(); 
    agregarFilaProducto(); 
}); 


//clonar cosas
// const btnagregar = document.getElementById("btn_agregar"); 
// const contendor1 = document.getElementById("tarjeta-productos"); 
// const contenedor2 = document.getElementById("tarjeta-sub-productos"); 


// btnagregar.addEventListener("click", () => {
//     const nuevo = contenedor2.content.cloneNode(true); 
//     contendor1.appendChild(nuevo); 
// });