document.addEventListener('DOMContentLoaded', function () {
    // Establish a connection to the SignalR hub
    const connection = new signalR.HubConnectionBuilder()
        .withUrl("/stream")
        .configureLogging(signalR.LogLevel.Information)
        .build();

    // Chart.js Config.
    // A line chart to display real-time BPM data.
    // The chart will update as new data comes in from the SignalR stream.
    const ctx = document.getElementById('realtimeChart').getContext('2d');
    const chart = new Chart(ctx, {
        type: 'line',
        data: {
            labels: [],
            datasets: [{
                label: 'BPM Live Data',
                data: [],
                borderColor: 'rgb(75, 192, 192)',
                tension: 0.1
            }]
        },
        options: {
            responsive: true,
            scales: {
                x: {
                    type: 'time',
                    time: {
                        unit: 'second'
                    }
                },
                y: {
                    beginAtZero: true
                }
            }
        }
    });

    // Array to hold all values for aggregate calculations
    let allValues = [];

    // Aggregate Data Functions
    function updateAggregates() {
        if (allValues.length > 0) {
            const min = Math.min(...allValues);
            const max = Math.max(...allValues);
            const sum = allValues.reduce((a, b) => a + b, 0);
            const avg = (sum / allValues.length).toFixed(0);

            document.getElementById('minValue').textContent = min;
            document.getElementById('maxValue').textContent = max;
            document.getElementById('avgValue').textContent = avg;
        }
    }

    // Start the SignalR Connection
    async function start() {
        try {
            await connection.start();
            console.log("🔌 SignalR Connected.");
        } catch (err) {
            console.error("❌", err);
            setTimeout(start, 5000);
        }

    };

    // Stream Data from SignalR Hub
    // This function will wait for the connection to be established before subscribing to the stream.
    // We shouldn't have to call this function multiple times, as it will handle reconnections automatically.
    async function stream() {

        while (connection.state !== signalR.HubConnectionState.Connected) {
            console.log("🔌 Waiting for connection...");
            await new Promise(resolve => setTimeout(resolve, 1000));
        }

        // --- SignalR Stream Subscription ---
        connection.stream("StreamData")
            .subscribe({
                next: (data) => {
                    const timestamp = new Date(data.timestamp);
                    const value = data.value;

                    // Add new data to the chart
                    chart.data.labels.push(timestamp);
                    chart.data.datasets[0].data.push(value);
                    chart.update();

                    // Update aggregate data
                    allValues.push(value);
                    updateAggregates();
                },
                complete: () => {
                    console.log("✅ Stream completed");
                },
                error: (err) => {
                    console.error("❌", err);
                },
            });
    }

    // Handle connection close and attempt to reconnect
    connection.onclose(async () => {
        console.error("⚡ Connection closed. Attempting to reconnect...");
        await start();
    });

    // Start the connection and begin streaming data
    start().then(() => {
        stream();
    });
});
