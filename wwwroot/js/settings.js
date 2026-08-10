document.addEventListener("DOMContentLoaded", function () {

    
    const toggle = document.getElementById("darkModeToggle");

    if (toggle) {

        const currentTheme = localStorage.getItem("theme") || "light";
        toggle.checked = currentTheme === "dark";

        toggle.addEventListener("change", function () {
            const theme = this.checked ? "dark" : "light";
            document.documentElement.setAttribute("data-theme", theme);
            localStorage.setItem("theme", theme);
        });
    }

  
    const form = document.getElementById("changePasswordForm");

    if (form) {

        form.addEventListener("submit", function (e) {

            e.preventDefault();

            const alertBox = document.getElementById("passwordAlert");
            alertBox.innerHTML = "";

            const currentPassword = document.getElementById("currentPassword").value;
            const newPassword = document.getElementById("newPassword").value;
            const confirmPassword = document.getElementById("confirmPassword").value;

            if (newPassword !== confirmPassword) {
                alertBox.innerHTML = `<div class="alert alert-danger">New passwords do not match.</div>`;
                return;
            }

            fetch("/Settings/ChangePassword", {
                method: "POST",
                headers: {
                    "Content-Type": "application/x-www-form-urlencoded"
                },
                body:
                    "currentPassword=" + encodeURIComponent(currentPassword) +
                    "&newPassword=" + encodeURIComponent(newPassword)
            })
                .then(r => r.json())
                .then(data => {

                    if (data.success) {
                        alertBox.innerHTML = `<div class="alert alert-success">Password updated successfully.</div>`;
                        form.reset();
                    } else {
                        alertBox.innerHTML = `<div class="alert alert-danger">${data.message}</div>`;
                    }

                })
                .catch(() => {
                    alertBox.innerHTML = `<div class="alert alert-danger">Something went wrong. Try again.</div>`;
                });

        });
    }

});