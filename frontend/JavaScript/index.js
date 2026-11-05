// ===== WhatsApp: número y mensajes (cambiar acá) =====

// Número con código de país, sin "+", espacios ni guiones. Ej.: 5491123456789
const NUMERO_WHATSAPP = "549XXXXXXXXXX";

const MENSAJES = {
    // Botón "Lo quiero" de cada categoría. {categoria} se reemplaza por el nombre de la tarjeta.
    categoria: "Hola ZUNOX 3D! Me interesa la categoría {categoria}. ¿Me pasan más info?",

    // Botón flotante verde.
    flotante: "Hola ZUNOX 3D! Quería hacer una consulta.",

    // Formulario de consulta. Cada {campo} se reemplaza por lo que completó la persona.
    formulario: "Hola ZUNOX 3D! Quería hacer una consulta.\n\n" +
        "Nombre: {nombre} {apellido}\n" +
        "Mail: {correo}\n" +
        "Zona: {zona}\n" +
        "Tipo de consulta: {tipo}\n\n" +
        "{descripcion}"
};

function linkWhatsapp(mensaje) {
    return `https://wa.me/${NUMERO_WHATSAPP}?text=${encodeURIComponent(mensaje)}`;
}

function completar(plantilla, datos) {
    return plantilla.replace(/\{(\w+)\}/g, (_, clave) => datos[clave] ?? "");
}

// Botones de las categorías
document.querySelectorAll(".btn-whatsapp[data-categoria]").forEach((boton) => {
    boton.href = linkWhatsapp(completar(MENSAJES.categoria, { categoria: boton.dataset.categoria }));
});

// Botón flotante: se muestra después de bajar un poco
const botonFlotante = document.getElementById("btn-whatsapp-flotante");
botonFlotante.href = linkWhatsapp(MENSAJES.flotante);

function mostrarFlotante() {
    botonFlotante.classList.toggle("visible", window.scrollY > 200);
}

window.addEventListener("scroll", mostrarFlotante, { passive: true });
mostrarFlotante();

// Formulario de consulta
const formConsulta = document.getElementById("form-consulta");

formConsulta.addEventListener("submit", (evento) => {
    evento.preventDefault();

    const datos = Object.fromEntries(new FormData(formConsulta));
    for (const clave in datos) {
        datos[clave] = datos[clave].trim();
    }
    datos.correo = datos.correo || "No indicado";

    window.open(linkWhatsapp(completar(MENSAJES.formulario, datos)), "_blank", "noopener");
});

// ===== Menú hamburguesa =====
const botonMenu = document.getElementById("btn-menu");
const panelMenu = document.getElementById("panel-menu");

function mostrarMenu(mostrar) {
    panelMenu.hidden = !mostrar;
    botonMenu.setAttribute("aria-expanded", String(mostrar));
    botonMenu.setAttribute("aria-label", mostrar ? "Cerrar menú" : "Abrir menú");
}

botonMenu.addEventListener("click", (evento) => {
    evento.stopPropagation();
    mostrarMenu(panelMenu.hidden);
});

// Se cierra al elegir una opción, al tocar fuera del panel o con Escape.
panelMenu.addEventListener("click", (evento) => {
    if (evento.target.closest("a")) {
        mostrarMenu(false);
    }
});

document.addEventListener("click", (evento) => {
    if (!panelMenu.hidden && !panelMenu.contains(evento.target)) {
        mostrarMenu(false);
    }
});

document.addEventListener("keydown", (evento) => {
    if (evento.key === "Escape" && !panelMenu.hidden) {
        mostrarMenu(false);
        botonMenu.focus();
    }
});

// ===== Animación de la impresora 3D =====
// Arma el nombre como un bloque de capas apiladas. En cada capa el cabezal cruza de un lado al otro,
// la capa aparece detrás de él y el pórtico sube un escalón.
const CAPAS = 16;
const ALTO_CAPA = 2.4;        // px por capa (eje Z)
const DURACION = 11000;       // ciclo completo en ms
const FIN_IMPRESION = 0.74;   // parte del ciclo dedicada a imprimir
const INICIO_FADE = 0.93;     // de acá al final se desvanece y vuelve a empezar
const RECORRIDO = 128;        // px que se mueve el cabezal a cada lado del centro

const pieza = document.getElementById("pieza");
const portico = document.getElementById("portico");
const carro = document.getElementById("carro");
const capaBase = pieza.querySelector(".capa");
const capas = [];

for (let i = 0; i < CAPAS; i++) {
    const capa = i === CAPAS - 1 ? capaBase : capaBase.cloneNode(true);
    capa.style.setProperty("--i", i);
    capa.setAttribute("aria-hidden", "true");
    // Las capas de abajo más oscuras (dan el lateral del bloque) y una de cada dos un poco más apagada,
    // así se notan las líneas del filamento.
    const brillo = 30 + Math.round((i / (CAPAS - 1)) * 70) - (i % 2 ? 6 : 0);
    capa.style.color = i === CAPAS - 1 ? "" : `color-mix(in srgb, #00E5FF ${brillo}%, #01262B)`;
    pieza.insertBefore(capa, capaBase);
    capas.push(capa);
}

const menosMovimiento = window.matchMedia("(prefers-reduced-motion: reduce)").matches;

if (!menosMovimiento) {
    const paso = FIN_IMPRESION / CAPAS;
    const animaciones = [];
    const movimientoX = [];
    const movimientoZ = [];

    capas.forEach((capa, i) => {
        const inicio = i * paso;
        const fin = (i + 1) * paso;
        const haciaDerecha = i % 2 === 0;
        const oculta = haciaDerecha ? "inset(0 100% 0 0)" : "inset(0 0 0 100%)";
        const visible = "inset(0 0 0 0)";

        animaciones.push(capa.animate([
            { clipPath: oculta, opacity: 1, offset: 0 },
            { clipPath: oculta, offset: inicio },
            { clipPath: visible, offset: fin },
            { clipPath: visible, opacity: 1, offset: INICIO_FADE },
            { clipPath: visible, opacity: 0, offset: 1 }
        ], { duration: DURACION, iterations: Infinity }));

        const desde = haciaDerecha ? -RECORRIDO : RECORRIDO;
        movimientoX.push({ transform: `translateX(${desde}px)`, offset: inicio });
        movimientoX.push({ transform: `translateX(${-desde}px)`, offset: fin });

        const z = `translateZ(${i * ALTO_CAPA + 2}px)`;
        movimientoZ.push({ transform: z, offset: inicio });
        movimientoZ.push({ transform: z, offset: fin });
    });

    // Al terminar, el cabezal se corre a un costado y espera. Después todo baja y arranca de nuevo.
    const zFinal = movimientoZ[movimientoZ.length - 1].transform;
    movimientoX.push({ transform: `translateX(${RECORRIDO + 22}px)`, offset: FIN_IMPRESION + 0.03 });
    movimientoX.push({ transform: `translateX(${RECORRIDO + 22}px)`, offset: INICIO_FADE });
    movimientoX.push({ transform: `translateX(${-RECORRIDO}px)`, offset: 1 });
    movimientoZ.push({ transform: zFinal, offset: INICIO_FADE });
    movimientoZ.push({ transform: "translateZ(2px)", offset: 1 });

    animaciones.push(carro.animate(movimientoX, { duration: DURACION, iterations: Infinity, easing: "linear" }));
    animaciones.push(portico.animate(movimientoZ, { duration: DURACION, iterations: Infinity }));

    // Se pausa cuando el hero no está en pantalla.
    new IntersectionObserver(([entrada]) => {
        animaciones.forEach((animacion) => entrada.isIntersecting ? animacion.play() : animacion.pause());
    }).observe(document.querySelector(".impresora"));
}
