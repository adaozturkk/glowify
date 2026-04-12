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
                "className": "text-center",
                "render": function (data) {
                    var today = new Date().getTime();
                    var lockout = new Date(data).getTime();

                    if (lockout > today) {
                        return '<span class="badge bg-danger rounded-pill px-3 py-2"><i class="bi bi-lock-fill"></i> Locked</span>';
                    } else {
                        return '<span class="badge bg-success rounded-pill px-3 py-2"><i class="bi bi-unlock-fill"></i> Active</span>';
                    }
                },
                "width": "15%"
            },
            {
                data: { id: "id", lockoutEnd: "lockoutEnd" },
                "render": function (data) {
                    var today = new Date().getTime();
                    var lockout = new Date(data.lockoutEnd).getTime();

                    if (lockout > today) {
                        return `
                        <div class="d-flex justify-content-center gap-2">
                            <a onclick="LockUnlock('${data.id}')" class="btn btn-outline-success btn-sm rounded-pill px-3 d-flex align-items-center justify-content-center gap-1" style="cursor:pointer; width: 100px;">
                                <i class="bi bi-unlock-fill"></i> Unlock
                            </a>
                            <a href="/Admin/User/RoleManagement?userId=${data.id}" class="btn btn-primary btn-sm rounded-pill px-3 d-flex align-items-center justify-content-center gap-1" style="cursor:pointer; width: 130px;">
                                 <i class="bi bi-person-gear"></i> Permission
                            </a>
                        </div>
                        `
                    }
                    else {
                        return `
                        <div class="d-flex justify-content-center gap-2">
                            <a onclick="LockUnlock('${data.id}')" class="btn btn-outline-danger btn-sm rounded-pill px-3 d-flex align-items-center justify-content-center gap-1" style="cursor:pointer; width: 100px;">
                                <i class="bi bi-lock-fill"></i> Lock
                            </a>
                            <a href="/Admin/User/RoleManagement?userId=${data.id}" class="btn btn-primary btn-sm rounded-pill px-3 d-flex align-items-center justify-content-center gap-1" style="cursor:pointer; width: 130px;">
                                 <i class="bi bi-person-gear"></i> Permission
                            </a>
                        </div>
                        `
                    }
                },
                "width": "20%"
            }
        ]
    });
}

function LockUnlock(id) {
    $.ajax({
        type: "POST",
        url: '/Admin/User/LockUnlock',
        data: JSON.stringify(id),
        contentType: "application/json",
        success: function (data) {
            if (data.success) {
                toastr.success(data.message);
                dataTable.ajax.reload();
            } else {
                toastr.error(data.message);
            }
        }
    });
}