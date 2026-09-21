document.addEventListener("DOMContentLoaded", function () {

    setupTransactionForm(
        "addType",
        "addCategory",
        "addCustomCategoryGroup",
        "addCustomCategory",
        "addTransactionButton"
    );


    setupTransactionForm(
        "editType",
        "editCategory",
        "editCustomCategoryGroup",
        "editCustomCategory",
        null
    );


    setupDeleteConfirmation();

});


function setupTransactionForm(
    typeId,
    categoryId,
    customGroupId,
    customInputId,
    buttonId
) {
    const typeSelect =
        document.getElementById(typeId);

    const categorySelect =
        document.getElementById(categoryId);

    const customGroup =
        document.getElementById(customGroupId);

    const customInput =
        document.getElementById(customInputId);

    const submitButton =
        buttonId
            ? document.getElementById(buttonId)
            : null;


    if (!typeSelect ||
        !categorySelect) {
        return;
    }


    function updateCategories() {

        const selectedType =
            typeSelect.value;


        Array.from(categorySelect.options)
            .forEach(function (option) {

                const optionType =
                    option.dataset.type;


                /*
                 * Placeholder options do not have a data-type.
                 */
                if (!optionType) {
                    return;
                }


                const allowed =
                    optionType === "Both"
                    ||
                    optionType === selectedType;


                option.hidden =
                    !allowed;

                option.disabled =
                    !allowed;

            });


        const selectedOption =
            categorySelect
                .options[
                    categorySelect.selectedIndex
                ];


        if (selectedOption &&
            selectedOption.disabled) {

            categorySelect.value =
                "";
        }


        updateCustomCategory();

        updateSubmitButton();

    }


    function updateCustomCategory() {

        if (!customGroup ||
            !customInput) {
            return;
        }


        const showCustom =
            categorySelect.value ===
            "Other";


        customGroup.classList.toggle(
            "is-hidden",
            !showCustom
        );


        if (showCustom) {

            customInput.setAttribute(
                "required",
                "required"
            );

        } else {

            customInput.removeAttribute(
                "required"
            );

        }

    }


    function updateSubmitButton() {

        if (!submitButton) {
            return;
        }


        const isIncome =
            typeSelect.value ===
            "Income";


        submitButton.textContent =
            isIncome
                ? "+ Add Income"
                : "− Add Expense";


        submitButton.classList.toggle(
            "income-button",
            isIncome
        );


        submitButton.classList.toggle(
            "expense-button",
            !isIncome
        );

    }


    typeSelect.addEventListener(
        "change",
        updateCategories
    );


    categorySelect.addEventListener(
        "change",
        updateCustomCategory
    );


    updateCategories();

}


function setupDeleteConfirmation() {

    const forms =
        document.querySelectorAll(
            "form[data-confirm]"
        );


    forms.forEach(function (form) {

        form.addEventListener(
            "submit",
            function (event) {

                const message =
                    form.dataset.confirm
                    ||
                    "Are you sure?";


                const confirmed =
                    window.confirm(
                        message
                    );


                if (!confirmed) {

                    event.preventDefault();

                }

            }
        );

    });

}