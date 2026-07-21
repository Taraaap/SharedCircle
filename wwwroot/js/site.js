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