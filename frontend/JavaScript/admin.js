const URL_API = "/api";

import { crearCelda, crearCeldaImg, crearButton, crearInput, crearOption } from "./funciones.js";
import { exigirSesion } from "./sesion.js";

const label = document.getElementById("lbl-mensaje"); 
label.innerHTML = ""; 
const btnagregar = document.getElementById("btn-agregar"); 
const contenedorProductos = document.getElementById("tarjeta-productos");
const plantillaProducto = document.getElementById("tarjeta-sub-productos");
const lblTotal = document.getElementById("lbl-total");

const formatoPesos = new Intl.NumberFormat("es-AR", { style: "currency", currency: "ARS" });

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

function calcularSubtotal(tarjeta) {
    const cantidad = Number(tarjeta.querySelector('[name="txt-cantidad"]').value) || 0;
    const precio = Number(tarjeta.querySelector('[name="txt-precio"]').value) || 0;

    return Math.round(cantidad * precio * 100) / 100;
}

function actualizarTotal() {
    const tarjetas = contenedorProductos.querySelectorAll(".sub-contendor-productos");
    const total = [...tarjetas].reduce((suma, tarjeta) => suma + calcularSubtotal(tarjeta), 0);

    lblTotal.textContent = formatoPesos.format(total);
}

function agregarFilaProducto() {
    const fila = plantillaProducto.content.cloneNode(true);
    const ddl_categorias = fila.querySelector('[name="ddl-categorias"]');
    const ddl_productos = fila.querySelector('[name="ddl-productos"]');
    const txt_precio = fila.querySelector('[name="txt-precio"]');
    const txt_cantidad = fila.querySelector('[name="txt-cantidad"]');
    const tarjeta = fila.querySelector(".sub-contendor-productos");
    const btn_eliminar = fila.querySelector(".btn-eliminar");
    const lbl_subtotal = fila.querySelector(".lbl-subtotal");

    function actualizarSubtotal() {
        lbl_subtotal.textContent = formatoPesos.format(calcularSubtotal(tarjeta));
        actualizarTotal();
    }

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

        txt_precio.value = "";
        actualizarSubtotal();
    });

    ddl_productos.addEventListener("change", () => {
        const idProducto = Number(ddl_productos.value); 
        const productos = array_productos.find(prod => prod.id_producto === idProducto); 

        txt_precio.value = productos ? productos.precio : "";

        if(!txt_cantidad.value){
            txt_cantidad.value = 1;
        }

        actualizarSubtotal();
    });

    txt_cantidad.addEventListener("input", actualizarSubtotal);
    txt_precio.addEventListener("input", actualizarSubtotal);

    btn_eliminar.addEventListener("click", () => {
        const cantidadTarjetas = contenedorProductos.querySelectorAll(".sub-contendor-productos").length;

        if (cantidadTarjetas > 1) {
            tarjeta.remove();
            actualizarTotal();
            return;
        }

        ddl_categorias.selectedIndex = 0;
        ddl_productos.length = 1;
        ddl_productos.selectedIndex = 0;
        txt_precio.value = "";
        txt_cantidad.value = "";
        actualizarSubtotal();
    });

    contenedorProductos.appendChild(fila);
    actualizarSubtotal();
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
        label.textContent = error.message;
    }
});


btnagregar.addEventListener("click", () => {
    agregarFilaProducto(); 
}); 

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
