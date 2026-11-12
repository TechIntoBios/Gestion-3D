const NUMERO_WHATSAPP = "5491123353700";

const MENSAJES = {
    categoria: "Hola ZUNOX 3D! Me interesa la categoría {categoria}. ¿Me pasan más info?",

    flotante: "Hola ZUNOX 3D! Quería hacer una consulta.",

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

document.querySelectorAll(".btn-whatsapp[data-categoria]").forEach((boton) => {
    boton.href = linkWhatsapp(completar(MENSAJES.categoria, { categoria: boton.dataset.categoria }));
});

const panel = document.getElementById("panel-categoria");
const panelCuerpo = document.getElementById("panel-cuerpo");
let mosaicoAbierto = null;

function abrirPanel(boton) {
    const mosaico = boton.closest(".mosaico");
    const imagen = mosaico.querySelector(".tarjeta-imagen").cloneNode(true);
    const detalle = [...mosaico.querySelector(".mosaico-detalle").children].map((nodo) => nodo.cloneNode(true));

    panelCuerpo.replaceChildren(imagen, ...detalle);
    panelCuerpo.querySelector("h3").id = "panel-titulo";
    mosaicoAbierto = boton;

    panel.showModal();
    history.pushState({ panel: true }, "");
}

document.querySelectorAll(".mosaico-boton[aria-haspopup]").forEach((boton) => {
    boton.addEventListener("click", () => abrirPanel(boton));
});

panel.querySelector("[data-cerrar-panel]").addEventListener("click", () => panel.close());

panel.addEventListener("click", (evento) => {
    if (evento.target === panel) {
        panel.close();
    }
});

panel.addEventListener("close", () => {
    if (history.state?.panel) {
        history.back();
    }
    mosaicoAbierto?.focus();
    mosaicoAbierto = null;
});

window.addEventListener("popstate", () => {
    if (panel.open) {
        panel.close();
    }
});

const botonFlotante = document.getElementById("btn-whatsapp-flotante");
botonFlotante.href = linkWhatsapp(MENSAJES.flotante);

function mostrarFlotante() {
    botonFlotante.classList.toggle("visible", window.scrollY > 200);
}

window.addEventListener("scroll", mostrarFlotante, { passive: true });
mostrarFlotante();

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

// ===== Impresora 3D del hero =====
// Imprime el nombre y después el logo, capa por capa. El visitante puede girarla
// arrastrando y elegir el color del filamento: las capas que faltan salen en ese color.

const CAPAS = 24;
const ALTO_CAPA = 1.2;
const DURACION = 26000;
const COLOR_CALIENTE = "#FFB35C";
const FILTRO_CALIENTE = "sepia(1) saturate(3) hue-rotate(-20deg) brightness(1.3)";
const TONO_LOGO = 195; // tono del cian del logo.svg, para teñirlo de otro color

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

const menosMovimiento = window.matchMedia("(prefers-reduced-motion: reduce)").matches;
const leerToken = (token) => getComputedStyle(document.documentElement).getPropertyValue(token).trim();

let colorFilamento = leerToken("--filamento-cian");

function brillo(i) {
    return (30 + Math.round((i / (CAPAS - 1)) * 70) - (i % 2 ? 6 : 0)) / 100;
}

// Color final de cada capa del nombre: más oscuras abajo, la de arriba más clara (da volumen)
function colorCapaNombre(i, color) {
    if (i === CAPAS - 1) {
        return `color-mix(in srgb, ${color} 35%, white)`;
    }
    return `color-mix(in srgb, ${color} ${Math.round(brillo(i) * 100)}%, color-mix(in srgb, ${color} 15%, black))`;
}

// Tono (0 a 360) y saturación (0 a 1) de un color #RRGGBB
function tonoYSaturacion(hex) {
    const [r, g, b] = [1, 3, 5].map((n) => parseInt(hex.slice(n, n + 2), 16) / 255);
    const max = Math.max(r, g, b);
    const min = Math.min(r, g, b);
    const d = max - min;
    if (d === 0) {
        return { tono: 0, saturacion: 0 };
    }
    const tono = max === r ? ((g - b) / d) % 6 : max === g ? (b - r) / d + 2 : (r - g) / d + 4;
    return { tono: (tono * 60 + 360) % 360, saturacion: d / (1 - Math.abs(max + min - 1)) };
}

// Filtro final de cada capa del logo: mismo orden de funciones que FILTRO_CALIENTE para que transicione suave
function filtroCapaLogo(i, color) {
    const luz = i === CAPAS - 1 ? 1.2 : brillo(i);
    const { tono, saturacion } = tonoYSaturacion(color);
    if (saturacion < 0.25) {
        return `sepia(0) saturate(0) hue-rotate(0deg) brightness(${(luz * 1.5).toFixed(2)})`;
    }
    const giro = color.toLowerCase() === leerToken("--filamento-cian").toLowerCase() ? 0 : Math.round(tono - TONO_LOGO);
    return `sepia(0) saturate(1.2) hue-rotate(${giro}deg) brightness(${luz})`;
}

// Cada capa guarda su color, el momento en que se imprime y su animación
const capas = [];

const capaNombre = piezaNombre.querySelector(".capa");
for (let i = 0; i < CAPAS; i++) {
    const capa = i === CAPAS - 1 ? capaNombre : capaNombre.cloneNode(true);
    capa.style.setProperty("--i", i);
    capa.setAttribute("aria-hidden", "true");
    capa.style.color = colorCapaNombre(i, colorFilamento);
    piezaNombre.insertBefore(capa, capaNombre);
    capas.push({ capa, i, tipo: "nombre", mitad: 0, color: colorFilamento, animacion: null });
}

function pintarCapa(c, color) {
    c.color = color;
    if (c.animacion) {
        c.animacion.effect.setKeyframes(cuadrosCapa(c));
    } else if (c.tipo === "nombre") {
        c.capa.style.color = colorCapaNombre(c.i, color);
    }
}

const momentoCapa = (mitad, i) => mitad + INICIO + i * (IMPRESION / CAPAS);

function cuadrosCapa(c) {
    const { i, mitad } = c;
    const haciaDerecha = i % 2 === 0;
    const oculta = haciaDerecha ? "inset(0 100% 0 0)" : "inset(0 0 0 100%)";
    const visible = "inset(0 0 0 0)";
    const propiedad = c.tipo === "nombre" ? "color" : "filter";
    const caliente = c.tipo === "nombre" ? COLOR_CALIENTE : FILTRO_CALIENTE;
    const final = c.tipo === "nombre" ? colorCapaNombre(i, c.color) : filtroCapaLogo(i, c.color);
    const cuadro = (offset, clipPath, opacity, valor) => ({ offset, clipPath, opacity, [propiedad]: valor });

    return [
        cuadro(0, oculta, 1, caliente),
        cuadro(momentoCapa(mitad, i), oculta, 1, caliente),
        cuadro(momentoCapa(mitad, i + 1), visible, 1, caliente),
        cuadro(momentoCapa(mitad, i + 3), visible, 1, final),
        cuadro(mitad + RETIRO, visible, 1, final),
        cuadro(mitad + FIN_RETIRO, visible, 0, final),
        cuadro(1, visible, 0, final)
    ];
}

const animaciones = [];

if (!menosMovimiento) {
    for (let i = 0; i < CAPAS; i++) {
        const capa = document.createElement("img");
        capa.src = "Iconos/logo.svg";
        capa.alt = "";
        capa.className = "capa capa-logo";
        capa.style.setProperty("--i", i);
        piezaLogo.append(capa);
        capas.push({ capa, i, tipo: "logo", mitad: 0.5, color: colorFilamento, animacion: null });
    }

    const opciones = { duration: DURACION, iterations: Infinity };
    const movimientoX = [];
    const movimientoZ = [];
    const tx = (x) => `translateX(${x}px)`;
    const tz = (z) => `translateZ(${z}px)`;

    function imprimir(tipo, mitad, radio) {
        capas.filter((c) => c.tipo === tipo).forEach((c) => {
            const { i } = c;
            c.animacion = c.capa.animate(cuadrosCapa(c), opciones);
            animaciones.push(c.animacion);

            const desde = i % 2 === 0 ? -radio : radio;
            movimientoX.push({ offset: momentoCapa(mitad, i), transform: tx(desde), easing: SUAVE });
            movimientoX.push({ offset: momentoCapa(mitad, i + 1), transform: tx(-desde), easing: SUAVE });
            movimientoZ.push({ offset: momentoCapa(mitad, i), transform: tz(i * ALTO_CAPA + 2) });
            movimientoZ.push({ offset: momentoCapa(mitad, i + 1), transform: tz(i * ALTO_CAPA + 2) });
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
    const radioLogo = piezaLogo.querySelector(".capa").offsetWidth / 2;

    movimientoX.push({ offset: 0, transform: tx(-radioNombre) });
    movimientoZ.push({ offset: 0, transform: tz(2) });
    imprimir("nombre", 0, radioNombre);
    imprimir("logo", 0.5, radioLogo);
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

// ===== Color del filamento =====
// Una capa cambia de color solo cuando no se ve (todavía no se imprimió o ya se retiró la pieza),
// así lo que ya está impreso queda como está y la pieza sale multicolor, como en una impresión real.

function progresoCiclo() {
    const tiempo = animaciones[0]?.currentTime ?? 0;
    return (tiempo % DURACION) / DURACION;
}

function actualizarCapas() {
    const p = progresoCiclo();
    capas.forEach((c) => {
        const pendiente = p < momentoCapa(c.mitad, c.i);
        const retirada = p > c.mitad + FIN_RETIRO;
        if (c.color !== colorFilamento && (!c.animacion || pendiente || retirada)) {
            pintarCapa(c, colorFilamento);
        }
    });
}

if (animaciones.length) {
    setInterval(() => {
        if (!impresora.classList.contains("pausada")) {
            actualizarCapas();
        }
    }, 250);
}

const botonesFilamento = document.querySelectorAll(".color-filamento");

botonesFilamento.forEach((boton) => {
    boton.addEventListener("click", () => {
        colorFilamento = leerToken(boton.dataset.color);
        impresora.style.setProperty("--color-filamento", `var(${boton.dataset.color})`);
        botonesFilamento.forEach((b) => b.setAttribute("aria-pressed", String(b === boton)));
        actualizarCapas();
    });
});

// ===== Girar la impresora =====
// Arrastrando de costado gira la cámara; de arriba abajo la inclina un poco. Al soltar vuelve sola.
// touch-action: pan-y deja que el dedo siga desplazando la página hacia abajo.

const GIRO_MAXIMO = 50;
const INCLINACION_MINIMA = -18;
const INCLINACION_MAXIMA = 10;
const limitar = (valor, minimo, maximo) => Math.min(maximo, Math.max(minimo, valor));

let giro = 0;
let inclinacion = 0;
let arrastre = null;

function aplicarGiro() {
    impresora.style.setProperty("--girar", `${giro}deg`);
    impresora.style.setProperty("--inclinar", `${inclinacion}deg`);
}

function volverAlCentro() {
    giro = 0;
    inclinacion = 0;
    aplicarGiro();
}

impresora.addEventListener("pointerdown", (evento) => {
    if (evento.target.closest(".colores-filamento") || evento.button > 0) {
        return;
    }
    arrastre = { id: evento.pointerId, x: evento.clientX, y: evento.clientY, giro, inclinacion };
    impresora.setPointerCapture(evento.pointerId);
    impresora.classList.add("arrastrando");
});

impresora.addEventListener("pointermove", (evento) => {
    if (!arrastre || evento.pointerId !== arrastre.id) {
        return;
    }
    giro = limitar(arrastre.giro + (evento.clientX - arrastre.x) * 0.35, -GIRO_MAXIMO, GIRO_MAXIMO);
    inclinacion = limitar(arrastre.inclinacion - (evento.clientY - arrastre.y) * 0.15, INCLINACION_MINIMA, INCLINACION_MAXIMA);
    aplicarGiro();
    if (Math.abs(evento.clientX - arrastre.x) > 12) {
        impresora.classList.add("girada");
    }
});

function soltar(evento) {
    if (!arrastre || evento.pointerId !== arrastre.id) {
        return;
    }
    arrastre = null;
    impresora.classList.remove("arrastrando");
    volverAlCentro();
}

impresora.addEventListener("pointerup", soltar);
impresora.addEventListener("pointercancel", soltar);

// Con teclado: flechas para girar e inclinar; al salir de la impresora vuelve al centro
impresora.addEventListener("keydown", (evento) => {
    const pasos = { ArrowLeft: [-10, 0], ArrowRight: [10, 0], ArrowUp: [0, 4], ArrowDown: [0, -4] };
    const paso = pasos[evento.key];
    if (!paso || evento.target !== impresora) {
        return;
    }
    evento.preventDefault();
    giro = limitar(giro + paso[0], -GIRO_MAXIMO, GIRO_MAXIMO);
    inclinacion = limitar(inclinacion + paso[1], INCLINACION_MINIMA, INCLINACION_MAXIMA);
    aplicarGiro();
    impresora.classList.add("girada");
});

impresora.addEventListener("blur", volverAlCentro);
