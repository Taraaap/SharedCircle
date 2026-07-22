const connection = new signalR.HubConnectionBuilder()
    .withUrl("/commentHub")
    .build();
let connectionId = "";

connection.start()
    .then(function () {

        connection.invoke("GetConnectionId")
            .then(function (id) {

                connectionId = id;

                console.log("Connection ID:", connectionId);

            });

    })
    .catch(function (err) {

        console.error(err);

    });




const upload = document.getElementById("imageUpload");

if (upload) {
    upload.addEventListener("change", function () {

        if (this.files.length > 0) {

            const reader = new FileReader();

            reader.onload = function (e) {
                let img = document.getElementById("previewImage");

                if (img) {
                    img.src = e.target.result;
                    img.classList.remove("d-none");
                }
            }

            reader.readAsDataURL(this.files[0]);
        }

    });
}


// LIKE BUTTON
document.querySelectorAll(".like-btn").forEach(btn => {

    btn.addEventListener("click", function () {

        let postId = this.dataset.postId;

        fetch("/Posts/ToggleLike", {
            method: "POST",
            headers: {
                "Content-Type": "application/x-www-form-urlencoded"
            },
            body: "postId=" + postId
        })
            .then(r => r.json())
            .then(data => {

                this.querySelector(".like-count").innerText = data.likes;

            });

    });

});


// Comment
document.querySelectorAll(".comment-btn").forEach(btn => {

    btn.addEventListener("click", function () {

        let postId = this.dataset.postId;

        let input = document.querySelector(
            ".comment-text[data-post-id='" + postId + "']"
        );

        let text = input.value;

        if (text.trim() === "")
            return;

        fetch("/Comments/Add", {

            method: "POST",

            headers: {
                "Content-Type": "application/x-www-form-urlencoded"
            },

            body:
                "postId=" + postId +
                "&text=" + encodeURIComponent(text) +
                "&connectionId=" + connectionId

        })
            .then(r => r.json())
            .then(data => {

                let html = `
                <div class="d-flex mb-2">

                    <img src="${data.profileImage}"
                         width="35"
                         height="35"
                         class="rounded-circle me-2"
                         style="object-fit:cover;">

                    <div class="bg-light rounded-3 px-3 py-2 w-100">

                        <strong>${data.user}</strong>

                        <div>${data.text}</div>

                        <small class="text-muted">${data.time}</small>

                    </div>

                </div>
            `;

                document
                    .getElementById("comments-" + postId)
                    .insertAdjacentHTML("beforeend", html);

                let count = document.querySelector(
                    ".comment-count[data-post-id='" + postId + "']" );

                if (data.comments > 0) {
                    count.innerText = data.comments;
                }
                else {
                    count.innerText = "";
                }
               

            });

    });

});


connection.on("ReceiveComment",
    function (postId, user, profileImage, text, time, comments) {

        let html = `
            <div class="d-flex mb-2">
                <img src="${profileImage}"
                     width="35"
                     height="35"
                     class="rounded-circle me-2"
                     style="object-fit:cover;">

                <div class="bg-light rounded-3 px-3 py-2 w-100">
                    <strong>${user}</strong>
                    <div>${text}</div>
                    <small class="text-muted">${time}</small>
                </div>
            </div>
        `;

        document
            .getElementById("comments-" + postId)
            .insertAdjacentHTML("beforeend", html);

        let count = document.querySelector(
            `.comment-count[data-post-id='${postId}']`
        );

        if (comments > 0) {
            count.innerText = comments;
        } else {
            count.innerText = "";
        }
    });



