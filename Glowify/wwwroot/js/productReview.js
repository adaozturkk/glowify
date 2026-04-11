var dataTable;

$(document).ready(function () {
    loadDataTable();
});

function loadDataTable() {
    dataTable = $('#tblData').DataTable({
        "ajax": { url: '/Admin/ProductReview/GetAll' },
        "order": [],
        "columns": [
            { data: 'product.name', "width": "15%" },
            { data: 'applicationUser.name', "width": "15%" },
            {
                data: 'rating',
                "width": "5%",
                "className": "text-center",
                "render": function (data) {
                    return `<span class="fw-bold">${data}</span> <i class="bi bi-star-fill text-warning"></i>`;
                }
            },
            {
                data: 'reviewDate',
                "className": "text-center",
                "render": function (data) {
                    if (!data) return "";
                    var date = new Date(data);
                    return date.toLocaleDateString('tr-TR');
                },
                "width": "10%"
            },
            {
                data: 'comment',
                "render": function (data) {
                    if (!data) return "-";
                    return `<div style="max-height: 80px; overflow-y: auto; white-space: normal; padding-right: 5px;">${data}</div>`;
                },
                "width": "30%"
            },
            {
                data: 'isApproved',
                "className": "text-center",
                "render": function (data) {
                    if (data) {
                        return `<span class="badge bg-success rounded-pill px-3 py-2"><i class="bi bi-check-circle"></i> Approved</span>`;
                    } else {
                        return `<span class="badge bg-warning text-dark rounded-pill px-3 py-2"><i class="bi bi-clock-history"></i> Pending</span>`;
                    }
                },
                "width": "10%"
            },
            {
                data: 'id',
                "render": function (data, type, row) {
                    if (row.isApproved) {
                        return `
                        <div class="d-flex justify-content-center gap-2">
                            <a onClick=Delete('/admin/productreview/delete/${data}') class="btn btn-outline-danger btn-sm rounded-pill px-3 d-flex align-items-center gap-1">
                                <i class="bi bi-trash-fill"></i> Delete
                            </a>
                        </div>
                        `;
                    } else {
                        return `
                        <div class="d-flex justify-content-center gap-2">
                            <a onClick=Approve('/admin/productreview/approve/${data}') class="btn btn-outline-success btn-sm rounded-pill px-3 d-flex align-items-center gap-1">
                                <i class="bi bi-check-circle-fill"></i> Approve
                            </a>
                            <a onClick=Delete('/admin/productreview/delete/${data}') class="btn btn-outline-danger btn-sm rounded-pill px-3 d-flex align-items-center gap-1">
                                <i class="bi bi-trash-fill"></i> Delete
                            </a>
                        </div>
                        `;
                    }
                },
                "width": "15%"
            }
        ]
    });
}


function Delete(url) {
    Swal.fire({
        title: 'Are you sure?',
        text: "You won't be able to revert this!",
        icon: 'warning',
        showCancelButton: true,
        customClass: {
            confirmButton: 'btn btn-danger rounded-pill px-4 mx-2',
            cancelButton: 'btn btn-dark rounded-pill px-4 mx-2'
        },
        buttonsStyling: false,
        confirmButtonText: 'Yes, delete it!'
    }).then((result) => {
        if (result.isConfirmed) {
            $.ajax({
                url: url,
                type: 'DELETE',
                success: function (data) {
                    if (data.success) {
                        dataTable.ajax.reload();
                        toastr.success(data.message);
                    } else {
                        toastr.error(data.message);
                    }
                }
            })
        }
    })
}

function Approve(url) {
    Swal.fire({
        title: 'Approve this review?',
        text: "This review will be visible to all customers on the product page.",
        icon: 'question',
        showCancelButton: true,
        customClass: {
            confirmButton: 'btn btn-success rounded-pill px-4 mx-2',
            cancelButton: 'btn btn-dark rounded-pill px-4 mx-2'
        },
        buttonsStyling: false,
        confirmButtonText: 'Yes, approve it!'
    }).then((result) => {
        if (result.isConfirmed) {
            $.ajax({
                url: url,
                type: 'POST',
                success: function (data) {
                    if (data.success) {
                        dataTable.ajax.reload();
                        toastr.success(data.message);
                    } else {
                        toastr.error(data.message);
                    }
                }
            })
        }
    })
}