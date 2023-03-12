var maxPressure = 0;
var maxChanged = true;
var xs = [];
var ys = [];
var chart 

document.addEventListener('DOMContentLoaded', async function () {   
    fullAreaContainer = document.getElementsByClassName("PressureGraph")[0];

    let request = await fetch("/SocketPort");
    let text = await request.text();
    let port = parseInt(text);

    let webSocketURL = "ws://localhost:"+port;

    var socket = new WebSocket(webSocketURL);

    socket.onopen = function (event) {
        console.log("Connected, send request now...");
        socket.send('{"id": "PressureMonitor"}');
        console.log("Initial Query sent");
        socket.send('{"method": "SendMaxPressureAsync"}');
    };

    socket.onmessage = function (message) {
        var json = JSON.parse(message.data);

        if (json.maxPressure != undefined) {
            maxPressure = json.maxPressure;
        }
        
        if (json.X != undefined) {
            xs = json.X;

            if (chart != undefined) {
                chart.data.labels = xs;
                chart.options.scales.x.max = xs.length * 100;
                chart.update();
            }
        }

        if (json.Y != undefined) {
            ys = json.Y;

            let max = Math.max(...ys);

            if (max > maxPressure) {
                maxPressure = max;
                maxChanged = true;
            }

            if (chart != undefined) {
                if (maxChanged) {
                    chart.options.scales.y.max = maxPressure;
                    maxChanged = false;
                }

                chart.data.datasets[0].data = ys;
                chart.update();
            }
        }
    }

    // build the graph using chart.js

    const chartCtx = document.getElementById('graph').getContext('2d');

    chart = new Chart(chartCtx, {
        type: 'line',
        data: {
            labels: xs,
            datasets: [
                {
                    label: 'Pressure',
                    data: ys,
                    backgroundColor: 'rgba(255, 99, 132, 0.2)',
                    borderColor: 'rgba(255, 99, 132, 1)',
                    borderWidth: 1
                }
            ]
        },
        options: {
            scales: {
                y: {
                    beginAtZero: true,
                    max: maxPressure,
                    min: 0
                },
                x: {
                    beginAtZero: true,
                    max: 4900,
                    min: 0
                }
            }
        }
    })
})