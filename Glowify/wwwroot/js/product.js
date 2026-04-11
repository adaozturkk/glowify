var dataTable;

$(document).ready(function () {
    loadDataTable();
});

function loadDataTable() {
    dataTable = $('#tblData').DataTable({
        "ajax": {
            "url": "/Admin/Product/GetAll"
        },
        "columns": [
            { "data": "name", "width": "25%" },
            {
                "data": "price",
                "width": "15%",
                "render": function (data) {
                    return parseFloat(data).toFixed(2) + ' ₺';
                }
            },
            {
                "data": "stock",
                "width": "15%",
                "className": "text-center",
                "render": function (data) {
                    if (data <= 0) {
                        return '<span class="badge bg-danger rounded-pill px-2">Out of Stock</span>';
                    }
                    return data;
                }
            },
            { "data": "category", "width": "20%" },
            {
                "data": "id",
                "render": function (data) {
                    return `
                        <div class="d-flex justify-content-center gap-2">
                            <a href="/Admin/Product/Upsert?id=${data}" class="btn btn-outline-primary btn-sm rounded-pill px-3 d-flex align-items-center gap-1"> 
                                <i class="bi bi-pencil-square"></i> Edit
                            </a>               
                            <a onClick=Delete('/Admin/Product/Delete/${data}') class="btn btn-outline-danger btn-sm rounded-pill px-3 d-flex align-items-center gap-1"> 
                                <i class="bi bi-trash-fill"></i> Delete
                            </a>
                        </div>`
                },
                "width": "25%"
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
                    }
                    else {
                        toastr.error(data.message);
                    }
                }
            })
        }
    })
}