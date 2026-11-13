// Переключение вкладок на странице импорта
document.querySelectorAll(".tabs button").forEach(function (tab) {
    tab.addEventListener("click", function () {
        document.querySelectorAll(".tabs button").forEach(function (t) { t.classList.remove("active"); });
        document.querySelectorAll(".tab-page").forEach(function (p) { p.classList.remove("active"); });
        tab.classList.add("active");
        document.getElementById(tab.dataset.tab).classList.add("active");
    });
});

// Щелчок по имени файла-примера подставляет путь в поле
document.querySelectorAll(".sample-list button").forEach(function (button) {
    button.addEventListener("click", function () {
        document.getElementById("filePath").value = button.dataset.path;
    });
});

// Фильтры каталога применяются сразу при выборе
document.querySelectorAll(".filters select").forEach(function (select) {
    select.addEventListener("change", function () { select.form.submit(); });
});
