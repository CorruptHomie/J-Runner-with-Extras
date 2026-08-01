/* Motion layer 2 — physicality pass. Inert until site.css gains the .m2-* rules and the
   pages load this file. Everything is gated on prefers-reduced-motion. */
(function () {
  const reduce = window.matchMedia("(prefers-reduced-motion: reduce)").matches;
  const raf = window.requestAnimationFrame;

  /* ══ 1. 3D tilt on the hardware cards ═══════════════════════════════════════════ */
  function deviceTilt() {
    if (reduce) return;
    document.querySelectorAll(".device").forEach((d) => {
      d.classList.add("m2-tilt");
      d.addEventListener("pointermove", (e) => {
        const r = d.getBoundingClientRect();
        const px = (e.clientX - r.left) / r.width - 0.5;
        const py = (e.clientY - r.top) / r.height - 0.5;
        d.style.setProperty("--rx", -py * 12 + "deg");
        d.style.setProperty("--ry", px * 14 + "deg");
        d.style.setProperty("--gx", (px + 0.5) * 100 + "%");
        d.style.setProperty("--gy", (py + 0.5) * 100 + "%");
      });
      d.addEventListener("pointerleave", () => {
        d.style.setProperty("--rx", "0deg");
        d.style.setProperty("--ry", "0deg");
      });
    });
  }

  /* ══ 2. Gallery marquee — duplicated track, pauses on hover ═════════════════════ */
  function galleryMarquee() {
    const g = document.querySelector(".gallery");
    if (!g || reduce) return;
    const items = [...g.children];
    if (items.length < 3) return;

    const viewport = document.createElement("div");
    viewport.className = "m2-marquee";
    const track = document.createElement("div");
    track.className = "m2-track";
    items.forEach((it) => track.appendChild(it));
    viewport.appendChild(track);
    g.replaceWith(viewport);

    // One "set" is the original items. Clone whole sets until the track is at least
    // twice the viewport wide, then translate by exactly one set for a seamless loop.
    const TILE = 330, GAP = 16;
    const setW = items.length * (TILE + GAP);
    const copies = Math.max(2, Math.ceil((window.innerWidth * 2) / setW) + 1);
    for (let c = 1; c < copies; c++) items.forEach((it) => track.appendChild(it.cloneNode(true)));

    track.style.setProperty("--shift", setW + "px");
    track.style.setProperty("--dur", items.length * 7 + "s");

    // Re-bind the lightbox on the cloned tiles.
    const box = document.querySelector("[data-lightbox]");
    if (box) {
      const img = box.querySelector("img"), cap = box.querySelector("figcaption");
      track.querySelectorAll("[data-shot]").forEach((el) => {
        el.addEventListener("click", () => {
          img.src = el.querySelector("img").src;
          cap.textContent = el.dataset.shot;
          box.classList.add("open");
        });
      });
    }
    // Colour arrives per tile as it enters the viewport. Clones start already lit so the
    // loop never shows a greyscale tile on later passes.
    const io = new IntersectionObserver(
      (es) => es.forEach((e) => e.isIntersecting && e.target.classList.add("shown")),
      { threshold: 0.3 }
    );
    track.querySelectorAll("figure").forEach((f) => {
      f.removeAttribute("data-reveal");
      f.classList.add("in", "rise");
      io.observe(f);
    });
  }

  /* ══ 3. Section number gutters — 01 / 02 / 03, scrubbing with scroll ════════════ */
  function sectionGutters() {
    const sections = [...document.querySelectorAll("section[id]")];
    if (!sections.length) return;
    sections.forEach((s, i) => {
      const g = document.createElement("i");
      g.className = "m2-gutter";
      g.dataset.n = String(i + 1).padStart(2, "0");
      s.appendChild(g);
      s.classList.add("m2-hasgutter");
    });
    if (reduce) return;
    let ticking = false;
    const on = () => {
      if (ticking) return;
      ticking = true;
      raf(() => {
        const mid = window.innerHeight / 2;
        sections.forEach((s) => {
          const r = s.getBoundingClientRect();
          const active = r.top < mid && r.bottom > mid;
          s.classList.toggle("m2-active", active);
          const p = Math.max(0, Math.min(1, (mid - r.top) / Math.max(1, r.height)));
          s.style.setProperty("--sp", p);
        });
        ticking = false;
      });
    };
    window.addEventListener("scroll", on, { passive: true });
    on();
  }

  /* ══ 4. Accent trail cursor — snaps to interactive elements ═════════════════════ */
  function trailCursor() {
    if (reduce || window.matchMedia("(pointer: coarse)").matches) return;
    const dot = document.createElement("i");
    dot.className = "m2-cursor";
    document.body.appendChild(dot);

    let tx = innerWidth / 2, ty = innerHeight / 2, cx = tx, cy = ty;
    let tw = 10, th = 10, cw = 10, ch = 10, tr = 999, cr = 999, snapped = false;
    addEventListener("pointermove", (e) => {
      const hit = e.target.closest("a, button, .device, figure.gitem, .preset, .twopt");
      if (hit) {
        const r = hit.getBoundingClientRect();
        tx = r.left + r.width / 2; ty = r.top + r.height / 2;
        tw = r.width + 6; th = r.height + 6;
        tr = parseFloat(getComputedStyle(hit).borderTopLeftRadius) || 0;
        tr = Math.min(tr + 3, Math.min(tw, th) / 2);
        snapped = true;
      } else {
        tx = e.clientX; ty = e.clientY; tw = th = 10; tr = 999; snapped = false;
      }
      dot.classList.toggle("snap", snapped);
    }, { passive: true });

    (function loop() {
      const k = snapped ? 0.24 : 0.34;
      cx += (tx - cx) * k; cy += (ty - cy) * k;
      cw += (tw - cw) * 0.22; ch += (th - ch) * 0.22;
      cr += (Math.min(tr, 999) - cr) * 0.22;
      dot.style.transform = `translate(${cx}px,${cy}px) translate(-50%,-50%)`;
      dot.style.width = cw + "px";
      dot.style.height = ch + "px";
      dot.style.borderRadius = Math.min(cr, Math.min(cw, ch) / 2) + "px";
      raf(loop);
    })();
  }

  /* ══ 5. Count-up ticking in the log console ═════════════════════════════════════ */
  function logCounters() {
    const log = document.querySelector("[data-log]");
    if (!log || reduce) return;
    // Any number inside a freshly-added line ticks up to its value.
    const animate = (node) => {
      const m = node.textContent.match(/(\d+)%/);
      if (!m) return;
      const target = +m[1], tpl = node.textContent;
      let v = 0;
      const id = setInterval(() => {
        v = Math.min(target, v + Math.max(1, Math.round(target / 18)));
        node.textContent = tpl.replace(/\d+%/, v + "%");
        if (v >= target) clearInterval(id);
      }, 45);
    };
    new MutationObserver((muts) =>
      muts.forEach((mu) => mu.addedNodes.forEach((n) => n.nodeType === 1 && animate(n)))
    ).observe(log, { childList: true });
  }

  function init() {
    deviceTilt();
    galleryMarquee();
    sectionGutters();
    trailCursor();
    logCounters();
  }
  if (document.readyState === "loading") document.addEventListener("DOMContentLoaded", init);
  else init();
})();
