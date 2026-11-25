import { test, before, after } from "node:test";
import assert from "node:assert/strict";
import { once } from "node:events";
import { crearServidor } from "./local.mjs";

let server, base;
before(async () => {
  server = crearServidor().listen(0, "127.0.0.1");
  await once(server, "listening");
  base = "http://127.0.0.1:" + server.address().port;
});
after(() => new Promise(resolve => server.close(resolve)));
function enviar(variables) {
  return fetch(base + "/api/simular", { method: "POST", headers: {"Content-Type":"application/json"},
    body: JSON.stringify({ escenario:"explanada_mexicali", variables: {horaLocal:10,nubosidad:25,alturaParedM:5,...variables} }) });
}
test("GET health", async () => {const r=await fetch(base+"/api/health"); assert.equal(r.status,200); assert.equal((await r.json()).estado,"ok");});
test("GET escenario", async () => {const r=await fetch(base+"/api/escenario"); assert.equal(r.status,200); const x=await r.json();assert.deepEqual(x.horas,[8,10,12,14,16]);assert.equal(x.mesaXM,4);});
test("GET con parámetro textual válido", async()=>{const r=await fetch(base+"/api/escenario?horaLocal=14"); assert.equal(r.status,200);assert.deepEqual((await r.json()).referenciaSolar,{horaLocal:14,elevacionSolGrados:45,direccionSombra:"este"});});
test("GET con hora fuera del catálogo", async()=>{const r=await fetch(base+"/api/escenario?horaLocal=11");assert.equal(r.status,400);});
for (const [nombre, vars, ang, lado, longitud, mesa, contraste] of [
  ["base",{},45,"oeste",5,true,.75],
  ["8 h",{horaLocal:8},30,"oeste",8.660254,true,.75],
  ["12 h",{horaLocal:12},60,"oeste",2.886751,false,.75],
  ["14 h",{horaLocal:14},45,"este",5,false,.75],
  ["16 h",{horaLocal:16},30,"este",8.660254,false,.75],
  ["80% de nubes",{nubosidad:80},45,"oeste",5,true,.2],
  ["100% de nubes: geometría sin contraste",{nubosidad:100},45,"oeste",5,true,0],
  ["límite 4 m",{alturaParedM:4},45,"oeste",4,true,.75],
  ["sin redondear para decidir",{alturaParedM:3.9999},45,"oeste",3.9999,false,.75]
]) test(nombre,async()=>{const r=await enviar(vars);assert.equal(r.status,200);const j=await r.json();const v=j.resultado;
 assert.equal(v.elevacionSolGrados,ang);assert.equal(v.direccionSombra,lado);assert.ok(Math.abs(v.longitudSombraM-longitud)<.00001);
 assert.equal(v.mesaEnSombra,mesa);assert.equal(j.visual.contrasteSombra,contraste);});
for(const [nombre,vars] of [["porcentaje 120",{nubosidad:120}],["altura 0",{alturaParedM:0}],["hora texto",{horaLocal:"10"}],["hora no admitida",{horaLocal:9}]])
 test(nombre, async()=>{const r=await enviar(vars);assert.equal(r.status,400);assert.ok((await r.json()).error);});
test("JSON mal formado",async()=>{const r=await fetch(base+"/api/simular",{method:"POST",headers:{"Content-Type":"application/json"},body:"{"}); assert.equal(r.status,400);});
test("tipo de contenido no admitido",async()=>{const r=await fetch(base+"/api/simular",{method:"POST",body:"hola"});assert.equal(r.status,415);});
