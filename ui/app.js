"use strict";

// Painel do DS339 Monitor. A página só conversa com o programa por mensagens (WebView2): manda comandos e recebe o
// estado (configuração) e o "ao vivo" (miniaturas das telas, cor do LED, sensores).

const bridge = window.chrome && window.chrome.webview;
const send = (cmd, args = {}) => bridge && bridge.postMessage({ cmd, ...args });

const ICON = {
  overview: '<svg viewBox="0 0 24 24"><rect x="3" y="3" width="7.5" height="9" rx="1.6"/><rect x="13.5" y="3" width="7.5" height="5.5" rx="1.6"/><rect x="13.5" y="11.5" width="7.5" height="9.5" rx="1.6"/><rect x="3" y="15" width="7.5" height="6" rx="1.6"/></svg>',
  thermalright: '<svg viewBox="0 0 24 24"><rect x="2" y="8" width="20" height="8" rx="1.8"/><path d="M6 12h4"/></svg>',
  proflow: '<svg viewBox="0 0 24 24"><rect x="4" y="6" width="16" height="10" rx="1.8"/><path d="M9 20h6M12 16v4"/></svg>',
  ds339: '<svg viewBox="0 0 24 24"><rect x="3" y="7" width="18" height="10" rx="1.8"/></svg>',
  fans: '<svg viewBox="0 0 24 24"><path d="M12 12c0-3 1.2-7 4-7 2 0 3 1.6 3 3 0 2.6-3.6 4-7 4z"/><path d="M12 12c3 0 7 1.2 7 4 0 2-1.6 3-3 3-2.6 0-4-3.6-4-7z"/><path d="M12 12c0 3-1.2 7-4 7-2 0-3-1.6-3-3 0-2.6 3.6-4 7-4z"/><path d="M12 12c-3 0-7-1.2-7-4 0-2 1.6-3 3-3 2.6 0 4 3.6 4 7z"/></svg>',
  game: '<svg viewBox="0 0 24 24"><path d="M6.5 8h11a4 4 0 0 1 3.9 4.8l-1 4.6a2.2 2.2 0 0 1-3.9.8L14.6 16H9.4l-1.9 2.2a2.2 2.2 0 0 1-3.9-.8l-1-4.6A4 4 0 0 1 6.5 8z"/><path d="M7.5 11v3M6 12.5h3"/><circle cx="16" cy="11.6" r=".9"/><circle cx="17.6" cy="13.6" r=".9"/></svg>',
  white: '<svg viewBox="0 0 24 24"><circle cx="12" cy="12" r="4"/><path d="M12 2v2.5M12 19.5V22M2 12h2.5M19.5 12H22M4.9 4.9l1.8 1.8M17.3 17.3l1.8 1.8M4.9 19.1l1.8-1.8M17.3 6.7l1.8-1.8"/></svg>',
  umbra: '<svg viewBox="0 0 24 24"><rect x="3" y="3" width="18" height="18" rx="3"/><circle cx="12" cy="12" r="4.5"/><path d="M6 7v10M18 7v10"/></svg>',
  argb: '<svg viewBox="0 0 24 24"><circle cx="12" cy="12" r="9"/><circle cx="12" cy="12" r="1.8"/><path d="M12 10.2c-.6-3 .4-5 3-4.8M13.6 12.9c2.9 1 4 2.9 2.4 4.9M10.4 12.9c-2.3 2-4.5 2.1-5.2-.4"/></svg>',
  led: '<svg viewBox="0 0 24 24"><path d="M9 18h6M10 21h4"/><path d="M12 3a6 6 0 0 0-3.6 10.8c.6.5 1 1.2 1 2V16h5.2v-.2c0-.8.4-1.5 1-2A6 6 0 0 0 12 3z"/></svg>',
  modes: '<svg viewBox="0 0 24 24"><path d="M12 3l1.8 4.6L18.5 9l-4.7 1.4L12 15l-1.8-4.6L5.5 9l4.7-1.4z"/><path d="M18.5 15l.9 2.1 2.1.9-2.1.9-.9 2.1-.9-2.1-2.1-.9 2.1-.9z"/></svg>',
  system: '<svg viewBox="0 0 24 24"><circle cx="12" cy="12" r="3"/><path d="M19.4 15a1.7 1.7 0 0 0 .3 1.8l.1.1a2 2 0 1 1-2.8 2.8l-.1-.1a1.7 1.7 0 0 0-1.8-.3 1.7 1.7 0 0 0-1 1.5V21a2 2 0 1 1-4 0v-.1a1.7 1.7 0 0 0-1.1-1.5 1.7 1.7 0 0 0-1.8.3l-.1.1a2 2 0 1 1-2.8-2.8l.1-.1a1.7 1.7 0 0 0 .3-1.8 1.7 1.7 0 0 0-1.5-1H3a2 2 0 1 1 0-4h.1a1.7 1.7 0 0 0 1.5-1.1 1.7 1.7 0 0 0-.3-1.8l-.1-.1a2 2 0 1 1 2.8-2.8l.1.1a1.7 1.7 0 0 0 1.8.3H9a1.7 1.7 0 0 0 1-1.5V3a2 2 0 1 1 4 0v.1a1.7 1.7 0 0 0 1 1.5 1.7 1.7 0 0 0 1.8-.3l.1-.1a2 2 0 1 1 2.8 2.8l-.1.1a1.7 1.7 0 0 0-.3 1.8V9a1.7 1.7 0 0 0 1.5 1H21a2 2 0 1 1 0 4h-.1a1.7 1.7 0 0 0-1.5 1z"/></svg>',
  power: '<svg viewBox="0 0 24 24"><path d="M12 3v8"/><path d="M6.3 6.8a8 8 0 1 0 11.4 0"/></svg>',
  video: '<svg viewBox="0 0 24 24"><rect x="3" y="5" width="18" height="14" rx="2"/><path d="M10 9.5v5l4.5-2.5z"/></svg>',
  plus: '<svg viewBox="0 0 24 24"><path d="M12 5v14M5 12h14"/></svg>',
  download: '<svg viewBox="0 0 24 24"><path d="M12 4v11M7 10l5 5 5-5M5 20h14"/></svg>',
  image: '<svg viewBox="0 0 24 24"><rect x="3" y="4" width="18" height="16" rx="2"/><circle cx="9" cy="10" r="2"/><path d="M21 16l-5-5-9 9"/></svg>',
  file: '<svg viewBox="0 0 24 24"><path d="M14 3H7a2 2 0 0 0-2 2v14a2 2 0 0 0 2 2h10a2 2 0 0 0 2-2V8z"/><path d="M14 3v5h5"/></svg>',
  folder: '<svg viewBox="0 0 24 24"><path d="M3 7a2 2 0 0 1 2-2h4l2 2h8a2 2 0 0 1 2 2v8a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2z"/></svg>',
  shield: '<svg viewBox="0 0 24 24"><path d="M12 3l8 3v6c0 4.5-3.4 8-8 9-4.6-1-8-4.5-8-9V6z"/></svg>',
  exit: '<svg viewBox="0 0 24 24"><path d="M15 4h3a2 2 0 0 1 2 2v12a2 2 0 0 1-2 2h-3M10 17l5-5-5-5M15 12H4"/></svg>',
};

const PAGES = [
  { id: "overview", label: "Visão geral" },
  { id: "thermalright", label: "Thermalright", screen: 0, group: "Telas" },
  { id: "proflow", label: "Pro Flow 360", screen: 1, group: "Telas" },
  { id: "ds339", label: "Jonsbo DS339", screen: 2, onlyConnected: true, group: "Telas" },
  { id: "led", label: "LED da placa", group: "Iluminação" },
  { id: "argb", label: "Fans e fitas", group: "Iluminação" },
  { id: "umbra", label: "Modo Umbra", group: "Iluminação" },
  { id: "white", label: "Balanço de branco", group: "Iluminação" },
  { id: "fans", label: "Ventoinhas", group: "Refrigeração" },
  { id: "game", label: "Modo jogo", group: "Extras" },
  { id: "modes", label: "Modos", group: "Extras" },
  { id: "system", label: "Sistema", group: "Extras" },
];

let state = null;
let stateJson = "";
let page = load("page") || "overview";
let interacting = false;
let pendingRender = false;
let previews = null;
let argbPreviews = null;
let previewKey = "";
const images = { thermalright: null, proflow: null, ds339: null };
const live = { readings: {}, led: null, ledStatus: "", status: "", connected: [false, false, false], argb: [], argbStatus: "", fans: null };

const $page = document.getElementById("page");
const $nav = document.getElementById("nav");
const $power = document.getElementById("all-off");
const $status = document.getElementById("status-line");

// ---------------------------------------------------------------- utilidades

function esc(text) {
  return String(text ?? "").replace(/[&<>"']/g, c => ({ "&": "&amp;", "<": "&lt;", ">": "&gt;", '"': "&quot;", "'": "&#39;" })[c]);
}
function attrs(data) {
  return Object.entries(data).map(([k, v]) => `data-${k}="${esc(v)}"`).join(" ");
}
function load(key) {
  try { return sessionStorage.getItem(key); } catch { return null; }
}
function save(key, value) {
  try { sessionStorage.setItem(key, value); } catch { /* sem armazenamento: tudo bem */ }
}
function fmt(value, unit) {
  if (value == null) return "–";
  return `${Math.round(value)}<em>${unit}</em>`;
}

// ---------------------------------------------------------------- peças

function toggle(cmd, data, checked, label) {
  return `<label class="switch" title="${esc(label || "")}"><input type="checkbox" ${attrs({ cmd, ...data })} ${checked ? "checked" : ""}><span></span></label>`;
}

function range(cmd, data, value, min, max, unit) {
  const fill = ((value - min) / (max - min)) * 100;
  return `<div class="range"><input type="range" min="${min}" max="${max}" value="${value}" style="--fill:${fill}%" ${attrs({ cmd, unit, ...data })}><output>${value}${unit}</output></div>`;
}

function segmented(cmd, data, options, current, key = "name", disabled = false) {
  return `<div class="segmented ${disabled ? "disabled" : ""}">${options
    .map(o => `<button type="button" class="${o === current ? "active" : ""}" ${attrs({ cmd, ...data, [key]: o })}>${esc(o)}</button>`)
    .join("")}</div>`;
}

function row(title, subtitle, control) {
  return `<div class="row"><div class="label"><b>${title}</b>${subtitle ? `<small>${subtitle}</small>` : ""}</div><div class="control">${control}</div></div>`;
}

function screenFrame(id, cls, big = false) {
  return `<div class="screen ${cls} ${big ? "big" : ""}" data-screen="${id}">
    <img data-preview="${id}" alt="">
    <div class="screen-off" data-offline="${id}">desconectada</div>
  </div>`;
}

function connChip(connected, allOff) {
  if (allOff) return `<span class="chip"><span class="dot dark"></span>apagada</span>`;
  return `<span class="chip"><span class="dot ${connected ? "ok" : ""}"></span>${connected ? "conectada" : "desconectada"}</span>`;
}

function effectMeta(name) {
  for (const group of state.led.groups) for (const e of group.effects) if (e.name === name) return e;
  return { name, desc: "", color: false, speed: false, white: false };
}

// ---------------------------------------------------------------- páginas

function overview(s) {
  const [tr, pf] = s.screens;
  const effect = effectMeta(s.led.effect);
  const tile = (title, temp, use, power, extraLabel) => `
    <div class="tile">
      <small>${title}</small>
      <div class="value" data-read="${temp}" data-unit="°C">–</div>
      <div class="sub"><span data-read-plain="${use}" data-unit="%">–</span> de uso${power ? ` · <span data-read-plain="${power}" data-unit=" W">–</span>` : ""}${extraLabel || ""}</div>
      <div class="meter"><i data-meter="${use}"></i></div>
    </div>`;
  return `<div class="page">
    <div class="page-head"><div>
      <h1>Visão geral</h1>
      <p>O seu gabinete, ao vivo: as duas telas mostram o que está nelas agora, e a luz da placa e de cada LED dos fans e fitas aparece onde ela fica.</p>
    </div></div>
    <div class="grid-2">
      <div class="card twin-card">
        ${twin(s)}
        <div class="twin-legend">
          <span class="chip"><span class="dot ${tr.connected && !s.allOff ? "ok" : ""}"></span>${esc(tr.name)} · ${esc(tr.theme)}</span>
          <span class="chip"><span class="dot ${pf.connected && !s.allOff ? "ok" : ""}"></span>${esc(pf.name)} · ${esc(pf.theme)}</span>
          <span class="chip">LED · ${esc(s.allOff ? "apagado" : s.led.effect)}</span>
          <span class="chip">Fans · ${esc(!s.argb.on ? "com o SignalRGB" : s.allOff ? "apagados" : s.argb.effect)}</span>
        </div>
      </div>
      <div class="stack">
        <div class="card">
          <div class="card-head"><h2>Agora</h2><span class="chip" data-status-chip>lendo…</span></div>
          <div class="tiles">
            ${tile("CPU", "CPUTemp", "CpuUsage", "CpuTEC")}
            ${tile("GPU", "GPUTemp", "GpuUsage", "GpuTEC")}
            <div class="tile"><small>MEMÓRIA</small><div class="value" data-read="MemoryUsage" data-unit="%">–</div><div class="sub">em uso</div><div class="meter"><i data-meter="MemoryUsage"></i></div></div>
            <div class="tile"><small>FAN DA GPU</small><div class="value" data-read="GpuFan" data-unit="fan">–</div><div class="sub">ventoinha da placa</div></div>
          </div>
        </div>
        <div class="card">
          <h2>Modos</h2>
          <div class="rows">
            ${row("Modo Mangá", "Efeitos por cima do vídeo e do mangá, ligados ao PC", toggle("manga", {}, s.manga.on))}
            ${row("Modo Grand Line", "As duas telas e o LED como um mar só", toggle("grandLine", {}, s.grandLine.on))}
          </div>
        </div>
        ${argbOverview(s)}
        <div class="card">
          <div class="card-head"><h2>LED da placa</h2><button class="btn" type="button" data-go="led">Trocar efeito</button></div>
          <div class="choice effect active" style="pointer-events:none">
            <span class="swatch" data-effect="${esc(effect.name)}"></span>
            <span class="text"><b>${esc(effect.name)}</b><small>${esc(effect.desc)}</small></span>
          </div>
        </div>
      </div>
    </div>
  </div>`;
}

function screenPage(s, index) {
  const sc = s.screens[index];
  const cls = { thermalright: "tr", proflow: "pf", ds339: "ds" }[sc.id];
  const usingGif = sc.theme === "GIF";
  const usingVideo = sc.theme === "Vídeo";
  const media = sc.media.length
    ? `<div class="media">${sc.media.map(m => {
        const active = (m.kind === "gif" && usingGif && m.file === sc.gif) || (m.kind === "video" && usingVideo && m.file === sc.video);
        const thumb = m.kind === "gif" ? `<img src="../media/${encodeURIComponent(m.file)}" alt="" loading="lazy">` : ICON.video;
        return `<button type="button" class="media-item ${active ? "active" : ""}" ${attrs({ cmd: "media", screen: sc.id, file: m.file })}>
          <span class="thumb">${thumb}</span><b title="${esc(m.file)}">${esc(m.file)}</b><small>${m.kind === "gif" ? "GIF" : "vídeo"} · ${m.mb} MB</small>
        </button>`;
      }).join("")}</div>`
    : `<p class="empty">Nenhum arquivo ainda. Importe um ${sc.id === "thermalright" ? "vídeo ou GIF" : "GIF"}.</p>`;
  const importButtons = sc.id === "thermalright"
    ? `<button class="btn" type="button" ${attrs({ cmd: "chooseVideo", screen: sc.id })}>${ICON.video}Importar vídeo…</button>
       <button class="btn" type="button" ${attrs({ cmd: "chooseGif", screen: sc.id })}>${ICON.image}Importar GIF…</button>`
    : sc.id === "proflow"
      ? `<button class="btn" type="button" ${attrs({ cmd: "chooseGif", screen: sc.id })}>${ICON.image}Importar GIF…</button>
         <button class="btn" type="button" data-cmd="onePiece">${ICON.download}Baixar painéis do One Piece…</button>`
      : "";

  return `<div class="page">
    <div class="page-head">
      <div><h1>${esc(sc.name)}</h1><p>${esc(sc.size)} · tema <b>${esc(sc.theme)}</b></p></div>
      ${connChip(sc.connected, s.allOff)}
    </div>
    <div class="card">${screenFrame(sc.id, cls, true)}</div>
    <div class="section-title">Tema</div>
    <div class="choices">${sc.themes.map(t => `
      <button type="button" class="choice ${t.name === sc.theme ? "active" : ""}" ${attrs({ cmd: "theme", screen: sc.id, name: t.name })}>
        <b>${esc(t.name)}</b><small>${esc(t.desc)}</small>
      </button>`).join("")}</div>
    ${sc.id === "ds339" ? "" : `
    <div class="card">
      <div class="card-head"><h2>Biblioteca</h2><div class="btn-row">${importButtons}</div></div>
      ${media}
      <p class="hint" style="margin:12px 0 0">Os arquivos ficam na pasta <b>media</b> do programa. Clicar num deles já troca o tema.</p>
    </div>`}
    <div class="card">
      <h2>Ajustes</h2>
      <div class="rows">
        ${row("Brilho", "A tela não tem controle de luz: o programa escurece a imagem", range("brightness", { screen: sc.id }, sc.brightness, 5, 100, "%"))}
        ${row("Girar 180°", "Em relação ao normal desta tela", toggle("rotate", { screen: sc.id }, sc.rotated))}
        ${sc.dataStyles ? row("Dados por cima", "CPU, GPU e hora sobre o vídeo, o GIF e o One Piece", segmented("data", { screen: sc.id }, sc.dataStyles, sc.data, "style")) : ""}
        ${sc.fps ? row("Quadros por segundo", "Máximo; menos quadros, menos CPU", range("fps", { screen: sc.id }, sc.fps, 1, 30, "")) : ""}
      </div>
    </div>
  </div>`;
}

function ledPage(s) {
  const led = s.led;
  const effect = effectMeta(led.effect);
  return `<div class="page">
    <div class="page-head"><div>
      <h1>LED da placa de vídeo</h1>
      <p>ASRock RX 6900 XT — uma cor para a placa inteira, mandada até 25 vezes por segundo. Todos os efeitos respeitam o limite de 3 clarões por segundo.</p>
    </div><span class="chip" data-led-status>…</span></div>
    <div class="card">
      <div class="led-bar big" data-led></div>
      <div class="rows" style="margin-top:16px">
        ${row("Brilho", "", range("ledBrightness", {}, led.brightness, 5, 100, "%"))}
        ${row("Tom do branco", "Para os efeitos brancos", segmented("ledTone", {}, led.tones, led.tone, "name", !effect.white))}
        ${row("Velocidade", "Para os efeitos com ritmo", segmented("ledSpeed", {}, led.speeds, led.speed, "name", !effect.speed))}
        ${row("Cor", "Para Cor fixa e Respiração", `<input type="color" value="${esc(led.color)}" data-cmd="ledColor" class="color">`)}
      </div>
    </div>
    ${led.groups.map(group => `
      <div class="section-title">${esc(group.title)}</div>
      <div class="choices">${group.effects.map(e => `
        <button type="button" class="choice effect ${e.name === led.effect ? "active" : ""}" ${attrs({ cmd: "ledEffect", name: e.name })}>
          <span class="swatch" data-effect="${esc(e.name)}"></span>
          <span class="text"><b>${esc(e.name)}</b><small>${esc(e.desc)}</small></span>
        </button>`).join("")}</div>`).join("")}
  </div>`;
}

// ---------------------------------------------------------------- modo umbra

// Um fan de mentira para as prévias: 12 LEDs do anel e 6 em cada lateral (a ordem dos quadros que o programa manda).
function umbraFan(key) {
  const ring = Array.from({ length: 12 }, (_, i) => `<i class="c" style="transform:rotate(${i * 30}deg) translateY(-12px)"></i>`).join("");
  const side = cls => Array.from({ length: 6 }, (_, i) => `<i class="${cls}" style="top:${5 + i * 6}px"></i>`).join("");
  return `<span class="umbra-fan" data-argb-effect="${esc(key)}">${ring}${side("l")}${side("r")}</span>`;
}

// Desenho de cada zona do fan (como os ícones do software): o anel e as duas laterais, aceso o que a zona controla.
function umbraZoneIcon(zone) {
  const on = { Centro: [1, 0, 0], Paralelo: [0, 1, 1], "Série": [0, 1, 2], Esquerda: [0, 1, 0], Direita: [0, 0, 1], Tudo: [1, 1, 1] }[zone] || [0, 0, 0];
  const fill = v => (v === 2 ? "var(--umbra-b)" : v ? "var(--umbra-a)" : "var(--line-strong)");
  return `<svg viewBox="0 0 40 40" class="umbra-zone-icon"><rect x="3" y="4" width="4" height="32" rx="2" fill="${fill(on[1])}"/>
    <circle cx="20" cy="20" r="10" fill="none" stroke-width="4" stroke="${fill(on[0])}"/>
    <rect x="33" y="4" width="4" height="32" rx="2" fill="${fill(on[2])}"/></svg>`;
}

function umbraRange(cmd, value) {
  return `<div class="range umbra-range"><input type="range" min="0" max="50" step="10" value="${value}" style="--fill:${value * 2}%" ${attrs({ cmd, unit: "%", scale: 2 })}><output>${value === 0 ? 1 : value * 2}%</output></div>`;
}

// A coluna de ajustes do software: cor, velocidade, brilho e direção — só o que o efeito usa.
function umbraSettings(u) {
  const s = u.slot;
  if (!s) return "";
  const r = u.rules;
  const groups = [];
  if (r.colors > 0) {
    groups.push(`<div class="card umbra-group"><h3>Cor</h3><div class="umbra-colors">${s.colors.map((c, i) => `
      <label class="umbra-swatch" style="background:${esc(c)}" title="Cor ${i + 1}"><input type="color" value="${esc(c)}" ${attrs({ cmd: "umbraColor", index: i })}></label>`).join("")}</div>
      <small>Clique numa cor para trocar</small></div>`);
  }
  if (r.speed) groups.push(`<div class="card umbra-group"><h3>Velocidade</h3>${umbraRange("umbraSpeed", s.speed)}</div>`);
  groups.push(`<div class="card umbra-group"><h3>Brilho</h3>${umbraRange("umbraBrightness", s.brightness)}</div>`);
  if (r.direction) {
    groups.push(`<div class="card umbra-group"><h3>Direção</h3><div class="umbra-dir-label">${s.direction ? "Normal" : "Inverso"}</div>
      <div class="umbra-dirs">
        <button type="button" class="btn ${s.direction === 0 ? "primary" : ""}" ${attrs({ cmd: "umbraDirection", dir: 0 })} title="Inverso (倒序)">←</button>
        <button type="button" class="btn ${s.direction === 1 ? "primary" : ""}" ${attrs({ cmd: "umbraDirection", dir: 1 })} title="Normal (正序)">→</button>
      </div></div>`);
  }
  return `<div class="umbra-settings">${groups.join("")}</div>`;
}

function umbraGrid(list, current, merge) {
  return `<div class="umbra-grid">${list.map(e => `
    <button type="button" class="umbra-tile ${e.id === current ? "active" : ""}" ${attrs({ cmd: "umbraEffect", id: e.id })} title="${esc(e.original)} — ${esc(e.desc)}">
      ${umbraFan(merge ? `umbra:m${e.id}` : `umbra:${e.id}`)}
      <b>${esc(e.name)}</b><small>${esc(e.desc)}</small>
    </button>`).join("")}</div>`;
}

function umbraPorts(u) {
  const icon = p => p.fans > 0 ? `<span class="umbra-port-fans">${"<i></i>".repeat(Math.min(p.fans, 3))}</span>` : `<span class="umbra-port-strip"></span>`;
  return `<div class="card umbra-col"><h3>Portas</h3>
    ${u.ports.map(p => `<button type="button" class="umbra-port ${u.port && p.name === u.port.name ? "active" : ""}" ${attrs({ cmd: "umbraPort", name: p.name })}>
      ${icon(p)}<span><b>${esc(p.name)}</b><small>${p.fans > 0 ? `${p.fans} ${p.fans === 1 ? "fan" : "fans"} · ` : ""}${p.leds} LEDs</small></span></button>`).join("")}
    ${u.fanPorts >= 2 ? `<div class="umbra-together"><span><b>Fans juntos</b><small>Mexer num fan muda todos</small></span>${toggle("umbraTogether", {}, u.together)}</div>` : ""}
  </div>`;
}

function umbraZones(u) {
  const p = u.port;
  if (!p || p.fans === 0) {
    return `<div class="card umbra-col"><h3>Zonas</h3><p class="hint">Esta peça não é um fan: o efeito vale para ela inteira.</p></div>`;
  }
  const layout = { Paralelo: "Centro + laterais iguais", "Série": "Centro + laterais em série", Laterais: "Centro + esquerda + direita", Tudo: "O fan inteiro" }[p.layout];
  return `<div class="card umbra-col"><h3>Zonas</h3>
    ${u.zones.map(z => `<button type="button" class="umbra-zone ${z === p.zone ? "active" : ""}" ${attrs({ cmd: "umbraZone", name: z })}>${umbraZoneIcon(z)}<b>${esc(z)}</b></button>`).join("")}
    <p class="hint">Agora: ${esc(layout)}</p>
  </div>`;
}

function umbraPresets(u) {
  const cards = u.presets.map(p => `<div class="umbra-preset">
      <button type="button" class="umbra-preset-apply" ${attrs({ cmd: "umbraPresetApply", index: p.index })} title="${esc(p.desc)}"><b>${esc(p.name)}</b><small>${esc(p.desc)}</small></button>
      <button type="button" class="umbra-preset-del" ${attrs({ cmd: "umbraPresetDelete", index: p.index })} title="Apagar">×</button>
    </div>`).join("");
  const save = u.presets.length < u.maxPresets
    ? `<div class="umbra-preset new"><input type="text" class="text-input" placeholder="Nome da predefinição" maxlength="30" data-cmd="umbraPresetSave" spellcheck="false"><small>Enter salva o efeito de todas as portas</small></div>`
    : "";
  return `<div class="section-title">Predefinições (${u.presets.length}/${u.maxPresets})</div><div class="umbra-presets">${cards}${save}</div>`;
}

// O modo tela: a lista de efeitos, o gabinete ao vivo e os ajustes do efeito.
function umbraCanvasPage(u, s) {
  const c = u.canvas;
  const list = `<div class="umbra-grid canvas">${c.effects.map(e => `
    <button type="button" class="umbra-tile ${e.key === c.effect ? "active" : ""}" ${attrs({ cmd: "umbraCanvasEffect", key: e.key })} title="${esc(e.original)}">
      <b>${esc(e.name)}</b><small>${esc(e.original)}</small>
    </button>`).join("")}</div>`;
  const colors = c.colors.length ? `<div class="card umbra-group"><h3>Cores</h3><div class="umbra-colors">${c.colors.map((hex, i) => `
      <label class="umbra-swatch" style="background:${esc(hex)}"><input type="color" value="${esc(hex)}" ${attrs({ cmd: "umbraCanvasColor", index: i })}></label>`).join("")}</div>
      ${c.changedColors ? `<button class="btn small" type="button" data-cmd="umbraCanvasColorsReset">Voltar às cores do efeito</button>` : "<small>As cores originais do efeito</small>"}</div>` : "";
  const settings = `<div class="umbra-settings">
    <div class="card umbra-group"><h3>Velocidade</h3>${range("umbraCanvasSpeed", {}, c.speed, 1, c.maxSpeed, "")}</div>
    <div class="card umbra-group"><h3>Brilho</h3>${range("umbraCanvasBrightness", {}, c.brightness, 1, 10, "")}</div>
    ${colors}
  </div>`;
  return `<div class="umbra-body two"><div class="umbra-stack">${list}<div class="card twin-card">${twin(s)}</div></div>${settings}</div>`;
}

function umbraPage(s) {
  const u = s.umbra;
  const warn = u.on && !u.controlling
    ? `<div class="card umbra-warn"><span>O Modo Umbra só aparece nos LEDs com os fans e fitas no programa.</span><button class="btn primary" type="button" data-cmd="argbTake">Controlar pelo programa</button></div>`
    : "";
  let body;
  if (u.mode === "Sincronizado") {
    body = `<div class="umbra-body two">${umbraGrid(u.effects, u.slot.effect, false)}${umbraSettings(u)}</div>`;
  } else if (u.mode === "Combinado") {
    body = `<div class="umbra-body two">${umbraGrid(u.merge, u.slot.effect, true)}${umbraSettings(u)}</div>`;
  } else if (u.mode === "Independente") {
    body = `<div class="umbra-body four">${umbraPorts(u)}${umbraZones(u)}${u.slot ? umbraGrid(u.effects, u.slot.effect, false) : ""}${umbraSettings(u)}</div>${umbraPresets(u)}`;
  } else {
    body = umbraCanvasPage(u, s);
  }
  const help = {
    Sincronizado: "O mesmo efeito em todas as peças, cada uma com a sua cópia.",
    Independente: "Cada porta (peça) com o seu efeito; num fan, o centro e as laterais podem ter efeitos diferentes.",
    Combinado: "Todas as portas em fila, como uma fita só: o efeito atravessa o gabinete.",
    Tela: "Os efeitos do modo tela do software, desenhados no gabinete pela posição de cada LED.",
  }[u.mode];
  return `<div class="page umbra">
    <div class="page-head"><div>
      <h1>Modo Umbra</h1>
      <p>O software do hub Asiahorse UMBRA, refeito para a Nollie: os mesmos modos, os mesmos 15 efeitos com os mesmos ajustes, as zonas de cada fan (centro e laterais) e as predefinições. Ligado, ele toma o lugar dos efeitos da página Fans e fitas, que ficam guardados.</p>
    </div></div>
    <div class="card"><div class="mode-head">
      <div><h2>Modo Umbra</h2><p>${esc(help)}</p></div>
      <div class="umbra-switches">
        <div class="umbra-switch"><span>Usar</span>${toggle("umbraOn", {}, u.on)}</div>
        <div class="umbra-switch"><span>Luzes</span>${toggle("umbraLights", {}, u.lights)}</div>
      </div>
    </div>
    <div class="umbra-tabs">${segmented("umbraMode", {}, u.modes, u.mode)}</div></div>
    ${warn}
    ${body}
  </div>`;
}

// ---------------------------------------------------------------- fans e fitas

function argbMeta(name) {
  for (const g of state.argb.effectGroups) for (const e of g.effects) if (e.name === name) return e;
  return { name, desc: "", color: false, speed: false };
}

// O gabinete na foto: a foto escurecida, as prévias das telas por cima de onde elas ficam e cada LED desenhado como
// um ponto de luz na posição dele (canvas, redesenhado a cada atualização ao vivo).
function twin(s) {
  const L = s.argb.layout;
  const [x0, y0, x1, y1] = L.view;
  const vw = x1 - x0, vh = y1 - y0;
  const box = r => `left:${((r[0] - x0) / vw) * 100}%;top:${((r[1] - y0) / vh) * 100}%;width:${((r[2] - r[0]) / vw) * 100}%;height:${((r[3] - r[1]) / vh) * 100}%`;
  return `<div class="twin" style="aspect-ratio:${vw} / ${vh}">
    <img class="twin-photo" src="${esc(L.photo)}" alt="" style="width:${(L.width / vw) * 100}%;left:${(-x0 / vw) * 100}%;top:${(-y0 / vh) * 100}%">
    <canvas class="twin-leds" data-twin></canvas>
    <img class="twin-screen" data-preview="thermalright" alt="" style="${box(L.thermalright)}">
    <img class="twin-screen" data-preview="proflow" alt="" style="${box(L.proflow)}">
  </div>`;
}

function drawTwin() {
  if (!state) return;
  const L = state.argb.layout;
  const [x0, y0, x1, y1] = L.view;
  document.querySelectorAll("canvas[data-twin]").forEach(cv => {
    const w = cv.clientWidth, h = cv.clientHeight;
    if (!w || !h) return;
    const dpr = window.devicePixelRatio || 1;
    if (cv.width !== Math.round(w * dpr) || cv.height !== Math.round(h * dpr)) {
      cv.width = Math.round(w * dpr);
      cv.height = Math.round(h * dpr);
    }
    const ctx = cv.getContext("2d");
    ctx.setTransform(dpr, 0, 0, dpr, 0, 0);
    ctx.clearRect(0, 0, w, h);
    ctx.globalCompositeOperation = "lighter";
    const sx = w / (x1 - x0), sy = h / (y1 - y0);

    // A luz da placa de vídeo: um brilho em volta dela.
    if (live.led && !state.allOff) {
      const [gx0, gy0, gx1, gy1] = L.gpu;
      // Luz que vaza pelas bordas da placa: um halo suave, mais forte em cima (onde fica o LED dela).
      ctx.save();
      ctx.globalAlpha = 0.5;
      ctx.strokeStyle = live.led;
      ctx.shadowColor = live.led;
      ctx.shadowBlur = 34 * sx;
      ctx.lineWidth = 3 * sx;
      ctx.lineJoin = "round";
      ctx.strokeRect((gx0 - x0) * sx, (gy0 - y0) * sy, (gx1 - gx0) * sx, (gy1 - gy0) * sy);
      ctx.globalAlpha = 0.8;
      ctx.beginPath();
      ctx.moveTo((gx0 + 30 - x0) * sx, (gy0 - y0) * sy);
      ctx.lineTo((gx1 - 30 - x0) * sx, (gy0 - y0) * sy);
      ctx.stroke();
      ctx.restore();
    }

    const zones = live.argb || [];
    L.points.forEach((pts, zi) => {
      const hex = zones[zi];
      if (!pts || !hex) return;
      for (let i = 0; i * 2 < pts.length && i * 6 < hex.length; i++) {
        const c = hex.substr(i * 6, 6);
        if (c === "000000") continue;
        const r = parseInt(c.substr(0, 2), 16), g = parseInt(c.substr(2, 2), 16), b = parseInt(c.substr(4, 2), 16);
        const level = Math.max(r, g, b) / 255;
        const x = (pts[i * 2] - x0) * sx, y = (pts[i * 2 + 1] - y0) * sy;
        const rad = (6 + 16 * level) * sx;
        const grad = ctx.createRadialGradient(x, y, 0, x, y, rad);
        grad.addColorStop(0, `rgba(${r},${g},${b},0.95)`);
        grad.addColorStop(0.3, `rgba(${r},${g},${b},0.35)`);
        grad.addColorStop(1, `rgba(${r},${g},${b},0)`);
        ctx.fillStyle = grad;
        ctx.fillRect(x - rad, y - rad, rad * 2, rad * 2);
      }
    });
  });
}

function ring(effect) {
  return `<span class="ring" data-argb-effect="${esc(effect)}">${Array.from({ length: 12 }, (_, i) =>
    `<i style="transform:rotate(${i * 30}deg) translateY(-12px)"></i>`).join("")}</span>`;
}

// Amostra dos efeitos de capa de cabo: 5 fileiras de 12 LEDs (o cabo deitado).
function cableGrid(effect) {
  return `<span class="cable-grid" data-argb-effect="${esc(effect)}">${"<i></i>".repeat(60)}</span>`;
}

function strip(index, leds, mini = false) {
  return `<div class="strip ${mini ? "mini" : ""}" data-argb-zone="${index}">${"<i></i>".repeat(leds)}</div>`;
}

function argbControl(a) {
  if (!a.on) {
    return `<div class="card"><div class="mode-head">
      <div><h2>Quem controla: SignalRGB</h2><p>Passando para o programa, ele fecha o SignalRGB pelo caminho normal e assume a controladora Nollie — só manda cores, nada é gravado nela. Dá para devolver quando quiser.</p></div>
      <button class="btn primary" type="button" data-cmd="argbTake">${ICON.argb}Controlar pelo programa</button>
    </div></div>`;
  }
  if (a.other) {
    const closable = a.other === "SignalRGB";
    return `<div class="card"><div class="mode-head">
      <div><h2>${esc(a.other)} aberto</h2><p>${closable
        ? "O SignalRGB foi aberto e ficou com os fans e fitas (só um programa pode mandar na controladora). O programa espera — ou assume agora, fechando o SignalRGB pelo caminho normal."
        : `Feche o ${esc(a.other)} para o programa assumir os fans e fitas (só um programa pode mandar na controladora).`}</p></div>
      ${closable ? `<button class="btn primary" type="button" data-cmd="argbTake">${ICON.argb}Assumir o controle</button>` : ""}
    </div></div>`;
  }
  return `<div class="card"><div class="mode-head">
    <div><h2>Com o programa</h2><p>Os fans e fitas são deste programa agora: efeito geral, uma configuração por parte, limite de energia e o "desligar tudo" junto com as telas.</p></div>
    <button class="btn" type="button" data-cmd="argbReturn">Devolver ao SignalRGB</button>
  </div></div>`;
}

function zoneRow(z, a) {
  const own = z.effect !== "Igual ao geral" && z.effect !== "Apagado";
  const usesColor = own ? argbMeta(z.effect).color : false;
  return `<div class="zone">
    <div class="zone-name">
      <input type="text" class="name-input" value="${esc(z.name)}" spellcheck="false" maxlength="40" ${attrs({ cmd: "zoneName", zone: z.index })}>
      <small>canal ${z.channel} · LEDs ${z.start + 1}–${z.start + z.leds}</small>
    </div>
    ${strip(z.index, z.leds)}
    <div class="zone-controls">
      <select ${attrs({ cmd: "zoneEffect", zone: z.index })}>${zoneOptions(a, z.effect)}</select>
      <input type="color" class="color small" value="${esc(z.color)}" title="Cor desta parte" ${attrs({ cmd: "zoneColor", zone: z.index })} ${usesColor || z.effect === "Igual ao geral" ? "" : "disabled"}>
      <div class="range small" title="Brilho desta parte em relação ao geral"><input type="range" min="0" max="200" step="5" value="${z.brightness}" style="--fill:${z.brightness / 2}%" ${attrs({ cmd: "zoneBrightness", unit: "%", zone: z.index })}><output>${z.brightness}%</output></div>
      ${toggle("zoneInvert", { zone: z.index }, z.invert, "Inverter o sentido das animações")}
      <button class="btn small" type="button" title="Acende só esta parte por 8 s" ${attrs({ cmd: "identify", zone: z.index })} ${a.on && !a.other && a.connected ? "" : "disabled"}>Identificar</button>
    </div>
  </div>`;
}

function zoneOptions(a, current) {
  const option = o => `<option ${o === current ? "selected" : ""}>${esc(o)}</option>`;
  return option(a.zoneEffects[0]) + a.zoneEffectGroups.map(g =>
    `<optgroup label="${esc(g.title)}">${g.effects.map(option).join("")}</optgroup>`).join("");
}

function argbPage(s) {
  const a = s.argb;
  const effect = argbMeta(a.effect);
  const choices = list => `<div class="choices">${list.map(e => `
      <button type="button" class="choice effect ${e.name === a.effect ? "active" : ""}" ${attrs({ cmd: "argbEffect", name: e.name })}>
        ${e.cable ? cableGrid(e.name) : ring(e.name)}
        <span class="text"><b>${esc(e.name)}</b><small>${esc(e.desc)}</small></span>
      </button>`).join("")}</div>`;
  const groups = a.groups.map(g => `
    <div class="card zone-group">
      <div class="zone-group-head">
        <input type="text" class="name-input big" value="${esc(g.name)}" spellcheck="false" maxlength="40" ${attrs({ cmd: "groupName", group: g.name })}>
        <span class="chip">canal ${esc(g.channels)} · ${g.leds} LEDs</span>
      </div>
      ${g.zones.map(z => zoneRow(z, a)).join("")}
    </div>`).join("");
  return `<div class="page">
    <div class="page-head"><div>
      <h1>Fans e fitas</h1>
      <p>Controladora Nollie 16CH · ${a.zoneCount} partes, ${a.leds} LEDs. As cenas do gabinete atravessam tudo pela posição de cada LED; cada parte também pode ter o seu efeito.</p>
    </div><span class="chip" data-argb-status>…</span></div>
    <div class="grid-2">
      <div class="card twin-card">${twin(s)}</div>
      <div class="stack">
        ${s.umbra && s.umbra.on ? `<div class="card umbra-warn"><span>O Modo Umbra está ligado: os efeitos desta página ficam guardados e voltam quando ele for desligado.</span><button class="btn" type="button" data-go="umbra">Abrir o Modo Umbra</button></div>` : ""}
        ${argbControl(a)}
        <div class="card">
          <div class="rows">
            ${row("Brilho geral", "", range("argbBrightness", {}, a.brightness, 5, 100, "%"))}
            ${row("Velocidade", "Para os efeitos com movimento", segmented("argbSpeed", {}, a.speeds, a.speed, "name", !effect.speed))}
            ${row("Cor", "Dos efeitos com cor — branco por padrão", `<input type="color" value="${esc(a.color)}" data-cmd="argbColor" class="color">`)}
            ${row("Limite de energia", "Acima dele, o brilho de tudo baixa por igual", segmented("argbLimit", {}, a.limits, a.limit))}
          </div>
        </div>
      </div>
    </div>
    ${a.effectGroups.map(g => `<div class="section-title">${esc(g.title)}</div>${choices(g.effects)}`).join("")}
    <div class="section-title">Na controladora</div>
    ${nollieCard(s)}
    <div class="section-title">Partes</div>
    ${groups || `<div class="card"><p class="empty">Nenhuma parte configurada. Importe do SignalRGB abaixo.</p></div>`}
    <div class="card">
      <div class="card-head">
        <div><h2>Partes do SignalRGB</h2><p class="hint" style="margin:4px 0 0">As partes vieram de como a Nollie está montada no SignalRGB (canais e componentes). Mudou lá? Importe de novo — os nomes e ajustes daqui voltam ao padrão.</p></div>
        <button class="btn" type="button" data-cmd="argbReimport">${ICON.download}Importar de novo</button>
      </div>
    </div>
  </div>`;
}

// O que fica guardado na própria controladora: o efeito de boot e o efeito sem programa.
function nollieCard(s) {
  const n = s.nollie;
  if (!n || !n.read) {
    return `<div class="card"><div class="card-head">
      <div><h2>Guardado na controladora</h2><p class="hint" style="margin:4px 0 0">Lendo o que está gravado na Nollie…</p></div>
      <button class="btn" type="button" data-cmd="nollieRead">Ler</button></div></div>`;
  }
  const column = (part, title, hint, e, list) => {
    const meta = list[e.effect] || { color2: true, size: true };
    const data = { part };
    return `<div class="nollie-col">
      <h3>${esc(title)}</h3><p class="hint">${esc(hint)}</p>
      <div class="rows">
        ${row("Efeito", "", `<select ${attrs({ cmd: "nollieField", part, field: "effect" })}>${list.map((x, i) => `<option value="${i}" ${i === e.effect ? "selected" : ""}>${esc(x.name)}</option>`).join("")}</select>`)}
        ${row("Cores", meta.color2 ? "Principal e segunda" : "Principal", `<input type="color" class="color small" value="${esc(e.color1)}" ${attrs({ cmd: "nollieField", part, field: "color1" })}>
          <input type="color" class="color small" value="${esc(e.color2)}" ${attrs({ cmd: "nollieField", part, field: "color2" })} ${meta.color2 ? "" : "disabled"}>`)}
        ${row("Brilho", "", `<div class="range small">${range("nollieField", { ...data, field: "brightness" }, e.brightness, 0, 100, "%").replace(/^<div class="range">|<\/div>$/g, "")}</div>`)}
        ${row("Velocidade", "", `<div class="range small">${range("nollieField", { ...data, field: "speed" }, e.speed, 1, 10, "").replace(/^<div class="range">|<\/div>$/g, "")}</div>`)}
        ${meta.size ? row("Tamanho", "Da parte que anda", `<div class="range small">${range("nollieField", { ...data, field: "size" }, e.size, 0, 50, "").replace(/^<div class="range">|<\/div>$/g, "")}</div>`) : ""}
      </div>
    </div>`;
  };
  const pending = n.pending.length
    ? `<div class="nollie-pending"><b>Ao gravar, muda na controladora:</b><ul>${n.pending.map(p => `<li>${esc(p)}</li>`).join("")}</ul></div>`
    : `<p class="hint">Tudo igual ao que está gravado nela.</p>`;
  return `<div class="card">
    <div class="card-head">
      <div><h2>Guardado na controladora</h2><p class="hint" style="margin:4px 0 0">A própria Nollie toca estes efeitos sozinha: o de boot uma vez ao ligar o PC, e o outro enquanto o programa não manda cores (até ele abrir, ou depois que ele fecha). Nada vai para ela até você gravar.</p></div>
      <span class="chip">${n.writeCount} gravações na flash dela</span>
    </div>
    <p class="hint">Gravado agora — boot: <b>${esc(n.storedBoot)}</b> · sem programa: <b>${esc(n.storedNormal)}</b> · LEDs por canal: <b>${esc(n.storedLeds)}</b> · volta ao efeito dela ${esc(n.storedIdle)}.</p>
    <div class="nollie-cols">
      ${bootColumn(n, column)}
      ${column("normal", "Sem o programa", "Depois do boot até o programa abrir, e depois que ele fecha", n.normal, n.loopEffects)}
    </div>
    <div class="rows">
      ${row("Voltar ao efeito dela", "Quanto tempo depois de o programa parar de mandar cores", `<select data-cmd="nollieIdle">${n.idleOptions.map(o => `<option value="${o.value}" ${o.value === n.idle ? "selected" : ""}>${esc(o.label)}</option>`).join("")}</select>`)}
    </div>
    <p class="hint">Ordem das peças nos efeitos dela, do piso para o topo: ${esc(n.order)}.</p>
    ${pending}
    <div class="btn-row" style="margin-top:12px">
      <button class="btn primary" type="button" data-cmd="nollieWrite" ${n.pending.length ? "" : "disabled"}>Gravar na controladora</button>
      <button class="btn" type="button" data-cmd="nolliePreview">${n.storedIntro === 2 || n.bootAnimation ? "Ver a animação tocada pela controladora" : "Ver o boot agora"}</button>
      <button class="btn" type="button" data-cmd="nollieRevert" ${n.pending.length ? "" : "disabled"}>Desfazer</button>
      <button class="btn" type="button" data-cmd="nollieRead">Ler de novo</button>
    </div>
  </div>`;
}

// Ao ligar: o efeito de boot da própria controladora, ou uma animação do programa gravada na memória dela.
function bootColumn(n, column) {
  const kinds = ["Efeito da controladora", "Animação do programa"];
  const kind = n.bootAnimation ? kinds[1] : kinds[0];
  const head = `<div style="margin-bottom:10px">${segmented("nollieBootKind", {}, kinds, kind)}</div>`;
  if (!n.bootAnimation) {
    return column("boot", "Ao ligar o PC", "Toca uma vez, antes do Windows e de qualquer programa", n.boot, n.bootEffects).replace("<h3>", head + "<h3>");
  }
  const meta = n.animations.find(a => a.name === n.bootAnimation) || { desc: "" };
  return `<div class="nollie-col">
    ${head}
    <h3>Ao ligar o PC</h3><p class="hint">Desenhada pelo programa LED por LED e gravada na memória da controladora: ela toca sozinha, uma vez, antes do Windows.</p>
    <div class="rows">
      ${row("Animação", esc(meta.desc), `<select data-cmd="nollieAnimation">${n.animations.map(a => `<option ${a.name === n.bootAnimation ? "selected" : ""}>${esc(a.name)}</option>`).join("")}</select>`)}
      ${row("Ver no gabinete", "O programa toca a animação nos fans agora, sem gravar nada", `<button class="btn small" type="button" data-cmd="nollieAnimationPreview">Ver</button>`)}
      ${row("Depois", "O programa costuma assumir de 30 s a 2,5 min depois de ligar o PC", `<select data-cmd="nollieContinue">${[
        [0, "O efeito sem programa"], [1, "O loop dos fans por 1 min"], [2, "O loop dos fans por 2 min"], [3, "O loop dos fans por 3 min"], [5, "O loop dos fans por 5 min"],
      ].map(([v, label]) => `<option value="${v}" ${v === n.continueMinutes ? "selected" : ""}>${label}</option>`).join("")}</select>`)}
      ${row("Cores trocadas", "Ligue se as brasas laranja das faíscas saírem verdes da controladora (e grave de novo)", toggle("nollieGrb", {}, !n.grb))}
    </div>
    <p class="hint">Com o loop: a animação emenda no efeito configurado nesta página, tocado pela própria controladora até o programa abrir — sem a cor sólida no meio. Usa o efeito, o brilho e o limite de energia de agora: mudou algum, grave de novo.</p>
  </div>`;
}

function argbOverview(s) {
  const a = s.argb;
  const who = !a.on ? "com o SignalRGB" : a.other ? `${a.other} aberto` : s.allOff ? "apagados" : a.effect;
  return `<div class="card">
    <div class="card-head"><h2>Fans e fitas</h2><button class="btn" type="button" data-go="argb">Ajustar</button></div>
    <div class="mini-groups">${a.groups.map(g => `
      <div class="mini-group"><small>${esc(g.name)}</small><div class="mini-strips">${g.zones.map(z => strip(z.index, z.leds, true)).join("")}</div></div>`).join("")}
    </div>
    <p class="hint" style="margin:12px 0 0">${esc(who)}</p>
  </div>`;
}

// ---------------------------------------------------------------- modo jogo

function gamePage(s) {
  const g = s.game;
  const cs2 = !g.cs2Found
    ? `<p class="hint">Não achei o CS2 nas bibliotecas da Steam.</p>`
    : `<div class="rows">
        ${row("Estatísticas da partida", g.cs2Installed
          ? (g.cs2Seen ? `Recebendo do CS2 (última mensagem ${esc(g.cs2Seen)})` : "Instalada — vale a partir da próxima vez que o CS2 abrir")
          : "Placar, rodada, vida, colete, dinheiro, abates e arma, pela integração oficial da Valve (Game State Integration)",
          g.cs2Installed
            ? `<button class="btn" type="button" data-cmd="cs2Uninstall">Remover do CS2</button>`
            : `<button class="btn primary" type="button" data-cmd="cs2Install">Conectar ao CS2</button>`)}
        ${row("O gabinete reage à partida", "Bomba plantada (pulso vermelho em ritmo fixo, sem cronômetro), explosão, desarme, vitória e derrota, seus abates e capacetes, flash, smoke e fogo em você, morte — nos fans, nas fitas e no LED da placa", toggle("gameReactions", {}, g.reactions))}
        ${row("Testar as reações", "Uma rodada de mentira de 16 s, sem precisar do jogo", `<button class="btn" type="button" data-cmd="cs2Test">Testar</button>`)}
      </div>
      <p class="hint" style="margin:6px 0 0">É o recurso oficial que a Valve oferece para placares e luzes (o mesmo das integrações de Logitech, SteelSeries e Philips Hue): um arquivo na pasta de configurações do CS2 faz o próprio jogo mandar o estado da partida para este programa (só neste PC). Não mexe no jogo nem na memória dele, e só usa o que o jogo manda para o próprio jogador — nada de dados de outros jogadores nem cronômetro da bomba (a Valve atrasa de propósito o aviso de "plantada" para impedir cronômetros).</p>`;
  return `<div class="page">
    <div class="page-head"><div>
      <h1>Modo jogo</h1>
      <p>Ao abrir um jogo da lista, a Thermalright troca o tema pelo painel de jogo — FPS, 1% low, frametime, o que limita (processador ou placa), latência, ping até o servidor, temperaturas — e volta ao tema quando o jogo fecha.</p>
    </div><span class="chip"><span class="dot ${g.status.endsWith("aberto") ? "ok" : ""}"></span>${esc(g.status)}</span></div>
    <div class="card"><div class="mode-head">
      <div><h2>Modo jogo</h2><p>Os quadros são medidos pelo PresentMon (código aberto da Intel), que lê os eventos de quadro do Windows. O ping é medido até o endereço com que o jogo mais troca pacotes — no CS2, o relé da Valve que leva ao servidor.</p></div>
      ${toggle("gameEnabled", {}, g.on)}
    </div>
    <div class="rows" style="margin-top:8px">
      ${row("Jogos", "Nomes dos executáveis, separados por vírgula (cs2, valorant, r5apex…)", `<input type="text" class="text-input" value="${esc(g.games)}" data-cmd="gameList" spellcheck="false">`)}
      ${row("Ver o painel de jogo", "Uma partida de mentira na Thermalright por 20 s", `<button class="btn" type="button" data-cmd="gamePreview">Ver</button>`)}
    </div></div>
    <div class="card"><h2>Counter-Strike 2</h2>${cs2}</div>
  </div>`;
}

// ---------------------------------------------------------------- balanço de branco

const WB_CHANNELS = [["Vermelho", "r"], ["Verde", "g"], ["Azul", "b"]];

function wbSlider(target, channel, value) {
  const [label, cls] = WB_CHANNELS[channel];
  return `<div class="wb-row ${cls}">
    <span>${label}</span>
    <input type="range" min="0" max="100" value="${value}" style="--fill:${value}%" ${attrs({ cmd: "whiteGain", target, channel, unit: "%", live: "1" })}>
    <output>${value}%</output>
  </div>`;
}

function wbSwatch(gains) {
  // A cor que vai para a peça quando o efeito pede branco (o olho vê branco nela, se o balanço estiver certo).
  const c = gains.map(g => Math.round(255 * Math.pow(g / 100, 1 / 2.2)));
  return `<span class="wb-swatch" style="background:rgb(${c.join(",")})"></span>`;
}

function wbCard(title, subtitle, target, gains, note) {
  return `<div class="card wb-card">
    <div class="card-head">
      <div class="wb-title">${wbSwatch(gains)}<div><h2>${esc(title)}</h2><small class="hint">${esc(subtitle)}</small></div></div>
      <button class="btn small" type="button" ${attrs({ cmd: "whiteReset", target })}>Voltar</button>
    </div>
    ${[0, 1, 2].map(i => wbSlider(target, i, gains[i])).join("")}
    ${note ? `<p class="hint" style="margin:8px 0 0">${note}</p>` : ""}
  </div>`;
}

function whitePage(s) {
  const w = s.white;
  const main = w.groups.filter(g => g.main), rest = w.groups.filter(g => !g.main);
  return `<div class="page">
    <div class="page-head"><div>
      <h1>Balanço de branco</h1>
      <p>Cada tipo de LED puxa o branco para um lado — um mais azulado, outro mais rosado. Ajuste quanto de vermelho, verde e azul sai em cada peça até todas parecerem o mesmo branco. Vale para todos os efeitos; os efeitos continuam pensando em branco puro.</p>
    </div></div>
    <div class="card"><div class="mode-head">
      <div><h2>Comparar brancos</h2><p>Acende a placa de vídeo e todos os fans e fitas no mesmo branco, para ver qual puxa para que lado enquanto você mexe. Volta ao efeito ao desligar, ao fechar o painel ou sozinho em 10 min.</p></div>
      ${toggle("whiteCalibrate", {}, w.calibrating)}
    </div>
    <div class="rows" style="margin-top:8px">${row("Brilho do branco", "Só para comparar", `<div class="range"><input type="range" min="10" max="100" value="${w.level}" style="--fill:${((w.level - 10) / 90) * 100}%" ${attrs({ cmd: "whiteLevel", unit: "%", live: "1" })}><output>${w.level}%</output></div>`)}</div>
    </div>
    <p class="hint">Puxa para o azul: baixe o azul. Rosado ou lilás: baixe o vermelho e o azul um pouco. Esverdeado: baixe o verde. Mais forte que as outras: baixe os três por igual. Escolha uma peça como referência (os fans, por exemplo) e acerte as outras por ela.</p>
    <div class="section-title">Principais</div>
    <div class="wb-grid">
      ${wbCard("Placa de vídeo", "LED da ASRock RX 6900 XT", "gpu", w.gpu.gains, w.gpu.note ? esc(w.gpu.note) : "")}
      ${main.map(g => wbCard(g.name, `${g.leds} LEDs`, g.name, g.gains)).join("")}
    </div>
    <div class="section-title">Outras peças</div>
    <div class="wb-grid">${rest.map(g => wbCard(g.name, `${g.leds} LEDs`, g.name, g.gains)).join("")}</div>
    <p class="hint">A animação de boot gravada na controladora leva o balanço quando for gravada de novo (o card "Guardado na controladora" avisa).</p>
  </div>`;
}

// ---------------------------------------------------------------- ventoinhas

function fansPage(s) {
  const f = s.fans;
  const outputs = (live.fans && live.fans.outputs) || [];
  const outputRows = outputs.length
    ? outputs.map(o => `<div class="fan-output ${o.index === f.output ? "chosen" : ""}">
        <div class="label"><b>${esc(o.name)}</b><small data-fan-output="${o.index}">…</small></div>
        <div class="control">
          <button class="btn small" type="button" ${attrs({ cmd: "fanTest", index: o.index })}>Testar</button>
          ${o.index === f.output
            ? `<span class="chip"><span class="dot ok"></span>hub</span>`
            : `<button class="btn small" type="button" ${attrs({ cmd: "fanOutput", index: o.index })}>É o hub</button>`}
        </div>
      </div>`).join("")
    : `<p class="empty">Lendo a placa-mãe…</p>`;
  return `<div class="page">
    <div class="page-head"><div>
      <h1>Ventoinhas</h1>
      <p>Curva dos fans do gabinete (o hub dos 2×360 e do 120) pela placa-mãe${live.fans && live.fans.board ? ` — ${esc(live.fans.board)}` : ""}.</p>
    </div><span class="chip" data-fan-status>…</span></div>
    <div class="card"><div class="mode-head">
      <div><h2>Curva pelo programa</h2><p>Ligada, a curva manda na saída do hub pela temperatura da CPU e da placa de vídeo (a maior das duas curvas). Desligada, ao fechar o programa ou sem leitura de temperatura por 15 s, a BIOS volta a mandar. Acima de 88 °C na CPU ou 85 °C na placa: 100%.</p></div>
      ${toggle("fanEnabled", {}, f.on)}
    </div></div>
    <div class="grid-2">
      <div class="card">
        <div class="card-head"><h2>Curva</h2>${segmented("fanProfile", {}, f.profiles, f.profile)}</div>
        <div class="fan-chart"><canvas data-fan-chart></canvas></div>
        <div class="fan-legend"><span><i class="cpu"></i>CPU</span><span><i class="gpu"></i>Placa de vídeo</span><span>arraste os pontos para ajustar</span></div>
        <div class="rows" style="margin-top:8px">${row("Mínimo", "A curva nunca desce disso", range("fanFloor", {}, f.floor, 15, 60, "%"))}</div>
      </div>
      <div class="stack">
        <div class="card">
          <h2>Agora</h2>
          <div class="tiles" style="margin-top:12px">
            <div class="tile"><small>CPU</small><div class="value" data-fan-read="cpu" data-unit="°C">–</div></div>
            <div class="tile"><small>PLACA DE VÍDEO</small><div class="value" data-fan-read="gpu" data-unit="°C">–</div></div>
            <div class="tile"><small>A CURVA PEDE</small><div class="value" data-fan-read="target" data-unit="%">–</div></div>
            <div class="tile"><small>NO HUB</small><div class="value" data-fan-read="applied" data-unit="%">–</div><div class="sub" data-fan-applied-sub></div></div>
          </div>
        </div>
        <div class="card">
          <h2>Saída do hub</h2>
          <p class="hint" style="margin:4px 0 10px">"Testar" põe a saída em 100% por 6 s: a que fizer os fans do gabinete acelerarem é a do hub. Não escolha a da bomba do water cooler.</p>
          ${outputRows}
          <p class="hint" data-fan-rpms style="margin:10px 0 0"></p>
        </div>
      </div>
    </div>
  </div>`;
}

const fanDrag = { curve: null, index: -1 };

function fanGeometry(cv) {
  const w = cv.clientWidth, h = cv.clientHeight;
  const pad = { l: 38, r: 12, t: 12, b: 26 };
  const x = t => pad.l + ((t - 20) / 80) * (w - pad.l - pad.r);
  const y = p => pad.t + (1 - p / 100) * (h - pad.t - pad.b);
  const t = px => 20 + ((px - pad.l) / (w - pad.l - pad.r)) * 80;
  const p = py => (1 - (py - pad.t) / (h - pad.t - pad.b)) * 100;
  return { w, h, pad, x, y, t, p };
}

function drawFanChart() {
  if (!state || !state.fans) return;
  document.querySelectorAll("canvas[data-fan-chart]").forEach(cv => {
    const g = fanGeometry(cv);
    if (!g.w || !g.h) return;
    const dpr = window.devicePixelRatio || 1;
    if (cv.width !== Math.round(g.w * dpr) || cv.height !== Math.round(g.h * dpr)) {
      cv.width = Math.round(g.w * dpr);
      cv.height = Math.round(g.h * dpr);
    }
    const ctx = cv.getContext("2d");
    ctx.setTransform(dpr, 0, 0, dpr, 0, 0);
    ctx.clearRect(0, 0, g.w, g.h);
    ctx.font = "11px Segoe UI";
    ctx.fillStyle = "#a19e96";
    ctx.strokeStyle = "#ebe9e3";
    ctx.lineWidth = 1;
    for (let t = 20; t <= 100; t += 10) {
      ctx.beginPath(); ctx.moveTo(g.x(t), g.pad.t); ctx.lineTo(g.x(t), g.h - g.pad.b); ctx.stroke();
      ctx.fillText(`${t}°`, g.x(t) - 8, g.h - 8);
    }
    for (let p = 0; p <= 100; p += 25) {
      ctx.beginPath(); ctx.moveTo(g.pad.l, g.y(p)); ctx.lineTo(g.w - g.pad.r, g.y(p)); ctx.stroke();
      ctx.fillText(`${p}%`, 2, g.y(p) + 4);
    }
    const f = state.fans;
    // mínimo
    ctx.fillStyle = "rgba(20,20,20,.05)";
    ctx.fillRect(g.pad.l, g.y(f.floor), g.w - g.pad.l - g.pad.r, g.y(0) - g.y(f.floor));
    const curve = (points, color, dash) => {
      ctx.save();
      ctx.strokeStyle = color;
      ctx.lineWidth = 2.5;
      ctx.setLineDash(dash);
      ctx.beginPath();
      ctx.moveTo(g.x(20), g.y(Math.max(points[0][1], f.floor)));
      points.forEach(([t, p]) => ctx.lineTo(g.x(t), g.y(Math.max(p, f.floor))));
      ctx.lineTo(g.x(100), g.y(Math.max(points[points.length - 1][1], f.floor)));
      ctx.stroke();
      ctx.setLineDash([]);
      points.forEach(([t, p]) => {
        ctx.beginPath();
        ctx.arc(g.x(t), g.y(p), 6, 0, Math.PI * 2);
        ctx.fillStyle = "#fff";
        ctx.fill();
        ctx.stroke();
      });
      ctx.restore();
    };
    curve(f.gpu, "#7c7a74", [6, 4]);
    curve(f.cpu, "#141414", []);
    // temperaturas de agora
    const lf = live.fans;
    if (lf) {
      [["cpu", "#141414"], ["gpu", "#7c7a74"]].forEach(([k, color]) => {
        const v = lf[k];
        if (v == null) return;
        ctx.strokeStyle = color;
        ctx.setLineDash([2, 3]);
        ctx.beginPath(); ctx.moveTo(g.x(Math.min(100, Math.max(20, v))), g.pad.t); ctx.lineTo(g.x(Math.min(100, Math.max(20, v))), g.h - g.pad.b); ctx.stroke();
        ctx.setLineDash([]);
      });
      if (lf.applied != null) {
        ctx.fillStyle = "#1f9d55";
        ctx.fillRect(g.pad.l, g.y(lf.applied) - 1, g.w - g.pad.l - g.pad.r, 2);
      }
    }
  });
}

function applyFans() {
  const lf = live.fans;
  drawFanChart();
  if (!lf || !state) return;
  document.querySelectorAll("[data-fan-status]").forEach(el => {
    el.innerHTML = `<span class="dot ${lf.applied != null ? "ok" : ""}"></span>${esc(lf.status)}`;
  });
  document.querySelectorAll("[data-fan-read]").forEach(el => {
    const v = lf[el.dataset.fanRead];
    el.innerHTML = v == null ? (el.dataset.fanRead === "applied" ? "BIOS" : "–") : fmt(v, el.dataset.unit);
  });
  document.querySelectorAll("[data-fan-applied-sub]").forEach(el => (el.textContent = lf.applied != null ? "pela curva" : "a BIOS manda"));
  document.querySelectorAll("[data-fan-output]").forEach(el => {
    const o = (lf.outputs || []).find(x => x.index === +el.dataset.fanOutput);
    el.textContent = !o ? "" : o.value == null ? "automático (curva da BIOS)" : `${o.value}%${o.index === state.fans.output && lf.applied != null ? " pela curva do programa" : " fixo"}`;
  });
  document.querySelectorAll("[data-fan-rpms]").forEach(el => {
    el.textContent = "Rotação lida pela placa: " + (lf.rpms || []).map(r => `${r.name.replace("Fan #", "entrada ")} ${r.rpm ?? "–"} rpm`).join(" · ") +
      ". O hub não informa rotação (só a bomba ou um fan aparecem).";
  });
}

function fanPointer(e, phase) {
  const cv = e.target.closest && e.target.closest("canvas[data-fan-chart]");
  if (phase === "down") {
    if (!cv) return;
    const g = fanGeometry(cv);
    const r = cv.getBoundingClientRect();
    const mx = e.clientX - r.left, my = e.clientY - r.top;
    let best = null;
    ["cpu", "gpu"].forEach(curve => state.fans[curve].forEach(([t, p], i) => {
      const d = Math.hypot(g.x(t) - mx, g.y(p) - my);
      if (d < 14 && (!best || d < best.d)) best = { curve, i, d };
    }));
    if (!best) return;
    fanDrag.curve = best.curve;
    fanDrag.index = best.i;
    fanDrag.canvas = cv;
    interacting = true;
    e.preventDefault();
    return;
  }
  if (!fanDrag.curve) return;
  const cvs = fanDrag.canvas;
  const g = fanGeometry(cvs);
  const r = cvs.getBoundingClientRect();
  const points = state.fans[fanDrag.curve];
  const i = fanDrag.index;
  const lo = i > 0 ? points[i - 1][0] + 1 : 20, hi = i < points.length - 1 ? points[i + 1][0] - 1 : 100;
  points[i] = [Math.round(Math.min(hi, Math.max(lo, g.t(e.clientX - r.left)))), Math.round(Math.min(100, Math.max(0, g.p(e.clientY - r.top))))];
  drawFanChart();
  if (phase === "up") {
    send("fanCurve", { curve: fanDrag.curve, points: JSON.stringify(points) });
    fanDrag.curve = null;
  }
}
document.addEventListener("pointerdown", e => fanPointer(e, "down"));
window.addEventListener("pointermove", e => fanPointer(e, "move"));
window.addEventListener("pointerup", e => fanPointer(e, "up"));

function modesPage(s) {
  const tests = (stories, enabled) => `<div class="tests">${stories
    .map(t => `<button type="button" class="test ${enabled ? "" : "disabled"}" ${attrs({ cmd: "story", name: t.story })}><b>${esc(t.label)}</b><small>${esc(t.hint)}</small></button>`)
    .join("")}</div>`;
  return `<div class="page">
    <div class="page-head"><div>
      <h1>Modos</h1>
      <p>Jeitos de o gabinete inteiro trabalhar junto. Só um modo fica ligado por vez.</p>
    </div></div>
    <div class="card">
      <div class="mode-head">
        <div><h2>Modo Mangá</h2><p>Mantém o vídeo da Thermalright e o slideshow da Pro Flow e encena por cima, nas duas telas e no LED: impactos, Haki, Gear 5 ao abrir um jogo, balões para o que você faz no PC.</p></div>
        ${toggle("manga", {}, s.manga.on)}
      </div>
      <div class="mode-body">
        <div class="rows">
          ${row("Ações do PC", "Print, download, copiar, pendrive, música, microfone, mudo", toggle("mangaOption", { option: "acoes" }, s.manga.actions))}
          ${row("Capítulo na hora cheia", "Página de título das 8h às 23h", toggle("mangaOption", { option: "capitulos" }, s.manga.chapters))}
          ${row("GIF do Gear 5", esc(s.manga.gear5 || "nenhum"), `<button class="btn" type="button" data-cmd="gear5">${ICON.image}Trocar…</button>`)}
        </div>
        <div><div class="section-title" style="margin:0 0 10px">Testar ${s.manga.on ? "" : "· ligue o modo para testar"}</div>${tests(s.manga.stories, s.manga.on)}</div>
      </div>
    </div>
    <div class="card">
      <div class="mode-head">
        <div><h2>Modo Grand Line</h2><p>"Coluna do Mar": a Thermalright é o céu, a Pro Flow o fundo do mar e a placa a luz das profundezas. As ações do PC viram histórias que atravessam as telas.</p></div>
        ${toggle("grandLine", {}, s.grandLine.on)}
      </div>
      <div class="mode-body">
        <div><div class="section-title" style="margin:0 0 10px">Testar ${s.grandLine.on ? "" : "· ligue o modo para testar"}</div>${tests(s.grandLine.stories, s.grandLine.on)}</div>
      </div>
    </div>
    <div class="card">
      <div class="mode-head">
        <div><h2>Desligar tudo</h2><p>Apaga a Pro Flow, desativa a Thermalright (a luz de fundo apaga), deixa o LED preto e apaga os fans e fitas (pelo programa, ou fechando o SignalRGB pelo caminho normal se ele estiver com eles). Religar devolve tudo.</p></div>
        <button class="btn primary" type="button" ${attrs({ cmd: "allOff", on: !s.allOff })}>${ICON.power}${s.allOff ? "Religar tudo" : "Desligar tudo"}</button>
      </div>
    </div>
  </div>`;
}

function systemPage(s) {
  return `<div class="page">
    <div class="page-head"><div><h1>Sistema</h1><p>DS339 Monitor ${esc(s.version)}</p></div></div>
    <div class="card">
      <h2>Programa</h2>
      <div class="rows">
        ${row("Iniciar com o Windows", "Abre sozinho no logon, já como administrador", toggle("autostart", {}, s.autostart))}
        ${row("Administrador", s.admin ? "Rodando como administrador: sensores da CPU, LED e telas liberados" : "Sem administrador, a temperatura e a potência da CPU não aparecem",
          s.admin ? `<span class="chip"><span class="dot ok"></span>ativo</span>` : `<button class="btn" type="button" data-cmd="admin">${ICON.shield}Reiniciar como administrador</button>`)}
      </div>
    </div>
    <div class="card">
      <h2>Arquivos</h2>
      <div class="btn-row" style="margin-top:12px">
        <button class="btn" type="button" data-cmd="capture">${ICON.image}Salvar imagem das telas</button>
        <button class="btn" type="button" data-cmd="log">${ICON.file}Abrir log</button>
        <button class="btn" type="button" data-cmd="folder">${ICON.folder}Abrir pasta do programa</button>
      </div>
    </div>
    <div class="card">
      <div class="card-head">
        <div><h2>Sair do programa</h2><p class="hint" style="margin:4px 0 0">As telas param e o LED volta às cores originais da placa. Fechar esta janela não fecha o programa.</p></div>
        <button class="btn danger" type="button" data-cmd="exit">${ICON.exit}Sair</button>
      </div>
    </div>
  </div>`;
}

// ---------------------------------------------------------------- desenho

function visiblePages() {
  return PAGES.filter(p => !p.onlyConnected || (state && state.screens[p.screen].connected));
}

function renderNav() {
  let group = null;
  $nav.innerHTML = visiblePages().map(p => {
    const dot = p.screen != null && state ? `<span class="dot ${state.screens[p.screen].connected ? "ok" : ""}"></span>` : "";
    const head = p.group && p.group !== group ? `<div class="nav-group">${esc(p.group)}</div>` : "";
    group = p.group || null;
    return `${head}<button type="button" class="nav ${p.id === page ? "active" : ""}" data-go="${p.id}" ${p.id === page ? 'aria-current="page"' : ""}>${ICON[p.id] || ""}<span>${esc(p.label)}</span>${dot}</button>`;
  }).join("");
  if (state) {
    $power.className = `power ${state.allOff ? "on" : ""}`;
    $power.innerHTML = `${ICON.power}${state.allOff ? "Religar tudo" : "Desligar tudo"}`;
  }
}

function render() {
  if (!state) return;
  pendingRender = false;
  if (!visiblePages().some(p => p.id === page)) page = "overview";
  renderNav();
  const top = $page.scrollTop;
  const p = PAGES.find(x => x.id === page);
  $page.innerHTML =
    page === "overview" ? overview(state)
      : page === "led" ? ledPage(state)
        : page === "argb" ? argbPage(state)
          : page === "umbra" ? umbraPage(state)
          : page === "fans" ? fansPage(state)
            : page === "white" ? whitePage(state)
              : page === "game" ? gamePage(state)
        : page === "modes" ? modesPage(state)
          : page === "system" ? systemPage(state)
            : screenPage(state, p.screen);
  $page.scrollTop = top;
  applyLive();
  requestPreviews();
}

function navigate(id) {
  if (id === page) return;
  page = id;
  save("page", id);
  render();
  $page.scrollTop = 0;
}

/** Prévias dos efeitos do LED: calculadas pelo programa (o mesmo motor do LED), pedidas de novo se o tom mudar. */
function requestPreviews() {
  if (!state || (page !== "led" && page !== "overview" && page !== "argb" && page !== "umbra")) return;
  const key = `${state.led.tone}|${state.led.speed}|${state.led.color}|${state.argb.color}|${state.argb.speed}|${state.umbra ? state.umbra.previewKey : ""}`;
  if (key === previewKey) return;
  previewKey = key;
  send("previews");
}

function applyLive() {
  document.querySelectorAll("img[data-preview]").forEach(img => {
    const src = images[img.dataset.preview];
    if (src && img.getAttribute("src") !== src) img.setAttribute("src", src);
  });
  if (state) {
    const ids = ["thermalright", "proflow", "ds339"];
    document.querySelectorAll("[data-offline]").forEach(el => {
      const i = ids.indexOf(el.dataset.offline);
      const connected = live.connected[i] ?? state.screens[i].connected;
      const show = !connected || !images[el.dataset.offline] || state.allOff;
      el.style.display = show ? "grid" : "none";
      el.textContent = state.allOff ? "apagada" : connected ? "aguardando imagem…" : "desconectada";
    });
  }
  const c = live.led;
  document.querySelectorAll("[data-led]").forEach(el => {
    el.style.background = c || "#1a1a1a";
    el.style.boxShadow = c ? `0 0 30px 3px ${c}70, 0 0 8px ${c}` : "none";
  });
  // O status do LED só vira texto quando diz algo além do nome do efeito (pausado, desligado, sem resposta...).
  const effectName = state ? state.led.effect.toLowerCase() : "";
  const ledStatus = live.ledStatus || "";
  const healthy = ledStatus === effectName || ledStatus.startsWith("apagado");
  document.querySelectorAll("[data-led-status]").forEach(el => {
    el.innerHTML = !ledStatus ? "" : healthy ? `<span class="dot ok"></span>${state && state.allOff ? "apagado" : "ativo"}` : esc(ledStatus);
    el.classList.toggle("chip", true);
  });
  document.querySelectorAll("[data-read]").forEach(el => {
    const v = live.readings[el.dataset.read];
    const unit = el.dataset.unit === "fan" ? (v != null && v > 100 ? " rpm" : "%") : el.dataset.unit;
    el.innerHTML = fmt(v, unit);
  });
  document.querySelectorAll("[data-read-plain]").forEach(el => {
    const v = live.readings[el.dataset.readPlain];
    el.textContent = v == null ? "–" : `${Math.round(v)}${el.dataset.unit}`;
  });
  document.querySelectorAll("[data-meter]").forEach(el => {
    const v = live.readings[el.dataset.meter];
    el.style.width = `${Math.max(0, Math.min(100, v ?? 0))}%`;
  });
  applyArgb();
  document.querySelectorAll("[data-status-chip]").forEach(el => (el.textContent = state && state.allOff ? "tudo desligado" : "ao vivo"));
  $status.textContent = state && state.allOff ? "Tudo desligado — telas, LED e SignalRGB." : live.status || "";
}

function applyArgb() {
  drawTwin();
  applyFans();
  const zones = live.argb || [];
  document.querySelectorAll("[data-argb-zone]").forEach(el => {
    const hex = zones[+el.dataset.argbZone];
    const dots = el.children;
    for (let i = 0; i < dots.length; i++) {
      const c = hex ? `#${hex.substr(i * 6, 6)}` : "";
      if (dots[i].dataset.c === c) continue;
      dots[i].dataset.c = c;
      dots[i].style.background = c;
      dots[i].style.boxShadow = c && c !== "#000000" ? `0 0 5px ${c}` : "none";
    }
  });
  if (!state) return;
  const a = state.argb;
  const status = live.argbStatus || "";
  document.querySelectorAll("[data-argb-status]").forEach(el => {
    const ok = status === "ativo" || status === "identificando" || status.startsWith("apagado");
    el.innerHTML = !a.on ? "com o SignalRGB" : ok ? `<span class="dot ${status.startsWith("apagado") ? "dark" : "ok"}"></span>${esc(status)}` : esc(status || "…");
  });
}

function toast(text, warning) {
  const el = document.createElement("div");
  el.className = `toast ${warning ? "warning" : ""}`;
  el.textContent = text;
  document.getElementById("toasts").appendChild(el);
  setTimeout(() => el.classList.add("leaving"), 4200);
  setTimeout(() => el.remove(), 4600);
}

// Amostras dos efeitos: cada uma toca os 12 s calculados pelo programa, em loop, no ritmo do LED de verdade
// (25 quadros por segundo) — e só enquanto há amostra na tela e a janela está visível.
function animate() {
  if (document.hidden) return;
  const i = Math.floor(performance.now() / 40) % 300;
  animateRings(i);
  if (!previews) return;
  const swatches = document.querySelectorAll(".swatch[data-effect]");
  if (!swatches.length) return;
  swatches.forEach(el => {
    const p = previews[el.dataset.effect];
    if (!p) return;
    const c = p[i % p.length];
    if (el.dataset.shown === c) return;
    el.dataset.shown = c;
    el.style.background = c;
    el.style.boxShadow = `0 0 16px ${c}66`;
  });
}

// Anéis dos efeitos dos fans: 12 LEDs calculados pelo programa; "Igual à placa de vídeo" toca a amostra do efeito do LED.
function animateRings(i) {
  if (!argbPreviews || !state) return;
  document.querySelectorAll("[data-argb-effect]").forEach(el => {
    const effect = el.dataset.argbEffect;
    let frame;
    if (effect === "Igual à placa de vídeo") {
      const p = previews && previews[state.led.effect];
      const c = p ? p[i % p.length].slice(1) : "ffffff";
      frame = c.repeat(el.children.length);
    } else {
      const p = argbPreviews[effect];
      if (!p) return;
      frame = p[i % p.length];
    }
    if (el.dataset.shown === frame) return;
    el.dataset.shown = frame;
    const dots = el.children;
    for (let k = 0; k < dots.length; k++) {
      const c = `#${frame.substr(k * 6, 6)}`;
      dots[k].style.background = c;
      dots[k].style.boxShadow = `0 0 4px ${c}`;
    }
  });
}

// ---------------------------------------------------------------- eventos

document.addEventListener("click", e => {
  const go = e.target.closest("[data-go]");
  if (go) {
    navigate(go.dataset.go);
    return;
  }
  const el = e.target.closest("[data-cmd]");
  if (!el || el.tagName === "INPUT") return;
  const { cmd, ...data } = el.dataset;
  if (data.on === "true" || data.on === "false") data.on = data.on === "true";
  // Resposta imediata: marca a escolha antes de o programa confirmar.
  if (el.classList.contains("choice") || el.classList.contains("media-item")) {
    el.parentElement.querySelectorAll(".active").forEach(x => x.classList.remove("active"));
    el.classList.add("active");
  }
  if (el.parentElement && el.parentElement.classList.contains("segmented")) {
    el.parentElement.querySelectorAll("button").forEach(x => x.classList.toggle("active", x === el));
  }
  send(cmd, data);
});

$power.addEventListener("click", () => state && send("allOff", { on: !state.allOff }));

document.addEventListener("change", e => {
  const el = e.target;
  if (!el.dataset || !el.dataset.cmd) return;
  const { cmd, unit, ...data } = el.dataset;
  if (el.type === "checkbox") send(cmd, { ...data, on: el.checked });
  else if (el.type === "range") {
    delete data.live;
    send(cmd, { ...data, value: Number(el.value) });
  }
  else if (el.type === "color") send(cmd, { ...data, hex: el.value });
  else if (el.tagName === "SELECT" || el.type === "text") send(cmd, { ...data, text: el.value });
});

// Enter num nome confirma (o "change" sai ao perder o foco).
document.addEventListener("keydown", e => {
  if (e.key === "Enter" && e.target.type === "text") e.target.blur();
});

// Enquanto um nome está sendo digitado, o redesenho espera (senão o campo perderia o foco).
document.addEventListener("focusout", e => {
  if (e.target.type !== "text" && e.target.tagName !== "SELECT") return;
  setTimeout(() => {
    if (pendingRender && !busy()) render();
  }, 0);
});

function busy() {
  const a = document.activeElement;
  return interacting || (a && (a.type === "text" || a.tagName === "SELECT"));
}

let liveSendAt = 0;
document.addEventListener("input", e => {
  const el = e.target;
  if (el.type !== "range") return;
  const fill = ((el.value - el.min) / (el.max - el.min)) * 100;
  el.style.setProperty("--fill", `${fill}%`);
  const out = el.parentElement.querySelector("output");
  if (out) out.textContent = el.dataset.scale ? `${Math.max(1, el.value * el.dataset.scale)}${el.dataset.unit || ""}` : `${el.value}${el.dataset.unit || ""}`;
  // Deslizantes "ao vivo" (balanço de branco): aplica enquanto arrasta, sem gravar; o "change" do fim grava.
  if (el.dataset.live === "1" && performance.now() - liveSendAt > 60) {
    liveSendAt = performance.now();
    const { cmd, unit, live, ...data } = el.dataset;
    send(cmd, { ...data, value: Number(el.value), live: true });
    const swatch = el.closest(".wb-card") && el.closest(".wb-card").querySelector(".wb-swatch");
    if (swatch) {
      const values = [...el.closest(".wb-card").querySelectorAll("input[type=range]")].map(x => Math.round(255 * Math.pow(x.value / 100, 1 / 2.2)));
      swatch.style.background = `rgb(${values.join(",")})`;
    }
  }
});

document.addEventListener("pointerdown", e => {
  if (e.target.type === "range") interacting = true;
});
window.addEventListener("pointerup", () => {
  if (!interacting) return;
  interacting = false;
  if (pendingRender && !busy()) render();
});

if (bridge) {
  bridge.addEventListener("message", e => {
    const m = e.data;
    switch (m.type) {
      case "state": {
        const json = JSON.stringify(m);
        if (json === stateJson) return;
        stateJson = json;
        state = m;
        if (busy()) pendingRender = true;
        else render();
        break;
      }
      case "live":
        for (const id of ["thermalright", "proflow", "ds339"]) if (m[id]) images[id] = m[id];
        if (m.readings) live.readings = m.readings;
        if (m.status) live.status = m.status;
        live.led = m.led;
        live.ledStatus = m.ledStatus;
        live.connected = m.connected || live.connected;
        if (m.argb) live.argb = m.argb;
        if (m.argbStatus != null) live.argbStatus = m.argbStatus;
        if (m.fans) {
          const outputsBefore = JSON.stringify((live.fans && live.fans.outputs || []).map(o => o.index));
          live.fans = m.fans;
          if (page === "fans" && outputsBefore !== JSON.stringify(m.fans.outputs.map(o => o.index)) && !busy()) render();
        }
        applyLive();
        break;
      case "effectPreviews":
        previews = m.previews;
        argbPreviews = m.argbPreviews;
        break;
      case "toast":
        toast(m.text, m.warning);
        break;
      case "navigate":
        if (PAGES.some(p => p.id === m.page)) navigate(m.page);
        break;
    }
  });
}

window.addEventListener("resize", () => { drawTwin(); drawFanChart(); });

renderNav();
setInterval(animate, 40);
send("ready");
