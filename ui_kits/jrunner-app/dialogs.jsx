const { Button, Radio } = window.JRunnerPremiumDesignSystem_5a2f40;

/* Recreation of Forms/GlitchChipProgrammer.cs — 420x300, PanelBg, 10px rounded, its own
   36px WindowBg title bar with a ✕ (U+2715) close, and an accent Program button. */
function GlitchChipDialog({ onClose }) {
  const [variant, setVariant] = React.useState("RPicoRGH");
  const [status, setStatus] = React.useState(null);
  const [busy, setBusy] = React.useState(false);
  const program = () => {
    setBusy(true); setStatus({ text:"Looking for a Pico in BOOTSEL mode...", ok:null });
    setTimeout(()=>{ setStatus({ text:"Programmed successfully. Unplug and replug the Pico.", ok:true }); setBusy(false); }, 1400);
  };
  return (
    <div style={{width:420,background:"var(--jr-panel-bg)",border:"1px solid var(--jr-border)",
      borderRadius:"var(--jr-radius-dialog-sm)",boxShadow:"var(--jr-shadow-dialog)",overflow:"hidden"}}>
      <div style={{display:"flex",alignItems:"center",height:36,background:"var(--jr-window-bg)"}}>
        <span style={{paddingLeft:14,font:"700 var(--jr-text-md)/1 var(--jr-font-ui)"}}>Program Glitch Chip</span>
        <span style={{flex:1}} />
        <button onClick={onClose} style={{width:36,height:36,background:"none",border:"none",cursor:"pointer",
          color:"var(--jr-chrome-glyph)",font:"13px var(--jr-font-ui)"}}>✕</button>
      </div>
      <div style={{padding:"14px 20px 20px"}}>
        <p style={{margin:0,color:"var(--jr-text-secondary)",font:"400 var(--jr-text-sm)/1.5 var(--jr-font-ui)",textWrap:"pretty"}}>
          Flashes a Raspberry Pi Pico set to BOOTSEL mode with RGH 1.2 glitcher firmware. Hold the BOOTSEL button while plugging the Pico into this PC, then click Program.
        </p>
        <div style={{display:"flex",flexDirection:"column",gap:6,margin:"16px 0 0 4px"}}>
          <Radio checked={variant==="RPicoRGH"} onChange={()=>setVariant("RPicoRGH")} label="RPicoRGH (recommended)" />
          <Radio checked={variant==="PicoRGH"} onChange={()=>setVariant("PicoRGH")} label="PicoRGH (original/legacy)" />
        </div>
        <div style={{minHeight:38,marginTop:16,font:"400 var(--jr-text-sm)/1.4 var(--jr-font-ui)",
          color: status ? (status.ok===true?"var(--jr-accent)":status.ok===false?"var(--jr-danger)":"var(--jr-text-secondary)") : "transparent"}}>
          {status ? status.text : "."}
        </div>
        <Button variant="primary" size="xl" disabled={busy} onClick={program} style={{width:110}}>Program</Button>
      </div>
    </div>
  );
}

Object.assign(window, { GlitchChipDialog });
