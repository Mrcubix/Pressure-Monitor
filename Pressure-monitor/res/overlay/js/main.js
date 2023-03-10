var maxPressure = 0;
var pressureMeasurements = [];

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
        
        if (json.pressureMeasurements != undefined) {
            pressureMeasurements = json.pressureMeasurements;
        }
    }

    // TODO: Implement graph stuff, prob via a library
})