// Wiki graph — ECharts force layout. echarts.min.js is already loaded in App.razor.
// API: window.wikiGraph.render(domId, data, dotNetRef), window.wikiGraph.dispose()
window.wikiGraph = (function () {
    let chart = null;
    let resizeHandler = null;
    let lastNodes = [];
    let lastEdges = [];

    function render(domId, data, dotNetRef) {
        const dom = document.getElementById(domId);
        if (!dom || typeof echarts === 'undefined') return;

        dispose();
        chart = echarts.init(dom);

        const nodes = (data.nodes || []).map(n => ({
            id: String(n.id),
            name: n.title,
            slug: n.slug,
            symbolSize: 18,
            label: { show: true }
        }));
        const edges = (data.edges || []).map(e => ({
            source: String(e.source),
            target: String(e.target)
        }));

        chart.setOption({
            tooltip: { formatter: p => p.dataType === 'node' ? p.data.name : '' },
            series: [{
                type: 'graph',
                layout: 'force',
                roam: true,
                draggable: true,
                force: { repulsion: 180, edgeLength: 120, gravity: 0.05 },
                label: { position: 'right', fontSize: 12 },
                lineStyle: { color: '#aaa', width: 1.2, curveness: 0.1 },
                emphasis: { focus: 'adjacency', lineStyle: { width: 3 } },
                data: nodes,
                edges: edges
            }]
        });

        if (dotNetRef) {
            chart.on('click', function (params) {
                if (params.dataType === 'node' && params.data.slug) {
                    dotNetRef.invokeMethodAsync('OnNodeClick', params.data.slug);
                }
            });
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
    }

    // Typed graph: domain↔keyword nodes (two categories, colored). data = { nodes:[{id,name,type,entityKey,weight}], edges:[{source,target}] }
    // Click → dotNetRef.OnNodeClick(type, entityKey).
    function renderTyped(domId, data, dotNetRef) {
        const dom = document.getElementById(domId);
        if (!dom || typeof echarts === 'undefined') return;
        dispose();
        chart = echarts.init(dom);

        // 4 categories by siteType: keyword (center), mainsite, satellite (managed), competitor.
        const categories = [
            { name: 'Keyword', itemStyle: { color: '#37b24d' } },
            { name: 'Mainsite', itemStyle: { color: '#e8590c' } },
            { name: 'Site phụ', itemStyle: { color: '#4263eb' } },
            { name: 'Đối thủ', itemStyle: { color: '#adb5bd' } }
        ];
        const CAT = { keyword: 0, mainsite: 1, satellite: 2, competitor: 3 };
        const catOf = n => CAT[n.siteType] ?? (n.type === 'keyword' ? 0 : 3);
        const nodes = (data.nodes || []).map(n => {
            const isKw = n.type === 'keyword';
            const st = n.siteType || (isKw ? 'keyword' : 'competitor');
            // Importance order for sizing: keyword & mainsite biggest, satellite mid, competitor small.
            const base = isKw ? 20 : (st === 'mainsite' ? 18 : (st === 'satellite' ? 11 : 7));
            return {
                id: n.id,
                name: n.name,
                category: catOf(n),
                entityKey: n.entityKey,
                nodeType: n.type,
                siteType: st,
                team: n.team || '',
                pic: n.pic || '',
                symbolSize: base + Math.min(22, (n.weight || 0) * 2),
                itemStyle: st === 'competitor' ? { opacity: 0.55 } : undefined,
                label: { show: isKw || st === 'mainsite' || (n.weight || 0) >= 3 }
            };
        });
        const edges = (data.edges || []).map(e => ({ source: e.source, target: e.target }));

        lastNodes = nodes;
        lastEdges = edges;
        chart.setOption({
            tooltip: {
                trigger: 'item',
                formatter: function (p) {
                    if (p.dataType !== 'node') return '';
                    const labelMap = { keyword: 'Keyword', mainsite: 'Mainsite', satellite: 'Site phụ', competitor: 'Đối thủ' };
                    const typeLabel = labelMap[p.data.siteType] || 'Domain';
                    const owner = (p.data.team || p.data.pic)
                        ? `<br/>Team: ${p.data.team || '—'} · PIC: ${p.data.pic || '—'}` : '';
                    return `<b>${p.data.name}</b><br/>${typeLabel}${owner}<br/><span style="color:#888">Click để xem chi tiết · kéo để di chuyển</span>`;
                }
            },
            legend: [{ data: categories.map(c => c.name), top: 4 }],
            series: [{
                type: 'graph', layout: 'force',
                roam: true,            // wheel/pinch zoom + drag-pan the canvas
                draggable: true,       // drag individual nodes
                categories: categories,
                zoom: 1,
                scaleLimit: { min: 0.2, max: 8 },
                force: { repulsion: 220, edgeLength: 90, gravity: 0.06, layoutAnimation: true },
                label: { position: 'right', fontSize: 11 },
                lineStyle: { color: '#bbb', width: 1, curveness: 0.1 },
                emphasis: { focus: 'adjacency', lineStyle: { width: 3 } },
                data: nodes, edges: edges
            }]
        });

        // Hold/drag: pin a node where it's dropped (fixed = true) so the layout stops shoving
        // it around. Double-click a node to release it back into the force simulation.
        chart.on('mouseup', function (params) {
            if (params.dataType === 'node' && lastNodes[params.dataIndex]) {
                lastNodes[params.dataIndex].fixed = true;
            }
        });
        chart.on('dblclick', function (params) {
            if (params.dataType === 'node' && lastNodes[params.dataIndex]) {
                lastNodes[params.dataIndex].fixed = false;
                chart.setOption({ series: [{ data: lastNodes }] });
            }
        });

        if (dotNetRef) {
            chart.on('click', function (params) {
                if (params.dataType === 'node' && params.data.entityKey) {
                    dotNetRef.invokeMethodAsync('OnNodeClick', params.data.nodeType, params.data.entityKey);
                }
            });
        }
        resizeHandler = function () { if (chart) chart.resize(); };
        window.addEventListener('resize', resizeHandler);
    }

    // Multiply current zoom by factor (>1 = zoom in, <1 = zoom out).
    function zoomBy(factor) {
        if (!chart) return;
        const opt = chart.getOption();
        const cur = (opt.series && opt.series[0] && opt.series[0].zoom) || 1;
        const next = Math.max(0.2, Math.min(8, cur * factor));
        chart.setOption({ series: [{ zoom: next }] });
    }

    // Search: highlight nodes whose name matches query (case/accent-insensitive substring).
    // Empty query clears highlight. Returns match count.
    function search(query) {
        if (!chart) return 0;
        const q = fold(query || '');
        if (!q) {
            chart.dispatchAction({ type: 'downplay', seriesIndex: 0 });
            chart.setOption({ series: [{ data: lastNodes.map(n => ({ ...n, itemStyle: undefined })) }] });
            return 0;
        }
        let count = 0;
        const data = lastNodes.map(n => {
            const hit = fold(n.name).indexOf(q) >= 0;
            if (hit) count++;
            return hit
                ? { ...n, itemStyle: { borderColor: '#f03e3e', borderWidth: 3 }, label: { show: true } }
                : { ...n, itemStyle: { opacity: 0.25 } };
        });
        chart.setOption({ series: [{ data: data }] });
        return count;
    }

    function fold(s) {
        return (s || '').normalize('NFD').replace(/[\u0300-\u036f]/g, '')
            .replace(/đ/g, 'd').replace(/Đ/g, 'D').toLowerCase().trim();
    }

    // Focus an exact node by name: highlight it (others dim), center the view on it, zoom in.
    function focusNode(name) {
        if (!chart || !name) return;
        const target = fold(name);
        const idx = lastNodes.findIndex(n => fold(n.name) === target);
        if (idx < 0) return;

        // Highlight target, dim the rest.
        const data = lastNodes.map(n => {
            const hit = fold(n.name) === target;
            return hit
                ? { ...n, itemStyle: { borderColor: '#f03e3e', borderWidth: 4 }, label: { show: true } }
                : { ...n, itemStyle: { opacity: 0.2 } };
        });
        chart.setOption({ series: [{ data: data }] });

        // Force layout assigns coordinates async — wait a tick, then read the node's
        // graphic layout position and recenter the series there with a zoom-in.
        setTimeout(function () {
            try {
                const series = chart.getModel().getSeriesByIndex(0);
                const graph = series.getGraph();
                const node = graph.getNodeByIndex(idx);
                const layout = node && node.getLayout(); // [x, y] in series coordinate space
                if (layout && layout.length === 2) {
                    chart.setOption({ series: [{ zoom: 2.4, center: [layout[0], layout[1]] }] });
                }
            } catch (e) { /* fallback: just zoom */ chart.setOption({ series: [{ zoom: 2.4 }] }); }
            chart.dispatchAction({ type: 'showTip', seriesIndex: 0, dataIndex: idx });
            chart.dispatchAction({ type: 'highlight', seriesIndex: 0, dataIndex: idx });
        }, 60);
    }

    // Node names for autocomplete suggestions.
    function nodeNames() { return lastNodes.map(n => n.name); }

    // Toggle competitor nodes/edges. visible=false hides all 'competitor' domains.
    function setCompetitorVisible(visible) {
        if (!chart) return;
        const data = lastNodes.map(n => n.siteType === 'competitor'
            ? { ...n, itemStyle: { ...(n.itemStyle || {}), opacity: visible ? 0.55 : 0 }, label: { show: false } }
            : n);
        // Hide edges touching a hidden competitor node.
        const hidden = new Set(lastNodes.filter(n => n.siteType === 'competitor').map(n => n.id));
        const edges = lastEdges.map(e =>
            (!visible && (hidden.has(e.source) || hidden.has(e.target)))
                ? { ...e, lineStyle: { opacity: 0 } }
                : { ...e, lineStyle: undefined });
        chart.setOption({ series: [{ data: data, edges: edges }] });
    }

    return { render: render, renderTyped: renderTyped, zoomBy: zoomBy, search: search, focusNode: focusNode, nodeNames: nodeNames, setCompetitorVisible: setCompetitorVisible, dispose: dispose };
})();
