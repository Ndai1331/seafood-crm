// ECharts rendering functions for SEO Cost Dashboard V2
// Called from Blazor via JS Interop

window.seoDashboard = {
    charts: {},

    /**
     * Render a donut/pie chart
     * @param {string} elementId - DOM element ID
     * @param {Array} data - Array of {name, value} objects
     * @param {string} title - Chart title
     * @param {Array} colors - Color palette (optional)
     */
    renderDonutChart: function (elementId, data, title, colors, valueUnit, totalOverride, filterPeriodDays, layout) {
        var dom = document.getElementById(elementId);
        if (!dom) return;

        // Dispose existing chart
        if (this.charts[elementId]) {
            this.charts[elementId].dispose();
        }

        var chart = echarts.init(dom);
        this.charts[elementId] = chart;

        var defaultColors = [
            '#5470c6', '#ee6666', '#91cc75', '#fac858','#434348','#73c0de',
            '#3ba272', '#fc8452', '#9a60b4', '#ea7ccc', '#4dc9f6',
            '#f7a35c', '#7cb5ec', '#434348', '#90ed7d', '#f15c80'
        ];

        var unit = valueUnit || '';
        var total = totalOverride || data.reduce(function (sum, item) { return sum + (item.value || 0); }, 0);
        var cfg = layout || {};
        // annotated = labels around donut (name + %), center total — like cost-type overview screenshot
        var annotated = cfg.annotated === true || cfg.mode === 'annotated';
        var radius = cfg.radius || (annotated ? ['42%', '68%'] : ['40%', '70%']);
        var center = cfg.center || (annotated ? ['50%', '52%'] : ['65%', '55%']);
        var labelShow = cfg.labelShow != null ? cfg.labelShow !== false : true;
        var legendShow = cfg.legendShow != null ? cfg.legendShow !== false : !annotated;
        var labelFontSize = cfg.labelFontSize || (annotated ? 12 : 11);
        var legendFontSize = cfg.legendFontSize || 13;
        var centerFontSize = cfg.centerFontSize || (annotated ? 22 : 22);
        var centerSubFontSize = cfg.centerSubFontSize || 13;
        var graphicLeft = cfg.graphicLeft || (annotated ? 'center' : '60%');
        var graphicTop = cfg.graphicTop || (annotated ? 'middle' : '48%');
        var legendTop = cfg.legendTop != null ? cfg.legendTop : 40;
        var labelLineLength = cfg.labelLineLength != null ? cfg.labelLineLength : (annotated ? 16 : 10);
        var labelLineLength2 = cfg.labelLineLength2 != null ? cfg.labelLineLength2 : (annotated ? 20 : 8);

        var option = {
            title: {
                text: title || '',
                left: 'left',
                top: 0,
                show: !!(title && String(title).length),
                textStyle: {
                    fontSize: 16,
                    fontWeight: 600,
                    color: '#212b36'
                }
            },
            tooltip: {
                trigger: 'item',
                formatter: function (params) {
                    var raw = params.value || 0;
                    var avgDays = filterPeriodDays && totalOverride
                        ? Math.round(raw / totalOverride)
                        : raw;
                    var pct = params.data && params.data.percent != null
                        ? Number(params.data.percent).toFixed(1)
                        : params.percent.toFixed(1);
                    var periodHint = filterPeriodDays
                        ? ' ngày TB/keyword / ' + filterPeriodDays + ' ngày'
                        : '';
                    return '<b>' + params.name + '</b><br/>' + avgDays.toLocaleString('en-US') + periodHint + ' (' + pct + '%)';
                }
            },
            legend: {
                show: legendShow,
                orient: 'vertical',
                left: 'left',
                top: legendTop,
                itemWidth: 10,
                itemHeight: 10,
                itemGap: 8,
                textStyle: { fontSize: legendFontSize, color: '#475569' },
                formatter: function (name) {
                    var item = data.find(function (d) { return d.name === name; });
                    if (item) {
                        var pct = item.percent != null
                            ? Number(item.percent).toFixed(1)
                            : (total > 0 ? ((item.value / total) * 100).toFixed(1) : '0.0');
                        return name + '  ' + pct + '%';
                    }
                    return name;
                }
            },
            color: colors || defaultColors,
            series: [{
                type: 'pie',
                radius: radius,
                center: center,
                avoidLabelOverlap: true,
                minShowLabelAngle: annotated ? 2 : 0,
                itemStyle: {
                    borderRadius: annotated ? 4 : 6,
                    borderColor: '#fff',
                    borderWidth: annotated ? 2 : 2
                },
                label: {
                    show: labelShow,
                    formatter: function (params) {
                        var pct = params.data && params.data.percent != null
                            ? Number(params.data.percent).toFixed(1)
                            : params.percent.toFixed(1);
                        return params.name + '\n' + pct + '%';
                    },
                    fontSize: labelFontSize,
                    fontWeight: annotated ? 500 : 400,
                    color: annotated ? '#334155' : '#64748b',
                    lineHeight: annotated ? 16 : 14
                },
                labelLine: {
                    show: labelShow,
                    length: labelLineLength,
                    length2: labelLineLength2,
                    smooth: annotated ? 0.15 : false,
                    lineStyle: {
                        width: 1
                    }
                },
                emphasis: {
                    label: {
                        show: labelShow,
                        fontSize: Math.max(labelFontSize + 1, 12),
                        fontWeight: 'bold'
                    },
                    itemStyle: {
                        shadowBlur: 10,
                        shadowOffsetX: 0,
                        shadowColor: 'rgba(0, 0, 0, 0.2)'
                    }
                },
                data: data
            }]
        };

        // Center total — use title anchored on pie center (avoids graphic group misalignment)
        var centerTotalText = total >= 1e9
            ? (total / 1e9).toLocaleString('en-US', { maximumFractionDigits: 1 }) + 'B'
            : total >= 1e6
                ? (total / 1e6).toLocaleString('en-US', { maximumFractionDigits: 1 }) + 'M'
                : total.toLocaleString('en-US');

        // Always pin center total to the pie series center (not chart box center)
        var pieCx = Array.isArray(center) ? center[0] : '50%';
        var pieCy = Array.isArray(center) ? center[1] : '50%';

        var centerTitle = {
            text: centerTotalText,
            subtext: unit || 'Tổng',
            left: pieCx,
            top: pieCy,
            textAlign: 'center',
            textVerticalAlign: 'middle',
            padding: 0,
            itemGap: 2,
            textStyle: {
                fontSize: centerFontSize,
                fontWeight: 700,
                color: '#212b36',
                lineHeight: centerFontSize + 2
            },
            subtextStyle: {
                fontSize: centerSubFontSize,
                fontWeight: 400,
                color: '#919eab',
                lineHeight: centerSubFontSize + 2
            }
        };

        if (filterPeriodDays) {
            centerTitle.subtext = (unit || 'Tổng') + '\n/' + filterPeriodDays + ' ngày';
        }

        // Keep chart title (if any) + center total as separate title blocks
        if (option.title && option.title.show) {
            option.title = [option.title, centerTitle];
        } else {
            option.title = centerTitle;
        }

        chart.setOption(option);

        // Responsive resize
        window.addEventListener('resize', function () {
            chart.resize();
        });
    },

    /**
     * Render a bar chart
     * @param {string} elementId - DOM element ID
     * @param {Array} categories - X-axis categories
     * @param {Array} values - Y-axis values
     * @param {string} title - Chart title
     */
    renderBarChart: function (elementId, categories, values, title) {
        var dom = document.getElementById(elementId);
        if (!dom) return;

        if (this.charts[elementId]) {
            this.charts[elementId].dispose();
        }

        var chart = echarts.init(dom);
        this.charts[elementId] = chart;

        var option = {
            title: {
                text: title,
                left: 'left',
                textStyle: {
                    fontSize: 16,
                    fontWeight: 600,
                    color: '#212b36'
                }
            },
            tooltip: {
                trigger: 'axis',
                formatter: function (params) {
                    var p = params[0];
                    return '<b>' + p.name + '</b><br/>' + p.value.toLocaleString('vi-VN');
                }
            },
            xAxis: {
                type: 'category',
                data: categories,
                axisLabel: {
                    rotate: 30,
                    fontSize: 11
                }
            },
            yAxis: {
                type: 'value',
                axisLabel: {
                    formatter: function (v) {
                        return v >= 1e9 ? (v / 1e9).toFixed(1) + 'B' :
                            v >= 1e6 ? (v / 1e6).toFixed(1) + 'M' :
                                v >= 1e3 ? (v / 1e3).toFixed(1) + 'K' : v;
                    }
                }
            },
            color: ['#5470c6'],
            series: [{
                type: 'bar',
                data: values,
                barWidth: '50%',
                itemStyle: {
                    borderRadius: [4, 4, 0, 0]
                }
            }],
            grid: {
                left: '8%',
                right: '5%',
                bottom: '15%',
                containLabel: true
            }
        };

        chart.setOption(option);
        window.addEventListener('resize', function () { chart.resize(); });
    },

    /**
     * Render a stacked bar chart
     * @param {string} elementId - DOM element ID
     * @param {Array} categories - X-axis categories
     * @param {Array} seriesData - Array of {name, data} objects
     * @param {string} title - Chart title
     */
    renderStackedBarChart: function (elementId, categories, seriesData, title) {
        var dom = document.getElementById(elementId);
        if (!dom) return;

        if (this.charts[elementId]) {
            this.charts[elementId].dispose();
        }

        var chart = echarts.init(dom);
        this.charts[elementId] = chart;

        var defaultColors = [
            '#5470c6', '#91cc75', '#fac858', '#ee6666', '#73c0de',
            '#3ba272', '#fc8452', '#9a60b4', '#ea7ccc', '#4dc9f6'
        ];

        var series = seriesData.map(function (s, idx) {
            return {
                name: s.name,
                type: 'bar',
                stack: 'total',
                emphasis: { focus: 'series' },
                itemStyle: {
                    color: defaultColors[idx % defaultColors.length]
                },
                data: s.data
            };
        });

        var option = {
            title: {
                text: title,
                left: 'left',
                textStyle: {
                    fontSize: 16,
                    fontWeight: 600,
                    color: '#212b36'
                }
            },
            tooltip: {
                trigger: 'axis',
                axisPointer: { type: 'shadow' },
                formatter: function (params) {
                    var result = '<b>' + params[0].name + '</b>';
                    params.forEach(function (p) {
                        if (p.value > 0) {
                            result += '<br/>' + p.marker + ' ' + p.seriesName + ': ' + p.value.toLocaleString('vi-VN');
                        }
                    });
                    return result;
                }
            },
            legend: {
                top: 30,
                textStyle: { fontSize: 12 }
            },
            grid: {
                left: '5%',
                right: '5%',
                bottom: '10%',
                top: 80,
                containLabel: true
            },
            xAxis: {
                type: 'category',
                data: categories,
                axisLabel: {
                    rotate: 20,
                    fontSize: 11
                }
            },
            yAxis: {
                type: 'value',
                axisLabel: {
                    formatter: function (v) {
                        return v >= 1e6 ? (v / 1e6).toFixed(1) + 'M' :
                            v >= 1e3 ? (v / 1e3).toFixed(1) + 'K' : v;
                    }
                }
            },
            series: series
        };

        chart.setOption(option);
        window.addEventListener('resize', function () { chart.resize(); });
    },

    /**
     * Render a horizontal bar chart (Metabase "row" display type)
     * @param {string} elementId - DOM element ID
     * @param {Array} categories - Y-axis categories (employee names)
     * @param {Array} values - X-axis values (costs)
     * @param {string} title - Chart title
     */
    renderHorizontalBarChart: function (elementId, categories, values, title) {
        var dom = document.getElementById(elementId);
        if (!dom) return;

        if (this.charts[elementId]) {
            this.charts[elementId].dispose();
        }

        var chart = echarts.init(dom);
        this.charts[elementId] = chart;

        var numericValues = values.map(function (v) { return Number(v) || 0; });
        var maxVal = Math.max.apply(null, numericValues.concat([0]));
        var axisMax = maxVal > 0 ? maxVal * 1.12 : 1;

        var option = {
            tooltip: {
                trigger: 'axis',
                axisPointer: { type: 'shadow' },
                formatter: function (params) {
                    var p = params[0];
                    var v = p.value;
                    var formatted = v >= 1e9 ? (v / 1e9).toFixed(1) + ' tỷ' :
                        v >= 1e6 ? (v / 1e6).toFixed(0) + ' triệu' :
                            v.toLocaleString('vi-VN');
                    return '<b>' + p.name + '</b><br/>' + formatted;
                }
            },
            grid: {
                left: '3%',
                right: '12%',
                bottom: '3%',
                top: 10,
                containLabel: true
            },
            xAxis: {
                type: 'value',
                min: 0,
                max: axisMax,
                axisLabel: {
                    formatter: function (v) {
                        return v >= 1e9 ? (v / 1e9).toFixed(0) + 'B' :
                            v >= 1e6 ? (v / 1e6).toFixed(0) + 'M' : v;
                    },
                    fontSize: 11
                }
            },
            yAxis: {
                type: 'category',
                data: categories.slice().reverse(),
                axisLabel: {
                    fontSize: 11,
                    width: 110,
                    overflow: 'truncate'
                },
                inverse: false
            },
            series: [{
                type: 'bar',
                data: values.slice().reverse(),
                barWidth: '55%',
                itemStyle: {
                    borderRadius: [0, 4, 4, 0],
                    color: function (params) {
                        var colors = [
                            '#5470c6', '#91cc75', '#fac858', '#ee6666', '#73c0de',
                            '#3ba272', '#fc8452', '#9a60b4', '#ea7ccc', '#4dc9f6',
                            '#5470c6', '#91cc75', '#fac858', '#ee6666', '#73c0de'
                        ];
                        return colors[params.dataIndex % colors.length];
                    }
                },
                label: {
                    show: true,
                    position: 'right',
                    formatter: function (params) {
                        var v = params.value;
                        return v >= 1e9 ? (v / 1e9).toFixed(1) + 'tỷ' :
                            v >= 1e6 ? (v / 1e6).toFixed(1) + 'triệu' :
                                v.toLocaleString('vi-VN');
                    },
                    fontSize: 11,
                    color: '#637381'
                }
            }]
        };

        chart.setOption(option);
        window.addEventListener('resize', function () { chart.resize(); });
    },

    /**
     * Dispose a chart to free memory
     * @param {string} elementId - DOM element ID
     */
    /**
     * Render a multi-series line chart for ranking trends
     */
    renderMultiLineChart: function (elementId, categories, seriesData, title, yAxisPercent) {
        var dom = document.getElementById(elementId);
        if (!dom) return;

        if (this.charts[elementId]) {
            this.charts[elementId].dispose();
        }

        var chart = echarts.init(dom);
        this.charts[elementId] = chart;

        var colors = ['#5470c6', '#91cc75', '#fac858', '#ee6666'];

        function getPointValue(point) {
            if (point == null) return 0;
            if (typeof point === 'object' && point.value != null) return Number(point.value) || 0;
            return Number(point) || 0;
        }

        function getPointDomains(point) {
            if (point && typeof point === 'object' && Array.isArray(point.domains)) {
                return point.domains.filter(function (d) { return d; });
            }
            return [];
        }

        function formatDomainTooltip(count, domains) {
            var rounded = Math.round(count);
            var suffix = ' domain';
            if (domains.length > 0) {
                return rounded + suffix + ' (' + domains.join(', ') + ')';
            }
            return rounded + suffix;
        }

        var series = (seriesData || []).map(function (s, idx) {
            return {
                name: s.name,
                type: 'line',
                smooth: true,
                symbol: 'circle',
                symbolSize: 6,
                itemStyle: { color: colors[idx % colors.length] },
                data: s.data
            };
        });

        chart.setOption({
            title: {
                text: title,
                left: 'left',
                textStyle: { fontSize: 16, fontWeight: 600, color: '#212b36' }
            },
            tooltip: {
                trigger: 'axis',
                confine: true,
                extraCssText: 'max-width:420px;white-space:normal;word-break:break-all;',
                formatter: yAxisPercent
                    ? undefined
                    : function (params) {
                        if (!params || !params.length) return '';
                        var result = '<b>' + params[0].axisValue + '</b>';
                        params.forEach(function (p) {
                            var count = getPointValue(p.data);
                            var domains = getPointDomains(p.data);
                            result += '<br/>' + p.marker + ' ' + p.seriesName + ': '
                                + formatDomainTooltip(count, domains);
                        });
                        return result;
                    },
                valueFormatter: yAxisPercent
                    ? function (v) { return v + '%'; }
                    : undefined
            },
            legend: { top: 30, textStyle: { fontSize: 12 } },
            grid: { left: '5%', right: '5%', bottom: '12%', top: 80, containLabel: true },
            xAxis: {
                type: 'category',
                data: categories,
                axisLabel: { rotate: 30, fontSize: 11 }
            },
            yAxis: {
                type: 'value',
                name: yAxisPercent ? undefined : 'Số domain',
                min: 0,
                max: yAxisPercent ? 100 : undefined,
                minInterval: 1,
                axisLabel: yAxisPercent ? { formatter: '{value}%' } : {}
            },
            color: colors,
            series: series
        });

        window.addEventListener('resize', function () { chart.resize(); });
    },

    /**
     * Render combo chart: bar (cost) + line (active keywords)
     */
    // barName/lineName/yLeftName/yRightName là tuỳ chọn: thiếu thì giữ nguyên nhãn cũ nên mọi
    // nơi gọi sẵn có không đổi gì. Có chúng thì chart nói đúng thứ đang vẽ, thay vì luôn ghi
    // "Keyword đang SEO" kể cả khi đường đó là số lần đạt top.
    renderComboChart: function (elementId, categories, barValues, lineValues, title, barName, lineName, yLeftName, yRightName) {
        barName = barName || 'Chi phí SEO';
        lineName = lineName || 'Keyword đang SEO';
        yLeftName = yLeftName || 'Chi phí';
        yRightName = yRightName || 'Keyword';
        var dom = document.getElementById(elementId);
        if (!dom) return;

        if (this.charts[elementId]) {
            this.charts[elementId].dispose();
        }

        var chart = echarts.init(dom);
        this.charts[elementId] = chart;

        chart.setOption({
            title: {
                text: title,
                left: 'left',
                textStyle: { fontSize: 16, fontWeight: 600, color: '#212b36' }
            },
            tooltip: {
                trigger: 'axis',
                axisPointer: { type: 'cross' }
            },
            legend: {
                data: [barName, lineName],
                top: 30
            },
            grid: { left: '5%', right: '8%', bottom: '12%', top: 80, containLabel: true },
            xAxis: {
                type: 'category',
                data: categories,
                axisLabel: { rotate: 30, fontSize: 11 }
            },
            yAxis: [
                {
                    type: 'value',
                    name: yLeftName,
                    axisLabel: {
                        formatter: function (v) {
                            return v >= 1e9 ? (v / 1e9).toFixed(1) + 'B' :
                                v >= 1e6 ? (v / 1e6).toFixed(0) + 'M' : v;
                        }
                    }
                },
                {
                    type: 'value',
                    name: yRightName,
                    minInterval: 1,
                    splitLine: { show: false }
                }
            ],
            series: [
                {
                    name: barName,
                    type: 'bar',
                    data: barValues,
                    itemStyle: { color: '#5470c6', borderRadius: [4, 4, 0, 0] }
                },
                {
                    name: lineName,
                    type: 'line',
                    yAxisIndex: 1,
                    smooth: true,
                    data: lineValues,
                    itemStyle: { color: '#91cc75' }
                }
            ]
        });

        window.addEventListener('resize', function () { chart.resize(); });
    },

    /**
     * Render performance bubble chart: x=top10 rate, y=cost per top10.
     */
    renderPerformanceBubbleChart: function (elementId, points, title) {
        var dom = document.getElementById(elementId);
        if (!dom) return;

        if (this.charts[elementId]) {
            this.charts[elementId].dispose();
        }

        var chart = echarts.init(dom);
        this.charts[elementId] = chart;

        var safePoints = Array.isArray(points) ? points : [];
        var top10Max = safePoints.reduce(function (max, p) {
            var value = Number(p.top10Rate) || 0;
            return Math.max(max, value);
        }, 0);
        var costMax = safePoints.reduce(function (max, p) {
            var value = Number(p.costPerTop10) || 0;
            return Math.max(max, value);
        }, 0);
        var positiveCosts = safePoints
            .map(function (p) { return Number(p.costPerTop10) || 0; })
            .filter(function (v) { return v > 0; });
        var costMinPositive = positiveCosts.length > 0 ? Math.min.apply(null, positiveCosts) : 0;
        var spreadRatio = costMinPositive > 0 ? costMax / costMinPositive : 0;
        var shouldUseLogScale = spreadRatio >= 20;

        function toStatusLabel(status) {
            if (status === 'Inactive') return 'Đã ngưng SEO';
            if (status === 'Intermittent') return 'SEO ngắt quãng';
            return 'Đang SEO';
        }

        function toStatusColor(status) {
            if (status === 'Inactive') return '#ef4444';
            if (status === 'Intermittent') return '#f59e0b';
            return '#22c55e';
        }

        function toBubbleSize(keywordCount) {
            var count = Math.max(1, Number(keywordCount) || 1);
            return Math.min(60, 14 + Math.sqrt(count) * 7);
        }

        var seriesData = safePoints.map(function (p) {
            var rawCostPerTop10 = Number(p.costPerTop10) || 0;
            var plottedCostPerTop10 = shouldUseLogScale
                ? Math.max(1, rawCostPerTop10)
                : rawCostPerTop10;
            return {
                name: p.pic,
                value: [
                    Number(p.top10Rate) || 0,
                    plottedCostPerTop10,
                    Number(p.keywordCount) || 0,
                    Number(p.top10Count) || 0,
                    Number(p.top3Count) || 0,
                    Number(p.totalCost) || 0,
                    rawCostPerTop10
                ],
                status: p.status,
                itemStyle: {
                    color: toStatusColor(p.status),
                    opacity: 0.8
                },
                symbolSize: toBubbleSize(p.keywordCount)
            };
        });

        chart.setOption({
            title: {
                text: title,
                left: 'left',
                textStyle: { fontSize: 16, fontWeight: 600, color: '#212b36' }
            },
            tooltip: {
                trigger: 'item',
                formatter: function (params) {
                    var v = params.value || [];
                    return '<b>' + params.name + '</b>'
                        + '<br/>Trạng thái: ' + toStatusLabel(params.data.status)
                        + '<br/>% keyword Top 10: ' + (v[0] || 0).toFixed(1) + '%'
                        + '<br/>Chi phí / Top 10: ' + Math.round(v[6] || 0).toLocaleString('vi-VN')
                        + '<br/>Tổng keyword: ' + Math.round(v[2] || 0).toLocaleString('vi-VN')
                        + '<br/>Keyword Top 10: ' + Math.round(v[3] || 0).toLocaleString('vi-VN')
                        + '<br/>Keyword Top 1-3: ' + Math.round(v[4] || 0).toLocaleString('vi-VN')
                        + '<br/>Tổng chi phí: ' + Math.round(v[5] || 0).toLocaleString('vi-VN');
                }
            },
            legend: {
                top: 34,
                data: ['Đang SEO', 'SEO ngắt quãng', 'Đã ngưng SEO']
            },
            grid: {
                left: '6%',
                right: '5%',
                bottom: '12%',
                top: 90,
                containLabel: true
            },
            xAxis: {
                type: 'value',
                name: '% keyword Top 10',
                min: 0,
                max: Math.max(100, Math.ceil(top10Max / 10) * 10),
                axisLabel: {
                    formatter: function (v) { return v + '%'; }
                }
            },
            yAxis: {
                type: shouldUseLogScale ? 'log' : 'value',
                name: shouldUseLogScale
                    ? 'Chi phí / keyword Top 10 (log scale)'
                    : 'Chi phí / keyword Top 10',
                min: shouldUseLogScale ? Math.max(1, Math.floor(costMinPositive || 1)) : 0,
                max: shouldUseLogScale
                    ? Math.max(10, Math.ceil((costMax || 10) * 1.1))
                    : (costMax > 0 ? Math.ceil(costMax * 1.1) : 1),
                axisLabel: {
                    formatter: function (v) { return Math.round(v).toLocaleString('vi-VN'); }
                }
            },
            series: [{
                type: 'scatter',
                data: seriesData
            }]
        });

        window.addEventListener('resize', function () { chart.resize(); });
    },

    /**
     * Render mixed chart: bar (%Top10) + line (cost/top10)
     */
    renderPerformanceMixedChart: function (
        elementId,
        categories,
        top10DayRates,
        costPerTop10Day,
        statuses,
        totalDays,
        top10Days,
        top3Days,
        totalCosts,
        title
    ) {
        var dom = document.getElementById(elementId);
        if (!dom) return;

        if (this.charts[elementId]) {
            this.charts[elementId].dispose();
        }

        var chart = echarts.init(dom);
        this.charts[elementId] = chart;

        var labels = Array.isArray(categories) ? categories : [];
        var rates = Array.isArray(top10DayRates) ? top10DayRates : [];
        var costs = Array.isArray(costPerTop10Day) ? costPerTop10Day : [];
        var sts = Array.isArray(statuses) ? statuses : [];
        var days = Array.isArray(totalDays) ? totalDays : [];
        var t10 = Array.isArray(top10Days) ? top10Days : [];
        var t3 = Array.isArray(top3Days) ? top3Days : [];
        var totals = Array.isArray(totalCosts) ? totalCosts : [];

        function toStatusLabel(status) {
            if (status === 'Inactive') return 'Đã ngưng SEO';
            if (status === 'Intermittent') return 'SEO ngắt quãng';
            return 'Đang SEO';
        }

        function toBarColor(status) {
            if (status === 'Inactive') return '#ef4444';
            if (status === 'Intermittent') return '#f59e0b';
            return '#22c55e';
        }

        var costMax = costs.reduce(function (max, v) { return Math.max(max, Number(v) || 0); }, 0);

        var rateSeriesData = labels.map(function (_, i) {
            return {
                value: Number(rates[i]) || 0,
                itemStyle: { color: toBarColor(sts[i]) }
            };
        });

        chart.setOption({
            title: {
                text: title,
                left: 'left',
                textStyle: { fontSize: 16, fontWeight: 600, color: '#212b36' }
            },
            legend: {
                top: 34,
                data: ['% ngày Top 10', 'Chi phí / ngày Top 10']
            },
            tooltip: {
                trigger: 'axis',
                axisPointer: { type: 'cross' },
                formatter: function (params) {
                    if (!params || !params.length) return '';
                    var i = params[0].dataIndex;
                    return '<b>' + labels[i] + '</b>'
                        + '<br/>Trạng thái: ' + toStatusLabel(sts[i])
                        + '<br/>% ngày Top 10: ' + (Number(rates[i]) || 0).toFixed(1) + '%'
                        + '<br/>Chi phí / ngày Top 10: ' + Math.round(Number(costs[i]) || 0).toLocaleString('vi-VN')
                        + '<br/>Tổng ngày: ' + Math.round(Number(days[i]) || 0).toLocaleString('vi-VN')
                        + '<br/>Số ngày Top 10: ' + Math.round(Number(t10[i]) || 0).toLocaleString('vi-VN')
                        + '<br/>Số ngày Top 1-3: ' + Math.round(Number(t3[i]) || 0).toLocaleString('vi-VN')
                        + '<br/>Tổng chi phí: ' + Math.round(Number(totals[i]) || 0).toLocaleString('vi-VN');
                }
            },
            grid: {
                left: '6%',
                right: '8%',
                bottom: '16%',
                top: 90,
                containLabel: true
            },
            xAxis: {
                type: 'category',
                data: labels,
                axisLabel: {
                    rotate: 25,
                    fontSize: 11
                }
            },
            yAxis: [
                {
                    type: 'value',
                    name: '% ngày Top 10',
                    min: 0,
                    max: 100,
                    axisLabel: {
                        formatter: function (v) { return v + '%'; }
                    }
                },
                {
                    type: 'value',
                    name: 'Chi phí / ngày Top 10',
                    min: 0,
                    max: costMax > 0 ? Math.ceil(costMax * 1.15) : 1,
                    axisLabel: {
                        formatter: function (v) { return Math.round(v).toLocaleString('vi-VN'); }
                    },
                    splitLine: { show: false }
                }
            ],
            series: [
                {
                    name: '% ngày Top 10',
                    type: 'bar',
                    data: rateSeriesData,
                    barMaxWidth: 36,
                    itemStyle: {
                        borderRadius: [5, 5, 0, 0]
                    }
                },
                {
                    name: 'Chi phí / ngày Top 10',
                    type: 'line',
                    yAxisIndex: 1,
                    data: costs.map(function (x) { return Number(x) || 0; }),
                    smooth: true,
                    symbol: 'circle',
                    symbolSize: 7,
                    itemStyle: { color: '#3366ff' },
                    lineStyle: { width: 2 }
                }
            ]
        });

        window.addEventListener('resize', function () { chart.resize(); });
    },

    renderMiniLineChart: function (elementId, values, color, labels, kind) {
        var dom = document.getElementById(elementId);
        if (!dom) return;

        if (this.charts[elementId]) {
            this.charts[elementId].dispose();
        }

        var chart = echarts.init(dom);
        this.charts[elementId] = chart;

        var data = (values || []).map(function (v) { return Number(v) || 0; });
        if (data.length === 0) data = [0, 0];
        if (data.length === 1) data = [data[0], data[0]];

        var max = Math.max.apply(null, data);
        var min = Math.min.apply(null, data);
        var pad = max === min ? Math.max(1, Math.abs(max) * 0.2) : (max - min) * 0.22;
        var xLabels = Array.isArray(labels) && labels.length === data.length
            ? labels
            : data.map(function (_, index) { return index + 1; });

        function compactMoney(value) {
            var n = Number(value) || 0;
            if (Math.abs(n) >= 1e9) return (n / 1e9).toFixed(1).replace('.0', '') + ' tỷ';
            if (Math.abs(n) >= 1e6) return (n / 1e6).toFixed(1).replace('.0', '') + ' triệu';
            return Math.round(n).toLocaleString('vi-VN') + ' đ';
        }

        function tooltipValue(value) {
            var n = Number(value) || 0;
            if (kind === 'cost') return 'Số tiền: ' + compactMoney(n);
            if (kind === 'top-count') return 'Số lần lên top: ' + Math.round(n).toLocaleString('vi-VN');
            if (kind === 'avg-rank') return 'Top trung bình: ' + n.toLocaleString('vi-VN', { maximumFractionDigits: 1 });
            return 'Số lượng: ' + Math.round(n).toLocaleString('vi-VN');
        }

        chart.setOption({
            animation: true,
            grid: { left: 0, right: 0, top: 4, bottom: 2 },
            xAxis: {
                type: 'category',
                boundaryGap: false,
                data: xLabels,
                show: false
            },
            yAxis: {
                type: 'value',
                show: false,
                min: min - pad,
                max: max + pad
            },
            tooltip: {
                trigger: 'axis',
                confine: true,
                formatter: function (params) {
                    var p = params && params[0] ? params[0] : { name: '', value: 0 };
                    return '<b>Tháng ' + p.name + '</b><br/>' + tooltipValue(p.value);
                }
            },
            series: [{
                type: 'line',
                data: data,
                smooth: false,
                symbol: 'circle',
                symbolSize: 4,
                lineStyle: {
                    width: 3,
                    color: color || '#1890ff'
                },
                areaStyle: {
                    color: {
                        type: 'linear',
                        x: 0, y: 0, x2: 0, y2: 1,
                        colorStops: [
                            { offset: 0, color: (color || '#1890ff') + '22' },
                            { offset: 1, color: (color || '#1890ff') + '00' }
                        ]
                    }
                }
            }]
        });

        window.addEventListener('resize', function () { chart.resize(); });
    },

    /**
     * Compact donut (PHÂN LOẠI KEYWORD style).
     * @param {string} elementId
     * @param {Array} data - [{ name, value, percent }]
     * @param {string[]} colors
     * @param {number} [totalOverride]
     * @param {{ size?: number, fontSize?: number, highlightNames?: string[] }} [options]
     *   Optional fields — omit to keep default 92px / 16px (existing callers unchanged).
     *   highlightNames: slice names to emphasize (bigger tooltip + stronger emphasis).
     */
    renderCompactDonutChart: function (elementId, data, colors, totalOverride, options) {
        var dom = document.getElementById(elementId);
        if (!dom) return;

        if (this.charts[elementId]) {
            this.charts[elementId].dispose();
        }

        options = options || {};
        var size = Number(options.size) > 0 ? Number(options.size) : 92;
        var fontSize = Number(options.fontSize) > 0 ? Number(options.fontSize) : 16;
        var highlightNames = Array.isArray(options.highlightNames)
            ? options.highlightNames.map(function (n) { return String(n || '').toUpperCase(); })
            : [];

        function isHighlight(name) {
            return highlightNames.indexOf(String(name || '').toUpperCase()) >= 0;
        }

        // Only override DOM box when caller passes size (keeps CSS default for old charts)
        if (Number(options.size) > 0) {
            dom.style.width = size + 'px';
            dom.style.height = size + 'px';
            dom.style.flex = '0 0 ' + size + 'px';
        }

        var chart = echarts.init(dom);
        this.charts[elementId] = chart;
        var total = totalOverride || (data || []).reduce(function (sum, item) { return sum + (Number(item.value) || 0); }, 0);

        var hasHighlight = highlightNames.length > 0;
        var seriesData = (data || []).map(function (item) {
            var hl = isHighlight(item.name);
            if (!hl) return item;
            // Permanently "pop out" like hover/selected state
            return Object.assign({}, item, {
                selected: true,
                itemStyle: {
                    borderRadius: 6,
                    borderColor: '#fff',
                    borderWidth: 4,
                    shadowBlur: 12,
                    shadowColor: 'rgba(13, 110, 253, 0.5)'
                },
                emphasis: {
                    scale: true,
                    scaleSize: 10
                }
            });
        });

        chart.setOption({
            color: colors || ['#22c55e', '#f59e0b', '#ef4444'],
            tooltip: {
                trigger: 'item',
                confine: true,
                formatter: function (params) {
                    var pct = params.data && params.data.percent != null
                        ? Number(params.data.percent).toFixed(1)
                        : params.percent.toFixed(1);
                    var valueText = Number(params.value || 0).toLocaleString('vi-VN') + ' (' + pct + '%)';
                    if (isHighlight(params.name)) {
                        return '<div style="padding:2px 0;line-height:1.35;">'
                            + '<div style="font-size:18px;font-weight:800;color:#0d6efd;letter-spacing:.2px;">' + params.name + '</div>'
                            + '<div style="font-size:15px;font-weight:700;color:#212b36;margin-top:2px;">' + valueText + '</div>'
                            + '</div>';
                    }
                    return '<b>' + params.name + '</b><br/>' + valueText;
                }
            },
            series: [{
                type: 'pie',
                radius: ['58%', '82%'],
                center: ['50%', '50%'],
                // Keep highlighted slice exploded without requiring hover
                selectedMode: hasHighlight ? 'multiple' : false,
                selectedOffset: hasHighlight ? 12 : 0,
                avoidLabelOverlap: true,
                label: { show: false },
                labelLine: { show: false },
                itemStyle: {
                    borderRadius: 5,
                    borderColor: '#fff',
                    borderWidth: 3
                },
                emphasis: {
                    scale: true,
                    scaleSize: 4
                },
                select: {
                    itemStyle: {
                        shadowBlur: 12,
                        shadowColor: 'rgba(13, 110, 253, 0.5)'
                    }
                },
                data: seriesData
            }],
            graphic: [{
                type: 'text',
                left: 'center',
                top: 'middle',
                style: {
                    text: Number(total || 0).toLocaleString('vi-VN'),
                    textAlign: 'center',
                    fontSize: fontSize,
                    fontWeight: 700,
                    fill: '#212b36'
                }
            }]
        });

        window.addEventListener('resize', function () { chart.resize(); });
    },

    disposeChart: function (elementId) {
        if (this.charts[elementId]) {
            this.charts[elementId].dispose();
            delete this.charts[elementId];
        }
    },

    /**
     * Dispose all charts
     */
    disposeAll: function () {
        for (var key in this.charts) {
            if (this.charts.hasOwnProperty(key)) {
                this.charts[key].dispose();
            }
        }
        this.charts = {};
    }
};

// CSV download helper — called via JSRuntime.InvokeVoidAsync("downloadCsv", filename, content)
// BOM prefix ensures Excel opens UTF-8 correctly
window.downloadCsv = function (filename, content) {
    var blob = new Blob(['﻿' + content], { type: 'text/csv;charset=utf-8;' });
    var url = URL.createObjectURL(blob);
    var a = document.createElement('a');
    a.href = url;
    a.download = filename;
    document.body.appendChild(a);
    a.click();
    document.body.removeChild(a);
    URL.revokeObjectURL(url);
};
