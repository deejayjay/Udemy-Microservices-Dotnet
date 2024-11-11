var dataTable;
$(document).ready(function () {
    const urlParams = new URLSearchParams(window.location.search);
    const status = urlParams.get("status") ?? "all";
    
    loadDataTable(status);
});

function loadDataTable(status) {
    dataTable = $('#tblOrderData').DataTable({
        order: [[0, 'desc']],
        ajax: {
            url: `/order/getall?status=${status}`,
            dataSrc: 'data'
        },        
        columns: [
            { data: 'orderHeaderId', width: "5%" },
            { data: 'email', width: "25%" },
            { data: 'name', width: "25%" },
            { data: 'phone', width: "15%" },
            { data: 'status', width: "10%" },
            { data: 'orderTotal', width: "10%" },
            {
                data: 'orderHeaderId',
                render: function (data) {
                    return `<div class="w-75 btn-group" role="group">
                                <a href="/order/orderDetails?orderId=${data}" class="btn btn-primary mx-2">
                                    <i class="bi bi-pencil-square"></i>
                                </a>
                            </div>`;
                }
            }
        ],
    })
}