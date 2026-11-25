// El servidor decide qué ocurre. El cliente decide cómo dibujarlo.
export const escenario = {
  escenario: "explanada_mexicali",
  paredXM: 0, mesaXM: 4, ejePositivo: "oeste",
  horas: [8, 10, 12, 14, 16],
  rangos: { nubosidad: [0, 100], alturaParedM: [1, 8] },
  unidades: { horaLocal: "h", nubosidad: "%", alturaParedM: "m" }
};
const sol = {8: [30, "oeste"], 10: [45, "oeste"], 12: [60, "oeste"],
             14: [45, "este"], 16: [30, "este"]};

export function referencia(horaLocal) {
  const [elevacionSolGrados, direccionSombra] = sol[horaLocal];
  return { horaLocal, elevacionSolGrados, direccionSombra };
}

export function simular(datos) {
  if (datos?.escenario !== escenario.escenario) throw Error("Escenario no reconocido.");
  const { horaLocal, nubosidad, alturaParedM } = datos.variables ?? {};
  if (!escenario.horas.includes(horaLocal)) throw Error("horaLocal debe ser el número 8, 10, 12, 14 o 16.");
  if (!Number.isFinite(nubosidad) || nubosidad < 0 || nubosidad > 100)
    throw Error("nubosidad debe ser un número de 0 a 100.");
  if (!Number.isFinite(alturaParedM) || alturaParedM < 1 || alturaParedM > 8)
    throw Error("alturaParedM debe ser un número de 1 a 8.");
  const { elevacionSolGrados, direccionSombra } = referencia(horaLocal);
  // tan(45°) es exactamente 1 en el modelo. Evitamos ruido en el caso límite de 4 m.
  const pendiente = elevacionSolGrados === 45 ? 1 : Math.tan(elevacionSolGrados * Math.PI / 180);
  const longitudSombraM = alturaParedM / pendiente;
  const mesaEnSombra = direccionSombra === "oeste" && longitudSombraM >= escenario.mesaXM;
  return {
    estado: "ok",
    resultado: { elevacionSolGrados, direccionSombra, longitudSombraM, mesaEnSombra },
    visual: { contrasteSombra: (100 - nubosidad) / 100 }
  };
}
