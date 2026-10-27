const URL_API = "https://localhost:7199/api/Categorias"; 

import { crearCelda, crearCeldaImg, crearButton, crearInput, crearOption } from "./funciones.js"; 

const label = document.getElementById("lbl-mensaje"); 
label.innerHTML = ""; 

async function listar_Categorias() {
    try{
        const respuesta_cat = await fetch(`${URL_API}/${"listar_Categorias"}`); 

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

    const txt_nombre = datos.get("txt-nombre"); 
    const txt_apellido = datos.get("txt-apellido"); 
    const txt_correo = datos.get("txt-correo"); 
    const txt_telefono = datos.get("txt-telefono"); 
    const txt_zona = datos.get("txt-zona");
    const txt_nota = datos.get("txt-nota"); 
    const ddl_categorias = datos.get("ddl-categorias").valueOf();
    const ddl_productos = datos.get("ddl-productos").valueOf(); 
    const txt_cantidad = datos.get("txt-cantidad"); 
    const txt_precio = datos.get("txt-precio"); 
    
    // label.textContent = `${txt_nombre}-${txt_apellido}-${txt_correo}-${txt_telefono}
    // -${txt_zona}-${txt_nota}-${ddl_categorias}-${ddl_productos}-${txt_cantidad}-${txt_precio}`;
});

document.addEventListener("DOMContentLoaded", async () => {
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