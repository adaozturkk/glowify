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
    else if (url.includes("completed")) {
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
            {
                "data": "orderStatus",
                "width": "15%",
                "className": "text-center",
                "render": function (data) {
                    let badgeClass = "bg-secondary";

                    if (data === "Pending") badgeClass = "bg-warning text-dark";
                    else if (data === "Approved") badgeClass = "bg-primary";
                    else if (data === "Processing") badgeClass = "bg-info text-dark";
                    else if (data === "Shipped") badgeClass = "bg-info";
                    else if (data === "Delivered") badgeClass = "bg-success";
                    else if (data === "Cancelled") badgeClass = "bg-danger";
                    else if (data === "Refunded") badgeClass = "bg-dark";

                    return `<span class="badge ${badgeClass} rounded-pill px-3 py-2">${data}</span>`;
                }
            },
            {
                "data": "orderTotal",
                "width": "10%",
                "render": function (data) {
                    return parseFloat(data).toFixed(2) + ' ₺';
                }
            },
            {
                "data": "id",
                "render": function (data) {
                    return `
                        <div class="d-flex justify-content-center gap-2">
                            <a href="${detailsUrl}?orderId=${data}" class="btn btn-outline-primary btn-sm rounded-pill px-4 d-flex align-items-center gap-1"> 
                                <i class="bi bi-pencil-square"></i> Details
                            </a>
                        </div>`
                },
                "width": "15%"
            }
        ]
    });
}