document.querySelectorAll(".follow-btn")
    .forEach(btn => {

        btn.addEventListener("click", function () {

            let userId = this.dataset.userId;


            fetch("/Follow/ToggleFollow", {

                method: "POST",

                headers: {
                    "Content-Type": "application/x-www-form-urlencoded"
                },

                body: "userId=" + userId

            })
                .then(r => r.json())
                .then(data => {

                    if (data.isFollowing) {
                        this.innerText = "Following";
                        this.classList.remove("btn-primary");
                        this.classList.add("btn-secondary");
                    }
                    else {
                        this.innerText = "Follow";
                        this.classList.remove("btn-secondary");
                        this.classList.add("btn-primary");
                    }

                });

        });

    });