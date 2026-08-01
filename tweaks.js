/* Tweaks — three controls that reshape the page's feel, not its pixels.
   Hidden unless opened; state persists under one namespaced localStorage key. */
(function () {
  const KEY = "jr.site.tweaks";
  const DEFAULTS = { storm: "drift", accent: "green", rhythm: "showcase" };

  const ACCENTS = {
    green:  { name: "J-Runner",  a: "#74c757", light: "#a1e082", dim: "#468037", on: "#141414" },
    blue:   { name: "XeLL",      a: "#4E44D8", light: "#7a72e6", dim: "#332c9e", on: "#ffffff" },
    orange: { name: "Swizzy",    a: "#FF6600", light: "#ff8c3d", dim: "#a34200", on: "#141414" },
    pink:   { name: "XTUDO",     a: "#FF66FF", light: "#ff99ff", dim: "#a340a3", on: "#141414" },
  };
  const STORM = {
    off:      { name: "Still",    count: 0,    speed: 0,   scale: 1,   fade: 0 },
    drift:    { name: "Drift",    count: 1,    speed: 1,   scale: 1,   fade: 1 },
    blizzard: { name: "Blizzard", count: 2.6,  speed: 2.4, scale: 1.5, fade: 1 },
  };
  const RHYTHM = {
    showcase:  "Showcase",
    workbench: "Workbench",
    terminal:  "Terminal",
  };

  let state = { ...DEFAULTS };
  try { Object.assign(state, JSON.parse(localStorage.getItem(KEY) || "{}")); } catch (e) {}

  function apply() {
    const a = ACCENTS[state.accent] || ACCENTS.green;
    const r = document.documentElement.style;
    r.setProperty("--jr-accent", a.a);
    r.setProperty("--jr-accent-light", a.light);
    r.setProperty("--jr-accent-dim", a.dim);
    r.setProperty("--jr-text-on-accent", a.on);
    r.setProperty("--jr-log-ok", a.light);

    document.body.dataset.rhythm = state.rhythm;
    window.__xfall && window.__xfall.set(STORM[state.storm] || STORM.drift);

    document.querySelectorAll("[data-tw]").forEach((el) => {
      el.classList.toggle("on", state[el.dataset.tw] === el.dataset.val);
    });
    try { localStorage.setItem(KEY, JSON.stringify(state)); } catch (e) {}
  }

  function group(label, hint, key, opts) {
    return (
      '<div class="twgroup"><div class="twlabel">' + label +
      '</div><div class="twhint">' + hint + '</div><div class="twrow">' +
      opts.map(([val, name]) =>
        '<button class="twopt" data-tw="' + key + '" data-val="' + val + '">' + name + "</button>"
      ).join("") + "</div></div>"
    );
  }

  const panel = document.createElement("div");
  panel.className = "tweaks";
  panel.innerHTML =
    '<button class="twtoggle" aria-label="Tweaks">Tweaks</button>' +
    '<div class="twbody">' +
      '<div class="twhead">Tweaks<button class="twclose" aria-label="Close">✕</button></div>' +
      group("Background", "How hard the X's fall behind everything.", "storm",
        Object.entries(STORM).map(([k, v]) => [k, v.name])) +
      group("Accent", "The one brand colour, borrowed from the XeLL presets.", "accent",
        Object.entries(ACCENTS).map(([k, v]) => [k, v.name])) +
      group("Rhythm", "Type scale, spacing and corners as one decision.", "rhythm",
        Object.entries(RHYTHM)) +
    "</div>";
  document.body.appendChild(panel);

  panel.querySelector(".twtoggle").addEventListener("click", () => panel.classList.toggle("open"));
  panel.querySelector(".twclose").addEventListener("click", () => panel.classList.remove("open"));
  panel.querySelectorAll("[data-tw]").forEach((b) =>
    b.addEventListener("click", () => { state[b.dataset.tw] = b.dataset.val; apply(); })
  );

  apply();
})();
