app.service('service', function ($http) {

    //Login Service
    this.authService = function (data) {
        var response = $http({
            url: "/Auth/Login",
            method: "post",
            data: JSON.stringify(data),
            headers: { "Content-Type": "application/json" }
        })
        return response;
    }
    this.verifyCodeService = function (code) {
        alert(code);
        var response = $http({
            url: "/Auth/VerifyCode",
            method: "post",
            data: JSON.stringify({code: code}),
            headers: {"Content-Type": "application/json"}
        })
        return response;
    }
    //Save service
    this.saveUnitService = function(data, images, id){ 
        var response = $http({
            url: "/System/SaveUnit",
            method: "post",
            data: {
                data: data,
                imageUrls: images,
                id: id
            },
            headers: {"Content-Type": "application/json"}
        })
        return response;
    }
    this.saveTenantService = function (tenantData, coOccupants, idFiles, id) {
        var response = $http({
            url: "/System/SaveTenant", 
            method: "post",
            data: {
                tenantData: tenantData,
                coOccupants: coOccupants,
                idFiles: idFiles,
                id: id
            },
            headers: { "Content-Type": "application/json" }
        });
        return response;
    };
    this.DeclineBookingService = function (id, reason) {
        return $http({
            method: 'post',
            url: '/System/DeclineBooking',
            data: {
                id: id,
                reason: reason
            }
        });
    };
    this.ConfirmBookingService = function (id) {
        return $http({
            method: 'post',
            url: '/System/ConfirmBooking',
            data: {
                id: id
            }
        });
    };
    this.ToggleBusyDateService = function (dateStr) {
        return $http({
            method: 'POST',
            url: '/System/ToggleBusyDate',
            data: {
                dateStr: dateStr
            }
        });
    };
    this.SaveMaintenanceRequestService = function (requestData, id) {
        return $http({
            method: 'post',
            url: '/System/SaveMaintenanceRequest',
            data: {
                requestData: requestData,
                id: id
            },
            headers: {"Content-Type": "application/json"}
        })
    }
    this.RemoveBusyDateService = function (dateStr) {
        var response = $http({
            method: 'POST',
            url: '/System/RemoveBusyDate',
            data: {
                dateStr: dateStr
            }
        });
        return response;

    };
    //Get service
    this.GetDashboardDataService = function () {
        return $http.get('/System/GetDashboardData');
    }
    this.GetAllUnitService = function () {
        return $http.get('/System/GetAllUnit');
    }
    this.GetAllTenantService = function () {
        return $http.get('/System/GetAllTenant');
    }
    this.GetAllBookingsService = function () {
        return $http.get('/System/GetAllBooking')
    }
    this.GetAllMaintenanceService = function () {
        return $http.get('/System/GetAllMaintenance')
    }
   

    //Delete Service
    this.DeleteUnitService = function (id) {
        var response = $http({
            url: "/System/DeleteUnit",
            method: "post",
            data: JSON.stringify({ Uid: id }),
            headers: {"Content-Type": "application/json"}
        })
        return response;
    }
    this.DeleteTenantService = function (id) {
        var response = $http({
            url: "/System/DeleteTenant",
            method: "post",
            data: JSON.stringify({ Tid: id }),
            headers: { "Content-Type": "application/json" }
        })
        return response;
    }
});