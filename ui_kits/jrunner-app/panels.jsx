const { Button, SplitButton, TextField, Select, NumberField, Checkbox, Radio, GroupBox, Tabs, DeviceCard, ProgressBar, LogConsole, DataTable } = window.JRunnerPremiumDesignSystem_5a2f40;

const CB_TYPES = ["Retail","Glitch","Glitch2","Glitch2m","JTAG","DEVGL"];

function NandColumn({ state, set, onWrite, onProgram }) {
  return (
    <div style={{display:"flex",flexDirection:"column",gap:8,width:470}}>
      <div style={{display:"flex",gap:8,alignItems:"flex-start"}}>
        <GroupBox title="Nand" style={{flex:1}} bodyStyle={{display:"flex",gap:6}}>
          {["Read Nand","Create ECC","Write ECC","Create XeBuild Image"].map(t=>
            <Button key={t} style={{width:72,height:52,whiteSpace:"normal",lineHeight:1.25}}>{t}</Button>)}
          <Button style={{width:72,height:52,whiteSpace:"normal",lineHeight:1.25}} onClick={onWrite}>Write Nand</Button>
        </GroupBox>
        <GroupBox title="Glitch Chip" style={{width:96}} bodyStyle={{display:"flex"}}>
          <Button style={{width:74,height:52,whiteSpace:"normal",lineHeight:1.25}} onClick={onProgram}>Program Timing File</Button>
        </GroupBox>
      </div>

      <div style={{display:"flex",gap:8,alignItems:"flex-start"}}>
        <div style={{display:"flex",flexDirection:"column",gap:8,width:250}}>
          <div style={{display:"flex",gap:8,alignItems:"flex-start"}}>
            <GroupBox title="Nand Reads" style={{width:66}} bodyStyle={{padding:"8px 8px 10px"}}>
              <NumberField value={state.reads} onChange={(v)=>set({reads:v})} width={44} />
            </GroupBox>
            <GroupBox title="Glitch Chip Programming" style={{flex:1}} bodyStyle={{display:"flex",flexDirection:"column",gap:6}}>
              <Radio checked={state.iface==="USB"} onChange={()=>set({iface:"USB"})} label="USB" />
              <Radio checked={state.iface==="LPT"} onChange={()=>set({iface:"LPT"})} label="LPT" />
            </GroupBox>
          </div>
          <div style={{display:"grid",gridTemplateColumns:"repeat(4,1fr)",gap:6}}>
            {["CPU Key Database","Create Donor","Extract Files","Patch Keyvault"].map(t=>
              <Button key={t} style={{height:46,whiteSpace:"normal",lineHeight:1.25,padding:"0 4px"}}>{t}</Button>)}
          </div>
        </div>
        <DeviceCard style={{flex:1}} height={112} image={state.flasher} name={state.flasher?"PicoFlasher":undefined} detail={state.flasher?"RP2040 · COM4":undefined} />
      </div>

      <div style={{display:"flex",gap:8,alignItems:"flex-start"}}>
        <div style={{flex:1,display:"flex",flexDirection:"column",gap:6}}>
          <div style={{display:"flex",gap:6}}><Button style={{width:88}}>Load Source</Button><TextField /></div>
          <div style={{display:"flex",gap:6}}><Button style={{width:88}}>Load Extra</Button><TextField /></div>
          <div style={{display:"flex",gap:6,alignItems:"center"}}>
            <span style={{width:88,fontSize:12,paddingLeft:4}}>CPU Key:</span>
            <TextField mono value={state.cpuKey} onChange={(v)=>set({cpuKey:v})} />
          </div>
        </div>
        <div style={{display:"flex",flexDirection:"column",gap:6,width:78}}>
          <Button block style={{height:46,whiteSpace:"normal",lineHeight:1.25}}>Nand Compare</Button>
          <Button block>Reload</Button>
        </div>
      </div>

      <div style={{display:"flex",gap:8,alignItems:"center"}}>
        <span style={{width:56,fontSize:12,color:"var(--jr-text-primary)"}}>Progress</span>
        <ProgressBar value={state.progress} style={{flex:1}} />
        <TextField width={70} readOnly value={state.eta} />
      </div>

      <LogConsole height={186} lines={state.log} />
    </div>
  );
}

function XeBuildPanel({ state, set }) {
  return (
    <GroupBox title="XeBuild" bodyStyle={{padding:"6px 8px 10px"}}>
      <Tabs tabs={["XeBuild","XB Settings","Patches","Dashlaunch"]} value={state.xbTab} onChange={(t)=>set({xbTab:t})}
        bodyStyle={{padding:10,minHeight:104,borderTop:"none"}}>
        <div style={{display:"flex",gap:12}}>
          <div style={{flex:1,display:"flex",flexDirection:"column",gap:10}}>
            <div>
              <div style={{fontSize:12,marginBottom:4}}>Kernel Version</div>
              <Select width={92} value={state.kernel} options={["17559","17544","16767","14719"]} onChange={(v)=>set({kernel:v})} placeholder="————" />
            </div>
            <div>
              <div style={{fontSize:12,marginBottom:4}}>Console Type</div>
              <div style={{display:"flex",gap:6,alignItems:"center"}}>
                <Select width={104} value={state.console} options={["Falcon","Jasper","Trinity","Corona","Corona 4GB"]} onChange={(v)=>set({console:v})} />
                <Button size="sm" style={{width:22,padding:0}}>?</Button>
              </div>
            </div>
          </div>
          <div style={{width:120,display:"flex",flexDirection:"column",gap:5}}>
            {CB_TYPES.map(t=>
              <Radio key={t} disabled={!state.console} checked={state.cb===t} onChange={()=>set({cb:t})} label={t} />)}
          </div>
        </div>
      </Tabs>
    </GroupBox>
  );
}

const NAND_FIELDS = [
  ["Console","CB Type"],["2BL [CB_A]",null],["2BL [CB_B]",null],["4BL [CD]",null],["5BL [CE]",null],
  ["6BL [CF] Patch 0","6BL [CF] Patch 1"],["7BL [CG] Patch 0","7BL [CG] Patch 1"],
];

function NandInfoPanel({ state, set }) {
  return (
    <Tabs tabs={["Nand Info","KV Info","Bad Blocks"]} value={state.infoTab} onChange={(t)=>set({infoTab:t})}
      bodyStyle={{padding:"12px 10px",minHeight:236}}>
      {state.infoTab==="Nand Info" && (
        <div style={{display:"flex",flexDirection:"column",gap:8}}>
          {NAND_FIELDS.map(([a,b],i)=>(
            <div key={i} style={{display:"grid",gridTemplateColumns:"1fr 1fr",gap:10}}>
              <TextField label={a} labelWidth={92} value={state.nand[a]||""} readOnly />
              {b ? <TextField label={b} labelWidth={92} value={state.nand[b]||""} readOnly /> : <span/>}
            </div>
          ))}
          <div style={{display:"grid",gridTemplateColumns:"1fr 1fr",gap:10}}>
            <div style={{display:"flex",gap:6,alignItems:"center",justifyContent:"flex-end"}}>
              <span style={{fontSize:12}}>LDV</span><TextField width={30} readOnly /><span style={{fontSize:12}}>PD</span><TextField width={78} readOnly />
            </div>
            <div style={{display:"flex",gap:6,alignItems:"center",justifyContent:"flex-end"}}>
              <span style={{fontSize:12}}>LDV</span><TextField width={30} readOnly /><span style={{fontSize:12}}>PD</span><TextField width={78} readOnly />
            </div>
          </div>
        </div>
      )}
      {state.infoTab==="KV Info" && (
        <div style={{display:"flex",flexDirection:"column",gap:8}}>
          {["Console ID","Console Serial","Console Type","Manufacturing Date","Game Region","DVD Key"].map(l=>
            <TextField key={l} label={l} labelWidth={116} readOnly value={state.kv[l]||""} mono={l==="DVD Key"} />)}
        </div>
      )}
      {state.infoTab==="Bad Blocks" && (
        <DataTable columns={[{key:"block",label:"Block",mono:true},{key:"addr",label:"Address",mono:true},{key:"kind",label:"Type"}]}
          rows={state.badBlocks} />
      )}
    </Tabs>
  );
}

function SidePanel({ state, set }) {
  return (
    <div style={{display:"flex",flexDirection:"column",gap:8,flex:1}}>
      <XeBuildPanel state={state} set={set} />
      <NandInfoPanel state={state} set={set} />
      <SplitButton onClick={()=>{}} onOpen={()=>{}} style={{alignSelf:"stretch"}}>Show Working Folder</SplitButton>
      <div style={{display:"grid",gridTemplateColumns:"1fr 1fr",gap:8}}>
        <div style={{display:"flex",flexDirection:"column",gap:6}}>
          <Button block>New Session</Button>
          <Button block>Restart</Button>
        </div>
        <div style={{display:"flex",flexDirection:"column",gap:6}}>
          <Button block>Settings</Button>
          <Button block>Exit</Button>
        </div>
      </div>
      <GroupBox bodyStyle={{display:"flex",flexDirection:"column",gap:6,padding:10}}>
        <TextField label="IP:" labelWidth={20} value={state.ip} onChange={(v)=>set({ip:v})} />
        <Button block>Get CPU Key</Button>
        <Button block>Scan IP</Button>
      </GroupBox>
    </div>
  );
}

Object.assign(window, { NandColumn, XeBuildPanel, NandInfoPanel, SidePanel });
