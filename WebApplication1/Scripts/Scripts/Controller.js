app.controller('controller', function (service, $scope, $timeout, $interval) {

    // Default Values
    $scope.thisMonthhs = new Date();
    $scope.editorModal = false;
    $scope.loading = false;
    $scope.step = 'credentials'
    $scope.toast = { toastShow: false, message: '', type: 'error' };

    $scope.searchQuery = '';
    $scope.statusFilter = 'All';
    $scope.occupancyFilter = 'All';
    $scope.sortAsc = true;

    //Unit Values
    $scope.beds = 'Studio';        // Sets default select option
    $scope.maxOccupants = 2;       // Sets default number
    $scope.status = 'active';      // Sets default status
    $scope.colorCode = '#1F6FEB';  // Sets default color
    $scope.address = 'Green Residences, Taft Avenue, Manila';
    $scope.price = 15000;
    $scope.amenities = [];         // Empty array for toggles

    // CHarts
    //Line graph
    $scope.linelabels = [];
    $scope.linedata = [[]];
    $scope.lineseries = ['Revenue'];


    // Bar Chart
    $scope.barlabels = [];
    $scope.bardata = [[], []];
    $scope.barseries = ['Occupied', 'Vacant'];

    //User Interactionn

    //filter functions
    $scope.filteredUnits = function () {
        var list = $scope.units || [];

        //search 
        if ($scope.searchQuery) {
            var q = $scope.searchQuery.toLowerCase();
            list = list.filter(function (u) {
                return u.unitName.toLowerCase().includes(q) ||
                    u.floor.toLowerCase().includes(q) ||
                    u.beds.toLowerCase().includes(q);
            });

        }
        // status filter
        if ($scope.statusFilter !== "All") {
            list = list.filter(function (u) {
                return u.status.toLowerCase() === $scope.statusFilter.toLowerCase();
            });
        }
        // occupancy filter
        if ($scope.occupancyFilter !== "All") {
            list = list.filter(function (u) {
                return u.occupancy === $scope.occupancyFilter;
            });
        }
        //sort a-z or z-a
        list = list.sort(function (a, b) {
            return $scope.sortAsc
                ? a.unitName.localeCompare(b.unitName)
                : b.unitName.localeCompare(a.unitName);
        });
        $scope.totalUnits = list.length;
        return list;
    }
    $scope.toggleSort = function () {
        $scope.sortAsc = !$scope.sortAsc;
    };

    $scope.setStatus = function (val) {
        $scope.statusFilter = val;
    };

    $scope.setOccupancy = function (val) {
        $scope.occupancyFilter = val;
    };


    //Toast
    $scope.showToast = function (message, type) {
        $scope.toast.message = message;
        $scope.toast.type = type;
        $scope.toastShow = true;

        // Auto-hide after 3 seconds
        $timeout(function () {
            $scope.toastShow = false;
        }, 8000);
    };
    // Cooldown
    function startCooldown() {
        $scope.resendCooldown = 30;
        cooldownTimer = $interval(function () {
            $scope.resendCooldown--;
            if ($scope.resendCooldown <= 0) $interval.cancel(cooldownTimer);
        }, 1000);
    }


    // 1. Initialize an empty array to hold the image strings
    $scope.imageUrls = [];

    // 2. The function triggered by your file input
    $scope.handleImageUpload = function (files) {

        // Because this happens outside of standard Angular, we wrap it in $apply
        $scope.$apply(function () {

            // Loop through however many images they selected
            for (var i = 0; i < files.length; i++) {

                var reader = new FileReader();

                // When the file finishes reading, push the string to our array
                reader.onload = function (event) {
                    $scope.$apply(function () {
                        $scope.imageUrls.push(event.target.result);
                        console.log("Current Images Array:", $scope.imageUrls);
                    });
                };

                // This converts the raw file into a long Base64 string
                reader.readAsDataURL(files[i]);
            }
        });
    };
    $scope.removeImage = function (index) {
        // This instantly removes the image from the array
        // Angular will automatically update the UI and remove it from the screen!
        $scope.imageUrls.splice(index, 1);
    };



    //System Functions

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

    //Auth
    $scope.authFunc = function () {
        if (!$scope.email || !$scope.password) {
            $scope.showToast("Please fill out email and password", "error");
            return;
        }
        $scope.loading = true;
        var authData = {
            email: $scope.email,
            password:$scope.password
        }
        service.authService(authData).then(function (response) {
            if (response.data.success) {
                $scope.step = 'verify';
                $scope.otp = [];
                startCooldown();
                $scope.showToast(response.data.message, "success");

            } else {
                $scope.showToast(response.data.message, "error");
            }
        }).finally(function () {
            $scope.loading = false;
        })
    }
    // OTP auto focus next
    $scope.otpNext = function (index) {
        var val = $scope.otp[index];
        if (!/^\d?$/.test(val)) { $scope.otp[index] = ''; return; }
        if (val && index < 5) {
            document.getElementById('otp' + (index + 1)).focus();
        }
    };
    $scope.verifyCode = function () {
        var code = $scope.otp.join('');
        if (code.length < 6) {
            $scope.showToast("Please enter a full 6-digit code", "error");
        }
        $scope.loading = true;
        service.verifyCodeService(code).then(function (response) {
            if (response.data.success) {
                $scope.showToast("Welcome Back!, Admin", "success");
                $scope.goTo('Dashboard');
            } else {
                $scope.showToast(response.data.message, "error");
            }
        }).finally(function () {
            $scope.loading = false;
        })
    }
    // Resend
    $scope.resend = function () {
        if ($scope.resendCooldown > 0) return;
        $scope.otp = [];
        $scope.authFunc();
    };

    // Save

    $scope.saveUnit = function (id) {
        var unitData = {
            unitName: $scope.unitName,
            price: $scope.price,
            beds: $scope.beds,
            sqm: $scope.sqm,
            floor: $scope.floor,
            description: $scope.description,
            videoUrl: $scope.videoUrl, // Assuming you have an input for this somewhere
            colorCode: $scope.colorCode,
            status: $scope.status,
            address: $scope.address,
            maxOccupants: $scope.maxOccupants,
        };
        console.log(unitData);
        console.log($scope.imageUrls);
        //INput valudation
        if (!$scope.unitName) { $scope.showToast("Unit Name is required", "error"); return; }
        if (!$scope.price || $scope.price <= 0) { $scope.showToast("Price must be greater than 0", "error"); return; }
        if (!$scope.floor) { $scope.showToast("Floor is required", "error"); return; }
        if (!$scope.sqm || $scope.sqm <= 0) { $scope.showToast("Size (sqm) must be greater than 0", "error"); return; }
        if (!$scope.maxOccupants || $scope.maxOccupants < 1 || $scope.maxOccupants > 20) { $scope.showToast("Max Occupants must be between 1 and 20", "error"); return; }
        service.saveUnitService(unitData, $scope.imageUrls, id).then(function (response) {
            if (response.data.success) {
                $scope.showToast(response.data.message, "success")
            } else {
                $scope.showToast(response.data.message, "error");
            }
        })
    }
    // Get
    $scope.getDataNum = function () {
        service.GetDashboardDataService().then(function (response) {
            $scope.activeUnitNum = response.data.activeUnitNum;
            $scope.pendingRequestNum = response.data.pendingRequestNum;
            $scope.pendingBookNum = response.data.pendingBookNum;
            $scope.totalPayment = response.data.totalPayment;
            $scope.recentBookings = response.data.recentBookings;

            const data = response.data.monthlyRevenue;

            const monthNames = [
                "", "Jan", "Feb", "Mar", "Apr", "May", "Jun",
                "Jul", "Aug", "Sep", "Oct", "Nov", "Dec"
            ];

            $scope.linelabels = data.map(x => monthNames[x.Month]);

            $scope.linedata = [
                data.map(x => x.Revenue)
            ];

            const barData = response.data.monthlyOccupant;
            const BmonthNames = [
                "", "Jan", "Feb", "Mar", "Apr", "May", "Jun",
                "Jul", "Aug", "Sep", "Oct", "Nov", "Dec"
            ];

            $scope.barlabels = barData.map(x => BmonthNames[x.Month]);
            $scope.barseries = ["Occupied", "Vacant"];
            $scope.bardata = [
                barData.map(x => x.Occupied),
                barData.map(x => x.Vacant)
            ];
        })

    }
    
    $scope.getAllUnits = function () {
        service.GetAllUnitService().then(function (response) {
            $scope.units = response.data.data;
            $scope.totalUnits = $scope.units.length;
        })
    }
})