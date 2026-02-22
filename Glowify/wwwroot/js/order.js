var dataTable;

$(document).ready(function () {
    var url = window.location.search;

    if (url.includes("inprocess")) {
        loadDataTable("inprocess");
    }
    else if (url.includes("shipped")) {
        loadDataTable("shipped");
    }
    else if (url.includes("pending")) {
        loadDataTable("pending");
    }
    else if (url.includes("approved")) {
        loadDataTable("approved");
    }
    else if (url.includes("cancelled")) {
        loadDataTable("cancelled");
    }
    else if (url.includes("completed"))
    {
        loadDataTable("completed")
    }
    else {
        loadDataTable("all");
    }
});

function loadDataTable(status) {
    var table = $('#tblData');
    var apiUrl = table.data('url');
    var detailsUrl = table.data('details-url');

    dataTable = $('#tblData').DataTable({
        "ajax": {
            "url": apiUrl + "?status=" + status
        },
        "order": [[0, "desc"]],
        "columns": [
            { "data": "id", "width": "5%" },
            { "data": "name", "width": "15%" },
            { "data": "phoneNumber", "width": "15%" },
            { "data": "applicationUser.email", "width": "15%" },
            { "data": "orderStatus", "width": "15%" },
            { "data": "orderTotal", "width": "10%" },
            {
                "data": "id",
                "render": function (data) {
                    return `
                        <div class="w-75 btn-group" role="group">
                        <a href="${detailsUrl}?orderId=${data}"
                        class="btn btn-primary mx-2"> <i class="bi bi-pencil-square"></i> Details</a>
                        </div>
                        `
                },
                "width": "15%"
            }
        ]
    });
}