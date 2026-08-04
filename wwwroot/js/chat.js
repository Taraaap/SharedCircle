document.addEventListener("DOMContentLoaded", function () {

    let currentConversationId = 0;

    // ==========================
    // SignalR
    // ==========================

    const chatConnection = new signalR.HubConnectionBuilder()
        .withUrl("/chatHub")
        .build();

    chatConnection.on("ReceiveMessage", function (sender, message) {

        const messages = document.getElementById("chatMessages");

        messages.innerHTML += `
            <div class="d-flex mb-3">
                <div class="bg-white shadow-sm rounded-4 px-3 py-2">
                    <div class="fw-bold">${sender}</div>
                    <div>${message}</div>
                </div>
            </div>
        `;

        messages.scrollTop = messages.scrollHeight;

    });

    chatConnection.start()
        .then(() => console.log("Chat Connected"))
        .catch(err => console.error(err));



    // ==========================
    // Load users
    // ==========================

    loadUsers("");



    // ==========================
    // Search
    // ==========================

    document.getElementById("searchUser")
        .addEventListener("keyup", function () {

            loadUsers(this.value);

        });



    function loadUsers(term) {

        fetch("/Chat/SearchUsers?term=" + encodeURIComponent(term))
            .then(r => r.json())
            .then(users => {

                let html = "";

                users.forEach(user => {

                    const image = user.profileImage || "/images/default-profile.png";

                    html += `
<div class="d-flex align-items-center p-3 border-bottom user-item"
     data-user-id="${user.id}"
     data-name="${user.fullName}"
     style="cursor:pointer;">

    <img src="${image}"
         width="50"
         height="50"
         class="rounded-circle me-3"
         style="object-fit:cover;">

    <div class="flex-grow-1">

        <div class="fw-semibold">
            ${user.fullName}
        </div>

        <small class="text-muted">
            Click to chat
        </small>

    </div>

</div>
`;
                });

                document.getElementById("userList").innerHTML = html;

            });

    }



    // ==========================
    // Click user
    // ==========================

    document.getElementById("userList").addEventListener("click", function (e) {

        const user = e.target.closest(".user-item");

        if (!user)
            return;

        const userId = user.dataset.userId;
        const userName = user.dataset.name;

        console.log("Clicked:", userName);

        fetch("/Chat/StartConversation", {

            method: "POST",

            headers: {
                "Content-Type": "application/x-www-form-urlencoded"
            },

            body: "userId=" + encodeURIComponent(userId)

        })
            .then(r => r.json())
            .then(data => {

                currentConversationId = data.conversationId;

                console.log("Conversation:", currentConversationId);

                document.getElementById("chatHeader").innerHTML = `
                <img src="/images/default-profile.png"
                     width="45"
                     height="45"
                     class="rounded-circle me-3">

                <div>
                    <div class="fw-bold">
                        ${userName}
                    </div>

                    <small class="text-success">
                        Online
                    </small>
                </div>
            `;

                document.getElementById("chatMessages").innerHTML = `
                <div class="text-center text-muted mt-5">
                    Start chatting with <b>${userName}</b>
                </div>
            `;

            });

    });

});