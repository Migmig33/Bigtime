app.controller('controller', function (service, $scope, $timeout, $interval) {

    // ==========================================
    // 1. INITIALIZATION & DEFAULT STATE
    // ==========================================
    $scope.thisMonthhs = new Date();
    $scope.loading = false;

    // Modals & Confirmations
    $scope.editorModal = false;
    $scope.deleteUnitConfirm = false;
    $scope.deleteTenantConfirm = false;

    // Auth State
    $scope.step = 'credentials';
    $scope.resendCooldown = 0;

    // Filters & Sorting (Global)
    $scope.searchQuery = '';
    $scope.statusFilter = 'All';
    $scope.occupancyFilter = 'All';
    $scope.sortAsc = true;

    $scope.tenantSearchQuery = '';
    $scope.tenantStatusFilter = 'All';
    $scope.tenantLiveStatusFilter = 'All';
    $scope.tenantSortAsc = true;
    $scope.tenantContractFilter = 'All';

    // Unit Defaults
    $scope.beds = 'Studio';
    $scope.maxOccupants = 2;
    $scope.status = 'active';
    $scope.colorCode = '#1F6FEB';
    $scope.address = 'Green Residences, Taft Avenue, Manila';
    $scope.price = 15000;
    $scope.amenities = [];

    // File Arrays
    $scope.imageUrls = [];
    $scope.idFiles = [];
    $scope.co_occupants = [];

    // Chart Defaults
    $scope.linelabels = [];
    $scope.linedata = [[]];
    $scope.lineseries = ['Revenue'];
    $scope.barlabels = [];
    $scope.bardata = [[], []];
    $scope.barseries = ['Occupied', 'Vacant'];


    // ==========================================
    // 2. UI & TOAST NOTIFICATIONS
    // ==========================================
    $scope.toast = { toastShow: false, message: '', type: 'error' };

    $scope.showToast = function (message, type) {
        $scope.toast.message = message;
        $scope.toast.type = type;
        $scope.toastShow = true;

        $timeout(function () {
            $scope.toastShow = false;
        }, 8000);
    };


    // ==========================================
    // 3. NAVIGATION
    // ==========================================
    $scope.goTo = function (page) {
        var routes = {
            'Home': '/System/RentersHome',
            'Browse': '/System/RentersBrowse',
            'MBookings': '/System/RentersBookings',
            'Dashboard': '/System/AdminDashboard',
            'Units': '/System/AdminUnits',
            'Bookings': '/System/AdminBookings',
            'Tenants': '/System/AdminTenants',
            'Payments': '/System/AdminPayments',
            'Maintenance': '/System/Maintenance'
        };
        if (routes[page]) window.location.href = routes[page];
    };


    // ==========================================
    // 4. AUTHENTICATION
    // ==========================================
    function startCooldown() {
        $scope.resendCooldown = 30;
        var cooldownTimer = $interval(function () {
            $scope.resendCooldown--;
            if ($scope.resendCooldown <= 0) $interval.cancel(cooldownTimer);
        }, 1000);
    }

    $scope.authFunc = function () {
        if (!$scope.email || !$scope.password) {
            return $scope.showToast("Please fill out email and password", "error");
        }

        $scope.loading = true;
        var authData = { email: $scope.email, password: $scope.password };

        service.authService(authData).then(function (response) {
            if (response.data.success) {
                $scope.step = 'verify';
                $scope.otp = [];
                startCooldown();
                $scope.showToast(response.data.message, "success");
            } else {
                $scope.showToast(response.data.message, "error");
            }
        }).finally(function () { $scope.loading = false; });
    };

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
            return $scope.showToast("Please enter a full 6-digit code", "error");
        }

        $scope.loading = true;
        service.verifyCodeService(code).then(function (response) {
            if (response.data.success) {
                $scope.showToast("Welcome Back!, Admin", "success");
                $scope.goTo('Dashboard');
            } else {
                $scope.showToast(response.data.message, "error");
            }
        }).finally(function () { $scope.loading = false; });
    };

    $scope.resend = function () {
        if ($scope.resendCooldown > 0) return;
        $scope.otp = [];
        $scope.authFunc();
    };


    // ==========================================
    // 5. DASHBOARD & DATA FETCHING
    // ==========================================
    $scope.getDataNum = function () {
        service.GetDashboardDataService().then(function (response) {
            var res = response.data;

            $scope.activeUnitNum = res.activeUnitNum;
            $scope.pendingRequestNum = res.pendingRequestNum;
            $scope.pendingBookNum = res.pendingBookNum;
            $scope.totalPayment = res.totalPayment;
            $scope.recentBookings = res.recentBookings;

            const monthNames = ["", "Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec"];

            // Line Chart
            $scope.linelabels = res.monthlyRevenue.map(x => monthNames[x.Month]);
            $scope.linedata = [res.monthlyRevenue.map(x => x.Revenue)];

            // Bar Chart
            $scope.barlabels = res.monthlyOccupant.map(x => monthNames[x.Month]);
            $scope.bardata = [
                res.monthlyOccupant.map(x => x.Occupied),
                res.monthlyOccupant.map(x => x.Vacant)
            ];
        });
    };


    // ==========================================
    // 6. UNIT MANAGEMENT (CRUD & Filters)
    // ==========================================
    $scope.getAllUnits = function () {
        service.GetAllUnitService().then(function (response) {
            $scope.units = response.data.data;
            $scope.totalUnits = $scope.units.length;
        });
    };

    $scope.filteredUnits = function () {
        var list = $scope.units || [];

        if ($scope.searchQuery) {
            var q = $scope.searchQuery.toLowerCase();
            list = list.filter(u =>
                u.unitName.toLowerCase().includes(q) ||
                u.floor.toLowerCase().includes(q) ||
                u.beds.toLowerCase().includes(q)
            );
        }
        if ($scope.statusFilter !== "All") {
            list = list.filter(u => u.status.toLowerCase() === $scope.statusFilter.toLowerCase());
        }
        if ($scope.occupancyFilter !== "All") {
            list = list.filter(u => u.occupancy === $scope.occupancyFilter);
        }

        list = list.sort((a, b) => $scope.sortAsc ? a.unitName.localeCompare(b.unitName) : b.unitName.localeCompare(a.unitName));
        $scope.totalUnits = list.length;
        return list;
    };

    $scope.toggleSort = function () { $scope.sortAsc = !$scope.sortAsc; };
    $scope.setStatus = function (val) { $scope.statusFilter = val; };
    $scope.setOccupancy = function (val) { $scope.occupancyFilter = val; };

    $scope.triggerEditUnit = function (unit) {
        $scope.editorModal = true;
        $scope.unitName = unit.unitName;
        $scope.price = unit.price;
        $scope.beds = unit.beds;
        $scope.sqm = unit.sqm;
        $scope.floor = unit.floor;
        $scope.description = unit.description;
        $scope.videoUrl = unit.videoUrl;
        $scope.colorCode = unit.colorCode;
        $scope.status = unit.status;
        $scope.address = unit.address;
        $scope.maxOccupants = unit.maxOccupants;
        $scope.imageUrls = unit.images ? unit.images.map(i => i.imageUrl) : [];
        $scope.selectedUnit = unit.Uid;
    };

    $scope.saveUnit = function (id) {
        var unitData = {
            unitName: $scope.unitName, price: $scope.price, beds: $scope.beds,
            sqm: $scope.sqm, floor: $scope.floor, description: $scope.description,
            videoUrl: $scope.videoUrl, colorCode: $scope.colorCode, status: $scope.status,
            address: $scope.address, maxOccupants: $scope.maxOccupants
        };

        if (!$scope.unitName) return $scope.showToast("Unit Name is required", "error");
        if (!$scope.price || $scope.price <= 0) return $scope.showToast("Price must be greater than 0", "error");
        if (!$scope.floor) return $scope.showToast("Floor is required", "error");
        if (!$scope.sqm || $scope.sqm <= 0) return $scope.showToast("Size (sqm) must be greater than 0", "error");
        if (!$scope.maxOccupants || $scope.maxOccupants < 1 || $scope.maxOccupants > 20) return $scope.showToast("Max Occupants must be between 1 and 20", "error");

        service.saveUnitService(unitData, $scope.imageUrls, id).then(function (response) {
            if (response.data.success) {
                $scope.showToast(response.data.message, "success");
                $scope.getAllUnits();
            } else {
                $scope.showToast(response.data.message, "error");
            }
        });
    };

    $scope.triggerConfirmDeleteUnit = function (id) {
        $scope.selectedUnit = id;
        $scope.deleteUnitConfirm = true;
    };

    $scope.deleteUnit = function (id) {
        service.DeleteUnitService(id).then(function (response) {
            $scope.showToast(response.data.message, response.data.success ? "success" : "error");
        }).finally(function () {
            $scope.deleteUnitConfirm = false;
            $scope.getAllUnits();
        });
    };


    // ==========================================
    // 7. TENANT MANAGEMENT (CRUD & Filters)
    // ==========================================
    $scope.getAllTenants = function () {
        service.GetAllTenantService().then(function (response) {
            $scope.tenants = response.data.data;
            $scope.totalTenants = $scope.tenants.length;
        });
    };
    $scope.onUnitChange = function () {
        // If they unselect a unit, reset the limit to 0
        if (!$scope.unitId) {
            $scope.maxCoOccupantsLimit = 0;
            return;
        }

        // Find the full unit object from your loaded units list
        var selectedUnit = $scope.units.find(function (u) {
            return u.Uid === $scope.unitId;
        });

        if (selectedUnit && selectedUnit.maxOccupants > 0) {
            // Subtract 1 to account for the main tenant
            $scope.maxCoOccupantsLimit = selectedUnit.maxOccupants - 1;

            // BONUS FIX: If they selected a big unit, added 3 people, and then changed 
            // to a smaller unit that only holds 1 person, this safely trims the array!
            if ($scope.co_occupants && $scope.co_occupants.length > $scope.maxCoOccupantsLimit) {
                $scope.co_occupants = $scope.co_occupants.slice(0, $scope.maxCoOccupantsLimit);
                $scope.showToast("Co-occupants adjusted to fit the new unit's capacity.", "info");
            }
        } else {
            $scope.maxCoOccupantsLimit = 0;
        }
    };
    $scope.triggerAddTenant = function () {
        // 1. Clear the editing identifiers
        $scope.editingTenant = null;
        $scope.selectedTenant = null;
        $scope.tenantNumber = '';

        // 2. Reset defaults
        $scope.occupancyTypeId = 1; // Default to 'Solo'
        $scope.passwordHash = '123'; // Default password you specified in HTML

        // 3. Clear all text fields
        $scope.name = '';
        $scope.email = '';
        $scope.phone = '';
        $scope.unitId = '';
        $scope.address = '';
        $scope.occupation = '';

        // 4. Clear dates
        $scope.leaseStart = null;
        $scope.leaseEnd = null;
        $scope.minLeaseEnd = null;

        // 5. Clear arrays and limits
        $scope.maxCoOccupantsLimit = 0;
        $scope.co_occupants = [];
        $scope.idFiles = [];
        $scope.deletedCoOccupants = [];
        // 6. Finally, open the modal
        $scope.editorModal = true;
    };

    $scope.filteredTenants = function () {
        var list = $scope.tenants || [];

        if ($scope.tenantSearchQuery) {
            var q = $scope.tenantSearchQuery.toLowerCase();
            list = list.filter(t =>
                (t.name && t.name.toLowerCase().includes(q)) ||
                (t.email && t.email.toLowerCase().includes(q)) ||
                (t.unit && t.unit.toLowerCase().includes(q)) ||
                (t.tenantNumber && t.tenantNumber.toLowerCase().includes(q))
            );
        }
        if ($scope.tenantStatusFilter !== 'All') {
            list = list.filter(t => t.status && t.status.toLowerCase() === $scope.tenantStatusFilter.toLowerCase());
        }
        if ($scope.tenantLiveStatusFilter !== 'All') {
            list = list.filter(t => t.liveStatus === $scope.tenantLiveStatusFilter);
        }
        if ($scope.tenantContractFilter !== 'All') {
            list = list.filter(function (t) {
                return t.contractType === $scope.tenantContractFilter;
            });
        }

        list = list.slice().sort((a, b) => $scope.tenantSortAsc ? a.name.localeCompare(b.name) : b.name.localeCompare(a.name));
        $scope.totalTenants = list.length;
        return list;
    };

    $scope.toggleTenantSort = function () { $scope.tenantSortAsc = !$scope.tenantSortAsc; };
    $scope.setTenantStatus = function (val) { $scope.tenantStatusFilter = val; };
    $scope.setTenantLiveStatus = function (val) { $scope.tenantLiveStatusFilter = val; };
    $scope.setTenantContract = function (val) {
        $scope.tenantContractFilter = val;
    };

    $scope.updateMinLeaseEnd = function () {
        if ($scope.leaseStart) {
            // 1. Get the selected start date
            var minDate = new Date($scope.leaseStart);

            // 2. Add exactly 3 months
            minDate.setMonth(minDate.getMonth() + 3);

            // 3. Format it safely to YYYY-MM-DD (Bypassing timezone bugs)
            var year = minDate.getFullYear();
            var month = String(minDate.getMonth() + 1).padStart(2, '0');
            var day = String(minDate.getDate()).padStart(2, '0');

            // This tells the HTML calendar exactly where to draw the line
            $scope.minLeaseEnd = year + '-' + month + '-' + day;

            // 4. If they already selected an end date that is now invalid, clear it
            if ($scope.leaseEnd && new Date($scope.leaseEnd) < minDate) {
                $scope.leaseEnd = null;
                $scope.showToast("Lease End cleared: Minimum contract is 3 months.", "info");
            }
        } else {
            $scope.minLeaseEnd = null;
        }
    };

    $scope.triggerEditTenant = function (tenant) {
        $scope.editorModal = true;
        $scope.editingTenant = tenant;
        $scope.selectedTenant = tenant.Tid;

        $scope.occupancyTypeId = tenant.occupancyTypeId;
        $scope.tenantNumber = tenant.tenantNumber;
        $scope.name = tenant.name;
        $scope.email = tenant.email;
        $scope.phone = tenant.phone;
        $scope.unitId = tenant.unitId;
        $scope.address = tenant.address;
        $scope.occupation = tenant.occupation;
        $scope.passwordHash = tenant.passwordHash;
        $scope.occupancyType = tenant.occupancyType;
        $scope.deletedCoOccupants = [];

        if (tenant.leaseStart) $scope.leaseStart = new Date(tenant.leaseStart);
        if (tenant.leaseEnd) $scope.leaseEnd = new Date(tenant.leaseEnd);

        $scope.updateMinLeaseEnd();

        $scope.maxCoOccupantsLimit = (tenant.maxOccupants && tenant.maxOccupants > 0) ? (tenant.maxOccupants - 1) : 0;
        $scope.co_occupants = tenant.coOccupants ? angular.copy(tenant.coOccupants) : [];
        $scope.idFiles = tenant.idFiles ? angular.copy(tenant.idFiles) : [];
    };

    $scope.addCoOccupant = function () {
        if (!$scope.co_occupants) $scope.co_occupants = [];

        if ($scope.co_occupants.length < $scope.maxCoOccupantsLimit) {
            $scope.co_occupants.push({ name: '', phone: '', address: '' });
            setTimeout(function () {
                if (typeof lucide !== 'undefined') lucide.createIcons();
            }, 50);
        }
    };

    $scope.removeCoOccupant = function (index) {
        var removedItem = $scope.co_occupants[index];

        // If the item has an 'id', it exists in the database. 
        // We must track it so we can tell the backend to delete it later.
        if (removedItem.id) {
            $scope.deletedCoOccupants.push(removedItem.id);
        }

        // Remove it from the UI array
        $scope.co_occupants.splice(index, 1);
    };
    $scope.shouldShowUnit = function (unit) {
        if ($scope.editingTenant && $scope.editingTenant.unitId === unit.Uid) {
            return true;
        }

        var isOccupied = false;

        if ($scope.tenants) {
            isOccupied = $scope.tenants.some(function (t) {
                return t.unitId === unit.Uid && t.liveStatus !== 'Terminated';
            });
        }

        return !isOccupied;
    };
    // 1. Color for Days Left
    $scope.getDaysLeftClass = function (days) {
        if (days < 0) return 'bg-red-50 text-red-700 border-red-100'; // Expired
        if (days <= 30) return 'bg-amber-50 text-amber-700 border-amber-100'; // Expiring soon
        return 'bg-blue-50 text-blue-700 border-blue-100'; // Plenty of time
    };

    // 2. Color for Contract Type
    $scope.getContractClass = function (type) {
        if (type === 'Short-term') return 'bg-purple-50 text-purple-700 border-purple-100';
        if (type === 'Mid-term') return 'bg-blue-50 text-blue-700 border-blue-100';
        if (type === 'Long-term') return 'bg-indigo-50 text-indigo-700 border-indigo-100';
        return 'bg-slate-50 text-slate-500 border-slate-200'; // Fallback
    };

    // 3. Color for Live Status
    $scope.getStatusClass = function (status) {
        if (status === 'Active') return 'bg-emerald-100 text-emerald-700';
        if (status === 'Expiring') return 'bg-amber-100 text-amber-700';
        if (status === 'Expired') return 'bg-red-100 text-red-700';
        if (status === 'Terminated') return 'bg-slate-200 text-slate-700';
        return 'bg-slate-100 text-slate-500'; // Fallback
    };

    // 4. Color for Payment Status
    $scope.getPaymentClass = function (status) {
        if (status === 'Paid') return 'bg-emerald-50 text-emerald-700 border-emerald-100';
        if (status === 'Pending') return 'bg-amber-50 text-amber-700 border-amber-100';
        if (status === 'Overdue') return 'bg-red-50 text-red-700 border-red-100';
        return 'bg-slate-50 text-slate-500 border-slate-200'; // None / No Record
    };
    $scope.saveTenant = function (id) {
        // 1. ADD THIS HELPER FUNCTION: 
        // It strips out messy timezones and returns a clean "YYYY-MM-DD" string
        var formatToDateString = function (dateObj) {
            if (!dateObj) return null;
            var d = new Date(dateObj);
            var year = d.getFullYear();
            var month = String(d.getMonth() + 1).padStart(2, '0');
            var day = String(d.getDate()).padStart(2, '0');
            return year + '-' + month + '-' + day;
        };

        // 2. UPDATE YOUR PAYLOAD:
        // Wrap your scope variables in the new helper function
        var tenantData = {
            name: $scope.name,
            email: $scope.email,
            phone: $scope.phone,
            unitId: $scope.unitId,

            // FIX IS HERE:
            leaseStart: formatToDateString($scope.leaseStart),
            leaseEnd: formatToDateString($scope.leaseEnd),

            address: $scope.address,
            occupation: $scope.occupation,
            passwordHash: $scope.passwordHash,
            occupancyTypeId: $scope.occupancyTypeId,
            deletedCoOccupants: $scope.deletedCoOccupants
        };
        
        if (!$scope.name) return $scope.showToast("Full Name is required", "error");
        if (!$scope.email || !/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test($scope.email)) return $scope.showToast("Valid email is required", "error");
        if (!$scope.occupancyTypeId) return $scope.showToast("Please select an Occupancy Type", "error");
        if (!$scope.leaseStart) return $scope.showToast("Lease Start date is required", "error");

        if ($scope.leaseStart && $scope.leaseEnd && new Date($scope.leaseEnd) <= new Date($scope.leaseStart)) {
            return $scope.showToast("Lease End date must be after the Lease Start date", "error");
        }

        if ($scope.occupancyTypeId === 2) {
            if (!$scope.co_occupants || $scope.co_occupants.length === 0) return $scope.showToast("Please add at least one additional occupant.", "error");
            for (var i = 0; i < $scope.co_occupants.length; i++) {
                if (!$scope.co_occupants[i].name || !$scope.co_occupants[i].phone) {
                    return $scope.showToast("Co-occupant #" + (i + 1) + " must have a name and phone number.", "error");
                }
            }
        }

        var targetTid = id || ($scope.editingTenant ? $scope.editingTenant.Tid : null);

        service.saveTenantService(tenantData, $scope.co_occupants, $scope.idFiles, targetTid).then(function (response) {
            if (response.data.success) {
                $scope.showToast(response.data.message, "success");
                $scope.editorModal = false;
                if (typeof $scope.getAllTenants === 'function') $scope.getAllTenants();
            } else {
                $scope.showToast(response.data.message || "Failed to save tenant.", "error");
            }
        }).finally(function () { $scope.editorModal = false; });
    };

    $scope.triggerConfirmDeleteTenant = function (id) {
        $scope.selectedTenant = id;
        $scope.deleteTenantConfirm = true;
    };

    $scope.deleteTenant = function (id) {
        service.DeleteTenantService(id).then(function (response) {
            $scope.showToast(response.data.message, response.data.success ? "success" : "error");
        }).finally(function () {
            $scope.deleteTenantConfirm = false;
            $scope.getAllTenants();
        });
    };

    // ==========================================
    // 9. Booking MANAGEMENT (CRUD & Filters)
    // ==========================================
    $scope.isBusyOpen = false;
    $scope.busyDates = []; // Make sure this connects to your database array if needed

    $scope.DAYS = ['Su', 'Mo', 'Tu', 'We', 'Th', 'Fr', 'Sa'];
    $scope.MONTHS_FULL = ['January', 'February', 'March', 'April', 'May', 'June', 'July', 'August', 'September', 'October', 'November', 'December'];

    // Track actual today to disable past days
    var today = new Date();
    $scope.todayDate = today.getDate();
    $scope.todayMonth = today.getMonth();
    $scope.todayYear = today.getFullYear();

    // Track currently viewed month/year in the mini calendar
    $scope.calMonth = today.getMonth();
    $scope.calYear = today.getFullYear();

    // Generate the grid for the current viewing month
    $scope.generateCalendar = function () {
        var firstDay = new Date($scope.calYear, $scope.calMonth, 1).getDay();
        var daysInMonth = new Date($scope.calYear, $scope.calMonth + 1, 0).getDate();

        // Arrays to power the HTML ng-repeats
        $scope.emptySlots = new Array(firstDay);
        $scope.monthDays = [];

        for (var i = 1; i <= daysInMonth; i++) {
            var dStr = $scope.calYear + '-' + String($scope.calMonth + 1).padStart(2, '0') + '-' + String(i).padStart(2, '0');
            var isPast = false;

            // Determine if date is in the past
            if ($scope.calYear < $scope.todayYear) {
                isPast = true;
            } else if ($scope.calYear === $scope.todayYear && $scope.calMonth < $scope.todayMonth) {
                isPast = true;
            } else if ($scope.calYear === $scope.todayYear && $scope.calMonth === $scope.todayMonth && i < $scope.todayDate) {
                isPast = true;
            }

            $scope.monthDays.push({
                num: i,
                dateStr: dStr,
                isPast: isPast,
                isToday: ($scope.calYear === $scope.todayYear && $scope.calMonth === $scope.todayMonth && i === $scope.todayDate)
            });
        }
    };

    // Navigation
    $scope.nextCal = function () {
        if ($scope.calMonth === 11) {
            $scope.calMonth = 0;
            $scope.calYear++;
        } else {
            $scope.calMonth++;
        }
        $scope.generateCalendar();
    };

    $scope.prevCal = function () {
        if ($scope.calMonth === 0) {
            $scope.calMonth = 11;
            $scope.calYear--;
        } else {
            $scope.calMonth--;
        }
        $scope.generateCalendar();
    };

    // Actions
    $scope.isBusy = function (dateStr) {
        return $scope.busyDates.indexOf(dateStr) !== -1;
    };

    $scope.toggleBusyDate = function (dayObj) {
        if (dayObj.isPast) return;

        var idx = $scope.busyDates.indexOf(dayObj.dateStr);
        if (idx === -1) {
            $scope.busyDates.push(dayObj.dateStr);
        } else {
            $scope.busyDates.splice(idx, 1);
        }
        $scope.busyDates.sort(); // Keep list ordered
    };

    $scope.removeBusyDate = function (dateStr) {
        var idx = $scope.busyDates.indexOf(dateStr);
        if (idx !== -1) {
            $scope.busyDates.splice(idx, 1);
        }
    };

    $scope.clearAllBusy = function () {
        $scope.busyDates = [];
    };

    // Display format function for the list (e.g. "May 12, 2026")
    $scope.formatDateLabel = function (dateStr) {
        var parts = dateStr.split('-');
        if (parts.length !== 3) return dateStr;
        var y = parseInt(parts[0], 10);
        var m = parseInt(parts[1], 10) - 1;
        var d = parseInt(parts[2], 10);
        return $scope.MONTHS_FULL[m] + ' ' + d + ', ' + y;
    };

    // Initialize calendar on load
    $scope.generateCalendar();






    // ==========================================
    // 10. FILE UPLOADS (Images & IDs)
    // ==========================================
    $scope.handleImageUpload = function (files) {
        $scope.$apply(function () {
            // Note: If you want to enforce the 10-photo maximum during upload,
            // check (files.length + $scope.imageUrls.length) against 10 here.
            for (var i = 0; i < files.length; i++) {
                var reader = new FileReader();
                reader.onload = function (event) {
                    $scope.$apply(function () {
                        $scope.imageUrls.push(event.target.result);
                    });
                };
                reader.readAsDataURL(files[i]);
            }
        });
    };

    $scope.removeImage = function (index) {
        $scope.imageUrls.splice(index, 1);
    };

    $scope.handleIdUpload = function (files) {
        if (!files || files.length === 0) return;

        $scope.$apply(function () {
            if (!$scope.idFiles) $scope.idFiles = [];
            var remainingSlots = 2 - $scope.idFiles.length;
            var filesToProcess = Math.min(files.length, remainingSlots);

            if (files.length > remainingSlots) {
                $scope.showToast("Maximum of 2 ID documents allowed. Only processing the first " + filesToProcess + ".", "error");
            }

            for (var i = 0; i < filesToProcess; i++) {
                (function (file) {
                    var reader = new FileReader();
                    reader.onload = function (event) {
                        $scope.$apply(function () {
                            $scope.idFiles.push({
                                fileObj: file,
                                name: file.name,
                                url: event.target.result
                            });
                        });
                    };
                    reader.readAsDataURL(file);
                })(files[i]);
            }
        });
    };

    $scope.removeIdFile = function (index) {
        $scope.idFiles.splice(index, 1);
    };
});