// controls.jsx — buttons, toggle, dropdown, text input, modal wrapper, stat card,
// drag-drop zone, segmented control. All states laid out side-by-side in the
// component library; same primitives reused inside the dialog mocks.

function Button({ kind = 'primary', state = 'default', children, icon, size = 'md', style }) {
  const base = {
    display: 'inline-flex', alignItems: 'center', justifyContent: 'center', gap: 6,
    fontFamily: 'var(--la-font-ui)', fontSize: 'var(--la-fs-body)',
    height: size === 'sm' ? 24 : 28,
    padding: size === 'sm' ? '0 10px' : '0 14px',
    borderRadius: 'var(--la-radius)',
    border: '1px solid transparent',
    cursor: state === 'disabled' ? 'default' : 'pointer',
    userSelect: 'none', whiteSpace: 'nowrap',
    transition: 'background .12s, border-color .12s, color .12s',
    opacity: state === 'disabled' ? 0.5 : 1,
  };
  const variants = {
    primary: {
      default: { background: 'var(--la-accent)',       color: 'var(--la-accent-fg)', borderColor: 'var(--la-accent)' },
      hover:   { background: 'var(--la-accent-hover)', color: 'var(--la-accent-fg)', borderColor: 'var(--la-accent-hover)' },
      pressed: { background: 'var(--la-accent-press)', color: 'var(--la-accent-fg)', borderColor: 'var(--la-accent-press)', boxShadow: 'inset 0 1px 0 rgba(0,0,0,.15)' },
      focus:   { background: 'var(--la-accent)',       color: 'var(--la-accent-fg)', borderColor: 'var(--la-accent)', boxShadow: '0 0 0 2px var(--la-accent-glow)' },
      disabled:{ background: 'var(--la-accent)',       color: 'var(--la-accent-fg)', borderColor: 'var(--la-accent)' },
    },
    secondary: {
      default: { background: 'var(--la-panel-elev)', color: 'var(--la-text)', borderColor: 'var(--la-border)' },
      hover:   { background: 'var(--la-row-hover)',  color: 'var(--la-text-strong)', borderColor: 'var(--la-border)' },
      pressed: { background: 'var(--la-row-selected)', color: 'var(--la-text-strong)', borderColor: 'var(--la-accent)' },
      focus:   { background: 'var(--la-panel-elev)', color: 'var(--la-text)', borderColor: 'var(--la-accent)', boxShadow: '0 0 0 2px var(--la-accent-glow)' },
      disabled:{ background: 'var(--la-panel-elev)', color: 'var(--la-text-muted)', borderColor: 'var(--la-border-soft)' },
    },
    ghost: {
      default: { background: 'transparent', color: 'var(--la-text)' },
      hover:   { background: 'var(--la-row-hover)', color: 'var(--la-text-strong)' },
      pressed: { background: 'var(--la-row-selected)', color: 'var(--la-text-strong)' },
      focus:   { background: 'transparent', color: 'var(--la-text)', borderColor: 'var(--la-accent)', boxShadow: '0 0 0 2px var(--la-accent-glow)' },
      disabled:{ background: 'transparent', color: 'var(--la-text-faint)' },
    },
    danger: {
      default: { background: 'transparent', color: 'var(--la-p1)', borderColor: 'var(--la-border)' },
      hover:   { background: 'var(--la-p1-soft)', color: 'var(--la-p1)', borderColor: 'var(--la-p1)' },
      pressed: { background: 'var(--la-p1-soft)', color: 'var(--la-p1)', borderColor: 'var(--la-p1)' },
      focus:   { background: 'transparent', color: 'var(--la-p1)', borderColor: 'var(--la-p1)' },
      disabled:{ background: 'transparent', color: 'var(--la-p1)', borderColor: 'var(--la-border-soft)' },
    },
  };
  return (
    <button style={{ ...base, ...variants[kind][state], ...style }}>
      {icon}{children}
    </button>
  );
}

/* ---------- Toggle switch ---------------------------- */
function Toggle({ on = false, disabled = false, label }) {
  return (
    <label style={{ display: 'inline-flex', alignItems: 'center', gap: 10, fontSize: 'var(--la-fs-body)', opacity: disabled ? 0.5 : 1 }}>
      <span style={{
        width: 34, height: 18, borderRadius: 999, position: 'relative',
        background: on ? 'var(--la-accent)' : 'var(--la-panel-elev)',
        border: '1px solid ' + (on ? 'var(--la-accent)' : 'var(--la-input-border)'),
        transition: 'all .15s',
        display: 'inline-block', flex: '0 0 auto',
      }}>
        <span style={{
          position: 'absolute', top: 2, left: on ? 18 : 2,
          width: 12, height: 12, borderRadius: 999,
          background: on ? '#fff' : 'var(--la-text-muted)',
          transition: 'left .15s, background .15s',
        }}/>
      </span>
      {label && <span style={{ color: 'var(--la-text)' }}>{label}</span>}
    </label>
  );
}

/* ---------- Dropdown / Select ------------------------ */
function Select({ value, placeholder = 'Select…', state = 'default', width = 200 }) {
  const border = state === 'focus' ? 'var(--la-input-focus)' : 'var(--la-input-border)';
  const shadow = state === 'focus' ? '0 0 0 2px var(--la-accent-glow)' : 'none';
  return (
    <div style={{
      display: 'inline-flex', alignItems: 'center', justifyContent: 'space-between',
      width, height: 28, padding: '0 8px 0 10px',
      background: 'var(--la-input-bg)',
      border: '1px solid ' + border,
      borderRadius: 'var(--la-radius)',
      color: value ? 'var(--la-text)' : 'var(--la-text-muted)',
      fontSize: 'var(--la-fs-body)',
      boxShadow: shadow,
    }}>
      <span style={{ overflow: 'hidden', textOverflow: 'ellipsis', whiteSpace: 'nowrap' }}>{value || placeholder}</span>
      <Icon.Chevron style={{ color: 'var(--la-text-muted)' }}/>
    </div>
  );
}

/* ---------- Text input ------------------------------- */
function TextInput({ value = '', placeholder = '', state = 'default', width = 220, mono = false, icon }) {
  const border = state === 'focus' ? 'var(--la-input-focus)' : (state === 'error' ? 'var(--la-error)' : 'var(--la-input-border)');
  const shadow = state === 'focus' ? '0 0 0 2px var(--la-accent-glow)' : 'none';
  return (
    <div style={{
      display: 'inline-flex', alignItems: 'center', gap: 6,
      width, height: 28, padding: '0 10px',
      background: 'var(--la-input-bg)',
      border: '1px solid ' + border,
      borderRadius: 'var(--la-radius)',
      boxShadow: shadow,
      fontSize: 'var(--la-fs-body)',
      fontFamily: mono ? 'var(--la-font-mono)' : 'var(--la-font-ui)',
    }}>
      {icon}
      <span style={{ color: value ? 'var(--la-text)' : 'var(--la-text-muted)', flex: 1, overflow: 'hidden', textOverflow: 'ellipsis', whiteSpace: 'nowrap' }}>
        {value || placeholder}
      </span>
      {state === 'focus' && <span style={{ width: 1, height: 14, background: 'var(--la-text)', animation: 'la-caret 1s steps(2) infinite' }}/>}
    </div>
  );
}

/* ---------- Stat card -------------------------------- */
function StatCard({ label, value, unit, hero = false, accent = 'text', sub }) {
  const color = {
    text:    'var(--la-text-strong)',
    p1:      'var(--la-p1)',
    median:  'var(--la-median)',
    accent:  'var(--la-accent)',
  }[accent];
  return (
    <div style={{
      padding: hero ? '14px 16px' : '10px 14px',
      background: hero ? 'var(--la-panel-elev)' : 'transparent',
      border: hero ? '1px solid var(--la-border)' : '1px solid var(--la-border-soft)',
      borderRadius: 'var(--la-radius)',
      display: 'flex', flexDirection: 'column', gap: hero ? 4 : 2,
      borderLeft: hero ? '3px solid var(--la-p1)' : '1px solid var(--la-border-soft)',
    }}>
      <div style={{
        fontSize: 'var(--la-fs-xs)',
        textTransform: 'uppercase', letterSpacing: '.06em',
        color: 'var(--la-text-muted)',
      }}>{label}</div>
      <div style={{ display: 'flex', alignItems: 'baseline', gap: 4 }}>
        <span style={{
          fontFamily: 'var(--la-font-mono)',
          fontSize: hero ? 'var(--la-fs-stat-hero)' : 'var(--la-fs-stat)',
          fontWeight: hero ? 600 : 500,
          color, lineHeight: 1, fontFeatureSettings: '"tnum" 1',
          letterSpacing: '-0.01em',
        }}>{value}</span>
        {unit && <span style={{ color: 'var(--la-text-muted)', fontSize: hero ? 'var(--la-fs-md)' : 'var(--la-fs-sm)' }}>{unit}</span>}
      </div>
      {sub && <div style={{ fontSize: 'var(--la-fs-xs)', color: 'var(--la-text-muted)' }}>{sub}</div>}
    </div>
  );
}

/* ---------- Modal frame (used by the three dialogs) -- */
function Modal({ title, width = 520, children, footer, theme = 'dark' }) {
  return (
    <div className={"la-root theme-" + theme} style={{
      width, background: 'var(--la-panel)',
      border: '1px solid var(--la-border)',
      borderRadius: 8,
      boxShadow: theme === 'dark' ? 'var(--la-shadow-pop)' : 'var(--la-shadow-light)',
      overflow: 'hidden',
      color: 'var(--la-text)',
    }}>
      <div style={{
        display: 'flex', alignItems: 'center',
        height: 36, padding: '0 6px 0 14px',
        background: 'var(--la-titlebar)',
        borderBottom: '1px solid var(--la-border-soft)',
        fontSize: 'var(--la-fs-body)',
        color: 'var(--la-text-strong)',
      }}>
        <span>{title}</span>
        <div style={{ flex: 1 }}/>
        <div style={{ width: 32, height: 32, display: 'flex', alignItems: 'center', justifyContent: 'center', color: 'var(--la-text-muted)' }}>
          <Icon.X/>
        </div>
      </div>
      <div>{children}</div>
      {footer && (
        <div style={{
          display: 'flex', justifyContent: 'flex-end', gap: 8,
          padding: '12px 16px',
          borderTop: '1px solid var(--la-border-soft)',
          background: 'var(--la-panel)',
        }}>{footer}</div>
      )}
    </div>
  );
}

/* ---------- Drag-drop zone --------------------------- */
function DropZone({ state = 'default' }) {
  const dashColor = state === 'drag-active' ? 'var(--la-accent)' : (state === 'hover' ? 'var(--la-text-muted)' : 'var(--la-border)');
  const bg = state === 'drag-active' ? 'var(--la-accent-soft)' : (state === 'hover' ? 'var(--la-panel-elev)' : 'transparent');
  return (
    <div style={{
      width: '100%', maxWidth: 520, padding: '40px 28px',
      borderRadius: 12,
      background: bg,
      backgroundImage: `repeating-linear-gradient(45deg, transparent, transparent 8px, ${state === 'drag-active' ? 'var(--la-accent-soft)' : 'transparent'} 8px, ${state === 'drag-active' ? 'var(--la-accent-soft)' : 'transparent'} 9px)`,
      outline: '1.5px dashed ' + dashColor,
      outlineOffset: -2,
      display: 'flex', flexDirection: 'column', alignItems: 'center', gap: 14,
      textAlign: 'center',
      color: state === 'drag-active' ? 'var(--la-accent)' : 'var(--la-text)',
      transition: 'all .15s',
    }}>
      <div style={{
        width: 56, height: 56, borderRadius: '50%',
        background: state === 'drag-active' ? 'var(--la-accent)' : 'var(--la-panel-elev)',
        color: state === 'drag-active' ? 'var(--la-accent-fg)' : 'var(--la-accent)',
        display: 'flex', alignItems: 'center', justifyContent: 'center',
        border: '1px solid ' + (state === 'drag-active' ? 'var(--la-accent)' : 'var(--la-border)'),
      }}>
        <Icon.Upload/>
      </div>
      <div style={{ fontSize: 'var(--la-fs-lg)', color: 'var(--la-text-strong)' }}>
        {state === 'drag-active' ? 'Drop to open' : 'Drag a log file here'}
      </div>
      <div style={{ fontSize: 'var(--la-fs-sm)', color: 'var(--la-text-muted)' }}>
        or <span style={{ color: 'var(--la-accent)', textDecoration: 'underline' }}>browse</span> &nbsp;·&nbsp; .log, .txt, .out — up to 2 GB
      </div>
    </div>
  );
}

Object.assign(window, { Button, Toggle, Select, TextInput, StatCard, Modal, DropZone });
