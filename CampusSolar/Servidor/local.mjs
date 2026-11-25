// Adaptador del laboratorio. Vercel usa los mismos tres archivos de api/ directamente.
import { createServer } from "node:http";
import { GET as health } from "./api/health.mjs";
import { GET as escenario } from "./api/escenario.mjs";
import { POST as simular } from "./api/simular.mjs";
import { pathToFileURL } from "node:url";

const rutas = new Map([
  ["/api/health", ["GET", health]], ["/api/escenario", ["GET", escenario]],
  ["/api/simular", ["POST", simular]]
]);
export function crearServidor() {
  return createServer(async (entrada, salida) => {
    const url = new URL(entrada.url, "http://localhost");
    const ruta = rutas.get(url.pathname);
    let r;
    try {
      if (!ruta) r = Response.json({ error: "Ruta inexistente." }, { status: 404 });
      else if (ruta[0] !== entrada.method)
        r = Response.json({ error: "Método incorrecto." }, { status: 405, headers: { Allow: ruta[0] } });
      else {
        const opciones = { method: entrada.method, headers: entrada.headers };
        if (entrada.method === "POST") {
          const partes = []; let n = 0;
          for await (const parte of entrada) {
            n += parte.length;
            if (n > 16384) { salida.writeHead(413); salida.end("Cuerpo demasiado grande."); return; }
            partes.push(parte);
          }
          opciones.body = Buffer.concat(partes);
        }
        r = await ruta[1](new Request(url, opciones));
      }
    } catch { r = Response.json({ error: "No se pudo procesar la petición." }, { status: 500 }); }
    salida.writeHead(r.status, Object.fromEntries(r.headers));
    salida.end(await r.text());
  });
}
if (process.argv[1] && import.meta.url === pathToFileURL(process.argv[1]).href) {
  const port = Number(process.env.PORT || 3000);
  crearServidor().listen(port, "127.0.0.1", () => console.log("API local: http://127.0.0.1:" + port + "/api/health"));
}
