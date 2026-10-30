// Descarga un archivo (por ejemplo un PDF) recibido como base64 desde C#
window.descargarArchivo = (nombre, base64, tipo) => {
    const enlace = document.createElement('a');
    enlace.href = `data:${tipo};base64,${base64}`;
    enlace.download = nombre;
    document.body.appendChild(enlace);
    enlace.click();
    document.body.removeChild(enlace);
};
