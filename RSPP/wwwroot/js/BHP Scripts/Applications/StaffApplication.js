
$(function () {

    // error message
    function ErrorMessage(error_id, error_message) {
        $(error_id).fadeIn('fast')
            .html("<div class=\"alert alert-danger\"><i class=\"fa fa-warning\"> </i> <span class=\"text-dark\"> " + error_message + " </span> </div>")
            .delay(9000)
            .fadeOut('fast');
        return;
    }

    // success message
    function SuccessMessage(success_id, success_message) {
        $(success_id).fadeIn('fast')
            .html("<div class=\"alert alert-success\"> <i class=\"fa fa-check-circle\"> </i> <span class=\"text-dark\">" + success_message + " </span> </div>")
            .delay(10000)
            .fadeOut('fast');
        return;
    }


    // application on staff desk count
    $.post("/Staffs/MyDeskCount", function (response) {
        $("#MyDeskCount").text(response);
        $("#staffDeskCount").text(response);
    });

    // schedules count for staff
    $.post("/Staffs/MySchduleCount", function (response) {
        $("#StaffScheduleCount").text(response);
    });

    // checking out of office reliver
    $.post("/OutOfOffices/CountRelieveStaff", {}, function (response) {
        $("#OutOfOfficeCout1").text(response);
        $("#OutOfOfficeCout2").text(response);
    });


    //Out of office start
    $.post("/OutOfOffices/TriggerOutOfOfficeStart", {}, function (response) {
        console.log(response);
    });


    //Out of office start
    $.post("/OutOfOffices/TriggerOutOfOfficeEnd", {}, function (response) {
        console.log(response);
    });


    setInterval(function () {

        // application on staff desk count
        $.post("/Staffs/MyDeskCount", function (response) {
            $("#MyDeskCount").text(response);
            $("#staffDeskCount").text(response);
        });


        // schedules count for staff
        $.post("/Staffs/MySchduleCount", function (response) {
            $("#StaffScheduleCount").text(response);
        });

        // checking out of office reliver
        $.post("/OutOfOffices/CountRelieveStaff", {}, function (response) {
            $("#OutOfOfficeCout1").text(response);
            $("#OutOfOfficeCout2").text(response);
        });


        //Out of office start
        $.post("/OutOfOffices/TriggerOutOfOfficeStart", {}, function (response) {
            console.log(response);
        });


        //Out of office start
        $.post("/OutOfOffices/TriggerOutOfOfficeEnd", {}, function (response) {
            console.log(response);
        });

    }, 60000); // 1 mins
    



    var apps = [];
    var myDeskApps = [];
    var staffApps = [];
    var HSEstaffApps = [];



    $("#btnPushApp").on('click', function (event) {
        event.preventDefault();

        apps.length = 0;

        var rows_selected = AppDropTable.column(0).checkboxes.selected();

        if (rows_selected.length !== 0) {

            $.each(rows_selected, function (index, rowId) {
                apps.push(rowId);
            });

            var str = apps.join(",");
            $("#showpush").val(str);

            $("#btnPushApp2").click();
            pushStaff.ajax.reload();
        }
        else {
            alert("Please select applications to assign");
        }
    });




    /*
     * Displaying all application on a particular staff desk for distribution
     */ 
    var staffDeskApps = $("#MyDeskAppTable").DataTable({

        'initComplete': function (settings) {
            var api = this.api();

            api.cells(
                api.rows(function (idx, data, node) {

                    return (data[8] === '<b>YES</b>') ? true : false;
                }).indexes(),
                0
            ).checkboxes.disable();
        },

        'columnDefs': [
            {
                'targets': 0,
                'checkboxes': {
                    'selectRow': true
                },
                'createdCell': function (td, cellData, rowData, row, col) {

                    if ($.trim(rowData["hasPushed"]) === 'true') {
                        this.api().cell(td).checkboxes.disable();
                    }
                }
            }
        ],

        dom: 'Bfrtip',
        buttons: [
            'pageLength',
            'copyHtml5',
            'excelHtml5',
            'csvHtml5',
            'pdfHtml5',
            {
                extend: 'print',
                text: 'Print all',
                exportOptions: {
                    modifier: {
                        selected: null
                    }
                }
            },
            {
                extend: 'colvis',
                collectionLayout: 'fixed two-column'
            }

        ],

        language: {
            buttons: {
                colvis: 'Change columns'
            }
        },

        'deferRender': true,
        "select": {
            'style': 'multi'
        }
    });







    /*
    * Getting applications on staffs desk for processing
    * 
    */
    var MyDeskTable = $("#MyDeskAppsTable").DataTable({

        'columnDefs': [
            {
                'targets': 0,
                'checkboxes': {
                    'selectRow': true
                },
                'createdCell': function (td, cellData, rowData, row, col) {

                    if ($.trim(rowData["hasPushed"]) === 'true') {
                        this.api().cell(td).checkboxes.disable();
                    }
                }
            }
        ],

        'deferRender': true,
        "select": {
            'style': 'multi'
        },

        ajax: {
            url: "/Applications/GetMyDeskApps",
            type: "POST",
            lengthMenu: [[10, 25, 50, 100], [10, 25, 50, 100]]
        },

        dom: 'Bfrtip',
        buttons: [
            'pageLength',
            'copyHtml5',
            'excelHtml5',
            'csvHtml5',
            'pdfHtml5',
            {
                extend: 'print',
                text: 'Print all',
                exportOptions: {
                    modifier: {
                        selected: null
                    }
                }
            },
            {
                extend: 'colvis',
                collectionLayout: 'fixed two-column'
            }

        ],

        language: {
            buttons: {
                colvis: 'Change columns'
            }
        },

        "processing": true,
        "serverSide": true,
       
        columns: [
            {
                data: "deskID"
            },
            {
                "render": function (data, type, row) {
                    return "<b class=\"text-danger\"> " + row["refNo"] + " </b>";
                }
            },
           
            {
                "render": function (data, type, row) {
                    return "<b class=\"text-dark\"> " + row["companyName"] + " </b>";
                }
            },
            { data: "companyAddress" },
            { data: "year" },
            {
                "render": function (data, type, row) {
                    return "<b class=\"text-dark\"> " + row["proposalApproved"] + " </b>";
                }
            },
            {
                "render": function (data, type, row) {
                    return "<b class=\"text-dark\"> " + row["reportApproved"] + " </b>";
                }
            },
            
            {
                "render": function (data, type, row) {
                    return "<button class=\"btn btn-sm btn-warning\"> " + row["status"] + " </button>";
                }
            },
            { data: "createdAt" }, 
            { data: "updatedAt" }, 
            {
                "render": function (data, type, row) {
                    return "<b class=\"text-primary\"> "+ row["activity"]+" </b>";
                }
            },
            {
                "render": function (data, type, row) {
                    return "<button class=\"btn btn-sm btn-info\" onclick=\"ViewApplication(" + row["deskID"] + "," + row["processID"] + ")\"> <i class=\"fa fa-eye\"> </i> view Application </button>";
                }
            }
        ]
    });



    $("#btnSinglePush").on('click', function (event) {

        var id = $("#Insshowpush");

        if (id.val() !== "") {
            $("#btnPushSingleApp").click();
        }
        else {
           
            Notify.alert({
                title: 'Failure',
                text: "Application link was broken and cannot be Assign"
            });
        }

    });




    // using
    $("#btnInsPushApp").on('click', function (event) {
        event.preventDefault();

        myDeskApps.length = 0;

        var rows_selected = MyDeskTable.column(0).checkboxes.selected();

        if (rows_selected.length !== 0) {

            $.each(rows_selected, function (index, rowId) {
                myDeskApps.push(rowId);
            });

            var str = myDeskApps.join(",");
            $("#Insshowpush").val(str);

            $("#btnInsPushApp2").click();
            MyDeskTable.ajax.reload();
        }
        else {

            alert("Please select applications to assign");
        }
    });



  





    /*
     * This is to rerout all application from the desk of one staff to another
     */
    $("#btnStaffRerouteApps").on('click', function (event) {
        event.preventDefault();

        staffApps.length = 0;

       // var rows_select = staffDeskApps.column(0).checkboxes.select();
        var rows_selected = staffDeskApps.column(0).checkboxes.selected();

        if (rows_selected.length !== 0) {

            $.each(rows_selected, function (index, rowId) {
                staffApps.push(rowId);
            });

            var str = staffApps.join(",");
            $("#Insshowpush").val(str);

            $("#btnGetStaffs").click();
        }
        else {

            alert("Please select staff and applications to Re-route");
        }
    });




    $("#btnGetStaffs").on('click', function (event) {

        var RouteApp = $("#TableDistributeApps").DataTable({

            ajax: {
                url: "/Deskes/GetRouteStaff?staff=" + $("#txtOriginalStaffID").val(),
                type: "POST",
                lengthMenu: [[10, 25, 50, 100], [10, 25, 50, 100]]
            },

            "destroy": true,
            "processing": true,
            "serverSide": true,
            "searching": false,
            "info": false,
            "paging": true,
            "lengthChange": false,

            columns: [
                { data: "lastName" },
                { data: "firstName" },

                { data: "staffEmail" },
                {
                    "render": function (data, type, row) {
                        return "<button class=\"btn btn-sm btn-info push\" onclick=\"RerouteApps(" + row["staffId"] + ")\"> Re-Route Application </button>";
                    }
                }
            ]
        });

    });



   




    /*
     * Push application to staff using
     */
    var DistributApps = $("#DivDistributApps").DataTable({

        ajax: {
            url: "/Applications/GetPushStaff",
            type: "POST",
            lengthMenu: [[10, 25, 50, 100], [10, 25, 50, 100]]
        },

        "processing": true,
        "serverSide": true,
        "searching": false,
        "info": false,
        "paging": true,
        "lengthChange": false,

        columns: [
            { data: "lastName" },
            { data: "firstName" },

            { data: "email" },
            { data: "deskCount" },
            {
                "render": function (data, type, row) {
                    return "<button class=\"btn btn-sm btn-info push\" onclick=\"DistributStaffApps(" + row["staffId"] + ")\"> Assign Application </button>";
                }
            }
        ]
    });



    /*
     * Saving an nominated staff
     */
    $("#btnAddStaff").on('click', function (event) {

        var txtNominatedStaff = $("#txtNominatedStaff");

        if (txtNominatedStaff.val() === "" || $("#txtRequestId").val() === "") {
            ErrorMessage("#AddStaffModalInfo", "Please select a staff");
        }
        else {

            Notify.confirm({
                title: 'Nominate Staff',
                text: 'Are you sure you want to nominate this staff?',
                ok: function () {
                    $("#AddStaffLoader").addClass("Submitloader");
                    $.post("/Applications/SaveNominatedStaff",
                        {
                            "RequestID": $("#txtRequestId").val(),
                            "txtStaffID": txtNominatedStaff.val(),
                            "txtPhone": $("#txtPhoneNumber").val(),
                            "txtDesignation": $("#txtDesignation").val(),
                        },
                        function (response) {
                        if ($.trim(response) === "Saved") {
                            SuccessMessage("#AddStaffModalInfo", "Nomination Successfully saved.");
                            txtNominatedStaff.val("");
                            $("#txtPhoneNumber").val("");
                            $("#txtDesignation").val("");
                            $("#divNominatedStaff").load(location.href + " #divNominatedStaff");
                            $("#AddStaffLoader").removeClass("Submitloader");
                        }
                        else {
                            ErrorMessage("#AddStaffModalInfo", response);
                            $("#AddStaffLoader").removeClass("Submitloader");
                        }
                    });
                }
            });

        }
    });


  



    /*
     * Saving an application Report
     */
    $("#btnSaveReport").on('click', function (event) {

        var txtReport = $("#txtAppReport");

        if (txtReport.val() === "" || $("#txtRequestId").val() === "") {
            ErrorMessage("#ReportModalInfo", "Please enter a comment for this report");
        }
        else {

            Notify.confirm({
                title: 'Save Report',
                text: 'Are you sure you want to save this report?',
                ok: function () {
                    $("#AppReportLoader").addClass("Submitloader");
                    $.post("/Applications/SaveReport", { "RequestID": $("#txtRequestId").val(), "txtReport": txtReport.val() }, function (response) {
                        if ($.trim(response) === "Report Saved") {
                            SuccessMessage("#ReportModalInfo", "Report successfully saved.");
                            txtReport.val("");
                            $("#divReport").load(location.href + " #divReport");
                            $("#AppReportLoader").removeClass("Submitloader");
                        }
                        else {
                            ErrorMessage("#ReportModalInfo", response);
                            $("#AppReportLoader").removeClass("Submitloader");
                        }
                    });
                }
            });

        }
    });



    /*
    * Editing an application Report
    */
    $("#btnEditReport").on('click', function (event) {

        var txtReport = $("#txtEditAppReport");

        if (txtReport.val() === "" || $("#txtEditReportID").val() === "") {
            ErrorMessage("#EditReportModalInfo", "Please enter a comment for this report");
        }
        else {

            Notify.confirm({
                title: 'Edit Report',
                text: 'Are you sure you want to edit this report?',
                ok: function () {

                    $("#AppEditReportLoadder").addClass("Submitloader");

                    $.post("/Applications/EditReport", { "ReportID": $("#txtEditReportID").val(), "txtReport": txtReport.val() }, function (response) {
                        if ($.trim(response) === "Report Edited") {
                            SuccessMessage("#EditReportModalInfo", "Report successfully edited.");
                            txtReport.val("");
                            $("#divReport").load(location.href + " #divReport");
                            $("#AppEditReportLoadder").removeClass("Submitloader");
                        }
                        else {
                            ErrorMessage("#EditReportModalInfo", response);
                            $("#AppEditReportLoadder").removeClass("Submitloader");
                        }
                    });
                }
            });

        }
    });


    /*
     * App schdule datetimepicker
     */
    $("#txtSchduleDate").datetimepicker({
        minDate: new Date().setDate(new Date().getDate() + 1), //- this is tomorrow;  use 0 for toady
        defaultDate: new Date().setDate(new Date().getDate() + 1),
        onGenerate: function (ct) {
            $(this).find('.xdsoft_date.xdsoft_weekend')
                .addClass('xdsoft_disabled');
        }
    });


    /*
     * edit App schdule datetimepicker
     */
    $("#txtEditSchduleDate").datetimepicker({
        minDate: new Date().setDate(new Date().getDate() + 1), //- this is tomorrow;  use 0 for toady
        defaultDate: new Date().setDate(new Date().getDate() + 1),
        onGenerate: function (ct) {
            $(this).find('.xdsoft_date.xdsoft_weekend')
                .addClass('xdsoft_disabled');
        }
    });



    /*
     * Create a schdule
     */
    $("#btnSaveSchdule").on('click', function (event) {
        event.preventDefault();

        if ($("#txtSchduleComment").val() === "" || $("#txtSchduleDate").val() === "") {
            ErrorMessage("#SchduleModalInfo", "Please enter schedule date and comment");
        }
        else {

            Notify.confirm({
                title: 'Create Schedule',
                text: 'Are you sure you want to create this schedule?',
                ok: function () {

                    $("#AppSchduleLoadder").addClass("Submitloader");

                    $.post("/Applications/CreateSchduleAsync",
                        {
                            "SchduleDate": $("#txtSchduleDate").val(),
                            "SchduleComment": $("#txtSchduleComment").val(),
                            "RequestID": $("#txtRequestId").val(),
                            "DeskID": $("#txtDeskID").val(), // dask id
                            "SchduleLocation": $("#txtSchduleLocation").val(),
                            "SchduleType": $("#txtSchduleType").val()
                        },
                        function (response) {
                            if ($.trim(response) === "Schdule Created") {
                                $("#divSchdule").load(location.href + " #divSchdule");
                                SuccessMessage("#SchduleModalInfo", "Schedule created successfully");
                                $("#txtSchduleComment").val('');
                                $("#txtSchduleDate").val('');
                                $("#AppSchduleLoadder").removeClass("Submitloader");
                            }
                            else {
                                ErrorMessage("#SchduleModalInfo", response);
                                $("#AppSchduleLoadder").removeClass("Submitloader");
                            }
                        });
                }
            });

        }
    });



    /*
     * Edit a schdule 
     */
    $("#btnEditSchdule").on('click', function (event) {
        event.preventDefault();

        if ($("#txtEditSchduleDate").val() === "" || $("#txtEditSchduleID").val() === "" || $("#txtEditSchduleComment").val() === "") {
            ErrorMessage("#EditSchduleModalInfo", "Please enter a comment annd date for this schedule.");
        }
        else {
           
            Notify.confirm({
                title: 'Edit Schedule',
                text: 'Are you sure you want to edit this schedule?',
                ok: function () {

                    $("#AppEditSchduleLoadder").addClass("Submitloader");

                    $.post("/Applications/EditSchduleAsync",
                        {
                            "schduleID": $("#txtEditSchduleID").val(),
                            "txtComment": $("#txtEditSchduleComment").val(),
                            "txtSchduleDate": $("#txtEditSchduleDate").val(),
                            "txtSchduleLoaction": $("#txtEditSchduleLocation").val(),
                            "txtSchduleType": $("#txtEditSchduleType").val()
                        },
                        function (response) {
                            if ($.trim(response) === "Schdule Edited") {
                                SuccessMessage("#EditSchduleModalInfo", "Schedule successfully edited.");
                                $("#txtEditSchduleDate").val("");
                                $("#txtEditSchduleComment").val("");
                                $("#divSchdule").load(location.href + " #divSchdule");
                                $("#AppEditSchduleLoadder").removeClass("Submitloader");
                            }
                            else {
                                ErrorMessage("#EditSchduleModalInfo", response);
                                $("#AppEditSchduleLoadder").removeClass("Submitloader");
                            }
                        });
                }
            });
        }

    });



    /*
     * Approval
     */
    $("#btnApprove").on('click', function (event) {
        event.preventDefault();

        var txtComment = $("#txtAppApproveComment");
        var txtDeskID = $("#txtDeskID");
        var txtAppID = $("#txtRequestId");

        var location = window.location.origin + "/Applications/MyDesk";

        if (txtComment.val() === "") {
            ErrorMessage("#ApproveModalInfo", "Please enter a comment.");
        }
        else if (txtDeskID.val() === "" || txtAppID.val() === "") {
            ErrorMessage("#ApproveModalInfo", "Error... Missing application. Refresh the page.");
        }

        else {
            
            Notify.confirm({
                title: 'Approve Application',
                text: 'Are you sure you want to Approve this application?',
                ok: function () {

                    $("#AppApproveLoadder").addClass("Submitloader");

                    $.post("/Applications/ApprovalAsync",
                        {
                            "txtComment": txtComment.val(),
                            "txtDeskID": txtDeskID.val(),
                            "txtRequestID": txtAppID.val()
                        },
                        function (response) {

                            if ($.trim(response) === "Approved Next") {
                                SuccessMessage("#ApproveModalInfo", "Application approved and moved to the next processing officer.");
                                alert("Application approved and moved to the next processing officer.");
                                window.location.href = location;
                                $("#AppApproveLoadder").removeClass("Submitloader");
                            }
                            else if ($.trim(response) === "Report Approved") {
                                SuccessMessage("#ApproveModalInfo", "Company application report approved successfully.");
                                alert("Company application report approved successfully.");
                                window.location.href = location;
                                $("#AppApproveLoadder").removeClass("Submitloader");
                            }
                            else if ($.trim(response) === "Approved") {
                                SuccessMessage("#ApproveModalInfo", "SUCCESSFUL FINAL APPROVED BY HEAD. ");
                                alert("FINAL APPLICATION FORM APPROVED SUCCESSFULLY.");
                                window.location.href = location;

                                $("#AppApproveLoadder").removeClass("Submitloader");
                            }
                            else {
                                ErrorMessage("#ApproveModalInfo", response);
                                $("#AppApproveLoadder").removeClass("Submitloader");
                            }
                        });
                }
            });
        }
    });



    /*
     * Button for external rejection
     */
    $("#btnReject").on('click', function (event) {

        var txtComment = $("#txtAppExRejectComment");
        var txtDeskID = $("#txtDeskID");
        

        if (txtComment.val() === "") {
            ErrorMessage("#ExRejectModalInfo", "Please enter a comment for rejection.");
        }
        else {

            var location = window.location.origin + "/Applications/MyDesk";

            Notify.confirm({
                title: 'Reject Application',
                text: 'Are you sure you want to reject this application?',
                ok: function () {

                    $("#AppExternalRejectionLoadder").addClass("Submitloader");

                    $.post("/Applications/RejectionAsync", { "DeskID": txtDeskID.val(), "txtComment": txtComment.val() }, function (response) {

                        if ($.trim(response) === "External Rejection") {
                            alert("Application has been rejected back to company.");
                            SuccessMessage("#ExRejectModalInfo", "Application has been rejected back to company.");

                            window.location.href = location;

                            $("#AppExternalRejectionLoadder").removeClass("Submitloader");
                        }
                        else if ($.trim(response) === "Internal Rejection") {
                            alert("Application has been rejected back to staff.");
                            SuccessMessage("#ExRejectModalInfo", "Application has been rejected back to staff.");

                            window.location.href = location;
                        }
                        else {
                            $("#AppExternalRejectionLoadder").removeClass("Submitloader");
                            ErrorMessage("#ExRejectModalInfo", response);
                        }
                    });
                }
            });
           
        }
    });



    

   

    

  


    /*
     * Reseting application step
     *
     */
    $("#btnResetStep").on('click', function (event) {
        var app_id = $("#btnResetStep").val();

        var msg = confirm("Are you sure you want to reset all step for this application?");

        if (msg === true) {
            $.post("/Applications/ResetStep", { "AppID": app_id }, function (response) {
                if ($.trim(response) === "Reset Done") {
                    SuccessMessage("#ResetStepInfo", "All reset done for this application.");
                }
                else {
                    ErrorMessage("#ResetStepInfo", response);
                }
            });
        }
    });







    $("#RecordView").on('click', function (event) {
        $("#CalendarDiv").slideUp();
        $("#SchduleDiv").slideDown();
    });

    $("#CalendarView").on('click', function (event) {
        $("#SchduleDiv").slideUp();
        $("#CalendarDiv").slideDown();
    });
});








/*
 * Distributing applications to role based staff
 */ 
function DistributStaffApps(id) {

    var DeskApps = $("#Insshowpush").val().split(",");

    if ($("#txtPushComment").val() === "") {
        Notify.alert({
            title: 'Failure',
            text: "Please enter a comment for your distribution"
        });
    }
    else if ($("#Insshowpush").val() === "") {
        Notify.alert({
            title: 'Failure',
            text: "Please select application(s) to distribute."
        });
    }
    else {
        Notify.confirm({
            title: 'Assign Application',
            text: 'Are you sure you want to assign application(s) to this staff?',
            ok: function () {

                $("#DivDistributAppsLoader").addClass("Submitloader");

                $.post("/Applications/DistributAppToStaffAsync",
                    {
                        "staffID": id,
                        "DeskID": DeskApps,
                        "PushComment": $("#txtPushComment").val()
                    },
                    function (response) {
                        if (response.trim() === "Pushed") {
                            Notify.suc({
                                title: 'Success',
                                text: 'Application(s) Assigned successfully...'
                            });
                            $("#DivDistributAppsLoader").removeClass("Submitloader");
                            var location = window.location.origin + "/Applications/MyDesk";
                            window.location.href = location;
                        }
                        else {
                            Notify.alert({
                                title: 'Failure',
                                text: response
                            });
                            $("#DivDistributAppsLoader").removeClass("Submitloader");
                            location.reload(true);
                        }
                    });
            }
        });
    }

}





/*
 * Push single application to inspector
 */
function SinglePushApp(id) {

    var apps = $("#txtSinglePush").val().split(",");
    var txtPushComment = $("#txtPushComment").val();

    var msg = confirm("Are you sure you want to assign application(s) to this staff?");

    if (msg === true) {

        $(".push").attr("disable", "disabled");

        $.post("/Applications/PushInsApplications", { "staffID": id, "DeskID": apps, "PushComment": txtPushComment }, function (respons) {
            if (respons.trim() === "Pushed") {
                alert("Application(s) Assigned successfully...");
                var location = window.location.origin + "/Applications/MyDesk";
                window.location.href = location;
            }
            else {
                $("#SinglePushInfo").text(respons);

                $(".push").removeAttr("disabled");
            }
        });
    }

}


/*
 * View application
 */ 
function ViewApplication(desk_id, process_id) {

    $.getJSON("/Helpers/GetEncrypt", { "desk_id": desk_id, "process_id": process_id }, function (response) {
        var r = response.split("|");
        var location = window.location.origin + "/Applications/ViewApplication/" + r[0] + "/" + r[1];
        window.location.href = location;
    });
}




/*
 * Get applicatioon report for edit
 */
function GetReport(ReportID) {

    $("#txtEditReportID").val(ReportID);
   
    $.getJSON("/Applications/GetReport", { "ReportID": ReportID }, function (response) {
        var res = $.trim(response).split("|");

        if (res[0] === "1") {
            $("#txtEditAppReport").val(res[1]);
        }
        else {
            ErrorMessage("#ReportModalInfo", res[1]);
        }
    });
}


/*
 * Deleteing an application report
 */
function DeleteReport(reportID) {

    Notify.confirm({
        title: 'Delete Report',
        text: 'Are you sure you want to delete this report?',
        ok: function () {

            $.getJSON("/Applications/DeleteReport", { "ReportID": reportID }, function (response) {

                if (response === "Report Deleted") {

                    Notify.suc({
                        title: 'Success',
                        text: 'Application report removed successfully.'
                    });

                    $("#divReport").load(location.href + " #divReport");
                }
                else {
                    Notify.alert({
                        title: 'Failure',
                        text: response
                    });
                }
            });
        }
    });
}



/*
 * Getting schdule
 */
function GetSchdule(schduleID) {

    $("#txtEditSchduleID").val(schduleID);

    $.getJSON("/Applications/GetSchdule", { "schduleID": schduleID }, function (response) {
        var res = $.trim(response).split("|");

        if (res[0] === "1") {
            $("#txtEditSchduleComment").val(res[1]);
            $("#txtEditSchduleDate").val(res[2]);
        }
        else {
            ErrorMessage("#EditSchduleModalInfo", res[1]);
        }
    });
}


/*
 * Delete a schdule
 */
function DeleteSchdule(schduleID) {

    Notify.confirm({
        title: 'Delete Schedule',
        text: 'Are you sure you want to delete this schedule?',
        ok: function () {

            $.getJSON("/Applications/DeleteSchdule", { "schduleID": schduleID }, function (response) {

                if (response === "Schdule Deleted") {
                   
                    Notify.suc({
                        title: 'Success',
                        text: 'Application schedule removed successfully.'
                    });
                    $("#divSchdule").load(location.href + " #divSchdule");
                }
                else {
                    Notify.alert({
                        title: 'Failure',
                        text: response
                    });
                }
            });
        }
    });
}






/*
 * Re route applicationn to another staff
 */
function RerouteApps(id) {

    var apps = $("#Insshowpush").val().split(",");

    var previousStaff = $("#txtOriginalStaffID").val();

    var msg = confirm("Are you sure you want to re-route application(s) to this staff?");

    if (msg === true) {

        $(".push").attr("disable", "disabled");

        $.post("/Deskes/RerouteApps", { "staffID": id, "previousStaff": previousStaff, "AppID": apps }, function (respons) {
            var result = respons.trim().split("|");

            if (result[0] === "1") {
                alert(result[1]);
                var location = window.location.origin + "/Deskes/StaffDesk";
                window.location.href = location;
            }
            else {
                ErrorMessage("#InsAppDropInfo", result[1]);
                $(".push").removeAttr("disabled");
            }
        });
    }
}



/*
 * delete staff nomination
 */
function DeleteNomination(id) {

    var msg = confirm("Are you sure you want to remove this nominated staff?");

    if (msg === true) {

        $.post("/Applications/DeleteNomination", { "NominationID": id }, function (respons) {
            var result = respons.trim();

            if (result === "Deleted") {

                $("#divNominatedStaff").load(location.href + " #divNominatedStaff");
                alert("Nominated staff removed successfully.");
            }
            else {
                alert(result);
            }
        });
    }
}




