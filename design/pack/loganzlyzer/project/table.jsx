// table.jsx — sortable log entries table

const LOG_ROWS = [
  { ts: '10:41:23.412', ms: 17,  raw: 'evt=tick scope=worker id=4a2 ok' },
  { ts: '10:41:23.430', ms: 18,  raw: 'evt=tick scope=worker id=4a3 ok' },
  { ts: '10:41:23.451', ms: 21,  raw: 'evt=flush scope=cache id=4a4 wrote=2.1MB' },
  { ts: '10:41:23.469', ms: 18,  raw: 'evt=tick scope=worker id=4a5 ok' },
  { ts: '10:41:23.495', ms: 26,  raw: 'evt=tick scope=worker id=4a6 ok' },
  { ts: '10:41:23.604', ms: 109, raw: 'evt=stall scope=io     id=4a7 wait=fsync', highlight: true },
  { ts: '10:41:23.628', ms: 24,  raw: 'evt=tick scope=worker id=4a8 ok' },
  { ts: '10:41:23.659', ms: 31,  raw: 'evt=gc-pause scope=runtime id=4a9 gen=2 reclaimed=4.7MB' },
  { ts: '10:41:23.678', ms: 19,  raw: 'evt=tick scope=worker id=4aa ok' },
  { ts: '10:41:23.701', ms: 23,  raw: 'evt=tick scope=worker id=4ab ok' },
  { ts: '10:41:23.722', ms: 21,  raw: 'evt=tick scope=worker id=4ac ok' },
  { ts: '10:41:23.749', ms: 27,  raw: 'evt=flush scope=cache id=4ad wrote=1.4MB' },
];

function LogTable({ height = 240, sortBy = 'delay_ms', sortDir = 'desc' }) {
  const cols = [
    { id: 'timestamp', label: 'timestamp', width: 130, mono: true },
    { id: 'delay_ms',  label: 'delay_ms',  width: 100, mono: true, align: 'right' },
    { id: 'raw_line',  label: 'raw_line',  width: 'auto', mono: true },
  ];
  return (
    <div style={{
      display: 'flex', flexDirection: 'column',
      background: 'var(--la-bg)',
      borderTop: '1px solid var(--la-border)',
      height, overflow: 'hidden',
    }}>
      {/* toolbar */}
      <div style={{
        display: 'flex', alignItems: 'center', gap: 8,
        padding: '6px 10px', height: 32,
        borderBottom: '1px solid var(--la-border-soft)',
        background: 'var(--la-panel)',
        color: 'var(--la-text-muted)', fontSize: 'var(--la-fs-xs)',
      }}>
        <span style={{ color: 'var(--la-text-strong)', fontSize: 'var(--la-fs-sm)', fontWeight: 600 }}>Entries</span>
        <span>4,128 events · sorted by {sortBy} {sortDir}</span>
        <div style={{ flex: 1 }}/>
        <div style={{ display: 'flex', alignItems: 'center', gap: 6, padding: '0 8px', height: 22, border: '1px solid var(--la-input-border)', borderRadius: 4, background: 'var(--la-input-bg)', color: 'var(--la-text-muted)' }}>
          <Icon.Search/> <span>filter rows…</span>
        </div>
      </div>
      {/* header */}
      <div style={{
        display: 'flex',
        background: 'var(--la-panel-elev)',
        borderBottom: '1px solid var(--la-border)',
        fontSize: 'var(--la-fs-xs)', textTransform: 'uppercase', letterSpacing: '.04em',
        color: 'var(--la-text-muted)',
        height: 26,
      }}>
        {cols.map(c => (
          <div key={c.id} style={{
            width: c.width === 'auto' ? undefined : c.width,
            flex: c.width === 'auto' ? 1 : '0 0 auto',
            padding: '0 12px',
            display: 'flex', alignItems: 'center', gap: 4,
            justifyContent: c.align === 'right' ? 'flex-end' : 'flex-start',
            color: sortBy === c.id ? 'var(--la-text-strong)' : 'var(--la-text-muted)',
            borderRight: '1px solid var(--la-border-soft)',
          }}>
            <span>{c.label}</span>
            {sortBy === c.id && (sortDir === 'desc' ? <Icon.SortDesc/> : <Icon.SortAsc/>)}
          </div>
        ))}
      </div>
      {/* body */}
      <div className="la-scroll" style={{ flex: 1, overflow: 'auto' }}>
        {LOG_ROWS.map((r, i) => (
          <div key={i} style={{
            display: 'flex', alignItems: 'center',
            height: 24, fontSize: 'var(--la-fs-sm)',
            fontFamily: 'var(--la-font-mono)',
            color: r.highlight ? 'var(--la-text-strong)' : 'var(--la-text)',
            background: r.highlight ? 'var(--la-row-selected)' : (i % 2 ? 'transparent' : 'var(--la-row-hover)'),
            borderLeft: r.highlight ? '2px solid var(--la-accent)' : '2px solid transparent',
          }}>
            <div style={{ width: 130, flex: '0 0 auto', padding: '0 12px', color: 'var(--la-text-muted)' }}>{r.ts}</div>
            <div style={{ width: 100, flex: '0 0 auto', padding: '0 12px', textAlign: 'right',
                         color: r.ms > 80 ? 'var(--la-p1)' : (r.ms > 40 ? 'var(--la-warning)' : 'var(--la-text)') }}>
              <span style={{ fontWeight: r.ms > 80 ? 700 : 500 }}>{r.ms}</span>
            </div>
            <div style={{ flex: 1, padding: '0 12px', overflow: 'hidden', textOverflow: 'ellipsis', whiteSpace: 'nowrap', color: 'var(--la-text)' }}>
              {r.raw}
            </div>
          </div>
        ))}
      </div>
    </div>
  );
}

window.LogTable = LogTable;
