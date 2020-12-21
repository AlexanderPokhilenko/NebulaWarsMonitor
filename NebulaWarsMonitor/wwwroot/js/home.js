var properties;

window.onload = function () {
    $("td input").change(function () {
        $(this).closest("tr").addClass("bg-warning");
    });
    $("td button").click(function () {
        var jqTr = $(this).closest("tr");
        if (jqTr.hasClass("bg-danger")) {
            jqTr.removeClass("bg-danger");
            this.textContent = "Видалити";
        } else {
            jqTr.addClass("bg-danger");
            this.textContent = "Відмінити";
        }
    });
};

function AddRow() {
    var lastRow = $("#mainTable").find("tr").last();
    var newRow = $("<tr>").addClass("bg-success");
    properties.forEach(function (property) {
        var newInput = $("<input>").attr("autocomplete", "off").attr("name", property);
        var datalistId = property + "-fk-list";
        if ($("#data-lists").find("datalist#" + datalistId).length) {
            newInput.attr("list", datalistId);
        }
        $("<td>").append(newInput).appendTo(newRow);
    });
    $("<td>").append($("<button>").addClass("btn btn-danger").text("Видалити").click(function () {
        var jqTr = $(this).closest("tr");
        if (jqTr.hasClass("bg-danger")) {
            jqTr.removeClass("bg-danger");
            this.textContent = "Видалити";
        } else {
            if (jqTr.find("input").filter(function () { return $(this).val(); }).length > 0) {
                jqTr.addClass("bg-danger");
                this.textContent = "Відмінити";
            } else {
                jqTr.remove();
            }
        }
    })).appendTo(newRow);
    newRow.insertAfter(lastRow);
}

function Save() {
    var changedRows = [];
    var createdRows = [];
    var removedRows = [];

    fillArray(changedRows, "tr.bg-warning", true);
    fillArray(createdRows, "tr.bg-success:not(.bg-danger)", false);
    $("tr.bg-danger").each(function (index, element) {
        var jqTr = $(element);
        var data = { oldPK: jqTr.data() };
        if (!jQuery.isEmptyObject(data.oldPK)) {
            removedRows.push(data);
        }
    });

    function fillArray(arr, trSelector, needOld) {
        $(trSelector).each(function (index, element) {
            var jqTr = $(element);
            var data = {};
            if (needOld) data.oldPK = jqTr.data();
            data.newVal = {};
            jqTr.find("input").each(function (ind, inp) {
                var jqInp = $(inp);
                data.newVal[jqInp.attr("name")] = jqInp.val();
            });
            arr.push(data);
        });
    }

    $.ajax({
        type: "post",
        url: "/Monitor/SaveChanges",
        data: { rows: JSON.stringify({ created: createdRows, changed: changedRows, removed: removedRows }) },
        success: function () {
            document.location.reload();
        },
        error: function (jqXHR) {
            alert(jqXHR.responseText);
        }
    });
}