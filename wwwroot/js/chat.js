document.addEventListener("DOMContentLoaded", function () {

    let currentConversationId = 0;
    let chatUserId = "";
    let connection = null;
    let displayedMessageIds = new Set();
    let lastMessageDate = null; 

    // signalR
    connection = new signalR.HubConnectionBuilder()
        .withUrl("/chatHub")
        .withAutomaticReconnect()
        .build();

    connection.on("ReceiveMessage", function (message) {

        console.log("SIGNALR MESSAGE:", message);

        if (currentConversationId === 0) {
            return;
        }

        addMessageToUI(message);

        scrollMessages();
    });

    connection.start()
        .then(() => {
            console.log("🔥 SIGNALR CONNECTED");
            console.log("Connection ID:", connection.connectionId);
        })
        .catch(error => {
            console.error("SignalR Connection Error:", error);
        });

    const loggedInUserId = String(window.loggedInUserId);

   

    const userList = document.getElementById("userList");
    const searchUser = document.getElementById("searchUser");

    const chatMessages = document.getElementById("chatMessages");
    const chatUserName = document.getElementById("chatUserName");
    const chatUserImage = document.getElementById("chatUserImage");
    const chatHeader = document.getElementById("chatHeader"); 

    const messageText = document.getElementById("messageText");
    const sendBtn = document.getElementById("sendBtn");
    const messageBox = document.getElementById("messageBox");

    chatUserImage.style.cursor = "pointer";
    chatUserName.style.cursor = "pointer";

    function goToProfile() {
        if (chatUserId) {
            window.location.href = "/Profile/ViewProfile?id=" + chatUserId;
        }
    }

    chatUserImage.addEventListener("click", goToProfile);
    chatUserName.addEventListener("click", goToProfile);

   

    loadUsers("");

    function loadUsers(term) {

        fetch("/Chat/SearchUsers?term=" + encodeURIComponent(term))
            .then(response => response.json())
            .then(users => {

                let html = "";

                users.forEach(user => {
                    const loggedInUserIdStr = String(loggedInUserId);

                    const image =
                        user.profileImage ||
                        "/images/default-profile.png";

                    const isUnread = (user.unreadCount || 0) > 0;

                    const lastMessage =
                        user.lastMessage ||
                        "Click to chat";

                    const lastMessagePreview =
                        user.lastMessage
                            ? (String(user.lastMessageSenderId) === loggedInUserIdStr
                                ? "You: " + user.lastMessage
                                : user.fullName + ": " + user.lastMessage)
                            : "Click to chat";

                    const unreadClass =
                        isUnread
                            ? "fw-bold text-dark"
                            : "text-muted";

                    const unreadBadge =
                        isUnread
                            ? `<span class="unread-badge badge bg-danger rounded-pill ms-auto">${user.unreadCount}</span>`
                            : `<span class="unread-badge badge bg-danger rounded-pill ms-auto d-none">0</span>`;

                  

                    html += `
    <div class="d-flex align-items-center p-3 border-bottom user-item"
         data-user-id="${user.id}"
         data-name="${user.fullName}"
         data-image="${image}"
         data-conversation-id="${user.conversationId || 0}"
         style="cursor:pointer;">

        <img src="${image}"
             width="50"
             height="50"
             class="rounded-circle me-3"
             style="object-fit:cover;">

        <div class="flex-grow-1">

            <div class="fw-semibold">
                ${escapeHtml(user.fullName)}
            </div>

            <small class="last-message ${unreadClass}">
    ${escapeHtml(lastMessagePreview)}
</small>

        </div>

        ${unreadBadge}

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

        const avatarClick = e.target.closest("img");

        if (avatarClick) {

            const clickedUser = avatarClick.closest(".user-item");

            if (clickedUser) {
                const clickedUserId = clickedUser.dataset.userId;
                window.location.href = "/Profile/ViewProfile?id=" + clickedUserId;
            }

            return;
        }

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
            .then(async data => {

                console.log("Conversation ID:", data.conversationId);

                currentConversationId = data.conversationId;
                chatUserId = userId;

                await fetch("/Chat/MarkAsRead", {
                    method: "POST",
                    headers: {
                        "Content-Type": "application/x-www-form-urlencoded"
                    },
                    body:
                        "conversationId=" +
                        encodeURIComponent(currentConversationId)
                });
                
                loadUnreadMessageCount();

               
                const selectedBadge =
                    user.querySelector(".unread-badge");

                if (selectedBadge) {
                    selectedBadge.innerText = "0";
                    selectedBadge.classList.add("d-none");
                }

                
                const lastMessage =
                    user.querySelector(".last-message");

                if (lastMessage) {
                    lastMessage.classList.remove("fw-bold");
                    lastMessage.classList.remove("text-dark");
                    lastMessage.classList.add("text-muted");
                }

                chatUserName.textContent = userName;
                chatUserImage.src = image;

                chatHeader.style.display = "flex";
                messageBox.style.display = "block";

                
                messageText.disabled = false;
                sendBtn.disabled = false;

                messageText.placeholder = "Type a message...";

                
                loadMessages(currentConversationId);

                
                messageText.focus();

                if (connection.state === "Connected") {

                    await connection.invoke(
                        "JoinConversation",
                        currentConversationId.toString()
                    );

                }

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

                displayedMessageIds.clear();
                lastMessageDate = null;

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

                loadUsers(searchUser.value);

            })
            .catch(error => {

                console.error("Send error:", error);

            })
            .finally(() => {

                sendBtn.disabled = false;
                messageText.focus();

            });

    });


    // helper
    function formatDateLabel(date) {

        const now = new Date();

        const msgDay = new Date(date.getFullYear(), date.getMonth(), date.getDate());
        const today = new Date(now.getFullYear(), now.getMonth(), now.getDate());

        const yesterday = new Date(today);
        yesterday.setDate(today.getDate() - 1);

        if (msgDay.getTime() === today.getTime()) return "Today";
        if (msgDay.getTime() === yesterday.getTime()) return "Yesterday";

        return date.toLocaleDateString("en-US", {
            month: "long",
            day: "numeric",
            year: "numeric"
        });
    }

    function addDateDividerIfNeeded(sentAt) {

        if (!sentAt) return;

        const msgDate = new Date(sentAt);
        const label = formatDateLabel(msgDate);

        if (lastMessageDate === label) return; 

        lastMessageDate = label;

        const divider = document.createElement("div");
        divider.style.textAlign = "center";
        divider.style.margin = "14px 0";

        divider.innerHTML = `
        <span style="
            background:#e4e6eb;
            color:#555;
            font-size:12px;
            padding:4px 12px;
            border-radius:12px;
        ">${label}</span>
    `;

        chatMessages.appendChild(divider);
    }
  // MESSAGE UI
    
    function addMessageToUI(message) {

        if (displayedMessageIds.has(message.id)) {
            console.log("Duplicate message ignored:", message.id);
            return;
        }

        displayedMessageIds.add(message.id);
        addDateDividerIfNeeded(message.sentAt);

    const senderId = String(message.senderId);
    const myId = String(loggedInUserId);

    const isMine = senderId === myId;

   

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

