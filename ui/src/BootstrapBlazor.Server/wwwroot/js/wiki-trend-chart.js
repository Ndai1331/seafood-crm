// Wiki auto-page trend line chart (ECharts, already loaded in App.razor).
// window.wikiTrend.render(domId, { dates:[], values:[], name, invertY })
//   invertY=true for rank (lower = better, show inverted).
window.wikiTrend = (function () {
    const charts = {};

    function render(domId, data) {
        const dom = document.getElementById(domId);
        if (!dom || typeof echarts === 'undefined') return;
        if (charts[domId]) { charts[domId].dispose(); }
        const chart = echarts.init(dom);
        charts[domId] = chart;

        chart.setOption({
            tooltip: { trigger: 'axis' },
            grid: { left: 50, right: 20, top: 24, bottom: 30 },
            xAxis: {
                type: 'category',
                data: data.dates || [],
                axisLabel: { fontSize: 11 }
            },
            yAxis: {
                type: 'value',
                inverse: !!data.invertY,
                axisLabel: { fontSize: 11 }
            },
            series: [{
                name: data.name || 'value',
                type: 'line',
                smooth: true,
                showSymbol: true,
                areaStyle: { opacity: 0.08 },
                lineStyle: { width: 2 },
                data: data.values || []
            }]
        });

        const handler = function () { if (charts[domId]) charts[domId].resize(); };
        chart._wikiResize = handler;
        window.addEventListener('resize', handler);
    }

    function dispose(domId) {
        const c = charts[domId];
        if (c) {
            if (c._wikiResize) window.removeEventListener('resize', c._wikiResize);
            c.dispose();
            delete charts[domId];
        }
    }

    return { render: render, dispose: dispose };
})();
