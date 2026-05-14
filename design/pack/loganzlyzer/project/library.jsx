// library.jsx — Component library sheet: every component with all states.
// Rendered once per theme (dark + light) on the canvas.

function ComponentSheet({ theme = 'dark', width = 1280 }) {
  return (
    <div className={"la-root theme-" + theme} style={{
      width,
      background: 'var(--la-bg)',
      color: 'var(--la-text)',
      padding: 32,
      display: 'flex', flexDirection: 'column', gap: 28,
      borderRadius: 6,
      border: '1px solid var(--la-border)',
    }}>
      <Header theme={theme}/>

      <Section title="Buttons" hint="primary / secondary / ghost / danger — default · hover · pressed · focus · disabled">
        <ButtonsBlock/>
      </Section>

      <Section title="Toggle switch" hint="off / on / disabled, with label">
        <Row>
          <Col label="off"><Toggle on={false}/></Col>
          <Col label="on"><Toggle on={true}/></Col>
          <Col label="off · disabled"><Toggle on={false} disabled/></Col>
          <Col label="on · disabled"><Toggle on={true} disabled/></Col>
          <Col label="with label"><Toggle on={true} label="Remember pattern per file"/></Col>
        </Row>
      </Section>

      <Section title="Text input" hint="default · focus · with-value · error · monospace">
        <Row>
          <Col label="placeholder"><TextInput placeholder="Search log..." icon={<Icon.Search style={{ color: 'var(--la-text-muted)' }}/>} width={210}/></Col>
          <Col label="focus"><TextInput value="evt=stall" state="focus" width={210}/></Col>
          <Col label="value"><TextInput value="C:\logs\boot.log" width={210}/></Col>
          <Col label="error"><TextInput value="bad-regex(" state="error" mono width={210}/></Col>
          <Col label="mono"><TextInput value="(\d{4}-\d{2}-\d{2})" mono width={210}/></Col>
        </Row>
      </Section>

      <Section title="Dropdown / Select" hint="placeholder · with-value · focus · open menu">
        <Row>
          <Col label="placeholder"><Select placeholder="Choose theme…"/></Col>
          <Col label="value"><Select value="yyyy-MM-dd HH:mm:ss.fff" width={220}/></Col>
          <Col label="focus"><Select value="Dark" state="focus" width={160}/></Col>
          <Col label="open">
            <div style={{ position: 'relative', width: 200 }}>
              <Select value="Dark" width={200}/>
              <div style={{
                position: 'absolute', top: 32, left: 0, width: 200,
                background: 'var(--la-panel-elev)',
                border: '1px solid var(--la-border)',
                borderRadius: 'var(--la-radius)',
                boxShadow: 'var(--la-shadow-pop)',
                padding: 4,
                zIndex: 10,
              }}>
                {[
                  { l: 'Dark', sel: true },
                  { l: 'Light', sel: false },
                  { l: 'Match system', sel: false },
                ].map(o => (
                  <div key={o.l} style={{
                    padding: '5px 10px', display: 'flex', alignItems: 'center', gap: 8,
                    background: o.sel ? 'var(--la-accent-soft)' : 'transparent',
                    color: o.sel ? 'var(--la-text-strong)' : 'var(--la-text)',
                    borderRadius: 3, fontSize: 'var(--la-fs-body)',
                  }}>
                    {o.sel ? <span style={{ color: 'var(--la-accent)' }}><Icon.Check/></span> : <span style={{ width: 11 }}/>}
                    {o.l}
                  </div>
                ))}
              </div>
            </div>
          </Col>
        </Row>
      </Section>

      <Section title="Tab strip" hint="closeable VS-Code-style tabs · default · hover · active · with dirty indicator · new-tab button">
        <div style={{ border: '1px solid var(--la-border)', borderRadius: 4, overflow: 'hidden' }}>
          <TabStrip
            activeIndex={1}
            tabs={[
              { label: 'startup-trace.log' },
              { label: 'boot-sequence.log' },
              { label: 'rpc-roundtrip.log', dirty: true },
              { label: 'cache-warm-2026-05-13.log' },
            ]}
          />
        </div>
      </Section>

      <Section title="Menu bar + Status bar" hint="native Windows convention — accelerator underline on first letter">
        <div style={{ border: '1px solid var(--la-border)', borderRadius: 4, overflow: 'hidden' }}>
          <MenuBar activeIndex={0}/>
          <div style={{ height: 60, background: 'var(--la-bg)', display: 'flex', alignItems: 'center', justifyContent: 'center', color: 'var(--la-text-faint)', fontSize: 'var(--la-fs-xs)' }}>
            window body
          </div>
          <StatusBar
            left={[
              { content: <><span style={{ color: 'var(--la-median)' }}>●</span> parsed</> },
              { content: <span className="la-mono" style={{ fontSize: 11 }}>C:\logs\boot-sequence.log</span> },
            ]}
            right={[{ content: '4,128 events' }, { content: 'pattern: yyyy-MM-dd HH:mm:ss.fff', accent: true }]}
          />
        </div>
      </Section>

      <Section title="Drop zone" hint="default · hover · drag-active (file over the window)">
        <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr 1fr', gap: 16 }}>
          <div style={{ padding: 8, border: '1px dashed var(--la-border-soft)', borderRadius: 6 }}>
            <Caption>default</Caption><DropZone state="default"/>
          </div>
          <div style={{ padding: 8, border: '1px dashed var(--la-border-soft)', borderRadius: 6 }}>
            <Caption>hover</Caption><DropZone state="hover"/>
          </div>
          <div style={{ padding: 8, border: '1px dashed var(--la-border-soft)', borderRadius: 6 }}>
            <Caption>drag-active</Caption><DropZone state="drag-active"/>
          </div>
        </div>
      </Section>

      <Section title="Stat cards" hint="small (median/P95) and hero (P1 prominent)">
        <Row>
          <div style={{ width: 280 }}><StatCard hero label="P1 — low 1% tail" value="248" unit="ms" sub="worst 1% of delays"/></div>
          <div style={{ width: 160 }}><StatCard label="Median" value="21" unit="ms" accent="median"/></div>
          <div style={{ width: 160 }}><StatCard label="P95" value="74" unit="ms"/></div>
          <div style={{ width: 160 }}><StatCard label="P99" value="156" unit="ms"/></div>
        </Row>
      </Section>

      <Section title="Sortable data table" hint="header click toggles sort · row highlight on selection · delay colour-codes by severity">
        <div style={{ border: '1px solid var(--la-border)', borderRadius: 4, overflow: 'hidden' }}>
          <LogTable height={260}/>
        </div>
      </Section>

      <Section title="Time-series chart" hint="line + area fill · median + P1 reference lines · hover tooltip">
        <div style={{ background: 'var(--la-bg)', border: '1px solid var(--la-border)', borderRadius: 4, padding: '8px 12px' }}>
          <DelayChart width={width - 64 - 26} height={240} hoverIdx={148}/>
        </div>
      </Section>

      <Section title="Modal wrapper" hint="title bar with close · body slot · footer-aligned buttons">
        <div style={{ padding: 24, background: 'var(--la-bg-elev)', border: '1px dashed var(--la-border)', borderRadius: 6, display: 'flex', justifyContent: 'center' }}>
          <Modal
            theme={theme}
            title="Modal title"
            width={420}
            footer={<>
              <Button kind="ghost">Cancel</Button>
              <Button kind="primary">Confirm</Button>
            </>}
          >
            <div style={{ padding: '18px 20px', color: 'var(--la-text)', fontSize: 'var(--la-fs-body)' }}>
              Body content goes here. The modal wrapper handles the title bar, close affordance, body slot, and footer alignment.
            </div>
          </Modal>
        </div>
      </Section>

      <Section title="Color tokens" hint="exact values for WinForms theming · click to copy in the real app">
        <ColorTokens/>
      </Section>

      <Section title="Type scale" hint="Segoe UI for UI · Cascadia / Consolas for log text">
        <TypeScale/>
      </Section>
    </div>
  );
}

/* ---------- Header banner ----------------------------- */
function Header({ theme }) {
  return (
    <div style={{ display: 'flex', alignItems: 'baseline', gap: 12, paddingBottom: 12, borderBottom: '1px solid var(--la-border)' }}>
      <Icon.Logo style={{ color: 'var(--la-accent)', width: 22, height: 22 }}/>
      <div style={{ fontSize: 22, fontWeight: 600, color: 'var(--la-text-strong)' }}>Component library</div>
      <div style={{ fontSize: 'var(--la-fs-sm)', color: 'var(--la-text-muted)' }}>
        — {theme} theme · all states for WinForms implementation
      </div>
    </div>
  );
}

/* ---------- Section + small layout helpers ------------ */
function Section({ title, hint, children }) {
  return (
    <div style={{ display: 'flex', flexDirection: 'column', gap: 12 }}>
      <div>
        <div style={{ fontSize: 'var(--la-fs-lg)', fontWeight: 600, color: 'var(--la-text-strong)' }}>{title}</div>
        {hint && <div style={{ fontSize: 'var(--la-fs-sm)', color: 'var(--la-text-muted)' }}>{hint}</div>}
      </div>
      <div>{children}</div>
    </div>
  );
}

function Row({ children }) { return <div style={{ display: 'flex', flexWrap: 'wrap', gap: 28, alignItems: 'flex-end' }}>{children}</div>; }
function Col({ label, children }) {
  return (
    <div style={{ display: 'flex', flexDirection: 'column', gap: 6 }}>
      <Caption>{label}</Caption>
      <div>{children}</div>
    </div>
  );
}
function Caption({ children }) {
  return <div style={{ fontSize: 'var(--la-fs-xs)', textTransform: 'uppercase', letterSpacing: '.06em', color: 'var(--la-text-muted)' }}>{children}</div>;
}

/* ---------- Buttons grid ------------------------------ */
function ButtonsBlock() {
  const states = ['default', 'hover', 'pressed', 'focus', 'disabled'];
  const kinds = ['primary', 'secondary', 'ghost', 'danger'];
  return (
    <div style={{
      display: 'grid',
      gridTemplateColumns: '110px repeat(5, 1fr)',
      gap: 12,
      alignItems: 'center',
    }}>
      <div/>
      {states.map(s => <Caption key={s}>{s}</Caption>)}
      {kinds.map(k => (
        <React.Fragment key={k}>
          <Caption>{k}</Caption>
          {states.map(s => (
            <div key={k + s}>
              <Button kind={k} state={s}>
                {k === 'danger' ? 'Clear cache' : 'Open log'}
              </Button>
            </div>
          ))}
        </React.Fragment>
      ))}
    </div>
  );
}

/* ---------- Color tokens grid ------------------------- */
function ColorTokens() {
  const groups = [
    {
      title: 'Surfaces',
      tokens: [
        ['--la-bg', 'background'],
        ['--la-bg-elev', 'bg elevated'],
        ['--la-panel', 'panel'],
        ['--la-panel-elev', 'panel elevated'],
        ['--la-tabstrip', 'tab strip'],
        ['--la-tab-active', 'active tab'],
        ['--la-border', 'border'],
        ['--la-border-soft', 'border soft'],
      ],
    },
    {
      title: 'Text',
      tokens: [
        ['--la-text-strong', 'text strong'],
        ['--la-text', 'text primary'],
        ['--la-text-muted', 'text muted'],
        ['--la-text-faint', 'text faint'],
      ],
    },
    {
      title: 'Accent + semantic',
      tokens: [
        ['--la-accent', 'accent (primary)'],
        ['--la-accent-hover', 'accent hover'],
        ['--la-accent-press', 'accent pressed'],
        ['--la-p1', 'P1 / error'],
        ['--la-median', 'median / success'],
        ['--la-warning', 'warning'],
      ],
    },
    {
      title: 'Chrome',
      tokens: [
        ['--la-titlebar', 'title bar'],
        ['--la-menubar', 'menu bar'],
        ['--la-statusbar', 'status bar'],
        ['--la-statusbar-fg', 'status bar fg'],
      ],
    },
  ];
  return (
    <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: 18 }}>
      {groups.map(g => (
        <div key={g.title}>
          <Caption>{g.title}</Caption>
          <div style={{ marginTop: 6, border: '1px solid var(--la-border)', borderRadius: 4, overflow: 'hidden' }}>
            {g.tokens.map(([tok, label], i) => (
              <div key={tok} style={{
                display: 'flex', alignItems: 'center', gap: 10,
                padding: '6px 10px',
                borderBottom: i < g.tokens.length - 1 ? '1px solid var(--la-border-soft)' : 'none',
                background: 'var(--la-panel-elev)',
                fontSize: 'var(--la-fs-sm)',
              }}>
                <div style={{
                  width: 22, height: 22, borderRadius: 3,
                  background: `var(${tok})`,
                  border: '1px solid var(--la-border)',
                  boxShadow: 'inset 0 0 0 1px rgba(255,255,255,0.04)',
                  flex: '0 0 auto',
                }}/>
                <div style={{ flex: 1, color: 'var(--la-text)' }}>{label}</div>
                <code style={{ fontFamily: 'var(--la-font-mono)', fontSize: 'var(--la-fs-xs)', color: 'var(--la-text-muted)' }}>{tok}</code>
              </div>
            ))}
          </div>
        </div>
      ))}
    </div>
  );
}

/* ---------- Type scale ------------------------------- */
function TypeScale() {
  const rows = [
    { label: 'Heading', size: 'var(--la-fs-xl)', weight: 600, mono: false, sample: 'P1 — low 1% tail' },
    { label: 'Body',    size: 'var(--la-fs-body)', weight: 400, mono: false, sample: 'Drag a log file here, or click to browse' },
    { label: 'Small',   size: 'var(--la-fs-sm)',  weight: 400, mono: false, sample: 'Confirm timestamp position' },
    { label: 'Muted',   size: 'var(--la-fs-xs)',  weight: 400, mono: false, sample: '4,128 events · UTF-8' },
    { label: 'Mono · log line', size: 'var(--la-fs-sm)', weight: 400, mono: true, sample: '2026-05-13 10:40:19.085  evt=tick scope=worker' },
    { label: 'Stat',    size: 'var(--la-fs-stat)', weight: 500, mono: true, sample: '248 ms' },
    { label: 'Stat · hero', size: 'var(--la-fs-stat-hero)', weight: 600, mono: true, sample: '248 ms' },
  ];
  return (
    <div style={{ border: '1px solid var(--la-border)', borderRadius: 4, overflow: 'hidden' }}>
      {rows.map((r, i) => (
        <div key={r.label} style={{
          display: 'flex', alignItems: 'center', gap: 20,
          padding: '12px 14px',
          borderBottom: i < rows.length - 1 ? '1px solid var(--la-border-soft)' : 'none',
          background: 'var(--la-panel-elev)',
        }}>
          <div style={{ width: 130 }}>
            <Caption>{r.label}</Caption>
            <div style={{ fontSize: 'var(--la-fs-xs)', color: 'var(--la-text-faint)', fontFamily: 'var(--la-font-mono)', marginTop: 2 }}>{r.size.replace('var(--la-fs-', '').replace(')', '')}</div>
          </div>
          <div style={{
            fontFamily: r.mono ? 'var(--la-font-mono)' : 'var(--la-font-ui)',
            fontSize: r.size,
            fontWeight: r.weight,
            color: 'var(--la-text-strong)',
            lineHeight: 1.2,
          }}>{r.sample}</div>
        </div>
      ))}
    </div>
  );
}

window.ComponentSheet = ComponentSheet;
