// app.jsx — root design canvas for LogAnzlyzer

function Note({ children, w = 220 }) {
  return (
    <div style={{
      width: w, padding: '10px 12px',
      background: '#fef4a8', color: '#5a4a2a',
      borderRadius: 4, fontSize: 12, lineHeight: 1.45,
      boxShadow: '0 1px 2px rgba(0,0,0,.06), 0 4px 14px rgba(0,0,0,.04)',
      fontFamily: '-apple-system, BlinkMacSystemFont, "Segoe UI", system-ui, sans-serif',
    }}>{children}</div>
  );
}

/* Wrap a screen in a fixed-size shell so the canvas can size it perfectly */
function Frame({ width, height, children }) {
  return (
    <div style={{ width, height, position: 'relative' }}>
      {children}
    </div>
  );
}

const MAIN_W = 1280, MAIN_H = 780;
const EMPTY_W = 1280, EMPTY_H = 780;

function App() {
  return (
    <DesignCanvas>
      <DCSection
        id="intro"
        title="LogAnzlyzer"
        subtitle="A Windows desktop log analyzer — original IDE-inspired aesthetic, not VS Code's trade dress. Built for WinForms / .NET 4.8. Pan + zoom this canvas; double-click any artboard to open it fullscreen."
      >
        <DCArtboard id="readme" label="README · design system" width={520} height={420}>
          <div style={{ width: '100%', height: '100%', padding: 24, fontFamily: '"Segoe UI", system-ui, sans-serif', background: '#11141a', color: '#d6dae3', borderRadius: 4, fontSize: 13, lineHeight: 1.6 }}>
            <div style={{ fontSize: 22, fontWeight: 600, color: '#f1f3f7', marginBottom: 8 }}>LogAnzlyzer · design pack</div>
            <p style={{ color: '#7a8497', marginTop: 0 }}>Six screens + a full component library, in both dark and light themes. WinForms-implementable: no gradients on chrome, no transparency tricks, all values readable from <code style={{ color: '#5b8def' }}>tokens.css</code>.</p>
            <ul style={{ paddingLeft: 18, margin: '12px 0' }}>
              <li><b>Main window</b> — VS-Code-style tab strip (closeable, draggable, "+"), graph + table split, stats sidebar with prominent P1.</li>
              <li><b>Empty state</b> — drop zone + format hint.</li>
              <li><b>Timestamp dialog</b> — auto-detected highlight with regex toggle.</li>
              <li><b>Settings dialog</b> — Appearance tab with theme + accent picker.</li>
              <li><b>About dialog</b> — version, repo, licence.</li>
              <li><b>Component library</b> — every control with every state.</li>
            </ul>
            <div style={{ marginTop: 16, padding: 10, background: '#181c24', border: '1px solid #262c36', borderRadius: 4, fontFamily: '"Cascadia Code", Consolas, monospace', fontSize: 12, color: '#7a8497' }}>
              <span style={{ color: '#59c2a8' }}>accent</span> &nbsp;#5b8def &nbsp;&nbsp; <span style={{ color: '#59c2a8' }}>P1</span> &nbsp;#ff8b6b &nbsp;&nbsp; <span style={{ color: '#59c2a8' }}>median</span> &nbsp;#59c2a8
            </div>
          </div>
        </DCArtboard>
        <DCArtboard id="note-trade-dress" label="why my palette ≠ VS Code's" width={300} height={420}>
          <Note w={'100%'}>
            The brief gave VS Code's exact hex codes (#1e1e1e / #007acc / #f48771). I can't pixel-clone proprietary trade dress, so I shipped an <b>original modern-IDE palette</b> that nails the same beats — deep neutral bg, cool blue accent, coral P1, teal median, all WinForms-implementable. Swap the values in <code>tokens.css</code> if your project legally permits the original ones.
          </Note>
        </DCArtboard>
      </DCSection>

      <DCSection
        id="main"
        title="1 · Main Window"
        subtitle="VS-Code-style chrome — closeable tab strip, graph + table split, stats sidebar. One log open: boot-sequence.log."
      >
        <DCArtboard id="main-dark" label="Dark theme" width={MAIN_W} height={MAIN_H}>
          <MainWindow theme="dark" width={MAIN_W} height={MAIN_H}/>
        </DCArtboard>
        <DCArtboard id="main-light" label="Light theme" width={MAIN_W} height={MAIN_H}>
          <MainWindow theme="light" width={MAIN_W} height={MAIN_H}/>
        </DCArtboard>
      </DCSection>

      <DCSection
        id="empty"
        title="2 · Empty State"
        subtitle="What you see before any log is loaded — large drop zone and a format hint."
      >
        <DCArtboard id="empty-dark" label="Dark theme" width={EMPTY_W} height={EMPTY_H}>
          <EmptyState theme="dark" width={EMPTY_W} height={EMPTY_H}/>
        </DCArtboard>
        <DCArtboard id="empty-light" label="Light theme" width={EMPTY_W} height={EMPTY_H}>
          <EmptyState theme="light" width={EMPTY_W} height={EMPTY_H}/>
        </DCArtboard>
      </DCSection>

      <DCSection
        id="ts-dialog"
        title="3 · Timestamp-Select dialog"
        subtitle="Opens once after a file is dropped. Auto-detected timestamp is highlighted; regex is editable for power users."
      >
        <DCArtboard id="ts-dark" label="Dark theme" width={680} height={560}>
          <div style={{ width: '100%', height: '100%', display: 'flex', alignItems: 'center', justifyContent: 'center', background: 'rgba(0,0,0,0.35)', padding: 20 }}>
            <TimestampDialog theme="dark"/>
          </div>
        </DCArtboard>
        <DCArtboard id="ts-detail" label="Adjust mode (note)" width={300} height={560}>
          <Note w={'100%'}>
            <b>Adjust state:</b> the highlighted timestamp segment becomes click-and-drag selectable inside each of the 5 sample lines — drag the left handle to move the start, the right to move the end. The regex below auto-updates in real time. Switch into this mode by clicking the secondary <i>Adjust</i> button in the footer; the <i>Use this pattern</i> button stays active throughout.
          </Note>
        </DCArtboard>
      </DCSection>

      <DCSection
        id="settings"
        title="4 · Settings dialog · Appearance tab"
        subtitle="Modal with tab list on the left. General / Cache tabs follow the same shell."
      >
        <DCArtboard id="settings-dark" label="Dark theme" width={720} height={560}>
          <div style={{ width: '100%', height: '100%', display: 'flex', alignItems: 'center', justifyContent: 'center', background: 'rgba(0,0,0,0.35)', padding: 20 }}>
            <SettingsDialog theme="dark"/>
          </div>
        </DCArtboard>
        <DCArtboard id="settings-light" label="Light theme · live re-theme preview" width={720} height={560}>
          <div style={{ width: '100%', height: '100%', display: 'flex', alignItems: 'center', justifyContent: 'center', background: 'rgba(0,0,0,0.08)', padding: 20 }}>
            <SettingsDialog theme="light"/>
          </div>
        </DCArtboard>
      </DCSection>

      <DCSection
        id="about"
        title="5 · About dialog"
        subtitle="Small modal. App identity + repo / licence / runtime metadata."
      >
        <DCArtboard id="about-dark" label="Dark theme" width={540} height={520}>
          <div style={{ width: '100%', height: '100%', display: 'flex', alignItems: 'center', justifyContent: 'center', background: 'rgba(0,0,0,0.35)', padding: 20 }}>
            <AboutDialog theme="dark"/>
          </div>
        </DCArtboard>
      </DCSection>

      <DCSection
        id="library-dark"
        title="6 · Component library — dark"
        subtitle="Every control, every state. Inspect with browser DevTools to read computed values for WinForms theming."
      >
        <DCArtboard id="lib-dark" label="All components · dark" width={1280} height={2900}>
          <ComponentSheet theme="dark" width={1280}/>
        </DCArtboard>
      </DCSection>

      <DCSection
        id="library-light"
        title="6b · Component library — light"
        subtitle="Same set, light theme. Values diverge mainly in surfaces, borders and the muted text ramp."
      >
        <DCArtboard id="lib-light" label="All components · light" width={1280} height={2900}>
          <ComponentSheet theme="light" width={1280}/>
        </DCArtboard>
      </DCSection>
    </DesignCanvas>
  );
}

ReactDOM.createRoot(document.getElementById('root')).render(<App/>);
