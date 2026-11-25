import { simular } from "../modelo.mjs";
export async function POST(request) {
  if (request.headers.get("content-type")?.split(";")[0].trim().toLowerCase() !== "application/json")
    return Response.json({ error: "Usa Content-Type: application/json." }, { status: 415 });
  let datos;
  try { datos = await request.json(); }
  catch { return Response.json({ error: "El cuerpo no es JSON válido." }, { status: 400 }); }
  try { return Response.json(simular(datos)); }
  catch (e) { return Response.json({ error: e.message }, { status: 400 }); }
}
