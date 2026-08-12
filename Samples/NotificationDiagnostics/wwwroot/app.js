const logElement = document.getElementById("log");

function send(command) {
    window.external.sendMessage(JSON.stringify(command));
}

function appendLog(timestamp, message) {
    const line = `[${timestamp}] ${message}\n`;
    logElement.textContent += line;
    logElement.scrollTop = logElement.scrollHeight;
}

function setText(id, value) {
    document.getElementById(id).textContent = value ?? "";
}

window.external.receiveMessage(message => {
    const payload = JSON.parse(message);

    if (payload.type === "log") {
        appendLog(payload.timestamp, payload.message);
        return;
    }

    if (payload.type === "state") {
        setText("applicationName", payload.applicationName);
        setText("notificationRegistrationId", payload.notificationRegistrationId);
        setText("notificationsEnabled", String(payload.notificationsEnabled));
        setText("isRunning", String(payload.isRunning));
        document.getElementById("cancelShutdownRequested").checked = Boolean(payload.cancelShutdownRequested);
        return;
    }
});

document.getElementById("cancelShutdownRequested").addEventListener("change", event => {
    send({
        command: "setCancelShutdownRequested",
        enabled: event.target.checked
    });
});

document.getElementById("showNotification").addEventListener("click", () => {
    send({
        command: "show",
        title: document.getElementById("title").value,
        body: document.getElementById("body").value,
        iconPath: document.getElementById("iconPath").value
    });
});

document.getElementById("enableNotifications").addEventListener("click", () => {
    send({
        command: "setNotificationsEnabled",
        enabled: true
    });
});

document.getElementById("disableNotifications").addEventListener("click", () => {
    send({
        command: "setNotificationsEnabled",
        enabled: false
    });
});

document.getElementById("shutdown").addEventListener("click", () => {
    send({
        command: "shutdown"
    });
});

send({
    command: "ready"
});