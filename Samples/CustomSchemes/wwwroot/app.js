(() => {
    const status = document.getElementById("status");
    const output = document.getElementById("output");
    const sendButton = document.getElementById("send");

    status.textContent = "Loaded from app://localhost/index.html";

    sendButton.addEventListener("click", () => {
        window.external.sendMessage("Hello from app:// custom scheme");
    });

    fetch("app://localhost/data.json")
        .then(response => response.text())
        .then(text => {
            output.textContent = `fetch(app://localhost/data.json) returned:\n${text}`;
        })
        .catch(error => {
            output.textContent = `fetch failed:\n${error}`;
        });
})();