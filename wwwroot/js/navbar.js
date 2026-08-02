
// search
const searchBox = document.getElementById("userSearch");
const resultBox = document.getElementById("searchResult");

searchBox.addEventListener("keyup", function () {

    let text = this.value.trim();

    if (text.length === 0) {
        resultBox.style.display = "none";
        resultBox.innerHTML = "";
        return;
    }

    fetch("/Profile/SearchUsers?term=" + encodeURIComponent(text))
        .then(r => r.json())
        .then(users => {

            let html = "";

            users.forEach(user => {

                let image = user.profileImage || "/images/default-profile.png";

                html += `
<a href="/Profile/ViewProfile/${user.id}"
   class="list-group-item list-group-item-action d-flex align-items-center">

    <img src="${image}"
         width="40"
         height="40"
         class="rounded-circle me-3"
         style="object-fit:cover;">

    <div>
        <strong>${user.fullName}</strong>
        ${user.isMe ? '<small class="text-muted ms-2">(You)</small>' : ''}
    </div>

</a>
`;
            });

            resultBox.innerHTML = html;
            resultBox.style.display = users.length ? "block" : "none";

        });

});