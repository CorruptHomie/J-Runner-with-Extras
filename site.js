/* Scroll reveals, the live log ticker, the XeLL preview and the lightbox. */
(function () {
  const reduce = window.matchMedia("(prefers-reduced-motion: reduce)").matches;

  /* ---- reveal on scroll ---- */
  const io = new IntersectionObserver((entries) => {
    entries.forEach((e) => { if (e.isIntersecting) { e.target.classList.add("in"); io.unobserve(e.target); } });
  }, { rootMargin: "0px 0px -8% 0px", threshold: 0.08 });
  document.querySelectorAll("[data-reveal]").forEach((el, i) => {
    el.style.setProperty("--d", (i % 6) * 60 + "ms");
    if (reduce) el.classList.add("in"); else io.observe(el);
  });

  /* ---- hero screenshot tilt ---- */
  const shot = document.querySelector("[data-tilt]");
  if (shot && !reduce) {
    shot.addEventListener("pointermove", (e) => {
      const r = shot.getBoundingClientRect();
      const px = (e.clientX - r.left) / r.width - 0.5, py = (e.clientY - r.top) / r.height - 0.5;
      const t = `perspective(1400px) rotateY(${px * 5}deg) rotateX(${-py * 4}deg) translateY(-4px)`;
      shot.style.transform = t;
      shot.parentElement.style.setProperty("--tilt", t);
    });
    shot.addEventListener("pointerleave", () => {
      shot.style.transform = "";
      shot.parentElement.style.removeProperty("--tilt");
    });
  }

  /* ---- live session log ---- */
  const log = document.querySelector("[data-log]");
  if (log) {
    const SCRIPTED = [
      ["J-Runner Premium", ""], ["Session: 07/30/2026 3:27:15", ""], ["Version: 4.0.0pre1", ""],
      ["Status: Up to date", "ok"], ["", ""],
      ["PicoFlasher detected on COM4", "ok"],
      ["Reading Nand... pass 1 of 2", "info"],
      ["Nand read OK — 0 bad blocks", "ok"],
      ["CB Type: Corona 4GB · LDV 12", "info"],
      ["Building XeBuild image (17559, glitch2)...", "info"],
      ["Image written to output\\updflash.bin", "ok"],
      ["Ready to write. Disconnect nothing.", "warn"],
    ];
    let i = 0;
    const push = () => {
      const [text, sev] = SCRIPTED[i % SCRIPTED.length];
      const line = document.createElement("div");
      line.className = "logline" + (sev ? " sev-" + sev : "");
      line.textContent = text || "\u00a0";
      log.appendChild(line);
      log.scrollTop = log.scrollHeight;
      while (log.children.length > 40) log.removeChild(log.firstChild);
      i++;
    };
    SCRIPTED.forEach(() => {});
    for (let k = 0; k < 5; k++) push();
    if (!reduce) setInterval(push, 1400);
  }

  /* ---- XeLL preview cycling ---- */
  const xell = document.querySelector("[data-xell]");
  if (xell) {
    const PRESETS = [
      { id: "default", name: "Default", bg: "4E44D8", fg: "FFFFFF" },
      { id: "swizzy",  name: "Swizzy",  bg: "000000", fg: "FF6600" },
      { id: "xtudo",   name: "XTUDO",   bg: "000000", fg: "FF66FF" },
      { id: "classic", name: "Classic", bg: "000000", fg: "008000" },
    ];
    const screen = xell.querySelector("[data-xell-screen]");
    const btns = xell.querySelectorAll("[data-preset]");
    let idx = 0;
    const apply = (n) => {
      idx = n;
      const p = PRESETS[n];
      screen.style.background = "#" + p.bg;
      screen.style.color = "#" + p.fg;
      btns.forEach((b, j) => b.classList.toggle("on", j === n));
    };
    btns.forEach((b, j) => b.addEventListener("click", () => { apply(j); clearInterval(timer); }));
    apply(0);
    let timer = reduce ? null : setInterval(() => apply((idx + 1) % PRESETS.length), 3200);
  }

  /* ---- lightbox ---- */
  const box = document.querySelector("[data-lightbox]");
  if (box) {
    const img = box.querySelector("img"), cap = box.querySelector("figcaption");
    document.querySelectorAll("[data-shot]").forEach((el) => {
      el.addEventListener("click", () => {
        img.src = el.querySelector("img").src;
        cap.textContent = el.dataset.shot;
        box.classList.add("open");
      });
    });
    const close = () => box.classList.remove("open");
    box.addEventListener("click", close);
    document.addEventListener("keydown", (e) => { if (e.key === "Escape") close(); });
  }

  /* ---- header condense ---- */
  const hdr = document.querySelector("[data-header]");
  if (hdr) {
    const on = () => hdr.classList.toggle("stuck", window.scrollY > 24);
    window.addEventListener("scroll", on, { passive: true }); on();
  }
})();
