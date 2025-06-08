document.addEventListener('DOMContentLoaded', function () {

    const dateRangeForm = document.getElementById('dateRangeForm');
    const queryResultEl = document.getElementById('queryResult');

    const historicalChartSection = document.getElementById('historical-chart-section');
    const historicalMinValueEl = document.getElementById('historicalMinValue');
    const historicalMaxValueEl = document.getElementById('historicalMaxValue');
    const historicalAvgValueEl = document.getElementById('historicalAvgValue');
    const historicalCanvas = document.getElementById('historicalChart');

    let historicalChart = null;

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

    dateRangeForm.addEventListener('submit', async function (event) {
        event.preventDefault();

        const timeValue = parseInt(document.getElementById('timeValue').value, 10);
        const timeUnit = document.getElementById('timeUnit').value;

        const endDate = new Date();
        const startDate = new Date();

        switch (timeUnit) {
            case 'seconds':
                startDate.setSeconds(startDate.getSeconds() - timeValue);
                break;
            case 'minutes':
                startDate.setMinutes(startDate.getMinutes() - timeValue);
                break;
            case 'hours':
                startDate.setHours(startDate.getHours() - timeValue);
                break;
            case 'days':
                startDate.setDate(startDate.getDate() - timeValue);
                break;
        }

        const startISO = startDate.toISOString();
        const endISO = endDate.toISOString();

        const baseUrl = '/messages';
        const params = new URLSearchParams({ start: startISO, end: endISO });
        const fullUrl = `${baseUrl}?${params.toString()}`;

        try {
            const response = await fetch(fullUrl);
            if (!response.ok) throw new Error(`HTTP error! Status: ${response.status}`);

            const data = await response.json();

            // Destroy the old chart instance if it exists
            if (historicalChart) {
                historicalChart.destroy();
            }

            // Format API data for Chart.js ({timestamp, value} -> {x, y})
            const chartData = data.map(item => {
                let parsed = JSON.parse(item);
                return { x: new Date(parsed.timestamp), y: parsed.value }
            });

            // Make the chart section visible
            historicalChartSection.style.display = 'flex';

            // Create the new chart instance
            historicalChart = new Chart(historicalCanvas, {
                type: 'line',
                data: {
                    datasets: [{
                        label: 'BPM Historical Data',
                        data: chartData,
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

            // Calculate initial aggregates for the entire dataset
            updateHistoricalAggregates(historicalChart);

        } catch (error) {
            console.error('💥 Error fetching/rendering historical data:', error);
        }
    });

});
