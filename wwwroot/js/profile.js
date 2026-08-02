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

        <a href="/Profile/ViewProfile/${user.id}">
            <img src="${image}"
                 width="45"
                 height="45"
                 class="rounded-circle me-3"
                 style="object-fit:cover;">
        </a>

        <div>
            <a href="/Profile/ViewProfile/${user.id}"
               class="text-decoration-none text-dark fw-bold">
                ${user.fullName}
            </a>
        </div>

    </div>

   ${user.isMe? `
        <span class="badge bg-secondary">
            You
        </span>
    `
                            : `
        <button
            class="btn btn-sm ${user.isFollowing ? "btn-outline-secondary" : "btn-primary"} follow-btn"
            data-user-id="${user.id}">

            ${user.isFollowing ? "Following" : "Follow"}

        </button>
    `
}

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

    const button = e.target.closest(".follow-btn");

    if (!button)
        return;

    e.preventDefault();

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

            const followers = document.getElementById("followersCount");
            const following = document.getElementById("followingCount");

            if (followers)
                followers.innerText = data.followersCount;

            if (following)
                following.innerText = data.followingCount;

        });

});

// Follow button on user profile page
const profileFollowBtn = document.getElementById("profileFollowBtn");

if (profileFollowBtn) {

    profileFollowBtn.addEventListener("click", function () {

        const button = this;
        const userId = button.dataset.userId;

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

                const followers = document.getElementById("followersCount");
                const following = document.getElementById("followingCount");

                const isOwn =
                    profileFollowBtn.dataset.isOwnProfile === "true";

                if (isOwn) {

                    followers.innerText = data.myFollowers;
                    following.innerText = data.myFollowing;

                }
                else {

                    followers.innerText = data.targetFollowers;
                    following.innerText = data.targetFollowing;

                }

            });

    });

}



