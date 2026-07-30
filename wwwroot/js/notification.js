


loadNotificationCount();

function loadNotificationCount() {

    $.get("/Notification/GetUnreadCount", function (count) {

        if (count > 0) {

            $("#notificationBadge")
                .removeClass("d-none")
                .text(count);

        }
        else {

            $("#notificationBadge").addClass("d-none");

        }

    });

}


$("#notificationDropdown").on("click", function () {

    $.get("/Notification/GetNotifications", function (notifications) {

        let html = "";

        if (notifications.length === 0) {

            html = `
                <div class="text-center text-muted py-4">
                    No notifications
                </div>`;
        }
        else {

            notifications.forEach(function (n) {

                let image = n.sender.profileImage;

                if (!image)
                    image = "/images/default-profile.png";

                html += `
                <div class="d-flex p-3 border-bottom">

                    <img src="${image}"
                         width="45"
                         height="45"
                         class="rounded-circle me-3"
                         style="object-fit:cover;">

                    <div class="flex-grow-1">

                        <strong>${n.sender.fullName}</strong>

                        ${n.message}

                        <br>

                        <small class="text-muted">
                            ${timeAgo(n.createdAt)}
                        </small>

                    </div>

                </div>`;
            });

        }

        $("#notificationList").html(html);

        $.post("/Notification/MarkAsRead", function () {

            $("#notificationBadge").addClass("d-none");

        });

    });

});

function timeAgo(date) {

    let seconds = Math.floor((new Date() - new Date(date)) / 1000);

    let interval = Math.floor(seconds / 31536000);
    if (interval >= 1) return interval + " year ago";

    interval = Math.floor(seconds / 2592000);
    if (interval >= 1) return interval + " month ago";

    interval = Math.floor(seconds / 86400);
    if (interval >= 1) return interval + " day ago";

    interval = Math.floor(seconds / 3600);
    if (interval >= 1) return interval + " hour ago";

    interval = Math.floor(seconds / 60);
    if (interval >= 1) return interval + " minute ago";

    return "Just now";
}



const connection = new signalR.HubConnectionBuilder()
    .withUrl("/notificationHub")
    .build();

connection.on("ReceiveNotification", function () {

    loadNotificationCount();

});

connection.start();
