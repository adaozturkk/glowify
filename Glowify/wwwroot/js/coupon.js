var dataTable;

$(document).ready(function () {
    loadDataTable();
});

function loadDataTable() {
    dataTable = $('#tblData').DataTable({
        "ajax": { url: '/Admin/Coupon/GetAll' },
        "columns": [
            { data: 'code', "width": "15%" },
            {
                data: 'discountAmount',
                "width": "15%",
                render: function (data) {
                    return data;
                }
            },
            {
                data: 'minAmount',
                "width": "15%",
                render: function (data) {
                    return data;
                }
            },
            {
                data: 'isActive',
                "width": "15%",
                render: function (data) {
                    if (data === true) {
                        return '<span class="badge bg-success">Active</span>';
                    } else {
                        return '<span class="badge bg-danger">Passive</span>';
                    }
                }
            },
            {
                data: 'id',
                "render": function (data) {
                    return `<div class="w-75 btn-group" role="group">
                     <a href="/Admin/Coupon/Upsert?id=${data}" class="btn btn-primary mx-2"> <i class="bi bi-pencil-square"></i> Edit</a>               
                     <a onClick=Delete('/Admin/Coupon/Delete/${data}') class="btn btn-danger mx-2"> <i class="bi bi-trash-fill"></i> Delete</a>
                    </div>`
                },
                "width": "25%"
            }
        ]
    });
}

function filterTable(status, button) {
    if (status === 'all') {
        dataTable.search('').columns().search('').draw();
    } else {
        dataTable.column(3).search(status).draw();
    }

    $('.btn-group .btn').removeClass('active');
    $(button).addClass('active');
}

function Delete(url) {
    Swal.fire({
        title: 'Are you sure?',
        text: "You won't be able to revert this!",
        icon: 'warning',
        showCancelButton: true,
        confirmButtonColor: '#3085d6',
        cancelButtonColor: '#d33',
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
                    }
                    else {
                        toastr.error(data.message);
                    }
                }
            })
        }
    })
}