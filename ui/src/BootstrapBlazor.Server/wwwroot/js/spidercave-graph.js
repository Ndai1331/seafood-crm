// SpiderCave money-flow graph — ECharts Sankey layout (source → hop → target, left-to-right).
// Flow width encodes amount so non-technical readers can see "who sent the most" at a glance.
// echarts.min.js is already loaded globally in App.razor.
// API: window.spiderCaveGraph.render(domId, data), window.spiderCaveGraph.dispose(), window.spiderCaveGraph.zoom(factor), window.spiderCaveGraph.resetView()
window.spiderCaveGraph = (function () {
    let chart = null;
    let resizeHandler = null;
    let clickHandler = null;
    let nodeById = {};

    const COLOR_OF = { source: '#3b82f6', hop: '#94a3b8', target: '#e94560' };
    const LABEL_OF = { source: 'Ví nguồn', hop: 'Ví trung gian', target: 'Ví hội tụ (target)' };
    // A source wallet funded by tickets from 2+ different PICs is the collusion signal —
    // paint it and its label red so it's visible on the chart itself, not just in a tooltip
    // the auditor has to hover to see.
    const COLLUSION_COLOR = '#dc2626';

    function render(domId, data, dotNetRef) {
        const dom = document.getElementById(domId);
        if (!dom || typeof echarts === 'undefined') return;

        dispose();
        chart = echarts.init(dom);

        nodeById = {};
        (data.nodes || []).forEach(n => { nodeById[n.id] = n; });

        // PIC names are NOT printed on the label — with up to 183 source rows the text overlaps
        // and becomes unreadable. Collusion wallets are marked by color alone (label + node red);
        // full PIC names live in the tooltip (hover) and the collusion table below the chart.
        const nodes = (data.nodes || []).map(n => {
            const isCollusion = n.type === 'source' && (n.picCount || 0) >= 2;
            return {
                name: n.id,
                label: {
                    formatter: isCollusion ? `⚠ ${n.label}` : n.label,
                    color: isCollusion ? COLLUSION_COLOR : '#334155',
                    fontWeight: isCollusion ? 700 : 400
                },
                itemStyle: { color: isCollusion ? COLLUSION_COLOR : (COLOR_OF[n.type] || '#94a3b8') },
                depth: n.type === 'source' ? 0 : (n.type === 'target' ? undefined : 1)
            };
        });
        const links = (data.edges || []).map(e => ({
            source: e.fromId,
            target: e.toId,
            value: e.amount > 0 ? e.amount : 1,
            lineStyle: { color: 'gradient', opacity: 0.35 }
        }));

        chart.setOption({
            tooltip: {
                trigger: 'item',
                formatter: p => {
                    if (p.dataType === 'edge') {
                        const src = nodeById[p.data.source];
                        const amount = (p.value || 0).toLocaleString('vi-VN');
                        return `<b>${src ? src.label : p.data.source}</b><br/>${amount} · click để xem chi tiết phiếu`;
                    }
                    const n = nodeById[p.name];
                    if (!n) return p.name;
                    const lines = [`<b>${n.label}</b>`, LABEL_OF[n.type] || ''];
                    if (n.subLabel) lines.push(n.subLabel);
                    if (n.picLabel) {
                        const isCollusion = n.type === 'source' && (n.picCount || 0) >= 2;
                        lines.push(isCollusion
                            ? `<b style="color:${COLLUSION_COLOR}">⚠ ${n.picCount} PIC khác nhau: ${n.picLabel}</b>`
                            : 'PIC: ' + n.picLabel);
                    }
                    if (n.type !== 'hop') lines.push('<span style="color:#888">Click để xem chi tiết phiếu</span>');
                    return lines.join('<br/>');
                }
            },
            series: [{
                type: 'sankey',
                left: 60, right: 160, top: 24, bottom: 24,
                nodeWidth: 14,
                nodeGap: 10,
                layoutIterations: 32,
                orient: 'horizontal',
                emphasis: { focus: 'adjacency' },
                label: { position: 'right', fontSize: 10, color: '#334155' },
                lineStyle: { curveness: 0.45 },
                data: nodes,
                links: links
            }]
        });

        if (dotNetRef) {
            clickHandler = function (params) {
                let address = null;
                if (params.dataType === 'node') {
                    const n = nodeById[params.data.name];
                    if (n && n.type !== 'hop') address = n.address;
                } else if (params.dataType === 'edge') {
                    const src = nodeById[params.data.source];
                    if (src) address = src.address;
                }
                if (address) dotNetRef.invokeMethodAsync('OnGraphWalletClicked', address);
            };
            chart.on('click', clickHandler);
        }

        resizeHandler = function () { if (chart) chart.resize(); };
        window.addEventListener('resize', resizeHandler);
    }

    function dispose() {
        if (resizeHandler) {
            window.removeEventListener('resize', resizeHandler);
            resizeHandler = null;
        }
        if (chart) {
            chart.dispose();
            chart = null;
        }
        clickHandler = null;
        nodeById = {};
    }

    function zoom(factor) {
        if (!chart) return;
        const dom = chart.getDom();
        const canvas = dom.querySelector('canvas');
        if (!canvas) return;
        const currentZoom = dom.dataset.scZoom ? parseFloat(dom.dataset.scZoom) : 1;
        const next = Math.min(4, Math.max(0.4, currentZoom * factor));
        dom.dataset.scZoom = String(next);
        canvas.style.transform = `scale(${next})`;
        canvas.style.transformOrigin = 'center center';
    }

    function resetView() {
        if (!chart) return;
        const dom = chart.getDom();
        const canvas = dom.querySelector('canvas');
        dom.dataset.scZoom = '1';
        if (canvas) canvas.style.transform = 'scale(1)';
    }

    // Scrolls the wallet-tx modal table to the row matched as "closest to the ticket amount"
    // (element id set by the Blazor markup as tx-row-<txHash>), so the auditor lands directly
    // on the candidate on-chain record instead of scanning 30+ rows by eye.
    function scrollToTxRow(txHash) {
        const row = document.getElementById(`tx-row-${txHash}`);
        if (row) row.scrollIntoView({ behavior: 'smooth', block: 'center' });
    }

    return { render, dispose, zoom, resetView, scrollToTxRow };
})();
