var dataTable;

$(document).ready(function () {
    loadDataTable();
});

function loadDataTable() {
    dataTable = $('#tblData').DataTable({
        "ajax": { url: '/admin/user/getall' },
        "columns": [
            { data: 'name', "width": "15%" },
            { data: 'email', "width": "20%" },
            { data: 'phoneNumber', "width": "15%" },
            { data: 'role', "width": "15%" },
            {
                data: 'lockoutEnd',
                "render": function (data) {
                    var today = new Date().getTime();
                    var lockout = new Date(data).getTime();
                    if (lockout > today) {
                        return '<span class="badge bg-danger rounded-pill">Locked</span>';
                    } else {
                        return '<span class="badge bg-success rounded-pill">Active</span>';
                    }
                },
                "width": "10%"
            },
            {
                data: 'id',
                "render": function (data) {
                    return `<div class="text-center"></div>`;
                },
                "width": "25%"
            }
        ]
    });
}