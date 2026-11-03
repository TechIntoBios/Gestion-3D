import { exigirSesion } from "./sesion.js";

document.addEventListener("DOMContentLoaded", async () => {
    if (!await exigirSesion()) {
        return;
    }
});
