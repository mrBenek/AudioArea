var btnApplyFilters = document.getElementById('btn-apply-filters');
var categoryCollapse = document.getElementById('category-collapse');
var companyCollapse = document.getElementById('company-collapse');
var inputPageNumber = document.getElementById('page-number');
var maxPageNumber = document.getElementById('max-page-number');
var dropdownCategories = document.getElementById('dropdown-categories');

function changeWidthApplyFilters() {
    btnApplyFilters.style.width = `${$(".col-xl-3:first-child").width()}px`;
}

function changeStyleApplyFiltersBtn() {
    var x = categoryCollapse.getBoundingClientRect().bottom;
    //        console.log((window.innerHeight || document.documentElement.clientHeight));
    if (x <= (window.innerHeight || document.documentElement.clientHeight))
        btnApplyFilters.classList.remove("flow");
    if (btnApplyFilters.getBoundingClientRect().bottom > (window.innerHeight || document.documentElement.clientHeight))
        btnApplyFilters.classList.add("flow");
}

function changeMaxPageNumber() {
    inputPageNumber.setAttribute("max", maxPageNumber.innerHTML);
}

function btnApplyFiltersClick() {
    changeMaxPageNumber();
}

btnApplyFilters.addEventListener('click', () => btnApplyFiltersClick())
categoryCollapse.addEventListener('hidden.bs.collapse', () => changeStyleApplyFiltersBtn())
companyCollapse.addEventListener('hidden.bs.collapse', () => changeStyleApplyFiltersBtn())
categoryCollapse.addEventListener('shown.bs.collapse', () => changeStyleApplyFiltersBtn())
companyCollapse.addEventListener('shown.bs.collapse', () => changeStyleApplyFiltersBtn())
document.addEventListener('scroll', () => changeStyleApplyFiltersBtn(), {
    passive: true
});

addEventListener("resize", (event) => changeWidthApplyFilters());

inputPageNumber.addEventListener("keypress", function (event) {
    if (event.key === "Enter") {
        inputPageNumber.blur();
    }
});

window.onload = function () {
    changeStyleApplyFiltersBtn();
    changeWidthApplyFilters();
    changeMaxPageNumber();
};

$(".btn-rotate").click(function () {
    $(this).find(".chevron-rotate").toggleClass("down");
})

dropdownCategories.click(function () {
    console.log('dropdownCategories');
})
