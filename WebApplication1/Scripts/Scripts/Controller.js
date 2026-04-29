app.controller('controller', function ($scope, $http) {
    $scope.goTo = function (page) {
        switch (page) {
            case 'Home':
                return window.location.href = '/System/RentersHome';
            case 'Browse':
                return window.location.href = '/System/RentersBrowse';
            case 'MBookings':
                return window.location.href = '/System/RentersBookings';
            case 'Dashboard':
                return window.location.href = '/System/AdminDashboard';
            case 'Units':
                return window.location.href = '/System/AdminUnits';
            case 'Bookings':
                return window.location.href = '/System/AdminBookings';
            case 'Tenants':
                return window.location.href = '/System/AdminTenants';   
        }
    }
})