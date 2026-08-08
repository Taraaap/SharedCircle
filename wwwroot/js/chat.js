document.addEventListener("DOMContentLoaded", function () {

    let currentConversationId = 0;
    let chatUserId = "";

    const loggedInUserId = String(window.loggedInUserId);

    console.log("LOGGED IN USER ID:", loggedInUserId);

    const userList = document.getElementById("userList");
    const searchUser = document.getElementById("searchUser");

    const chatMessages = document.getElementById("chatMessages");
    const chatUserName = document.getElementById("chatUserName");
    const chatUserImage = document.getElementById("chatUserImage");

    const messageText = document.getElementById("messageText");
    const sendBtn = document.getElementById("sendBtn");


   

    loadUsers("");

    function loadUsers(term) {

        fetch("/Chat/SearchUsers?term=" + encodeURIComponent(term))
            .then(response => response.json())
            .then(users => {

                let html = "";

                users.forEach(user => {

                    const image =
                        user.profileImage ||
                        "/images/default-profile.png";

                    html += `
                        <div class="d-flex align-items-center p-3 border-bottom user-item"
                             data-user-id="${user.id}"
                             data-name="${user.fullName}"
                             data-image="${image}"
                             style="cursor:pointer;">

                            <img src="${image}"
                                 width="50"
                                 height="50"
                                 class="rounded-circle me-3"
                                 style="object-fit:cover;">

                            <div>
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

                userList.innerHTML = html;
            })
            .catch(error => {
                console.error("Load users error:", error);
            });
    }


    
    // SEARCH
   

    searchUser.addEventListener("keyup", function () {

        loadUsers(this.value);

    });


   
    // SELECT USER
   

    userList.addEventListener("click", function (e) {

        const user = e.target.closest(".user-item");

        if (!user)
            return;

        const userId = user.dataset.userId;
        const userName = user.dataset.name;
        const image = user.dataset.image;

        console.log("Selected:", userName);

        fetch("/Chat/StartConversation", {

            method: "POST",

            headers: {
                "Content-Type":
                    "application/x-www-form-urlencoded"
            },

            body:
                "userId=" +
                encodeURIComponent(userId)

        })
            .then(response => {

                if (!response.ok)
                    throw new Error("Failed to start conversation");

                return response.json();

            })
            .then(data => {

                console.log("Conversation ID:", data.conversationId);

                currentConversationId = data.conversationId;
                chatUserId = userId;

               
                chatUserName.textContent = userName;
                chatUserImage.src = image;

                
                loadMessages(currentConversationId);

                messageText.focus();

            })
            .catch(error => {

                console.error(
                    "Start conversation error:",
                    error
                );

            });

    });


    
    // LOAD MESSAGES
   

    function loadMessages(conversationId) {

        fetch(
            "/Chat/GetMessages?conversationId=" +
            conversationId
        )
            .then(response => {

                if (!response.ok)
                    throw new Error("Failed to load messages");

                return response.json();

            })
            .then(messages => {

                chatMessages.innerHTML = "";

                if (messages.length === 0) {

                    chatMessages.innerHTML = `
                    <div class="text-center text-muted mt-5">
                        Start chatting 💬
                    </div>
                `;

                    return;
                }

                messages.forEach(message => {

                    addMessageToUI(message);

                });

                scrollMessages();

            })
            .catch(error => {

                console.error(
                    "Load messages error:",
                    error
                );

            });

    }

    // SEND MESSAGE
 

    sendBtn.addEventListener("click", function () {

        // Prevent duplicate requests
        if (sendBtn.disabled) {
            return;
        }

        if (currentConversationId === 0) {
            alert("Select someone first.");
            return;
        }

        const text = messageText.value.trim();

        if (text === "") {
            return;
        }

        // Disable immediately
        sendBtn.disabled = true;

        console.log("SENDING MESSAGE:", text);

        fetch("/Chat/SendMessage", {
            method: "POST",
            headers: {
                "Content-Type": "application/x-www-form-urlencoded"
            },
            body:
                "conversationId=" +
                encodeURIComponent(currentConversationId) +
                "&text=" +
                encodeURIComponent(text)
        })
            .then(response => {

                if (!response.ok) {
                    throw new Error("Send failed: " + response.status);
                }

                return response.json();
            })
            .then(message => {

                console.log("MESSAGE SAVED:", message);

                addMessageToUI(message);

                messageText.value = "";

            })
            .catch(error => {

                console.error("Send error:", error);

            })
            .finally(() => {

                sendBtn.disabled = false;
                messageText.focus();

            });

    });


    function sendMessage() {

        console.log("SEND CLICKED");

        if (currentConversationId === 0) {

            alert("Select someone first.");

            return;
        }

        const text = messageText.value.trim();

        if (text === "")
            return;

        sendBtn.disabled = true;

        fetch("/Chat/SendMessage", {

            method: "POST",

            headers: {
                "Content-Type":
                    "application/x-www-form-urlencoded"
            },

            body:
                "conversationId=" +
                encodeURIComponent(currentConversationId) +
                "&text=" +
                encodeURIComponent(text)

        })
            .then(response => {

                if (!response.ok)
                    throw new Error(
                        "Send message failed: " +
                        response.status
                    );

                return response.json();

            })
            .then(message => {

                console.log("MESSAGE SENT:", message);

                addMessageToUI(message);

                messageText.value = "";

                scrollMessages();

            })
            .catch(error => {

                console.error(
                    "Send message error:",
                    error
                );

            })
            .finally(() => {

                sendBtn.disabled = false;

                messageText.focus();

            });

    }

  // MESSAGE UI
    

function addMessageToUI(message) {

    const senderId = String(message.senderId);
    const myId = String(loggedInUserId);

    const isMine = senderId === myId;

    console.log("Sender:", senderId);
    console.log("Me:", myId);
    console.log("Is mine:", isMine);

    const wrapper = document.createElement("div");

    if (isMine) { wrapper.style.display = "flex"; wrapper.style.justifyContent = "flex-end"; wrapper.style.marginBottom = "10px"; }
    else { wrapper.style.display = "flex"; wrapper.style.justifyContent = "flex-start"; wrapper.style.marginBottom = "10px"; }

    const bubble = document.createElement("div");

    if (isMine)
    {
        bubble.style.backgroundColor = "#0d6efd"; bubble.style.color = "white";
    }  
    else
    {
        bubble.style.backgroundColor = "#e4e6eb"; bubble.style.color = "#000";
    }

    bubble.style.maxWidth = "70%";
    bubble.style.padding = "8px 14px";
    bubble.style.borderRadius = "18px";
    bubble.style.boxShadow = "0 1px 3px rgba(0,0,0,0.1)";

    bubble.innerHTML = ` <div> ${escapeHtml(message.text)} </div>
    <div style=" font-size:11px; margin-top:4px; opacity:0.7; ">
    ${message.time} </div> `;

    wrapper.appendChild(bubble);

    chatMessages.appendChild(wrapper);

    scrollMessages();
}


    // SCROLL
   function scrollMessages() {

        chatMessages.scrollTop =
            chatMessages.scrollHeight;

    }
 // SECURITY

    function escapeHtml(text) {

        const div =
            document.createElement("div");

        div.textContent = text;

        return div.innerHTML;

    }

});