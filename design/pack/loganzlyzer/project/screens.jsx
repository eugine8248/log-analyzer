// screens.jsx — Main Window, Empty State, and dialog screens.

/* ---------- StatsSidebar -------------------------------- */
function StatsSidebar() {
  return (
    <div style={{
      width: 240, flex: '0 0 240px',
      background: 'var(--la-panel)',
      borderLeft: '1px solid var(--la-border)',
      padding: '14px 14px',
      display: 'flex', flexDirection: 'column', gap: 10,
      overflow: 'auto',
    }} className="la-scroll">
      <div style={{ fontSize: 'var(--la-fs-xs)', textTransform: 'uppercase', letterSpacing: '.08em', color: 'var(--la-text-muted)', marginBottom: 2 }}>
        Stats
      </div>

      <StatCard hero label="P1 — low 1% tail" value="248" unit="ms" sub="worst 1% of delays — performance drops here"/>

      <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: 6, marginTop: 4 }}>
        <StatCard label="Median" value="21" unit="ms" accent="median"/>
        <StatCard label="P95" value="74" unit="ms"/>
        <StatCard label="P99" value="156" unit="ms"/>
        <StatCard label="Events" value="4,128"/>
      </div>

      <div style={{ height: 1, background: 'var(--la-border-soft)', margin: '6px 0' }}/>

      <div style={{ fontSize: 'var(--la-fs-xs)', textTransform: 'uppercase', letterSpacing: '.08em', color: 'var(--la-text-muted)' }}>
        Span
      </div>
      <div style={{ fontFamily: 'var(--la-font-mono)', fontSize: 'var(--la-fs-sm)', color: 'var(--la-text)', lineHeight: 1.6 }}>
        <div>start &nbsp; 10:40:19.085</div>
        <div>end &nbsp;&nbsp;&nbsp; 10:42:47.302</div>
        <div style={{ color: 'var(--la-text-muted)' }}>2 min 28 sec</div>
      </div>

      <div style={{ height: 1, background: 'var(--la-border-soft)', margin: '6px 0' }}/>

      <div style={{ fontSize: 'var(--la-fs-xs)', textTransform: 'uppercase', letterSpacing: '.08em', color: 'var(--la-text-muted)' }}>
        Distribution
      </div>
      {/* mini histogram */}
      <svg width="100%" height="64" viewBox="0 0 220 64" preserveAspectRatio="none" style={{ display: 'block' }}>
        {Array.from({ length: 28 }).map((_, i) => {
          const heights = [4,8,18,32,46,52,40,28,20,14,10,8,7,6,6,5,5,4,4,3,3,3,3,4,5,7,9,14];
          const h = heights[i];
          const x = 4 + i * 7.7;
          return <rect key={i} x={x} y={62 - h} width="6" height={h} fill={i >= 25 ? 'var(--la-p1)' : (i === 0 ? 'var(--la-median)' : 'var(--la-accent)')} opacity={i >= 25 ? 0.9 : 0.65}/>;
        })}
        <line x1="0" y1="63" x2="220" y2="63" stroke="var(--la-border)" strokeWidth="1"/>
      </svg>
      <div style={{ display: 'flex', justifyContent: 'space-between', fontFamily: 'var(--la-font-mono)', fontSize: 10, color: 'var(--la-text-muted)' }}>
        <span>0</span><span>50</span><span>100</span><span>200+</span>
      </div>
    </div>
  );
}

/* ---------- Chart toolbar (zoom / pan controls) -------- */
function ChartToolbar() {
  return (
    <div style={{
      display: 'flex', alignItems: 'center', gap: 8,
      padding: '8px 12px', height: 36,
      borderBottom: '1px solid var(--la-border-soft)',
      background: 'var(--la-panel)',
    }}>
      <div style={{ fontSize: 'var(--la-fs-sm)', color: 'var(--la-text-strong)', fontWeight: 600 }}>Delay over time</div>
      <span style={{ color: 'var(--la-text-muted)', fontSize: 'var(--la-fs-xs)', fontFamily: 'var(--la-font-mono)' }}>· y-axis: ms · x-axis: timestamp</span>
      <div style={{ flex: 1 }}/>
      {/* range chips */}
      <div style={{ display: 'flex', border: '1px solid var(--la-border)', borderRadius: 4, overflow: 'hidden' }}>
        {['10s', '30s', '1m', 'all'].map((r, i) => (
          <div key={r} style={{
            padding: '3px 9px', fontSize: 'var(--la-fs-xs)',
            background: i === 3 ? 'var(--la-accent-soft)' : 'transparent',
            color: i === 3 ? 'var(--la-accent)' : 'var(--la-text-muted)',
            borderRight: i < 3 ? '1px solid var(--la-border)' : 'none',
            fontFamily: 'var(--la-font-mono)',
          }}>{r}</div>
        ))}
      </div>
      <div style={{ display: 'flex', gap: 2 }}>
        {['−','+','⤢'].map((c, i) => (
          <div key={i} style={{
            width: 24, height: 24, display: 'flex', alignItems: 'center', justifyContent: 'center',
            border: '1px solid var(--la-border)', borderRadius: 4,
            background: 'var(--la-panel-elev)', color: 'var(--la-text-muted)',
            fontSize: 13,
          }}>{c}</div>
        ))}
      </div>
    </div>
  );
}

/* ---------- Main Window screen ------------------------- */
function MainWindow({ theme = 'dark', width = 1280, height = 780 }) {
  return (
    <WindowFrame theme={theme} width={width} height={height} title="LogAnzlyzer — boot-sequence.log">
      <MenuBar/>
      <TabStrip
        activeIndex={1}
        tabs={[
          { label: 'startup-trace.log' },
          { label: 'boot-sequence.log' },
          { label: 'cache-warm-2026-05-13.log' },
          { label: 'rpc-roundtrip.log', dirty: true },
        ]}
      />
      <div style={{ flex: 1, display: 'flex', minHeight: 0 }}>
        {/* left: graph + table */}
        <div style={{ flex: 1, display: 'flex', flexDirection: 'column', minWidth: 0 }}>
          <ChartToolbar/>
          <div style={{ background: 'var(--la-bg)', padding: '8px 12px 4px' }}>
            <DelayChart width={width - 240 - 24} height={Math.max(220, Math.floor((height - 200) * 0.5))} hoverIdx={148}/>
          </div>
          <LogTable height={Math.max(180, height - 32 - 28 - 34 - 36 - Math.max(220, Math.floor((height - 200) * 0.5)) - 12 - 22)}/>
        </div>
        {/* right: stats */}
        <StatsSidebar/>
      </div>
      <StatusBar
        left={[
          { content: <><span style={{ color: 'var(--la-median)' }}>●</span> parsed</> },
          { content: <span className="la-mono" style={{ fontSize: 11 }}>C:\logs\boot-sequence.log</span> },
        ]}
        right={[
          { content: '4,128 events' },
          { content: 'UTF-8' },
          { content: 'pattern: yyyy-MM-dd HH:mm:ss.fff', accent: true },
        ]}
      />
    </WindowFrame>
  );
}

/* ---------- Empty State ------------------------------- */
function EmptyState({ theme = 'dark', width = 1280, height = 780 }) {
  return (
    <WindowFrame theme={theme} width={width} height={height} title="LogAnzlyzer">
      <MenuBar/>
      <div style={{
        height: 34, background: 'var(--la-tabstrip)',
        borderBottom: '1px solid var(--la-border)',
        display: 'flex', alignItems: 'center',
        paddingLeft: 12, color: 'var(--la-text-muted)', fontSize: 'var(--la-fs-xs)',
      }}>
        <Icon.Plus/>
        <span style={{ marginLeft: 8 }}>No log files open. Drop one below to begin.</span>
      </div>
      <div style={{ flex: 1, display: 'flex', alignItems: 'center', justifyContent: 'center', padding: 40, background: 'var(--la-bg)' }}>
        <div style={{ width: 560, display: 'flex', flexDirection: 'column', alignItems: 'center', gap: 28 }}>
          <div style={{ textAlign: 'center' }}>
            <div style={{ fontSize: 22, fontWeight: 600, color: 'var(--la-text-strong)', letterSpacing: '-0.01em' }}>
              Open a log to inspect its tail latency
            </div>
            <div style={{ fontSize: 'var(--la-fs-body)', color: 'var(--la-text-muted)', marginTop: 6 }}>
              LogAnzlyzer parses any timestamped log and surfaces the P1 (low&nbsp;1%) where stability slips.
            </div>
          </div>
          <DropZone/>
          {/* sample format */}
          <div style={{ width: '100%' }}>
            <div style={{ fontSize: 'var(--la-fs-xs)', textTransform: 'uppercase', letterSpacing: '.08em', color: 'var(--la-text-muted)', marginBottom: 6 }}>
              Expected format
            </div>
            <div style={{
              background: 'var(--la-panel-elev)',
              border: '1px solid var(--la-border)',
              borderRadius: 6,
              padding: '10px 14px',
              fontFamily: 'var(--la-font-mono)', fontSize: 'var(--la-fs-sm)',
              color: 'var(--la-text)',
              lineHeight: 1.7,
            }}>
              <div><span style={{ color: 'var(--la-accent)', background: 'var(--la-accent-soft)', borderRadius: 2, padding: '1px 2px' }}>2026-05-13 10:40:19.085</span> &nbsp; worker booted, pool=8</div>
              <div><span style={{ color: 'var(--la-accent)', background: 'var(--la-accent-soft)', borderRadius: 2, padding: '1px 2px' }}>2026-05-13 10:40:19.103</span> &nbsp; cache.warm started</div>
              <div><span style={{ color: 'var(--la-accent)', background: 'var(--la-accent-soft)', borderRadius: 2, padding: '1px 2px' }}>2026-05-13 10:40:19.221</span> &nbsp; rpc.bind localhost:7211</div>
            </div>
            <div style={{ fontSize: 'var(--la-fs-xs)', color: 'var(--la-text-muted)', marginTop: 6, textAlign: 'center' }}>
              Other formats work too — you'll confirm the timestamp position after opening.
            </div>
          </div>
        </div>
      </div>
      <StatusBar
        left={[{ content: 'Ready' }]}
        right={[{ content: 'no file loaded' }]}
      />
    </WindowFrame>
  );
}

/* ---------- Timestamp-Select dialog ------------------ */
function TimestampDialog({ theme = 'dark' }) {
  const lines = [
    '2026-05-13 10:40:19.085 worker booted, pool=8',
    '2026-05-13 10:40:19.103 cache.warm started target=hot',
    '2026-05-13 10:40:19.221 rpc.bind localhost:7211 ok',
    '2026-05-13 10:40:19.412 evt=tick scope=worker id=001 ok',
    '2026-05-13 10:40:19.430 evt=tick scope=worker id=002 ok',
  ];
  const TS_LEN = 23; // "2026-05-13 10:40:19.085"
  return (
    <Modal
      title="Confirm timestamp position"
      width={620}
      theme={theme}
      footer={<>
        <Button kind="ghost">Cancel</Button>
        <Button kind="secondary">Adjust</Button>
        <Button kind="primary" icon={<Icon.Check/>}>Use this pattern</Button>
      </>}
    >
      <div style={{ padding: '16px 18px 8px' }}>
        <div style={{ fontSize: 'var(--la-fs-body)', color: 'var(--la-text)', marginBottom: 4 }}>
          We auto-detected this timestamp. Confirm it matches every line.
        </div>
        <div style={{ fontSize: 'var(--la-fs-xs)', color: 'var(--la-text-muted)' }}>
          File &nbsp;<span style={{ fontFamily: 'var(--la-font-mono)' }}>boot-sequence.log</span>&nbsp; · &nbsp;4,128 lines · &nbsp;showing first 5
        </div>
      </div>

      <div style={{
        margin: '0 18px',
        background: 'var(--la-input-bg)',
        border: '1px solid var(--la-border)',
        borderRadius: 6,
        fontFamily: 'var(--la-font-mono)', fontSize: 'var(--la-fs-sm)',
        color: 'var(--la-text)',
        overflow: 'hidden',
      }}>
        {lines.map((ln, i) => (
          <div key={i} style={{
            display: 'flex', padding: '5px 10px',
            borderBottom: i < lines.length - 1 ? '1px solid var(--la-border-soft)' : 'none',
          }}>
            <span style={{ width: 22, color: 'var(--la-text-faint)', textAlign: 'right', paddingRight: 10, userSelect: 'none' }}>{i + 1}</span>
            <span style={{
              background: 'var(--la-accent-soft)',
              color: 'var(--la-accent)',
              borderRadius: 2,
              padding: '1px 3px',
              boxShadow: 'inset 0 0 0 1px var(--la-accent)',
            }}>{ln.slice(0, TS_LEN)}</span>
            <span style={{ color: 'var(--la-text)' }}>{ln.slice(TS_LEN)}</span>
          </div>
        ))}
      </div>

      {/* legend */}
      <div style={{ display: 'flex', alignItems: 'center', gap: 14, padding: '10px 18px 4px', fontSize: 'var(--la-fs-xs)', color: 'var(--la-text-muted)' }}>
        <span style={{ display: 'inline-flex', alignItems: 'center', gap: 6 }}>
          <span style={{ width: 10, height: 10, background: 'var(--la-accent-soft)', border: '1px solid var(--la-accent)', borderRadius: 2 }}/>
          detected timestamp
        </span>
        <span style={{ color: 'var(--la-text-faint)' }}>·</span>
        <span>Click <span style={{ color: 'var(--la-text)' }}>Adjust</span> to drag-select the bounds yourself.</span>
      </div>

      {/* regex toggle */}
      <div style={{ padding: '12px 18px 16px' }}>
        <div style={{
          display: 'flex', alignItems: 'center', gap: 10,
          marginBottom: 8,
        }}>
          <Toggle on={true}/>
          <span style={{ fontSize: 'var(--la-fs-body)' }}>Show regex</span>
          <span style={{ fontSize: 'var(--la-fs-xs)', color: 'var(--la-text-muted)' }}>· power-user override</span>
        </div>
        <div style={{
          background: 'var(--la-input-bg)',
          border: '1px solid var(--la-input-border)',
          borderRadius: 4,
          padding: '8px 10px',
          fontFamily: 'var(--la-font-mono)', fontSize: 'var(--la-fs-sm)',
          color: 'var(--la-text)',
          display: 'flex', alignItems: 'center', gap: 8,
        }}>
          <span style={{ color: 'var(--la-text-faint)' }}>^</span>
          <span style={{ color: 'var(--la-accent)' }}>(\d&#123;4&#125;-\d&#123;2&#125;-\d&#123;2&#125; \d&#123;2&#125;:\d&#123;2&#125;:\d&#123;2&#125;\.\d&#123;3&#125;)</span>
          <span style={{ color: 'var(--la-text-faint)' }}>\s+(.*)$</span>
          <div style={{ flex: 1 }}/>
          <span style={{ fontSize: 'var(--la-fs-xs)', color: 'var(--la-median)', display: 'flex', alignItems: 'center', gap: 4 }}>
            <Icon.Check/> matches 5/5
          </span>
        </div>
      </div>
    </Modal>
  );
}

/* ---------- Settings dialog --------------------------- */
function SettingsDialog({ theme = 'dark' }) {
  const tabs = ['General', 'Appearance', 'Cache'];
  return (
    <Modal
      title="Settings"
      width={640}
      theme={theme}
      footer={<>
        <Button kind="ghost">Cancel</Button>
        <Button kind="primary">Apply</Button>
      </>}
    >
      <div style={{ display: 'flex', minHeight: 360 }}>
        {/* tabs */}
        <div style={{ width: 140, background: 'var(--la-panel)', borderRight: '1px solid var(--la-border-soft)', padding: '10px 0' }}>
          {tabs.map((t, i) => (
            <div key={t} style={{
              padding: '8px 16px',
              borderLeft: i === 1 ? '3px solid var(--la-accent)' : '3px solid transparent',
              background: i === 1 ? 'var(--la-row-selected)' : 'transparent',
              color: i === 1 ? 'var(--la-text-strong)' : 'var(--la-text)',
              fontSize: 'var(--la-fs-body)',
              marginLeft: 0,
            }}>{t}</div>
          ))}
        </div>
        {/* body */}
        <div style={{ flex: 1, padding: '18px 22px', display: 'flex', flexDirection: 'column', gap: 18 }}>
          <div>
            <SectionLabel>Theme</SectionLabel>
            <div style={{ display: 'flex', gap: 10, marginTop: 8 }}>
              {[
                { id: 'dark',  label: 'Dark',  bg: '#11141a', fg: '#d6dae3', active: theme === 'dark' },
                { id: 'light', label: 'Light', bg: '#fbfbfd', fg: '#20242b', active: theme === 'light' },
                { id: 'auto',  label: 'Match system', auto: true, active: false },
              ].map(o => (
                <div key={o.id} style={{
                  width: 130, padding: 8,
                  border: '1.5px solid ' + (o.active ? 'var(--la-accent)' : 'var(--la-border)'),
                  borderRadius: 6,
                  background: 'var(--la-panel-elev)',
                  position: 'relative',
                }}>
                  <div style={{
                    height: 56,
                    background: o.auto ? 'linear-gradient(90deg, #11141a 0%, #11141a 50%, #fbfbfd 50%, #fbfbfd 100%)' : o.bg,
                    border: '1px solid var(--la-border-soft)',
                    borderRadius: 3,
                    position: 'relative',
                  }}>
                    <div style={{ height: 8, background: 'rgba(255,255,255,0.04)', borderBottom: '1px solid rgba(0,0,0,0.2)' }}/>
                    <div style={{ position: 'absolute', top: 14, left: 6, width: 10, height: 4, background: o.auto ? '#5b8def' : (o.id === 'dark' ? '#5b8def' : '#3461d6'), borderRadius: 1 }}/>
                    <div style={{ position: 'absolute', top: 22, left: 6, width: 24, height: 2, background: o.fg, opacity: 0.6, borderRadius: 1 }}/>
                    <div style={{ position: 'absolute', top: 28, left: 6, width: 18, height: 2, background: o.fg, opacity: 0.4, borderRadius: 1 }}/>
                  </div>
                  <div style={{ marginTop: 8, fontSize: 'var(--la-fs-sm)', color: 'var(--la-text)', display: 'flex', alignItems: 'center', justifyContent: 'space-between' }}>
                    {o.label}
                    {o.active && <span style={{ color: 'var(--la-accent)' }}><Icon.Check/></span>}
                  </div>
                </div>
              ))}
            </div>
          </div>

          <div style={{ height: 1, background: 'var(--la-border-soft)' }}/>

          <div>
            <SectionLabel>Accent color</SectionLabel>
            <div style={{ display: 'flex', gap: 8, marginTop: 8 }}>
              {['#5b8def', '#7a6cf0', '#59c2a8', '#e8a86a', '#ff8b6b', '#9aa3b7'].map((c, i) => (
                <div key={c} style={{
                  width: 26, height: 26, borderRadius: '50%',
                  background: c,
                  border: '2px solid ' + (i === 0 ? 'var(--la-text-strong)' : 'var(--la-border)'),
                  boxShadow: i === 0 ? '0 0 0 2px var(--la-bg) inset' : 'none',
                  position: 'relative',
                }}>
                  {i === 0 && <span style={{ position: 'absolute', inset: 0, display: 'flex', alignItems: 'center', justifyContent: 'center', color: '#fff' }}><Icon.Check/></span>}
                </div>
              ))}
            </div>
            <div style={{ fontSize: 'var(--la-fs-xs)', color: 'var(--la-text-muted)', marginTop: 6 }}>
              Used for graph line, selection, and links.
            </div>
          </div>

          <div style={{ height: 1, background: 'var(--la-border-soft)' }}/>

          <div style={{ display: 'flex', flexDirection: 'column', gap: 12 }}>
            <SettingRow label="Show stats sidebar" hint="Right panel with P1, median, P95/P99 values">
              <Toggle on={true}/>
            </SettingRow>
            <SettingRow label="Highlight P1 events in table" hint="Tail-latency rows get coral marker + bold delay">
              <Toggle on={true}/>
            </SettingRow>
            <SettingRow label="Datetime format" hint="Used in tooltips and the status bar">
              <Select value="yyyy-MM-dd HH:mm:ss.fff" width={220}/>
            </SettingRow>
          </div>
        </div>
      </div>
    </Modal>
  );
}

function SectionLabel({ children }) {
  return (
    <div style={{
      fontSize: 'var(--la-fs-xs)', textTransform: 'uppercase',
      letterSpacing: '.08em', color: 'var(--la-text-muted)',
      marginBottom: 2,
    }}>{children}</div>
  );
}

function SettingRow({ label, hint, children }) {
  return (
    <div style={{ display: 'flex', alignItems: 'center', gap: 16 }}>
      <div style={{ flex: 1 }}>
        <div style={{ fontSize: 'var(--la-fs-body)', color: 'var(--la-text)' }}>{label}</div>
        {hint && <div style={{ fontSize: 'var(--la-fs-xs)', color: 'var(--la-text-muted)' }}>{hint}</div>}
      </div>
      <div style={{ flex: '0 0 auto' }}>{children}</div>
    </div>
  );
}

/* ---------- About dialog ------------------------------ */
function AboutDialog({ theme = 'dark' }) {
  return (
    <Modal
      title="About LogAnzlyzer"
      width={460}
      theme={theme}
      footer={<>
        <Button kind="ghost">View licence</Button>
        <Button kind="primary">Close</Button>
      </>}
    >
      <div style={{ padding: '24px 24px 18px', display: 'flex', flexDirection: 'column', alignItems: 'center', textAlign: 'center', gap: 14 }}>
        <div style={{
          width: 56, height: 56, borderRadius: 12,
          background: 'var(--la-accent-soft)',
          color: 'var(--la-accent)',
          display: 'flex', alignItems: 'center', justifyContent: 'center',
        }}>
          <svg width="34" height="34" viewBox="0 0 34 34">
            <rect x="3" y="3" width="28" height="28" rx="5" stroke="currentColor" strokeWidth="2" fill="none"/>
            <polyline points="8,22 13,15 18,18 25,9" stroke="currentColor" strokeWidth="2.4" fill="none" strokeLinecap="round" strokeLinejoin="round"/>
          </svg>
        </div>
        <div>
          <div style={{ fontSize: 22, fontWeight: 600, color: 'var(--la-text-strong)' }}>LogAnzlyzer</div>
          <div style={{ fontSize: 'var(--la-fs-sm)', color: 'var(--la-text-muted)', fontFamily: 'var(--la-font-mono)' }}>
            v0.4.2 · build 2026.05.13
          </div>
        </div>
        <div style={{ fontSize: 'var(--la-fs-body)', color: 'var(--la-text)', maxWidth: 320 }}>
          A Windows desktop tool for inspecting per-event delays and surfacing the P1 (low 1%) tail latency in any timestamped log.
        </div>
      </div>
      <div style={{
        margin: '0 24px 20px',
        padding: '12px 14px',
        background: 'var(--la-panel-elev)',
        border: '1px solid var(--la-border-soft)',
        borderRadius: 6,
        fontSize: 'var(--la-fs-sm)',
      }}>
        <div style={{ display: 'flex', justifyContent: 'space-between', padding: '4px 0' }}>
          <span style={{ color: 'var(--la-text-muted)' }}>Repository</span>
          <span style={{ color: 'var(--la-accent)', fontFamily: 'var(--la-font-mono)' }}>github.com/you/loganzlyzer</span>
        </div>
        <div style={{ display: 'flex', justifyContent: 'space-between', padding: '4px 0' }}>
          <span style={{ color: 'var(--la-text-muted)' }}>Licence</span>
          <span style={{ color: 'var(--la-text)' }}>MIT</span>
        </div>
        <div style={{ display: 'flex', justifyContent: 'space-between', padding: '4px 0' }}>
          <span style={{ color: 'var(--la-text-muted)' }}>Runtime</span>
          <span style={{ color: 'var(--la-text)', fontFamily: 'var(--la-font-mono)' }}>.NET Framework 4.8 · WinForms</span>
        </div>
        <div style={{ display: 'flex', justifyContent: 'space-between', padding: '4px 0' }}>
          <span style={{ color: 'var(--la-text-muted)' }}>Author</span>
          <span style={{ color: 'var(--la-text)' }}>open-source community</span>
        </div>
      </div>
    </Modal>
  );
}

Object.assign(window, { MainWindow, EmptyState, TimestampDialog, SettingsDialog, AboutDialog });
