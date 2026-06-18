// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.
document.addEventListener('DOMContentLoaded', function () {
    const btnTopo = document.getElementById("btnTopo");

    if (btnTopo) {
        window.addEventListener("scroll", () => {
            if (window.scrollY > 300) {
                btnTopo.classList.add("mostrar");
            } else {
                btnTopo.classList.remove("mostrar");
            }
        });

        btnTopo.addEventListener("click", () => {
            window.scrollTo({
                top: 0,
                behavior: "smooth"
            });
        });
    }
});
