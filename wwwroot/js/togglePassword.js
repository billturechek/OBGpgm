$(document).ready(function () {
    const showIcon = document.querySelector("#hideShow");
    const passwordField = document.querySelector("#password-field");

    showIcon.addEventListener("click", function () {
        this.classList.toggle("fa-eye-slash");
        const type = passwordField.getAttribute("type")
            === "password" ? "text" : "password";
        passwordField.setAttribute("type", type);

        this.classList.toggle("fa-eye");        
    })
});