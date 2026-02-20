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
            { data: 'rating', "width": "5%" },
            {
                data: 'reviewDate',
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
                        <div class="w-100 btn-group" role="group">
                            <a onClick=Delete('/admin/productreview/delete/${data}') class="btn btn-danger mx-1 rounded">
                                <i class="bi bi-trash-fill"></i> Delete
                            </a>
                        </div>
                        `;
                    } else {
                        return `
                        <div class="w-100 btn-group" role="group">
                            <a onClick=Approve('/admin/productreview/approve/${data}') class="btn btn-success mx-1 rounded">
                                <i class="bi bi-check-circle-fill"></i> Approve
                            </a>
                            <a onClick=Delete('/admin/productreview/delete/${data}') class="btn btn-danger mx-1 rounded">
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
        confirmButtonColor: '#d33',
        cancelButtonColor: '#6c757d',
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
        confirmButtonColor: '#198754',
        cancelButtonColor: '#6c757d',
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