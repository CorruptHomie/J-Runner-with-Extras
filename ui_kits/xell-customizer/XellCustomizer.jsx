const CONSOLE_COLORS = {
  RED:"FF0000", BLUE:"4E44D8", GREEN:"008000", BLACK:"000000", WHITE:"FFFFFF", GREY:"C0C0C0",
  BROWN:"993300", PURPLE:"9900FF", YELLOW:"FFFF00", ORANGE:"FF6600", PINK:"FF66FF",
};
const PRESETS = [
  { id:"default", name:"Default Theme", description:"Classic XeLL blue background with white text", bg:"4E44D8", fg:"FFFFFF" },
  { id:"swizzy",  name:"Swizzy Theme",  description:"Black background with orange text - Swizzy's favorite!", bg:"000000", fg:"FF6600" },
  { id:"xtudo",   name:"XTUDO Theme",   description:"Black background with pink text - Niceshot's favorite!", bg:"000000", fg:"FF66FF" },
  { id:"classic", name:"Classic Theme", description:"Black background with green text", bg:"000000", fg:"008000" },
];
const ASCII = `   __  __     ____  ____
  |  \\/  |   |  __||  __|
  |      | ___| |__ | |__
  |  |\\/| |/ _ \\  __||  __|
  |__|  |_|\\___/____||____|`;

const CONSOLE_TEXT = ({ ascii }) => [
"  * Xenos FB with 148x41 (1280x720) at 0x9e000000 initialized.","",
"XeLL - Xenon linux loader second stage v0.993-git-7526b02 2026-07-30 (LibXenon.org)","",
"Built with GCC 13.3.0 and Binutils 2.44","",
ascii,"",
"        Free60.org XeLL - Xenon Linux Loader v0.993-git-7526b02",
"         Special Corona & Winchester Compatible XeLL version","",
"  * nand init","  * network init","  * initializing lwip 1.4.1...",
"Reinit PHY...","Waiting for link...link still down.","  * requesting dhcp...................."].join("\n");

const isDark = (hex) => {
  const r = parseInt(hex.slice(0,2),16), g = parseInt(hex.slice(2,4),16), b = parseInt(hex.slice(4,6),16);
  return (r*299 + g*587 + b*114) / 1000 < 128;
};

const Card = ({ children, style }) => (
  <div style={{ background:"var(--xc-card)", border:"1px solid var(--xc-border)", borderRadius:14,
    boxShadow:"0 4px 6px -1px rgb(0 0 0 / .2)", marginBottom:24, ...style }}>{children}</div>
);
const CardHeader = ({ icon, title, description }) => (
  <div style={{ padding:"24px 24px 12px" }}>
    <div style={{ display:"flex", alignItems:"center", gap:8, font:"600 16px/1.4 var(--jr-font-ui)", color:"var(--xc-foreground)" }}>
      <i data-lucide={icon} style={{ width:20, height:20, color:"var(--xc-icon)" }}></i>{title}
    </div>
    <div style={{ marginTop:6, font:"400 14px/1.4 var(--jr-font-ui)", color:"var(--xc-muted-foreground)" }}>{description}</div>
  </div>
);

function XellCustomizer() {
  const [bg, setBg] = React.useState("4E44D8");
  const [fg, setFg] = React.useState("FFFFFF");
  const [tab, setTab] = React.useState("presets");
  const [ascii, setAscii] = React.useState(ASCII);
  React.useEffect(() => { if (window.lucide) window.lucide.createIcons(); });

  const TabBtn = ({ id, label }) => (
    <button onClick={() => setTab(id)} style={{ padding:"7px 12px", borderRadius:8, border:"none", cursor:"pointer",
      background: tab===id ? "var(--xc-card)" : "transparent", color: tab===id ? "var(--xc-foreground)" : "var(--xc-muted-foreground)",
      font:"500 14px/1 var(--jr-font-ui)", boxShadow: tab===id ? "0 1px 2px rgb(0 0 0/.3)" : "none" }}>{label}</button>
  );

  const swatchGrid = (setter, current) => (
    <div style={{ display:"grid", gridTemplateColumns:"repeat(6,1fr)", gap:8 }}>
      {Object.entries(CONSOLE_COLORS).map(([name, val]) => (
        <button key={name} onClick={() => setter(val)}
          style={{ height:40, display:"flex", alignItems:"center", justifyContent:"flex-start", gap:4, padding:"0 10px",
            background:"#"+val, color: isDark(val) ? "#fff" : "#0f172a", border:"1px solid var(--xc-border)",
            borderRadius:8, cursor:"pointer", font:"500 12px/1 var(--jr-font-ui)", textTransform:"uppercase" }}>
          {name.toLowerCase()}
          {current===val && <i data-lucide="check" style={{width:14,height:14}}></i>}
        </button>
      ))}
    </div>
  );

  return (
    <div style={{ maxWidth:1024, margin:"0 auto", padding:"32px 16px" }}>
      <div style={{ textAlign:"center", marginBottom:40 }}>
        <div style={{ display:"inline-flex", padding:8, borderRadius:999, background:"rgb(30 58 138 / .3)" }}>
          <i data-lucide="terminal" style={{ width:24, height:24, color:"#60a5fa" }}></i>
        </div>
        <h1 style={{ margin:"16px 0 8px", font:"700 36px/1.1 var(--jr-font-ui)",
          background:"linear-gradient(to right,var(--xc-heading-from),var(--xc-heading-to))",
          WebkitBackgroundClip:"text", backgroundClip:"text", color:"transparent" }}>XeLL Theme Customizer</h1>
        <p style={{ margin:0, color:"var(--xc-muted-foreground)", font:"400 16px/1.5 var(--jr-font-ui)" }}>
          Customize the appearance of your XeLL console with colors and ASCII art</p>
        <p style={{ marginTop:12, font:"400 14px/1 var(--jr-font-ui)", color:"var(--xc-foreground)" }}>
          <strong>Thanks to:</strong> <a href="#" style={{ color:"#60a5fa" }}>Cancer_</a></p>
      </div>

      <Card>
        <CardHeader icon="monitor" title="Console Preview" description="Live preview of your XeLL console" />
        <div style={{ padding:"0 24px 24px" }}>
          <div style={{ border:"8px solid #1e293b", borderRadius:10, boxShadow:"0 10px 15px -3px rgb(0 0 0/.4)" }}>
            <div style={{ aspectRatio:"16 / 9", overflow:"auto", padding:"8px 32px", whiteSpace:"pre",
              font:"400 13px/1.15 var(--jr-font-mono)", background:"#"+bg, color:"#"+fg }}>
              {CONSOLE_TEXT({ ascii })}
            </div>
          </div>
        </div>
      </Card>

      <Card>
        <CardHeader icon="layout" title="Settings" description="Customize your console appearance" />
        <div style={{ padding:"0 24px 24px" }}>
          <div style={{ display:"grid", gridTemplateColumns:"repeat(4,1fr)", gap:4, padding:4, borderRadius:10,
            background:"var(--xc-muted)", marginBottom:20 }}>
            <TabBtn id="presets" label="Presets" /><TabBtn id="background" label="Background" />
            <TabBtn id="foreground" label="Text" /><TabBtn id="ascii" label="ASCII Art" />
          </div>

          {tab==="presets" && (
            <div style={{ display:"grid", gridTemplateColumns:"1fr 1fr", gap:16 }}>
              {PRESETS.map(p => (
                <button key={p.id} onClick={() => { setBg(p.bg); setFg(p.fg); }}
                  style={{ textAlign:"left", padding:16, borderRadius:10, cursor:"pointer",
                    background:"rgb(255 255 255 / .04)", border:"1px solid var(--xc-border)", color:"var(--xc-foreground)" }}>
                  <div style={{ display:"flex", alignItems:"center", justifyContent:"space-between" }}>
                    <span style={{ font:"600 14px/1 var(--jr-font-ui)" }}>{p.name}</span>
                    <span style={{ display:"flex", gap:8 }}>
                      <span style={{ width:24, height:24, borderRadius:999, background:"#"+p.bg, border:"1px solid #cbd5e1" }} />
                      <span style={{ width:24, height:24, borderRadius:999, background:"#"+p.fg, border:"1px solid #cbd5e1" }} />
                    </span>
                  </div>
                  <p style={{ margin:"6px 0 0", font:"400 12px/1.4 var(--jr-font-ui)", color:"var(--xc-muted-foreground)" }}>{p.description}</p>
                </button>
              ))}
            </div>
          )}
          {tab==="background" && swatchGrid(setBg, bg)}
          {tab==="foreground" && swatchGrid(setFg, fg)}
          {tab==="ascii" && (
            <textarea value={ascii} onChange={(e)=>setAscii(e.target.value)} placeholder="Enter your ASCII art here..."
              style={{ width:"100%", minHeight:150, boxSizing:"border-box", padding:12, borderRadius:8,
                background:"rgb(255 255 255 / .04)", border:"1px solid var(--xc-border)", color:"var(--xc-foreground)",
                font:"400 13px/1.3 var(--jr-font-mono)", resize:"vertical" }} />
          )}

          <div style={{ height:1, background:"var(--xc-border)", margin:"24px 0" }} />
          <button style={{ width:"100%", height:40, borderRadius:8, border:"none", cursor:"pointer",
            background:"var(--xc-foreground)", color:"var(--xc-card)", font:"500 14px/1 var(--jr-font-ui)" }}>
            Generate Custom XeLL Build</button>
        </div>
      </Card>

      <p style={{ textAlign:"center", color:"var(--xc-muted-foreground)", font:"400 13px/1.4 var(--jr-font-ui)" }}>
        XeLL Theme Customizer — Created by <a href="https://github.com/barrenechea" style={{color:"#60a5fa"}}>barrenechea</a>
      </p>
    </div>
  );
}
Object.assign(window, { XellCustomizer });
