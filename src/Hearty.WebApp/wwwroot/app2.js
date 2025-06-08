document.addEventListener('DOMContentLoaded', function () {

    const dateRangeForm = document.getElementById('dateRangeForm');
    const queryResultEl = document.getElementById('queryResult');

    dateRangeForm.addEventListener('submit', async function (event) {
        event.preventDefault();

        const timeValue = parseInt(document.getElementById('timeValue').value, 10);
        const timeUnit = document.getElementById('timeUnit').value;

        // 1. Calculate the date range
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

        // Format dates into ISO 8601 strings (e.g., "2025-06-07T12:30:00.000Z")
        const startISO = startDate.toISOString();
        const endISO = endDate.toISOString();

        // Fetch data
        const baseUrl = '/messages';
        const params = new URLSearchParams({
            start: startISO,
            end: endISO
        });
        const fullUrl = `${baseUrl}?${params.toString()}`;

        console.log(`👀 Fetching data from: ${fullUrl}`);
        queryResultEl.textContent = 'Loading...';

        try {
            const response = await fetch(fullUrl);

            if (!response.ok) {
                const errorText = await response.text();
                throw new Error(`💥 HTTP error! Status: ${response.status}. Message: ${errorText}`);
            }

            const data = await response.json();

            // Display the received data
            if (data && data.length > 0) {
                // Use JSON.stringify for pretty printing the JSON in the <pre> tag
                queryResultEl.textContent = JSON.stringify(data, null, 2);
            } else {
                queryResultEl.textContent = 'No data returned for the selected time range.';
            }

        } catch (error) {
            console.error('💥 Error fetching historical data:', error);
            queryResultEl.textContent = `💥 Error fetching data:\n${error.message}`;
        }
    });
});
