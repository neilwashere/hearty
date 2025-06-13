window.initialiseHistoricalChart = function (chartData) {

    if (!chartData || chartData.length === 0) {
        // Nothing to do here, move along
        return;
    }

    const historicalCanvas = document.getElementById('historicalChart');
    if (!historicalCanvas) {
        console.error('Historical chart canvas not found');
        return;
    }

    // If a chart already exists, destroy it to avoid memory leaks and conflicts
    if (historicalCanvas.chart) {
        historicalCanvas.chart.destroy();
    }

    const historicalMinValueEl = document.getElementById('historicalMinValue');
    const historicalMaxValueEl = document.getElementById('historicalMaxValue');
    const historicalAvgValueEl = document.getElementById('historicalAvgValue');

    // Calculates and updates the aggregate values based on the currently visible data in the chart.
    function updateHistoricalAggregates(chart) {
        const { min, max } = chart.scales.x; // Get the min/max timestamp of the visible range

        // Filter the chart's full dataset to get only the points within the visible range
        const visibleDataPoints = chart.data.datasets[0].data.filter(p => p.x >= min && p.x <= max);

        if (visibleDataPoints.length === 0) {
            historicalMinValueEl.textContent = 'N/A';
            historicalMaxValueEl.textContent = 'N/A';
            historicalAvgValueEl.textContent = 'N/A';
            return;
        }

        const visibleValues = visibleDataPoints.map(p => p.y);
        const minValue = Math.min(...visibleValues);
        const maxValue = Math.max(...visibleValues);
        const sum = visibleValues.reduce((a, b) => a + b, 0);
        const avgValue = (sum / visibleValues.length).toFixed(0);

        historicalMinValueEl.textContent = minValue;
        historicalMaxValueEl.textContent = maxValue;
        historicalAvgValueEl.textContent = avgValue;
    }


        // Create the new chart instance
    const chart = new Chart(historicalCanvas, {
        type: 'line',
        data: {
            datasets: [{
                label: 'BPM Historical Data',
                data: chartData.map(item => ({
                    x: new Date(item.timestamp), // Convert timestamp to Date object
                    y: item.value // Assuming 'value' is the BPM value
                })),
                borderColor: 'rgb(153, 102, 255)',
                backgroundColor: 'rgba(153, 102, 255, 0.5)',
                borderWidth: 1,
                pointRadius: 1,
            }]
        },
        options: {
            scales: {
                x: {
                    type: 'time',
                    time: {
                        tooltipFormat: 'DD T'
                    },
                    title: {
                        display: true,
                        text: 'Date'
                    }
                },
                y: {
                    title: {
                        display: true,
                        text: 'Value'
                    }
                }
            },
            plugins: {
                zoom: {
                    pan: {
                        enabled: true,
                        mode: 'x', // Pan only the x-axis
                    },
                    zoom: {
                        wheel: {
                            enabled: true,
                        },
                        pinch: {
                            enabled: true
                        },
                        mode: 'x', // Zoom only the x-axis
                        onZoomComplete: ({chart}) => updateHistoricalAggregates(chart),
                        onPanComplete: ({chart}) => updateHistoricalAggregates(chart)
                    }
                }
            }
        }
    });

    // Store the chart instance on the canvas element for later reference
    historicalCanvas.chart = chart;
    // show the chart section
    document.getElementById('historical-chart-section').style.display = 'flex';
    // Update aggregates initially
    updateHistoricalAggregates(chart);
};
