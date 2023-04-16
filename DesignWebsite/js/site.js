var btnApplyFilters = document.getElementById('btn-apply-filters');
var categoryCollapse = document.getElementById('category-collapse');

function changeWidthApplyFilters() {
    btnApplyFilters.style.width = `${$(".col-xl-3:first-child").width()}px`;
}

window.onload = function () {
    changeStyleApplyFiltersBtn();
    changeWidthApplyFilters();
};

$(".btn-rotate").click(function () {
    $(this).find(".chevron-rotate").toggleClass("down");
})

function changeStyleApplyFiltersBtn() {
    var x = categoryCollapse.getBoundingClientRect().bottom;
    //        console.log((window.innerHeight || document.documentElement.clientHeight));
    if (x <= (window.innerHeight || document.documentElement.clientHeight))
        btnApplyFilters.classList.remove("flow");
    if (btnApplyFilters.getBoundingClientRect().bottom > (window.innerHeight || document.documentElement.clientHeight))
        btnApplyFilters.classList.add("flow");
}

categoryCollapse.addEventListener('hidden.bs.collapse', () => changeStyleApplyFiltersBtn())
categoryCollapse.addEventListener('shown.bs.collapse', () => changeStyleApplyFiltersBtn())
document.addEventListener('scroll', () => changeStyleApplyFiltersBtn(), {
    passive: true
});
addEventListener("resize", (event) => changeWidthApplyFilters());
