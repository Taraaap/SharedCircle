const chatConnection = new signalR.HubConnectionBuilder()
    .withUrl("/chatHub")
    .build();

chatConnection.on("ReceiveMessage", function (sender, message) {

    const messages = document.getElementById("messageList");

    messages.innerHTML += `
        <div class="mb-2">
            <strong>${sender}</strong><br/>
            ${message}
        </div>`;
});

chatConnection.start()
    .then(() => console.log("Chat Connected"))
    .catch(err => console.error(err));