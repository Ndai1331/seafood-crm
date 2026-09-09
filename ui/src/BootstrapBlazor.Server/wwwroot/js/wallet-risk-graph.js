(() => {
  const charts = new Map();
  const text = value => String(value ?? '').replace(/[&<>'"]/g, char => ({ '&': '&amp;', '<': '&lt;', '>': '&gt;', "'": '&#39;', '"': '&quot;' })[char]);

  const positionNodes = (element, input) => {
    const width = Math.max(element.clientWidth, 640);
    const height = Math.max(element.clientHeight, 320);
    const centerX = width / 2;
    const columns = new Map();
    const normalized = input.map(node => {
      const direction = node.type === 'start' ? 'start' : node.direction === 'in' ? 'in' : 'out';
      const hop = node.type === 'start' ? 0 : Math.max(1, Number(node.hop) || 1);
      return { node, direction, hop, key: `${direction}:${hop}` };
    });
    for (const item of normalized) {
      if (!columns.has(item.key)) columns.set(item.key, []);
      columns.get(item.key).push(item.node);
    }

    return normalized.map(({ node, direction, hop, key }) => {
      const column = columns.get(key) || [node];
      const index = column.indexOf(node);
      const spread = height / (column.length + 1);
      const x = node.type === 'start' ? centerX : centerX + (direction === 'in' ? -1 : 1) * hop * width * .2;
      return {
        ...node,
        name: node.id,
        x,
        y: node.type === 'start' ? height / 2 : spread * (index + 1),
        fixed: node.type === 'start',
        symbolSize: node.type === 'start' ? 58 : hop === 1 ? 38 : 30,
        itemStyle: { color: node.type === 'start' ? '#075985' : direction === 'in' ? '#0f766e' : '#9a3412' }
      };
    });
  };

  window.walletRiskGraph = {
    render(domId, payload, dotNetRef) {
      const element = document.getElementById(domId);
      if (!element || !window.echarts) return;
      this.dispose(domId);
      const chart = echarts.init(element);
      const nodes = positionNodes(element, payload.nodes || []);
      const links = (payload.edges || []).map(edge => ({ ...edge, source: edge.fromId, target: edge.toId, value: edge.amount, label: edge.amountLabel }));
      chart.setOption({
        animationDuration: 250,
        tooltip: { formatter: item => item.dataType === 'edge'
          ? `${text(item.data.label)}`
          : `<strong>${text(item.data.label)}</strong><br>${text(item.data.address)}` },
        // Pan-by-drag only: scrolling the page over the canvas must not zoom the graph away.
        series: [{ type: 'graph', layout: 'none', roam: 'move', draggable: true, data: nodes, links,
          label: { show: true, position: 'bottom', formatter: value => value.data.label || value.data.address },
          lineStyle: { color: '#94a3b8', width: 2, curveness: .04 },
          edgeSymbol: ['none', 'arrow'], edgeSymbolSize: 8,
          emphasis: { focus: 'adjacency' }
        }]
      });
      chart.on('click', item => {
        if (!dotNetRef) return;
        const address = item.dataType === 'node' ? item.data.address : null;
        const txHash = item.dataType === 'edge' ? item.data.txHash : null;
        dotNetRef.invokeMethodAsync('FilterEvidenceAsync', address, txHash);
      });
      const resize = () => chart.resize();
      charts.set(domId, { chart, resize, payload, dotNetRef });
      window.addEventListener('resize', resize);
    },
    resetView(domId) {
      // Re-render from the stored payload: positionNodes is deterministic, so this
      // restores the original centered layout no matter how far the user panned.
      const entry = charts.get(domId);
      if (entry) this.render(domId, entry.payload, entry.dotNetRef);
    },
    dispose(domId) {
      const entry = charts.get(domId);
      if (entry) {
        window.removeEventListener('resize', entry.resize);
        entry.chart.dispose();
      }
      charts.delete(domId);
    }
  };
})();
