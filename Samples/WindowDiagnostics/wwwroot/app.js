const eventNames = [
    "Creating",
    "Created",
    "Closing",
    "Closed",
    "LocationChanged",
    "SizeChanged",
    "Activated",
    "Deactivated",
    "Maximized",
    "Minimized",
    "Restored",
    "FullScreenEntered",
    "FullScreenExited",
    "StateChanged",
    "WebMessageReceived"
];

const defaultEnabledEvents = new Set([
    "Creating",
    "Created",
    "Closing",
    "Closed",
    "Activated",
    "Deactivated",
    "Maximized",
    "Minimized",
    "Restored",
    "FullScreenEntered",
    "FullScreenExited",
    "StateChanged"
]);

const enabledEvents = new Set(defaultEnabledEvents);
const logEntries = [];

const eventOptions = document.getElementById("event-options");
const logOutput = document.getElementById("log-output");
const stateOutput = document.getElementById("state-output");
const windowInfo = document.getElementById("window-info");
const cancelClosing = document.getElementById("cancel-closing");
const logActions = document.getElementById("log-actions");

initializeEventOptions();
initializeButtons();
initializeHostMessaging();

sendMessage({
    type: "ready"
});

function initializeEventOptions() {
    for (const eventName of eventNames) {
        const label = document.createElement("label");
        label.className = "event-option";

        const checkbox = document.createElement("input");
        checkbox.type = "checkbox";
        checkbox.checked = enabledEvents.has(eventName);
        checkbox.dataset.eventName = eventName;

        checkbox.addEventListener("change", () => {
            if (checkbox.checked) {
                enabledEvents.add(eventName);
            } else {
                enabledEvents.delete(eventName);
            }

            renderLog();
        });

        label.appendChild(checkbox);
        label.appendChild(document.createTextNode(eventName));

        eventOptions.appendChild(label);
    }

    document.getElementById("select-all-events").addEventListener("click", () => {
        enabledEvents.clear();

        for (const checkbox of eventOptions.querySelectorAll("input[type=checkbox]")) {
            checkbox.checked = true;
            enabledEvents.add(checkbox.dataset.eventName);
        }

        renderLog();
    });

    document.getElementById("clear-all-events").addEventListener("click", () => {
        enabledEvents.clear();

        for (const checkbox of eventOptions.querySelectorAll("input[type=checkbox]")) {
            checkbox.checked = false;
        }

        renderLog();
    });

    document.getElementById("clear-log").addEventListener("click", () => {
        logEntries.length = 0;
        renderLog();
    });

    document.getElementById("copy-log").addEventListener("click", async () => {
        await navigator.clipboard.writeText(logOutput.textContent ?? "");
    });

    cancelClosing.addEventListener("change", () => {
        sendMessage({
            type: "setCancelClosing",
            value: cancelClosing.checked
        });
    });

    logActions.addEventListener("change", () => {
        sendMessage({
            type: "setLogActions",
            value: logActions.checked
        });
    });
}

function initializeButtons() {
    for (const button of document.querySelectorAll("button[data-action]")) {
        button.addEventListener("click", () => {
            sendMessage({
                type: "action",
                target: button.dataset.target,
                action: button.dataset.action
            });
        });
    }
}

function initializeHostMessaging() {
    if (!window.external ||
        typeof window.external.sendMessage !== "function" ||
        typeof window.external.receiveMessage !== "function") {
        appendLocalLog("Host bridge is not available.");
        return;
    }

    window.external.receiveMessage(message => {
        let payload;

        try {
            payload = JSON.parse(message);
        } catch {
            appendLocalLog(`Invalid host message: ${message}`);
            return;
        }

        handleHostMessage(payload);
    });
}

function handleHostMessage(payload) {
    switch (payload.type) {
        case "init":
            windowInfo.textContent = payload.parentId
                ? `Window: ${payload.id} | Parent: ${payload.parentId}`
                : `Window: ${payload.id}`;
            break;

        case "state":
            renderState(payload);
            break;

        case "event":
        case "diagnostic":
            logEntries.push(payload);
            writeConsoleLog(payload);
            renderLog();
            break;
    }
}

function sendMessage(payload) {
    const json = JSON.stringify(payload);

    if (window.external && typeof window.external.sendMessage === "function") {
        window.external.sendMessage(json);
    } else {
        appendLocalLog(`Unable to send message to host: ${json}`);
    }
}

function renderState(state) {
    stateOutput.innerHTML = "";

    addStateRow("Window", state.id);
    addStateRow("Parent", state.parentId ?? "-");
    addStateRow("Initialized", state.isInitialized);
    addStateRow("Closed", state.isClosed);
    addStateRow("Fullscreen", state.fullScreen);
    addStateRow("WindowState", state.state);
    addStateRow("Has child", state.hasChild);
    addStateRow("Size", `${state.size.width} x ${state.size.height}`);
    addStateRow("Location", `${state.location.x}, ${state.location.y}`);
}

function addStateRow(name, value) {
    const term = document.createElement("dt");
    term.textContent = name;

    const description = document.createElement("dd");
    description.textContent = String(value);

    stateOutput.appendChild(term);
    stateOutput.appendChild(description);
}

function renderLog() {
    const lines = [];

    for (const entry of logEntries) {
        if (entry.type === "event" && !enabledEvents.has(entry.eventName)) {
            continue;
        }

        lines.push(formatLogEntry(entry));
    }

    logOutput.textContent = lines.join("\n");
    logOutput.scrollTop = logOutput.scrollHeight;
}

function writeConsoleLog(entry) {
    if (entry.type === "event" && !enabledEvents.has(entry.eventName)) {
        return;
    }

    console.log(formatLogEntry(entry));
}

function formatLogEntry(entry) {
    const timestamp = new Date(entry.timestamp).toLocaleTimeString("en-US", {
        hour12: false,
        hour: "2-digit",
        minute: "2-digit",
        second: "2-digit",
        fractionalSecondDigits: 3
    });

    const payload = entry.payload
        ? ` | ${JSON.stringify(entry.payload)}`
        : "";

    const category = entry.type === "diagnostic"
        ? "diagnostic"
        : entry.eventName;

    return `${timestamp} | ${entry.windowId} | ${category}${payload}`;
}

function appendLocalLog(message) {
    const entry = {
        type: "diagnostic",
        windowId: "browser",
        eventName: "Local",
        timestamp: new Date().toISOString(),
        payload: {
            message
        }
    };

    logEntries.push(entry);
    renderLog();
}