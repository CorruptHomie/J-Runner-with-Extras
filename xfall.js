/* Falling X glyphs — a scaled-up, full-page version of UI/SnowfallBackground.cs.
   Same three depth tiers, same sway, same round-capped crossed strokes. */
(function () {
  const c = document.createElement("canvas");
  c.id = "xfall";
  document.body.appendChild(c);
  const ctx = c.getContext("2d");
  const reduce = window.matchMedia("(prefers-reduced-motion: reduce)").matches;
  const TIERS = [
    { size: 5,  colour: "#202025", w: 1 },
    { size: 8,  colour: "#26262c", w: 1.4 },
    { size: 12, colour: "#2d2d34", w: 1.8 },
  ];
  let W = 0, H = 0, dpr = 1, flakes = [];

  function make(seeded) {
    const t = TIERS[(Math.random() * 3) | 0];
    const tier = TIERS.indexOf(t);
    return {
      x: Math.random() * W,
      y: seeded ? Math.random() * H : -t.size * 2,
      speed: 0.6 + tier * 0.35 + Math.random() * 0.4,
      size: t.size, colour: t.colour, lw: t.w,
      drift: 6 + Math.random() * 16,
      phase: Math.random() * Math.PI * 2,
    };
  }

  function resize() {
    dpr = Math.min(2, window.devicePixelRatio || 1);
    W = window.innerWidth; H = window.innerHeight;
    c.width = W * dpr; c.height = H * dpr;
    c.style.width = W + "px"; c.style.height = H + "px";
    ctx.setTransform(dpr, 0, 0, dpr, 0, 0);
    const count = Math.max(40, Math.min(140, ((W * H) / 12000) | 0));
    while (flakes.length < count) flakes.push(make(true));
    flakes.length = count;
  }

  function draw() {
    ctx.clearRect(0, 0, W, H);
    ctx.lineCap = "round";
    for (const f of flakes) {
      if (!reduce) { f.y += f.speed; f.phase += 0.02; if (f.y - f.size > H) Object.assign(f, make(false)); }
      const x = f.x + Math.sin(f.phase) * f.drift, y = f.y, s = f.size;
      ctx.strokeStyle = f.colour; ctx.lineWidth = f.lw;
      ctx.beginPath();
      ctx.moveTo(x - s, y - s); ctx.lineTo(x + s, y + s);
      ctx.moveTo(x + s, y - s); ctx.lineTo(x - s, y + s);
      ctx.stroke();
    }
    requestAnimationFrame(draw);
  }

  window.addEventListener("resize", resize);
  resize(); draw();
})();
