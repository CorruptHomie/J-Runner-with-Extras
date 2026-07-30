const { TitleBar, StatusBar, MenuDropdown, MessageDialog, FlashOverlay } = window.JRunnerPremiumDesignSystem_5a2f40;

const MENUS = {
  Tools: ["Address Calculator","CPU Key Generator","SMC Config Editor","-","Hex Editor","Keyvault Decrypter"],
  Nand: ["Read Nand","Write Nand","Create ECC","-","Extract Files","Nand Compare"],
  Advanced: ["Program Glitch Chip","XeLL Customizer","-","Create Donor Nand","Patch Keyvault","-","Settings"],
};

const INITIAL_LOG = [
  "J-Runner Premium",
  "Session: 07/30/2026 3:27:15",
  "Version: 4.0.0devpre4",
  { text: "Status: Up to date", severity: "ok" },
  "",
  { text: "XeLL Customizer: Starting local server on port 2222...", severity: "muted" },
  { text: "XeLL Customizer: [@octokit/request] \"POST /repos/stackflow85/xell-builder/actions/workflows/build.yml/dispatches\" is deprecated.", severity: "muted" },
];

function MainWindow() {
  const [menu, setMenu] = React.useState(null);
  const [modal, setModal] = React.useState(null);
  const [progress, setProgress] = React.useState(0);
  const [state, setState] = React.useState({
    reads: 2, iface: "USB", cpuKey: "", flasher: null, eta: "",
    kernel: "", console: "", cb: null, ip: "192.168.12.",
    xbTab: "XeBuild", infoTab: "Nand Info",
    nand: {}, kv: {}, badBlocks: [], log: INITIAL_LOG, progress: 0,
  });
  const set = (patch) => setState((s) => ({ ...s, ...patch }));
  const say = (line) => setState((s) => ({ ...s, log: [...s.log, line] }));

  // Detect a flasher a moment after load, exactly like the real app's device poll.
  React.useEffect(() => {
    const t = setTimeout(() => {
      set({ flasher: "../../assets/device-picoflasher.png" });
      say({ text: "PicoFlasher detected on COM4", severity: "ok" });
    }, 900);
    return () => clearTimeout(t);
  }, []);

  const startFlash = () => {
    setModal(null); setProgress(0); setModal("flashing");
    let v = 0;
    const id = setInterval(() => {
      v += 4; setProgress(v);
      if (v >= 100) { clearInterval(id); setTimeout(() => { setModal("done"); say({ text: "Nand write complete.", severity: "ok" }); }, 700); }
    }, 120);
  };

  return (
    <div style={{position:"relative",width:832,height:697,background:"var(--jr-window-bg)",
      boxShadow:"var(--jr-shadow-window)",display:"flex",flexDirection:"column",overflow:"hidden"}}
      onClick={() => setMenu(null)}>
      <div onClick={(e)=>e.stopPropagation()}>
        <TitleBar logo="../../assets/logo-jr.png" menu={Object.keys(MENUS)} activeMenu={menu}
          onMenu={(m)=>setMenu(menu===m?null:m)}
          right={<span style={{fontSize:12,paddingRight:4}}>V4.0.0devpre4</span>}
          onMinimize={()=>{}} onClose={()=>{}} />
        {menu && (
          <div style={{position:"absolute",zIndex:40,top:34,left:Object.keys(MENUS).indexOf(menu)*0+8+Object.keys(MENUS).slice(0,Object.keys(MENUS).indexOf(menu)).reduce((a,m)=>a+m.length*7+20,26)}}>
            <MenuDropdown items={MENUS[menu]} onSelect={(it)=>{
              setMenu(null);
              if (it === "Program Glitch Chip") setModal("glitch");
              else if (it === "Write Nand") setModal("confirm");
              else say({ text: it + ": not available in this recreation.", severity: "muted" });
            }} />
          </div>
        )}
      </div>

      <div style={{flex:1,display:"flex",gap:10,padding:"10px 10px 0",minHeight:0}}>
        <NandColumn state={{...state, progress}} set={set}
          onWrite={()=>setModal("confirm")} onProgram={()=>setModal("glitch")} />
        <SidePanel state={state} set={set} />
      </div>
      <StatusBar items={[{label:"XeBuild",value:"1.21"},{label:"Dashlaunch",value:"3.21"}]} />

      {(modal==="confirm"||modal==="done"||modal==="glitch") && (
        <div style={{position:"absolute",inset:0,background:"var(--jr-scrim)",display:"flex",
          alignItems:"center",justifyContent:"center",zIndex:60}}>
          {modal==="confirm" && <MessageDialog kind="yesno" title="Confirm Flash"
            message="Are you sure? Did you make sure to select the correct options and patches?"
            onYes={startFlash} onNo={()=>setModal(null)} />}
          {modal==="done" && <MessageDialog title="Flash Complete"
            message="Remember to disconnect the flasher from the computer before booting!"
            onOk={()=>setModal(null)} />}
          {modal==="glitch" && <GlitchChipDialog onClose={()=>setModal(null)} />}
        </div>
      )}
      {modal==="flashing" && <FlashOverlay value={progress} logo="../../assets/xbox-sphere.png" size={240} />}
    </div>
  );
}

Object.assign(window, { MainWindow });
