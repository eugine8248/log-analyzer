// chart.jsx — delay time-series with P1 reference line, hover tooltip, axis labels.
// Pure SVG so the WinForms team can read exact coordinates / colors.

/* Deterministic delay series — looks like real latency with occasional spikes.
   Returns array of {t (sec offset), ms}. */
function genSeries(n = 220, seed = 7) {
  let s = seed;
  const rnd = () => { s = (s * 1103515245 + 12345) & 0x7fffffff; return (s / 0x7fffffff); };
  const out = [];
  let base = 18;
  for (let i = 0; i < n; i++) {
    base += (rnd() - 0.5) * 1.6;
    base = Math.max(8, Math.min(40, base));
    let v = base + (rnd() - 0.5) * 8;
    // burst regions
    if (i > 70 && i < 90) v += 30 + rnd() * 40;
    if (i > 140 && i < 148) v += 60 + rnd() * 80;
    if (i === 178) v += 180;
    if (i === 102) v += 95;
    out.push({ t: i, ms: Math.max(2, v) });
  }
  return out;
}

function percentile(arr, p) {
  const sorted = arr.slice().sort((a, b) => a - b);
  const idx = Math.floor((p / 100) * (sorted.length - 1));
  return sorted[idx];
}

function DelayChart({ width = 720, height = 280, showTooltip = true, hoverIdx = 148 }) {
  const data = React.useMemo(() => genSeries(220), []);
  const values = data.map(d => d.ms);
  const yMax = Math.ceil(Math.max(...values) / 50) * 50;          // 250
  const p99 = percentile(values, 99);
  const p95 = percentile(values, 95);
  const median = percentile(values, 50);
  const p1Low = percentile(values, 1);
  // "P1 (low 1%) tail latency" — the worst 1% delays. Use 99th percentile.
  const p1Tail = p99;

  const padL = 44, padR = 14, padT = 18, padB = 26;
  const innerW = width - padL - padR;
  const innerH = height - padT - padB;
  const x = (i) => padL + (i / (data.length - 1)) * innerW;
  const y = (v) => padT + innerH - (v / yMax) * innerH;

  const path = data.map((d, i) => (i === 0 ? 'M' : 'L') + x(i).toFixed(1) + ' ' + y(d.ms).toFixed(1)).join(' ');
  const areaPath = path + ' L ' + x(data.length - 1).toFixed(1) + ' ' + (padT + innerH) + ' L ' + padL + ' ' + (padT + innerH) + ' Z';

  // y-axis ticks
  const ticks = [0, yMax / 4, yMax / 2, (3 * yMax) / 4, yMax];

  // x-axis time labels
  const xLabels = ['10:40:00', '10:40:30', '10:41:00', '10:41:30', '10:42:00', '10:42:30'];

  const hover = data[hoverIdx];
  const hx = x(hoverIdx), hy = y(hover.ms);

  return (
    <svg width={width} height={height} style={{ display: 'block', overflow: 'visible' }}>
      <defs>
        <linearGradient id="la-area" x1="0" y1="0" x2="0" y2="1">
          <stop offset="0%" stopColor="var(--la-accent)" stopOpacity="0.28"/>
          <stop offset="100%" stopColor="var(--la-accent)" stopOpacity="0"/>
        </linearGradient>
      </defs>

      {/* grid + y ticks */}
      {ticks.map((t, i) => (
        <g key={i}>
          <line x1={padL} y1={y(t)} x2={width - padR} y2={y(t)} stroke="var(--la-grid)" strokeWidth="1"/>
          <text x={padL - 8} y={y(t) + 3} textAnchor="end" fontSize="10" fill="var(--la-text-muted)" fontFamily="var(--la-font-mono)">
            {t}
          </text>
        </g>
      ))}

      {/* y axis title */}
      <text x={padL - 32} y={padT + innerH / 2} transform={`rotate(-90 ${padL - 32} ${padT + innerH / 2})`} fontSize="10" fill="var(--la-text-muted)" textAnchor="middle">
        delay (ms)
      </text>

      {/* x axis labels */}
      {xLabels.map((l, i) => {
        const xp = padL + (i / (xLabels.length - 1)) * innerW;
        return <text key={i} x={xp} y={height - 8} fontSize="10" fill="var(--la-text-muted)" textAnchor="middle" fontFamily="var(--la-font-mono)">{l}</text>;
      })}

      {/* median reference */}
      <line x1={padL} y1={y(median)} x2={width - padR} y2={y(median)} stroke="var(--la-median)" strokeWidth="1" strokeDasharray="2 3" opacity="0.85"/>
      <rect x={width - padR - 88} y={y(median) - 16} width="86" height="14" rx="2" fill="var(--la-median-soft)"/>
      <text x={width - padR - 4} y={y(median) - 5} fontSize="10" fill="var(--la-median)" textAnchor="end" fontFamily="var(--la-font-mono)" fontWeight="600">
        median  {Math.round(median)} ms
      </text>

      {/* P1 (low 1%) tail reference */}
      <line x1={padL} y1={y(p1Tail)} x2={width - padR} y2={y(p1Tail)} stroke="var(--la-p1)" strokeWidth="1.5" strokeDasharray="4 3"/>
      <rect x={width - padR - 102} y={y(p1Tail) - 16} width="100" height="14" rx="2" fill="var(--la-p1-soft)"/>
      <text x={width - padR - 4} y={y(p1Tail) - 5} fontSize="10" fill="var(--la-p1)" textAnchor="end" fontFamily="var(--la-font-mono)" fontWeight="700">
        P1 (low 1%)  {Math.round(p1Tail)} ms
      </text>

      {/* area + line */}
      <path d={areaPath} fill="url(#la-area)"/>
      <path d={path} fill="none" stroke="var(--la-accent)" strokeWidth="1.4" strokeLinejoin="round" strokeLinecap="round"/>

      {/* hover crosshair + tooltip */}
      {showTooltip && (
        <g>
          <line x1={hx} y1={padT} x2={hx} y2={padT + innerH} stroke="var(--la-text-muted)" strokeWidth="1" strokeDasharray="2 3" opacity="0.7"/>
          <circle cx={hx} cy={hy} r="4.5" fill="var(--la-bg)" stroke="var(--la-accent)" strokeWidth="2"/>
          <g transform={`translate(${Math.min(hx + 10, width - padR - 156)}, ${Math.max(hy - 52, padT + 4)})`}>
            <rect width="150" height="46" rx="4" fill="var(--la-panel-elev)" stroke="var(--la-border)"/>
            <text x="10" y="16" fontSize="10" fill="var(--la-text-muted)" fontFamily="var(--la-font-mono)">10:41:23.604</text>
            <text x="10" y="34" fontSize="14" fill="var(--la-text-strong)" fontFamily="var(--la-font-mono)" fontWeight="600">
              {Math.round(hover.ms)} <tspan fontSize="10" fill="var(--la-text-muted)">ms</tspan>
            </text>
            <text x="140" y="34" fontSize="10" fill="var(--la-p1)" textAnchor="end" fontFamily="var(--la-font-mono)" fontWeight="600">in tail</text>
          </g>
        </g>
      )}
    </svg>
  );
}

window.DelayChart = DelayChart;
window.genSeries = genSeries;
window.percentile = percentile;
