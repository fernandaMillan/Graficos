import { escenario, referencia } from "../modelo.mjs";
export function GET(request) {
  const texto = new URL(request.url).searchParams.get("horaLocal");
  if (texto === null) return Response.json(escenario);
  if (!escenario.horas.map(String).includes(texto))
    return Response.json({ error: "Hora de consulta no válida." }, {status: 400});
  // "14" es texto en la URL. Aquí lo convertimos a número.
  return Response.json({ ...escenario, referenciaSolar: referencia(Number(texto)) });
}
