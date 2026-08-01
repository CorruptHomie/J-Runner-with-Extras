/* Motion layer — the site's animation system.
   Grounded in two things the app actually does: the flash overlay's "colour rising through
   greyscale", and the drifting X's. Everything else is restraint plus timing.
   Every effect is gated on prefers-reduced-motion. */
(function () {
  const reduce = window.matchMedia("(prefers-reduced-motion: reduce)").matches;
  const raf = window.requestAnimationFrame;

  /* ══ 1. Boot sequence — a short POST-code splash, once per session ══════════════ */
  function boot() {
    if (reduce || sessionStorage.getItem("jr.booted")) return;
    if (!document.body.classList.contains("is-home")) return;
    sessionStorage.setItem("jr.booted", "1");

    const el = document.createElement("div");
    el.className = "bootveil";
    el.innerHTML =
      '<div class="bootinner">' +
        '<div class="bootmark"><img src="assets/logo-jr2.png" alt=""></div>' +
        '<div class="bootlog"></div>' +
        '<div class="bootbar"><i></i></div>' +
      "</div>";
    document.body.appendChild(el);
    document.body.classList.add("booting");

    const LINES = ["init nand", "init network", "detect flasher", "ready"];
    const log = el.querySelector(".bootlog");
    let i = 0;
    const step = () => {
      if (i < LINES.length) {
        const d = document.createElement("div");
        d.textContent = "  * " + LINES[i];
        log.appendChild(d);
        i++;
        setTimeout(step, 130);
      } else {
        setTimeout(() => {
          el.classList.add("gone");
          document.body.classList.remove("booting");
          setTimeout(() => el.remove(), 700);
        }, 180);
      }
    };
    setTimeout(step, 220);
  }

  /* ══ 2. Headline word reveal — words rise out of a mask, staggered ══════════════ */
  function splitHeadlines() {
    document.querySelectorAll("h1, [data-split]").forEach((h) => {
      if (h.dataset.split === "done") return;
      const walk = (node) => {
        [...node.childNodes].forEach((n) => {
          if (n.nodeType === 3) {
            const frag = document.createDocumentFragment();
            n.textContent.split(/(\s+)/).forEach((tok) => {
              if (!tok.trim()) { frag.appendChild(document.createTextNode(tok)); return; }
              const w = document.createElement("span");
              w.className = "w";
              const inner = document.createElement("i");
              inner.textContent = tok;
              w.appendChild(inner);
              frag.appendChild(w);
            });
            node.replaceChild(frag, n);
          } else if (n.nodeType === 1 && !n.classList.contains("w")) {
            walk(n);
          }
        });
      };
      walk(h);
      h.querySelectorAll(".w i").forEach((el, k) => (el.style.transitionDelay = k * 55 + "ms"));
      h.dataset.split = "done";
      h.classList.add("split");
      if (reduce) h.classList.add("shown");
    });
  }

  /* ══ 3. One observer, several behaviours ════════════════════════════════════════ */
  const io = new IntersectionObserver(
    (entries) => {
      entries.forEach((e) => {
        if (!e.isIntersecting) return;
        e.target.classList.add("shown");
        io.unobserve(e.target);
      });
    },
    { rootMargin: "0px 0px -10% 0px", threshold: 0.15 }
  );
  const watch = (sel, cls) =>
    document.querySelectorAll(sel).forEach((el) => {
      if (cls) el.classList.add(cls);
      if (reduce) el.classList.add("shown");
      else io.observe(el);
    });

  /* ══ 4. Colour rising through greyscale — the app's own flash motif ═════════════ */
  function riseImages() {
    document
      .querySelectorAll(".shot, figure.gitem, .frow .visual, .device")
      .forEach((el) => el.classList.add("rise"));
    watch(".rise");
  }

  /* ══ 5. Scroll progress rail in the header ══════════════════════════════════════ */
  function progressRail() {
    const hdr = document.querySelector("header.site");
    if (!hdr) return;
    const rail = document.createElement("i");
    rail.className = "scrollrail";
    hdr.appendChild(rail);
    const on = () => {
      const max = document.documentElement.scrollHeight - window.innerHeight;
      rail.style.transform = "scaleX(" + (max > 0 ? window.scrollY / max : 0) + ")";
    };
    window.addEventListener("scroll", on, { passive: true });
    window.addEventListener("resize", on);
    on();
  }

  /* ══ 6. Nav scroll-spy with a sliding underline ═════════════════════════════════ */
  function scrollSpy() {
    const nav = document.querySelector("nav.top");
    if (!nav) return;
    const links = [...nav.querySelectorAll('a[href*="#"]')];
    const targets = links
      .map((a) => {
        const id = a.getAttribute("href").split("#")[1];
        const el = id && document.getElementById(id);
        return el ? { a, el } : null;
      })
      .filter(Boolean);
    if (!targets.length) return;

    const slider = document.createElement("i");
    slider.className = "navslider";
    nav.appendChild(slider);

    let current = null;
    const move = (a) => {
      if (!a) { slider.style.opacity = 0; return; }
      const r = a.getBoundingClientRect(), n = nav.getBoundingClientRect();
      slider.style.opacity = 1;
      slider.style.width = r.width + "px";
      slider.style.transform = "translateX(" + (r.left - n.left) + "px)";
    };
    const on = () => {
      let found = null;
      for (const t of targets) {
        const r = t.el.getBoundingClientRect();
        if (r.top <= 140 && r.bottom > 140) found = t.a;
      }
      if (found !== current) { current = found; move(found); }
    };
    window.addEventListener("scroll", on, { passive: true });
    window.addEventListener("resize", () => move(current));
    on();
  }

  /* ══ 7. Magnetic buttons + press ripple ═════════════════════════════════════════ */
  function magnetic() {
    if (reduce) return;
    document.querySelectorAll(".btn").forEach((b) => {
      b.addEventListener("pointermove", (e) => {
        const r = b.getBoundingClientRect();
        const dx = (e.clientX - (r.left + r.width / 2)) / r.width;
        const dy = (e.clientY - (r.top + r.height / 2)) / r.height;
        b.style.transform = "translate(" + dx * 5 + "px," + dy * 4 + "px)";
        b.style.setProperty("--mx", ((e.clientX - r.left) / r.width) * 100 + "%");
      });
      b.addEventListener("pointerleave", () => (b.style.transform = ""));
    });
  }

  /* ══ 8. Hero cursor spotlight ═══════════════════════════════════════════════════ */
  function spotlight() {
    const hero = document.querySelector(".hero");
    if (!hero || reduce) return;
    hero.classList.add("haslight");
    hero.addEventListener("pointermove", (e) => {
      const r = hero.getBoundingClientRect();
      hero.style.setProperty("--lx", e.clientX - r.left + "px");
      hero.style.setProperty("--ly", e.clientY - r.top + "px");
    });
  }

  /* ══ 9. Hero parallax on scroll ═════════════════════════════════════════════════ */
  function heroParallax() {
    const wrap = document.querySelector(".hero .shotwrap");
    if (!wrap || reduce) return;
    let ticking = false;
    const on = () => {
      if (ticking) return;
      ticking = true;
      raf(() => {
        const y = Math.min(window.scrollY, 700);
        wrap.style.setProperty("--py", y * 0.06 + "px");
        wrap.style.setProperty("--ps", 1 - Math.min(y / 4200, 0.035));
        ticking = false;
      });
    };
    window.addEventListener("scroll", on, { passive: true });
    on();
  }

  /* ══ 10. CRT treatment on the XeLL preview ══════════════════════════════════════ */
  function crt() {
    const s = document.querySelector(".xscreen");
    if (s) s.classList.add("crt");
  }

  /* ══ 11. Docs: step numbers light up as you pass them ═══════════════════════════ */
  function steps() {
    const items = document.querySelectorAll(".steps li");
    if (!items.length) return;
    const so = new IntersectionObserver(
      (es) => es.forEach((e) => e.target.classList.toggle("active", e.isIntersecting)),
      { rootMargin: "-40% 0px -40% 0px" }
    );
    items.forEach((li) => (reduce ? li.classList.add("active") : so.observe(li)));
  }

  /* ══ 12. Docs TOC scroll-spy ════════════════════════════════════════════════════ */
  function tocSpy() {
    const toc = document.querySelector(".toc");
    if (!toc) return;
    const links = [...toc.querySelectorAll("a")];
    const map = links
      .map((a) => ({ a, el: document.getElementById(a.getAttribute("href").slice(1)) }))
      .filter((x) => x.el);
    const on = () => {
      let cur = map[0];
      for (const m of map) if (m.el.getBoundingClientRect().top <= 160) cur = m;
      links.forEach((a) => a.classList.toggle("on", cur && a === cur.a));
    };
    window.addEventListener("scroll", on, { passive: true });
    on();
  }

  /* ══ 13. Internal page transition ═══════════════════════════════════════════════ */
  function pageOut() {
    if (reduce) return;
    document.querySelectorAll('a[href$=".html"]').forEach((a) => {
      a.addEventListener("click", (e) => {
        if (e.metaKey || e.ctrlKey || e.shiftKey || a.target) return;
        e.preventDefault();
        document.body.classList.add("leaving");
        setTimeout(() => (location.href = a.href), 240);
      });
    });
  }

  /* ══ boot it all ════════════════════════════════════════════════════════════════ */
  function init() {
    splitHeadlines();
    watch("h1.split, h2, .eyebrow");
    riseImages();
    watch(".release, .callout, table.spec, .steps");
    progressRail();
    scrollSpy();
    magnetic();
    spotlight();
    heroParallax();
    crt();
    steps();
    tocSpy();
    pageOut();
    boot();
    raf(() => document.body.classList.add("ready"));
  }

  if (document.readyState === "loading") document.addEventListener("DOMContentLoaded", init);
  else init();
})();
