let myChart;
let allQuotes = [];
let selectedSymbol = 'AAPL';
let searchTerm = '';

async function fetchStocks() {
    try {
        const response = await fetch('/api/stocks');
        const data = await response.json();
        console.log("Data received:", data); // CHECK IN CONSOLE
        allQuotes = data;
        render();
    } catch (err) {
        console.error("Error fetching data:", err);
    }
}

async function handleSearchInput() {
    const input = document.getElementById('searchInput');
    if (!input) return;
    searchTerm = input.value.toUpperCase();
    render();

    if (searchTerm.length >= 2) {
        try {
            const response = await fetch(`/api/stocks/autocomplete/${searchTerm}`);
            if (response.ok) {
                const suggestions = await response.json();
                const datalist = document.getElementById('stock-suggestions');
                // We use the optional chain ?. to avoid errors
                datalist.innerHTML = (suggestions || []).map(s =>
                    `<option value="${s.symbol}">${s.shortname || s.name || ''}</option>`
                ).join('');
            }
        } catch (err) { console.error("Autocomplete error:", err); }
    }
}

function render() {
    const body = document.getElementById('stocks-body');
    const searchContainer = document.getElementById('global-search-container');
    if (!body) return;

    // Create map last price
    const latestMap = new Map();
    // Create, for last the records were first
    const sorted = [...allQuotes].sort((a, b) => new Date(b.recordedAt) - new Date(a.recordedAt));

    sorted.forEach(q => {
        if (!latestMap.has(q.symbol)) latestMap.set(q.symbol, q);
    });

    const uniqueStocks = Array.from(latestMap.values());
    const filtered = uniqueStocks.filter(s => s.symbol.toUpperCase().includes(searchTerm));

    // Draw the table (make sure the field names match the JSON)
    body.innerHTML = filtered.map(s => `
        <tr onclick="selectSymbol('${s.symbol}')" class="${s.symbol === selectedSymbol ? 'table-active' : ''}" style="cursor:pointer">
            <td><strong>${s.symbol}</strong></td>
            <td>$${(s.price || 0).toFixed(2)}</td>
            <td class="${(s.changesPercentage || 0) >= 0 ? 'text-success' : 'text-danger'}">
                ${(s.changesPercentage || 0) >= 0 ? '▲' : '▼'} ${Math.abs(s.changesPercentage || 0).toFixed(2)}%
            </td>
            <td>${new Date(s.recordedAt).toLocaleTimeString()}</td>
        </tr>
    `).join('');

    // button gloobal search
    if (filtered.length === 0 && searchTerm.length >= 2) {
        searchContainer.innerHTML = `
            <div class="alert alert-info mt-2">
                Ticker "${searchTerm}" not found. 
                <button class="btn btn-sm btn-primary ms-2" onclick="searchGlobal('${searchTerm}')">Search Global</button>
            </div>`;
    } else {
        searchContainer.innerHTML = '';
    }

    updateChart();
}

async function searchGlobal(symbol) {
    try {
        const response = await fetch(`/api/stocks/search/${symbol}`);
        if (response.ok) {
            await fetchStocks();
            document.getElementById('searchInput').value = '';
            searchTerm = '';
            selectedSymbol = symbol.toUpperCase();
            render();
        }
    } catch (err) { alert("Error adding stock"); }
}

function selectSymbol(symbol) {
    selectedSymbol = symbol;
    const title = document.getElementById('chart-title');
    if (title) title.innerText = `Price History: ${symbol}`;
    render();
}

function updateChart() {
    const canvas = document.getElementById('stockChart');
    if (!canvas) return;
    const ctx = canvas.getContext('2d');

    // 1. Filter the data. Try both 'Price' and 'price' in case of different JSON settings
    const history = allQuotes
        .filter(q => (q.symbol || q.Symbol) === selectedSymbol)
        .sort((a, b) => new Date(a.recordedAt || a.RecordedAt) - new Date(b.recordedAt || b.RecordedAt));

    if (myChart) myChart.destroy();

    if (history.length === 0) {
        ctx.clearRect(0, 0, canvas.width, canvas.height);
        ctx.fillStyle = "#888";
        ctx.textAlign = "center";
        ctx.fillText("No price history for " + selectedSymbol, canvas.width / 2, canvas.height / 2);
        return;
    }

     
    const prices = history.map(q => q.price !== undefined ? q.price : q.Price);
    const labels = history.map(q => {
        const date = new Date(q.recordedAt || q.RecordedAt);
        return date.toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' });
    });

    myChart = new Chart(ctx, {
        type: 'line',
        data: {
            labels: labels,
            datasets: [{
                label: `Price of ${selectedSymbol} ($)`,
                data: prices,
                borderColor: '#0d6efd',
                backgroundColor: 'rgba(13, 110, 253, 0.1)',
                borderWidth: 3,
                fill: true,
                tension: 0.4,  
                pointRadius: history.length === 1 ? 6 : 2,
                pointBackgroundColor: '#0d6efd'
            }]
        },
        options: {
            responsive: true,
            maintainAspectRatio: false,
            scales: {
                y: {
                    beginAtZero: false,  
                    grid: { color: '#333' },
                    ticks: { color: '#aaa', callback: (value) => '$' + value.toFixed(2) }
                },
                x: {
                    grid: { display: false },
                    ticks: { color: '#aaa' }
                }
            },
            plugins: {
                legend: { display: false }
            }
        }
    });
}

fetchStocks();
setInterval(fetchStocks, 60000);