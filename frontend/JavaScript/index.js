const NUMERO_WHATSAPP = "5491123353700";

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



const CAPAS = 24;
const ALTO_CAPA = 1.2;          
const DURACION = 26000;         
const COLOR_CALIENTE = "#FFB35C";


const INICIO = 0.02;            
const IMPRESION = 0.34;         
const RETIRO = 0.44;            
const FIN_RETIRO = 0.48;        

const SUAVE = "cubic-bezier(0.45, 0, 0.55, 1)"; 

const impresora = document.querySelector(".impresora");
const piezaNombre = document.getElementById("pieza-nombre");
const piezaLogo = document.getElementById("pieza-logo");
const sombra = document.querySelector(".sombra-pieza");
const portico = document.getElementById("portico");
const carro = document.getElementById("carro");


function brillo(i) {
    return (30 + Math.round((i / (CAPAS - 1)) * 70) - (i % 2 ? 6 : 0)) / 100;
}

const capaNombre = piezaNombre.querySelector(".capa");
const capasNombre = [];

for (let i = 0; i < CAPAS; i++) {
    const capa = i === CAPAS - 1 ? capaNombre : capaNombre.cloneNode(true);
    const final = i === CAPAS - 1 ? "#A6F7FF" : `color-mix(in srgb, #00E5FF ${Math.round(brillo(i) * 100)}%, #01262B)`;
    capa.style.setProperty("--i", i);
    capa.setAttribute("aria-hidden", "true");
    capa.style.color = final;
    piezaNombre.insertBefore(capa, capaNombre);
    capasNombre.push({ capa, final });
}

const menosMovimiento = window.matchMedia("(prefers-reduced-motion: reduce)").matches;

if (!menosMovimiento) {
    const capasLogo = [];

    for (let i = 0; i < CAPAS; i++) {
        const capa = document.createElement("img");
        capa.src = "Iconos/logo.svg";
        capa.alt = "";
        capa.className = "capa capa-logo";
        capa.style.setProperty("--i", i);
        capasLogo.push({ capa, final: `sepia(0) saturate(1) hue-rotate(0deg) brightness(${i === CAPAS - 1 ? 1.2 : brillo(i)})` });
        piezaLogo.append(capa);
    }

    const opciones = { duration: DURACION, iterations: Infinity };
    const animaciones = [];
    const movimientoX = [];
    const movimientoZ = [];
    const tx = (x) => `translateX(${x}px)`;
    const tz = (z) => `translateZ(${z}px)`;

    function imprimir(capas, mitad, propiedad, caliente, radio) {
        const paso = IMPRESION / CAPAS;
        const momento = (i) => mitad + INICIO + i * paso;

        capas.forEach(({ capa, final }, i) => {
            const haciaDerecha = i % 2 === 0;
            const oculta = haciaDerecha ? "inset(0 100% 0 0)" : "inset(0 0 0 100%)";
            const visible = "inset(0 0 0 0)";
            const cuadro = (offset, clipPath, opacity, valor) => ({ offset, clipPath, opacity, [propiedad]: valor });

            animaciones.push(capa.animate([
                cuadro(0, oculta, 1, caliente),
                cuadro(momento(i), oculta, 1, caliente),
                cuadro(momento(i + 1), visible, 1, caliente),
                cuadro(momento(i + 3), visible, 1, final),
                cuadro(mitad + RETIRO, visible, 1, final),
                cuadro(mitad + FIN_RETIRO, visible, 0, final),
                cuadro(1, visible, 0, final)
            ], opciones));

            const desde = haciaDerecha ? -radio : radio;
            movimientoX.push({ offset: momento(i), transform: tx(desde), easing: SUAVE });
            movimientoX.push({ offset: momento(i + 1), transform: tx(-desde), easing: SUAVE });
            movimientoZ.push({ offset: momento(i), transform: tz(i * ALTO_CAPA + 2) });
            movimientoZ.push({ offset: momento(i + 1), transform: tz(i * ALTO_CAPA + 2) });
        });

        const zFinal = (CAPAS - 1) * ALTO_CAPA + 2;
        movimientoX.push({ offset: mitad + INICIO + IMPRESION + 0.03, transform: tx(radio + 34) });
        movimientoX.push({ offset: mitad + FIN_RETIRO, transform: tx(radio + 34) });
        movimientoZ.push({ offset: mitad + INICIO + IMPRESION + 0.02, transform: tz(zFinal) });
        movimientoZ.push({ offset: mitad + INICIO + IMPRESION + 0.04, transform: tz(zFinal + 16) });
        movimientoZ.push({ offset: mitad + FIN_RETIRO, transform: tz(zFinal + 16) });
        movimientoZ.push({ offset: mitad + 0.5, transform: tz(2) });
    }

    function retirar(pieza, mitad) {
        const quieta = "translate3d(0, 0, 0)";
        animaciones.push(pieza.animate([
            { offset: 0, transform: quieta },
            { offset: mitad + RETIRO, transform: quieta, easing: "ease-in" },
            { offset: mitad + FIN_RETIRO, transform: "translate3d(0, -40px, 50px)" },
            { offset: mitad + FIN_RETIRO + 0.001, transform: quieta },
            { offset: 1, transform: quieta }
        ], opciones));
    }

    const radioNombre = capaNombre.offsetWidth / 2;
    const radioLogo = capasLogo[0].capa.offsetWidth / 2;

    movimientoX.push({ offset: 0, transform: tx(-radioNombre) });
    movimientoZ.push({ offset: 0, transform: tz(2) });
    imprimir(capasNombre, 0, "color", COLOR_CALIENTE, radioNombre);
    imprimir(capasLogo, 0.5, "filter", "sepia(1) saturate(3) hue-rotate(-20deg) brightness(1.3)", radioLogo);
    movimientoX.push({ offset: 1, transform: tx(-radioNombre) });

    retirar(piezaNombre, 0);
    retirar(piezaLogo, 0.5);

    const cuadrosSombra = [{ offset: 0, opacity: 0 }];
    for (const mitad of [0, 0.5]) {
        cuadrosSombra.push(
            { offset: mitad + INICIO, opacity: 0 },
            { offset: mitad + INICIO + IMPRESION, opacity: 1 },
            { offset: mitad + RETIRO, opacity: 1 },
            { offset: mitad + FIN_RETIRO, opacity: 0 }
        );
    }
    cuadrosSombra.push({ offset: 1, opacity: 0 });
    animaciones.push(sombra.animate(cuadrosSombra, opciones));

    animaciones.push(carro.animate(movimientoX, opciones));
    animaciones.push(portico.animate(movimientoZ, opciones));

    new IntersectionObserver(([entrada]) => {
        animaciones.forEach((animacion) => entrada.isIntersecting ? animacion.play() : animacion.pause());
        impresora.classList.toggle("pausada", !entrada.isIntersecting);
    }).observe(impresora);
}
