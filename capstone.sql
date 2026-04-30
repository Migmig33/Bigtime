-- --------------------------------------------------------
-- Host:                         127.0.0.1
-- Server version:               10.4.32-MariaDB - mariadb.org binary distribution
-- Server OS:                    Win64
-- HeidiSQL Version:             12.10.0.7000
-- --------------------------------------------------------

/*!40101 SET @OLD_CHARACTER_SET_CLIENT=@@CHARACTER_SET_CLIENT */;
/*!40101 SET NAMES utf8 */;
/*!50503 SET NAMES utf8mb4 */;
/*!40103 SET @OLD_TIME_ZONE=@@TIME_ZONE */;
/*!40103 SET TIME_ZONE='+00:00' */;
/*!40014 SET @OLD_FOREIGN_KEY_CHECKS=@@FOREIGN_KEY_CHECKS, FOREIGN_KEY_CHECKS=0 */;
/*!40101 SET @OLD_SQL_MODE=@@SQL_MODE, SQL_MODE='NO_AUTO_VALUE_ON_ZERO' */;
/*!40111 SET @OLD_SQL_NOTES=@@SQL_NOTES, SQL_NOTES=0 */;

-- Dumping data for table db_greenresidences.amenity: ~10 rows (approximately)
INSERT INTO `amenity` (`id`, `name`) VALUES
	(1, 'Pool'),
	(2, 'Study Lounge'),
	(3, 'WiFi'),
	(4, '24/7 Security'),
	(5, 'Gym'),
	(6, 'Parking'),
	(7, 'Pet-Friendly'),
	(8, 'Balcony'),
	(9, 'Concierge'),
	(10, 'Laundry Service');

-- Dumping data for table db_greenresidences.booking: ~8 rows (approximately)
INSERT INTO `booking` (`id`, `Uid`, `guestName`, `guestEmail`, `guestPhone`, `bookingDatetime`, `status`, `notes`, `cancelReason`) VALUES
	(1, 1, 'Guest A', 'guest@example.com', '0919-000-0001', '2026-04-29 11:00:00', 'Pending', 'Interested in long-term lease. Will bring spouse for viewing.', NULL),
	(2, 2, 'Carlo Mendez', 'carlo@example.com', '0917-888-1234', '2026-04-29 11:00:00', 'Confirmed', 'Student, looking for 6-month lease.', NULL),
	(3, 1, 'Ana Villanueva', 'ana@example.com', '0918-777-5678', '2026-04-27 10:00:00', 'Declined', 'Budget does not fit. Will consider smaller unit.', NULL),
	(4, 1, 'Ben Torres', 'ben@example.com', '0921-666-9012', '2026-05-04 09:00:00', 'Pending', 'Would like to know if parking is included.', NULL),
	(5, 3, 'Sofia Ramos', 'sofia@example.com', '0922-555-3456', '2026-05-07 10:00:00', 'Confirmed', 'Family of 4, needs pet-friendly option.', NULL),
	(6, 1, 'Miguel Reyes', 'miguel@example.com', '0923-444-7890', '2026-05-09 14:00:00', 'Pending', 'Remote worker, needs fast WiFi.', NULL),
	(7, 4, 'Lena Aquino', 'lena@example.com', '0924-333-1122', '2026-05-11 11:00:00', 'Confirmed', 'Looking to move in by end of May.', NULL),
	(8, 1, 'aldwin', 'diego@example.com', '0925-222-3344', '2026-05-14 15:00:00', 'Cancelled', 'Had an interest in the studio layout.', 'Found a place closer to my workplace.');

-- Dumping data for table db_greenresidences.co_occupant: ~5 rows (approximately)
INSERT INTO `co_occupant` (`id`, `Tid`, `name`, `phone`, `address`) VALUES
	(1, 0, 'Pedro Santos', '0918-234-0001', 'Quezon City, Metro Manila'),
	(2, 0, 'Lara Santos', '0918-234-0002', 'Quezon City, Metro Manila'),
	(3, 0, 'Sarah Torres', '0919-888-7778', 'Pasig City'),
	(4, 0, 'Liam Torres', '0919-888-7779', 'Pasig City'),
	(5, 0, 'Emma Torres', '0919-888-7780', 'Pasig City');

-- Dumping data for table db_greenresidences.maintenance_request: ~6 rows (approximately)
INSERT INTO `maintenance_request` (`id`, `Uid`, `Tid`, `category`, `description`, `status`, `priority`, `reportedDate`, `resolvedDate`) VALUES
	(1, 0, 0, 'Plumbing', 'Leaking faucet in bathroom sink. Water dripping consistently since last week.', 'Pending', 'High', '2026-04-10', NULL),
	(2, 0, 0, 'Air Conditioning', 'AC unit not cooling properly. Room temperature stays above 28°C.', 'In Progress', 'Medium', '2026-04-08', NULL),
	(3, 0, 0, 'Electrical', 'Two power outlets in the bedroom are not working. Suspected wiring issue.', 'Resolved', 'High', '2026-03-25', '2026-03-28'),
	(4, 0, 0, 'Internet', 'WiFi router keeps disconnecting. ISP already checked.', 'Pending', 'Low', '2026-04-12', NULL),
	(5, 0, 0, 'Carpentry', 'Cabinet door hinge is broken in the kitchen.', 'Resolved', 'Low', '2026-04-20', '2026-04-22'),
	(6, NULL, NULL, NULL, NULL, 'Pending', NULL, NULL, NULL);

-- Dumping data for table db_greenresidences.occupancy_type: ~2 rows (approximately)
INSERT INTO `occupancy_type` (`occupancyTypeId`, `occupancyType`) VALUES
	(1, 'Solo'),
	(2, 'co-occupants');

-- Dumping data for table db_greenresidences.payment: ~8 rows (approximately)
INSERT INTO `payment` (`id`, `Tid`, `Uid`, `amount`, `dueDate`, `paidDate`, `status`, `billingPeriod`) VALUES
	(1, 0, 0, 18000.00, '2026-04-05', '2026-04-03', 'Paid', 'April 2026'),
	(2, 0, 0, 0.00, '2026-04-05', NULL, 'Overdue', 'April 2026'),
	(3, 0, 0, 0.00, '2026-04-05', '2026-04-05', 'Paid', 'April 2026'),
	(4, 0, 0, 18000.00, '2026-03-05', '2026-03-04', 'Paid', 'March 2026'),
	(5, 0, 0, 42000.00, '2026-03-05', '2026-03-06', 'Paid', 'March 2026'),
	(6, 0, 0, 25000.00, '2026-05-05', NULL, 'Unpaid', 'May 2026'),
	(7, 0, 0, 15000.00, '2026-02-05', '2026-02-01', 'Paid', 'February 2026'),
	(8, 0, 0, 38000.00, '2026-05-01', '2026-04-28', 'Paid', 'May 2026');

-- Dumping data for table db_greenresidences.tenant: ~5 rows (approximately)
INSERT INTO `tenant` (`Tid`, `tenantNumber`, `unitId`, `name`, `email`, `phone`, `leaseStart`, `leaseEnd`, `status`, `address`, `occupation`, `passwordHash`, `occupancyTypeId`) VALUES
	(1, 'TN-0001', '1', 'Alex Cruz', 'alex@example.com', '0917-123-4567', '2026-01-10', '2026-12-31', 'active', 'Makati City, Metro Manila', 'Student', '123', 1),
	(2, 'TN-0002', '3', 'Maria Santos', 'maria@example.com', '0918-234-5678', '2026-02-03', '2026-05-31', 'expiring', 'Quezon City, Metro Manila', 'Police Officer', '123', 1),
	(3, 'TN-0003', '2', 'John Dela Cruz', 'john@example.com', '0920-345-6789', '2026-03-01', '2027-02-28', 'active', 'BGC, Taguig City', 'Software Engineer', '123', 1),
	(4, 'TN-0004', '4', 'Rosa Reyes', 'rosa@example.com', '0921-456-7890', '2025-06-01', '2026-03-01', 'inactive', 'Mandaluyong City', 'Nurse', '123', 1),
	(5, 'TN-0005', '6', 'Michael Torres', 'michael@example.com', '0919-888-7777', '2026-04-15', '2027-04-15', 'active', 'Pasig City', 'Architect', '123', 1);

-- Dumping data for table db_greenresidences.tenant_document: ~5 rows (approximately)
INSERT INTO `tenant_document` (`id`, `Tid`, `fileName`, `fileType`, `fileUrl`) VALUES
	(1, '1', 'Passport.pdf', 'application/pdf', '/uploads/docs/t1_passport.pdf'),
	(2, '1', 'UniversityID.jpg', 'image/jpeg', '/uploads/docs/t1_univ.jpg'),
	(3, '2', 'PoliceID.jpg', 'image/jpeg', '/uploads/docs/t2_police.jpg'),
	(4, '3', 'CompanyID.pdf', 'application/pdf', '/uploads/docs/t3_company.pdf'),
	(5, '4', 'PRC_License.jpg', 'image/jpeg', '/uploads/docs/t4_prc.jpg');

-- Dumping data for table db_greenresidences.unit: ~8 rows (approximately)
INSERT INTO `unit` (`Uid`, `unitName`, `price`, `beds`, `sqm`, `floor`, `description`, `videoUrl`, `colorCode`, `status`, `address`, `maxOccupants`) VALUES
	(1, 'Studio A101', 18000.00, 'Studio', 22, '3F', 'Bright, fully furnished studio with stunning city views.', 'https://commondatastorage.googleapis.com/gtv-videos-bucket/sample/ForBiggerBlazes.mp4', '#1F6FEB', 'active', 'Green Residences, Taft Avenue, Manila', 2),
	(2, '1BR B201', 25000.00, '1 BR', 36, '5F', 'Spacious one-bedroom unit with a modern kitchen.', 'https://commondatastorage.googleapis.com/gtv-videos-bucket/sample/ElephantsDream.mp4', '#F59E0B', 'active', 'Green Residences, Taft Avenue, Manila', 2),
	(3, '2BR C301', 42000.00, '2 BR', 58, '8F', 'Elegant two-bedroom unit featuring a master suite.', 'https://commondatastorage.googleapis.com/gtv-videos-bucket/sample/ForBiggerEscapes.mp4', '#10B981', 'active', 'Green Residences, Taft Avenue, Manila', 4),
	(4, 'Studio D102', 15000.00, 'Studio', 20, '2F', 'Cozy studio unit perfect for solo living.', 'https://commondatastorage.googleapis.com/gtv-videos-bucket/sample/ForBiggerFun.mp4', '#8B5CF6', 'active', 'Green Residences, Taft Avenue, Manila', 1),
	(5, '1BR E301', 28000.00, '1 BR', 40, '7F', 'Modern one-bedroom with wide balcony.', 'https://commondatastorage.googleapis.com/gtv-videos-bucket/sample/ForBiggerJoyrides.mp4', '#EF4444', 'active', 'Green Residences, Taft Avenue, Manila', 2),
	(6, '2BR F401', 38000.00, '2 BR', 55, '10F', 'Stylish two-bedroom with designer furniture.', 'https://commondatastorage.googleapis.com/gtv-videos-bucket/sample/ForBiggerMeltdowns.mp4', '#06B6D4', 'active', 'Green Residences, Taft Avenue, Manila', 4),
	(7, '3BR G501', 55000.00, '3 BR', 80, '12F', 'Flagship three-bedroom penthouse unit.', 'https://commondatastorage.googleapis.com/gtv-videos-bucket/sample/SubaruOutbackOnStreetAndDirt.mp4', '#F97316', 'active', 'Green Residences, Taft Avenue, Manila', 6),
	(8, 'Studio H201', 16000.00, 'Studio', 21, '4F', 'Affordable studio unit with modern minimalist design.', 'https://commondatastorage.googleapis.com/gtv-videos-bucket/sample/WeAreGoingOnBullrun.mp4', '#6366F1', 'inactive', 'Green Residences, Taft Avenue, Manila', 2);

-- Dumping data for table db_greenresidences.unit_amenity: ~0 rows (approximately)

-- Dumping data for table db_greenresidences.unit_image: ~6 rows (approximately)
INSERT INTO `unit_image` (`id`, `Uid`, `imageUrl`, `displayOrder`) VALUES
	(1, 0, 'https://images.unsplash.com/photo-1648468562569-0d64a17378da', 1),
	(2, 0, 'https://images.unsplash.com/photo-1584973547886-92c625508e24', 2),
	(3, 0, 'https://images.unsplash.com/photo-1631049422186-4b0569fed517', 1),
	(4, 0, 'https://images.unsplash.com/photo-1774311237295-a65a4c1ff38a', 2),
	(5, 0, 'https://images.unsplash.com/photo-1741764014072-68953e93cd48', 1),
	(6, 0, 'https://images.unsplash.com/photo-1774311237295-a65a4c1ff38a', 1);

/*!40103 SET TIME_ZONE=IFNULL(@OLD_TIME_ZONE, 'system') */;
/*!40101 SET SQL_MODE=IFNULL(@OLD_SQL_MODE, '') */;
/*!40014 SET FOREIGN_KEY_CHECKS=IFNULL(@OLD_FOREIGN_KEY_CHECKS, 1) */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40111 SET SQL_NOTES=IFNULL(@OLD_SQL_NOTES, 1) */;
