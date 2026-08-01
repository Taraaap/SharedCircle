const modal = new bootstrap.Modal(document.getElementById("followModal"));

function loadFollowList(type, userId) {

    document.getElementById("followModalTitle").innerText = type;

    document.getElementById("followList").innerHTML =
        `<div class="text-center p-4">Loading...</div>`;

    fetch(`/Profile/Get${type}?id=${userId}`)
        .then(response => {

            if (!response.ok)
                throw new Error("Failed to load.");

            return response.json();
        })
        .then(data => {

            let html = "";

            if (data.length === 0) {

                html = `
                    <div class="text-center p-4 text-muted">
                        No ${type.toLowerCase()}
                    </div>
                `;
            }
            else {

                data.forEach(user => {

                    let image = user.profileImage || "/images/default-profile.png";

                    html += `
                        <div class="d-flex justify-content-between align-items-center p-3 border-bottom">

                            <div class="d-flex align-items-center">

                                <img src="${image}"
                                     width="45"
                                     height="45"
                                     class="rounded-circle me-3"
                                     style="object-fit:cover;">

                                <div>

                                    <strong>${user.fullName}</strong>

                                </div>

                            </div>

                            <button
                                class="btn btn-sm ${user.isFollowing ? "btn-outline-secondary" : "btn-primary"} follow-btn"
                                data-user-id="${user.id}">

                                ${user.isFollowing ? "Following" : "Follow"}

                            </button>

                        </div>
                    `;
                });

            }

            document.getElementById("followList").innerHTML = html;

        })
        .catch(err => {

            console.error(err);

            document.getElementById("followList").innerHTML =
                `<div class="text-danger text-center p-4">
                    Failed to load.
                 </div>`;

        });

    modal.show();
}

document.getElementById("followersBtn")
    .addEventListener("click", function (e) {

        e.preventDefault();

        loadFollowList("Followers", this.dataset.userId);

    });

document.getElementById("followingBtn")
    .addEventListener("click", function (e) {

        e.preventDefault();

        loadFollowList("Following", this.dataset.userId);

    });

document.addEventListener("click", function (e) {

    if (!e.target.classList.contains("follow-btn"))
        return;

    let button = e.target;

    let userId = button.dataset.userId;

    fetch("/Follow/ToggleFollow", {

        method: "POST",

        headers: {
            "Content-Type": "application/x-www-form-urlencoded"
        },

        body: "userId=" + encodeURIComponent(userId)

    })
        .then(r => r.json())
        .then(data => {

            if (data.isFollowing) {

                button.classList.remove("btn-primary");
                button.classList.add("btn-outline-secondary");

                button.innerText = "Following";

            }
            else {

                button.classList.remove("btn-outline-secondary");
                button.classList.add("btn-primary");

                button.innerText = "Follow";

            }

        });

});

let followers = document.getElementById("followersCount");
let following = document.getElementById("followingCount");

if (followers && following) {

    if (data.isFollowing) {

        following.innerText = parseInt(following.innerText) + 1;

    }
    else {

        following.innerText = parseInt(following.innerText) - 1;

    }

}