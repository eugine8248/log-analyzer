// chrome.jsx — window chrome, menu bar, tab strip, status bar, icons
// Original Windows-Fluent-inspired look, no proprietary trade dress.

/* ---------- Icons (inline strokes) -------------------- */
const Icon = {
  Min: (p) => <svg width="10" height="10" viewBox="0 0 10 10" {...p}><line x1="1.5" y1="5" x2="8.5" y2="5" stroke="currentColor" strokeWidth="1"/></svg>,
  Max: (p) => <svg width="10" height="10" viewBox="0 0 10 10" {...p}><rect x="1.5" y="1.5" width="7" height="7" stroke="currentColor" strokeWidth="1" fill="none"/></svg>,
  Close: (p) => <svg width="10" height="10" viewBox="0 0 10 10" {...p}><line x1="1.5" y1="1.5" x2="8.5" y2="8.5" stroke="currentColor" strokeWidth="1"/><line x1="8.5" y1="1.5" x2="1.5" y2="8.5" stroke="currentColor" strokeWidth="1"/></svg>,
  X: (p) => <svg width="11" height="11" viewBox="0 0 11 11" {...p}><line x1="2.5" y1="2.5" x2="8.5" y2="8.5" stroke="currentColor" strokeWidth="1.4" strokeLinecap="round"/><line x1="8.5" y1="2.5" x2="2.5" y2="8.5" stroke="currentColor" strokeWidth="1.4" strokeLinecap="round"/></svg>,
  Plus: (p) => <svg width="11" height="11" viewBox="0 0 11 11" {...p}><line x1="5.5" y1="2" x2="5.5" y2="9" stroke="currentColor" strokeWidth="1.4" strokeLinecap="round"/><line x1="2" y1="5.5" x2="9" y2="5.5" stroke="currentColor" strokeWidth="1.4" strokeLinecap="round"/></svg>,
  Chevron: (p) => <svg width="9" height="9" viewBox="0 0 9 9" {...p}><polyline points="2,3.2 4.5,5.8 7,3.2" stroke="currentColor" strokeWidth="1.3" fill="none" strokeLinecap="round" strokeLinejoin="round"/></svg>,
  ChevronRight: (p) => <svg width="9" height="9" viewBox="0 0 9 9" {...p}><polyline points="3.2,2 5.8,4.5 3.2,7" stroke="currentColor" strokeWidth="1.3" fill="none" strokeLinecap="round" strokeLinejoin="round"/></svg>,
  Caret: (p) => <svg width="8" height="6" viewBox="0 0 8 6" {...p}><polygon points="0,1 8,1 4,5.5" fill="currentColor"/></svg>,
  SortAsc: (p) => <svg width="9" height="9" viewBox="0 0 9 9" {...p}><polyline points="2,5.5 4.5,3 7,5.5" stroke="currentColor" strokeWidth="1.4" fill="none" strokeLinecap="round" strokeLinejoin="round"/></svg>,
  SortDesc: (p) => <svg width="9" height="9" viewBox="0 0 9 9" {...p}><polyline points="2,3.5 4.5,6 7,3.5" stroke="currentColor" strokeWidth="1.4" fill="none" strokeLinecap="round" strokeLinejoin="round"/></svg>,
  File: (p) => <svg width="13" height="13" viewBox="0 0 13 13" {...p}><path d="M2.5 1.5 H7.5 L10.5 4.5 V11.5 H2.5 Z" stroke="currentColor" strokeWidth="1" fill="none"/><path d="M7.5 1.5 V4.5 H10.5" stroke="currentColor" strokeWidth="1" fill="none"/></svg>,
  Search: (p) => <svg width="13" height="13" viewBox="0 0 13 13" {...p}><circle cx="5.5" cy="5.5" r="3.5" stroke="currentColor" strokeWidth="1.3" fill="none"/><line x1="8.2" y1="8.2" x2="11.5" y2="11.5" stroke="currentColor" strokeWidth="1.3" strokeLinecap="round"/></svg>,
  Cog: (p) => <svg width="13" height="13" viewBox="0 0 13 13" {...p}><circle cx="6.5" cy="6.5" r="1.6" stroke="currentColor" strokeWidth="1.1" fill="none"/><circle cx="6.5" cy="6.5" r="4.4" stroke="currentColor" strokeWidth="1.1" fill="none" strokeDasharray="1.4 1.6"/></svg>,
  Upload: (p) => <svg width="28" height="28" viewBox="0 0 28 28" {...p}><path d="M14 19 V7 M9 12 L14 7 L19 12" stroke="currentColor" strokeWidth="1.6" fill="none" strokeLinecap="round" strokeLinejoin="round"/><path d="M5 19 V22 H23 V19" stroke="currentColor" strokeWidth="1.6" fill="none" strokeLinecap="round" strokeLinejoin="round"/></svg>,
  Check: (p) => <svg width="11" height="11" viewBox="0 0 11 11" {...p}><polyline points="2,5.8 4.4,8.2 9,3.2" stroke="currentColor" strokeWidth="1.6" fill="none" strokeLinecap="round" strokeLinejoin="round"/></svg>,
  Info: (p) => <svg width="14" height="14" viewBox="0 0 14 14" {...p}><circle cx="7" cy="7" r="6" stroke="currentColor" strokeWidth="1.1" fill="none"/><circle cx="7" cy="4" r=".8" fill="currentColor"/><line x1="7" y1="6.5" x2="7" y2="10.5" stroke="currentColor" strokeWidth="1.4" strokeLinecap="round"/></svg>,
  Drag: (p) => <svg width="9" height="13" viewBox="0 0 9 13" {...p}><circle cx="2.5" cy="2.5" r="1" fill="currentColor"/><circle cx="6.5" cy="2.5" r="1" fill="currentColor"/><circle cx="2.5" cy="6.5" r="1" fill="currentColor"/><circle cx="6.5" cy="6.5" r="1" fill="currentColor"/><circle cx="2.5" cy="10.5" r="1" fill="currentColor"/><circle cx="6.5" cy="10.5" r="1" fill="currentColor"/></svg>,
  Logo: (p) => (
    <svg width="14" height="14" viewBox="0 0 14 14" {...p}>
      <rect x="1.5" y="1.5" width="11" height="11" rx="2" stroke="currentColor" strokeWidth="1.2" fill="none"/>
      <polyline points="3.5,9 6,6 7.5,7.5 10.5,4" stroke="currentColor" strokeWidth="1.3" fill="none" strokeLinecap="round" strokeLinejoin="round"/>
    </svg>
  ),
};

/* ---------- Title bar -------------------------------- */
function TitleBar({ title }) {
  return (
    <div style={{
      display: 'flex', alignItems: 'center',
      height: 32, paddingLeft: 12, paddingRight: 0,
      background: 'var(--la-titlebar)',
      borderBottom: '1px solid var(--la-border-soft)',
      color: 'var(--la-text-muted)',
      fontSize: 'var(--la-fs-sm)',
      userSelect: 'none',
      flex: '0 0 32px',
    }}>
      <Icon.Logo style={{ color: 'var(--la-accent)' }}/>
      <span style={{ marginLeft: 8 }}>{title}</span>
      <div style={{ flex: 1 }}/>
      <div className="la-winbtns" style={{ display: 'flex', height: '100%' }}>
        {[Icon.Min, Icon.Max, Icon.Close].map((Ic, i) => (
          <div key={i} style={{
            width: 46, display: 'flex', alignItems: 'center', justifyContent: 'center',
            color: 'var(--la-text-muted)',
            background: i === 2 ? 'transparent' : 'transparent',
          }}>
            <Ic/>
          </div>
        ))}
      </div>
    </div>
  );
}

/* ---------- Menu bar --------------------------------- */
function MenuBar({ items = ['File', 'View', 'Settings', 'Help'], activeIndex = -1 }) {
  return (
    <div style={{
      display: 'flex', alignItems: 'center',
      height: 28, paddingLeft: 6,
      background: 'var(--la-menubar)',
      borderBottom: '1px solid var(--la-border-soft)',
      fontSize: 'var(--la-fs-sm)',
      color: 'var(--la-text)',
      flex: '0 0 28px',
    }}>
      {items.map((it, i) => (
        <div key={it} style={{
          padding: '4px 9px',
          borderRadius: 3,
          margin: '0 1px',
          background: i === activeIndex ? 'var(--la-accent-soft)' : 'transparent',
          color: i === activeIndex ? 'var(--la-text-strong)' : 'var(--la-text)',
        }}>
          <span style={{ textDecoration: 'underline', textDecorationColor: 'transparent' }}>
            <u style={{ textDecorationColor: 'currentColor' }}>{it[0]}</u>{it.slice(1)}
          </span>
        </div>
      ))}
    </div>
  );
}

/* ---------- Tab strip -------------------------------- */
function TabStrip({ tabs, activeIndex = 0 }) {
  return (
    <div style={{
      display: 'flex', alignItems: 'stretch',
      height: 34,
      background: 'var(--la-tabstrip)',
      borderBottom: '1px solid var(--la-border)',
      flex: '0 0 34px',
      paddingLeft: 0,
      overflow: 'hidden',
    }}>
      {tabs.map((t, i) => {
        const active = i === activeIndex;
        return (
          <div key={i} style={{
            display: 'flex', alignItems: 'center', gap: 8,
            padding: '0 10px 0 14px',
            height: '100%',
            background: active ? 'var(--la-tab-active)' : 'transparent',
            color: active ? 'var(--la-text-strong)' : 'var(--la-text-muted)',
            borderRight: '1px solid var(--la-border)',
            borderTop: active ? '1.5px solid var(--la-accent)' : '1.5px solid transparent',
            marginTop: active ? -1 : 0,
            fontSize: 'var(--la-fs-sm)',
            minWidth: 0,
            maxWidth: 240,
            position: 'relative',
          }}>
            <Icon.File style={{ color: active ? 'var(--la-accent)' : 'var(--la-text-muted)', flex: '0 0 auto' }}/>
            <span style={{
              whiteSpace: 'nowrap', overflow: 'hidden', textOverflow: 'ellipsis',
              flex: '1 1 auto',
            }}>{t.label}{t.dirty && <span style={{ color: 'var(--la-warning)' }}> •</span>}</span>
            <span style={{
              display: 'flex', alignItems: 'center', justifyContent: 'center',
              width: 18, height: 18, borderRadius: 3,
              color: active ? 'var(--la-text)' : 'var(--la-text-faint)',
              flex: '0 0 auto',
            }}><Icon.X/></span>
          </div>
        );
      })}
      {/* new tab button */}
      <div style={{
        display: 'flex', alignItems: 'center', justifyContent: 'center',
        width: 32, height: '100%',
        color: 'var(--la-text-muted)',
      }}>
        <Icon.Plus/>
      </div>
      <div style={{ flex: 1 }}/>
    </div>
  );
}

/* ---------- Status bar ------------------------------- */
function StatusBar({ left = [], right = [] }) {
  const Seg = ({ children, accent }) => (
    <div style={{
      display: 'flex', alignItems: 'center', gap: 6,
      padding: '0 10px', height: '100%',
      background: accent ? 'rgba(255,255,255,0.12)' : 'transparent',
      color: 'var(--la-statusbar-fg)',
      whiteSpace: 'nowrap',
    }}>{children}</div>
  );
  return (
    <div style={{
      display: 'flex', alignItems: 'stretch',
      height: 22, background: 'var(--la-statusbar)',
      color: 'var(--la-statusbar-fg)',
      fontSize: 'var(--la-fs-xs)',
      flex: '0 0 22px',
    }}>
      {left.map((s, i) => <Seg key={'l' + i} accent={s.accent}>{s.content}</Seg>)}
      <div style={{ flex: 1 }}/>
      {right.map((s, i) => <Seg key={'r' + i} accent={s.accent}>{s.content}</Seg>)}
    </div>
  );
}

/* ---------- Window frame (root) ---------------------- */
function WindowFrame({ theme = 'dark', width, height, title = 'LogAnzlyzer', children }) {
  return (
    <div className={"la-root theme-" + theme} style={{
      width, height,
      display: 'flex', flexDirection: 'column',
      background: 'var(--la-bg)',
      color: 'var(--la-text)',
      overflow: 'hidden',
      boxShadow: theme === 'dark' ? 'var(--la-shadow-pop)' : 'var(--la-shadow-light)',
      borderRadius: 6,
    }}>
      <TitleBar title={title}/>
      {children}
    </div>
  );
}

Object.assign(window, { Icon, TitleBar, MenuBar, TabStrip, StatusBar, WindowFrame });
