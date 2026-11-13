// Щелчок по имени файла-примера подставляет путь в поле
document.querySelectorAll("[data-path]").forEach(function (link) {
    link.addEventListener("click", function (e) {
        e.preventDefault();
        document.getElementById("filePath").value = link.dataset.path;
    });
});
