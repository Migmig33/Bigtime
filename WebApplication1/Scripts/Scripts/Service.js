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