import '../../../_content/BootstrapBlazor.Chart/js/chart.umd.js'
import Data from '../../../_content/BootstrapBlazor/modules/data.js'

// Track chart instances for cleanup
const chartInstances = {};

function destroyChart(id) {
    const existing = chartInstances[id] || Data.get(id);
    if (existing) {
        existing.destroy();
        Data.remove(id);
        delete chartInstances[id];
    }
}

function storeChart(id, chart) {
    chartInstances[id] = chart;
    Data.set(id, chart);
}

/**
 * Render PIC doughnut chart showing error + missing distribution
 */
export function renderPicChart(id, data) {
    destroyChart(id);

    const ctx = document.getElementById(id);
    if (!ctx) return;

    const chart = new Chart(ctx, {
        type: 'doughnut',
        data: {
            labels: data.labels,
            datasets: [{
                label: 'Error + Missing',
                data: data.errorData.map((e, i) => e + (data.missingData ? data.missingData[i] : 0)),
                backgroundColor: data.backgroundColor,
                borderWidth: 2
            }]
        },
        options: {
            responsive: true,
            maintainAspectRatio: false,
            plugins: {
                legend: { display: true, position: 'right' },
                tooltip: {
                    callbacks: {
                        label: function (ctx) {
                            const i = ctx.dataIndex;
                            const err = data.errorData[i] || 0;
                            const miss = data.missingData ? data.missingData[i] : 0;
                            const ok = data.okData ? data.okData[i] : 0;
                            return `${ctx.label}: Error=${err}, Missing=${miss}, OK=${ok}`;
                        }
                    }
                }
            }
        }
    });

    storeChart(id, chart);
}

/**
 * Render OS doughnut chart showing error distribution
 */
export function renderOsChart(id, data) {
    destroyChart(id);

    const ctx = document.getElementById(id);
    if (!ctx) return;

    const chart = new Chart(ctx, {
        type: 'doughnut',
        data: {
            labels: data.labels,
            datasets: [{
                label: 'Errors',
                data: data.errorData,
                backgroundColor: data.backgroundColor,
                borderWidth: 2
            }]
        },
        options: {
            responsive: true,
            maintainAspectRatio: false,
            plugins: {
                legend: { display: true, position: 'right' },
                tooltip: {
                    callbacks: {
                        label: function (ctx) {
                            const i = ctx.dataIndex;
                            const err = data.errorData[i] || 0;
                            const ok = data.okData ? data.okData[i] : 0;
                            return `${ctx.label}: Error=${err}, OK=${ok}`;
                        }
                    }
                }
            }
        }
    });

    storeChart(id, chart);
}

/**
 * Render daily trend line chart with OK/Missing/Error lines
 */
export function renderTrendChart(id, data) {
    destroyChart(id);

    const ctx = document.getElementById(id);
    if (!ctx) return;

    const chart = new Chart(ctx, {
        type: 'line',
        data: {
            labels: data.labels,
            datasets: [
                {
                    label: 'OK',
                    data: data.okData,
                    borderColor: '#28a745',
                    backgroundColor: 'rgba(40, 167, 69, 0.1)',
                    fill: true,
                    tension: 0.3
                },
                {
                    label: 'Missing',
                    data: data.missingData,
                    borderColor: '#dc3545',
                    backgroundColor: 'rgba(220, 53, 69, 0.1)',
                    fill: true,
                    tension: 0.3
                },
                {
                    label: 'Error',
                    data: data.errorData,
                    borderColor: '#fd7e14',
                    backgroundColor: 'rgba(253, 126, 20, 0.1)',
                    fill: true,
                    tension: 0.3
                }
            ]
        },
        options: {
            responsive: true,
            maintainAspectRatio: false,
            plugins: {
                legend: { display: true, position: 'top' }
            },
            scales: {
                y: { beginAtZero: true }
            }
        }
    });

    storeChart(id, chart);
}

/**
 * Destroy all tracked charts
 */
export function destroyAllCharts() {
    for (const id of Object.keys(chartInstances)) {
        destroyChart(id);
    }
}

export function init(id) {
    // No-op: charts are rendered via explicit calls
}

export function dispose(id) {
    destroyAllCharts();
}
