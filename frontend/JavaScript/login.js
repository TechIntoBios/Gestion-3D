const URL_API = "https://localhost:7199/api"; 

import { crearCelda, crearCeldaImg, crearButton, crearInput, crearOption } from "./funciones.js"; 

const formulario = document.getElementById("formulario");

formulario.addEventListener("submit", async (evento) => {
    evento.defaultPrevented(); 
    
    const data = new FormData(formulario); 

    try{
        const respuesta_us = await fetch(`${URL_API}/Login/autorizacion`, {
            method: "POST", 
            headers: {"Content-Type": "application/json"}, 
            body: JSON.stringify({
                usuario: data.get("txt-usuario"), 
                contrasena: data.get("txt-contrasena")
            })
        }); 

        const respuesta = await respuesta_us.json(); 

        if(!respuesta_us.ok){
            throw new Error(respuesta.mensaje); 
        }

        
    }
    catch(error){

    }
}); 

document.addEventListener("DOMContentLoaded", async () => {

}); 