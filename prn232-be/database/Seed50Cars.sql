-- =========================================================================
-- SCRIPT THÉM 50 XE VÀO CƠ SỞ DỮ LIỆU CARSHOWROOMDB
-- =========================================================================
USE CarShowroomDB;
GO

-- 1. Đảm bảo các Hãng xe (CarBrands) đã tồn tại
IF NOT EXISTS (SELECT 1 FROM CarBrands WHERE BrandName = 'Toyota')
    INSERT INTO CarBrands (BrandName, Country, Description) VALUES ('Toyota', 'Japan', 'Toyota Motor Corporation');

IF NOT EXISTS (SELECT 1 FROM CarBrands WHERE BrandName = 'BMW')
    INSERT INTO CarBrands (BrandName, Country, Description) VALUES ('BMW', 'Germany', 'Bayerische Motoren Werke AG');

IF NOT EXISTS (SELECT 1 FROM CarBrands WHERE BrandName = 'Ford')
    INSERT INTO CarBrands (BrandName, Country, Description) VALUES ('Ford', 'USA', 'Ford Motor Company');

IF NOT EXISTS (SELECT 1 FROM CarBrands WHERE BrandName = 'Tesla')
    INSERT INTO CarBrands (BrandName, Country, Description) VALUES ('Tesla', 'USA', 'Tesla Electric Vehicles');

IF NOT EXISTS (SELECT 1 FROM CarBrands WHERE BrandName = 'Honda')
    INSERT INTO CarBrands (BrandName, Country, Description) VALUES ('Honda', 'Japan', 'Honda Motor Co., Ltd.');

IF NOT EXISTS (SELECT 1 FROM CarBrands WHERE BrandName = 'Audi')
    INSERT INTO CarBrands (BrandName, Country, Description) VALUES ('Audi', 'Germany', 'Audi AG');

IF NOT EXISTS (SELECT 1 FROM CarBrands WHERE BrandName = 'Mercedes-Benz')
    INSERT INTO CarBrands (BrandName, Country, Description) VALUES ('Mercedes-Benz', 'Germany', 'Mercedes-Benz Group AG');

IF NOT EXISTS (SELECT 1 FROM CarBrands WHERE BrandName = 'Porsche')
    INSERT INTO CarBrands (BrandName, Country, Description) VALUES ('Porsche', 'Germany', 'Porsche AG');

IF NOT EXISTS (SELECT 1 FROM CarBrands WHERE BrandName = 'Mazda')
    INSERT INTO CarBrands (BrandName, Country, Description) VALUES ('Mazda', 'Japan', 'Mazda Motor Corporation');

IF NOT EXISTS (SELECT 1 FROM CarBrands WHERE BrandName = 'Hyundai')
    INSERT INTO CarBrands (BrandName, Country, Description) VALUES ('Hyundai', 'South Korea', 'Hyundai Motor Company');

IF NOT EXISTS (SELECT 1 FROM CarBrands WHERE BrandName = 'VinFast')
    INSERT INTO CarBrands (BrandName, Country, Description) VALUES ('VinFast', 'Vietnam', 'VinFast Auto LLC');

IF NOT EXISTS (SELECT 1 FROM CarBrands WHERE BrandName = 'Kia')
    INSERT INTO CarBrands (BrandName, Country, Description) VALUES ('Kia', 'South Korea', 'Kia Corporation');

GO

-- 2. Khai báo lấy BrandId động
DECLARE @ToyotaId INT = (SELECT TOP 1 BrandId FROM CarBrands WHERE BrandName = 'Toyota');
DECLARE @BmwId INT = (SELECT TOP 1 BrandId FROM CarBrands WHERE BrandName = 'BMW');
DECLARE @FordId INT = (SELECT TOP 1 BrandId FROM CarBrands WHERE BrandName = 'Ford');
DECLARE @TeslaId INT = (SELECT TOP 1 BrandId FROM CarBrands WHERE BrandName = 'Tesla');
DECLARE @HondaId INT = (SELECT TOP 1 BrandId FROM CarBrands WHERE BrandName = 'Honda');
DECLARE @AudiId INT = (SELECT TOP 1 BrandId FROM CarBrands WHERE BrandName = 'Audi');
DECLARE @MercId INT = (SELECT TOP 1 BrandId FROM CarBrands WHERE BrandName = 'Mercedes-Benz');
DECLARE @PorscheId INT = (SELECT TOP 1 BrandId FROM CarBrands WHERE BrandName = 'Porsche');
DECLARE @MazdaId INT = (SELECT TOP 1 BrandId FROM CarBrands WHERE BrandName = 'Mazda');
DECLARE @HyundaiId INT = (SELECT TOP 1 BrandId FROM CarBrands WHERE BrandName = 'Hyundai');
DECLARE @VinFastId INT = (SELECT TOP 1 BrandId FROM CarBrands WHERE BrandName = 'VinFast');
DECLARE @KiaId INT = (SELECT TOP 1 BrandId FROM CarBrands WHERE BrandName = 'Kia');

-- Nếu hãng nào chưa có id thì lấy id đầu tiên làm fallback
IF @ToyotaId IS NULL SET @ToyotaId = (SELECT TOP 1 BrandId FROM CarBrands);
IF @BmwId IS NULL SET @BmwId = @ToyotaId;
IF @FordId IS NULL SET @FordId = @ToyotaId;
IF @TeslaId IS NULL SET @TeslaId = @ToyotaId;
IF @HondaId IS NULL SET @HondaId = @ToyotaId;
IF @AudiId IS NULL SET @AudiId = @ToyotaId;
IF @MercId IS NULL SET @MercId = @ToyotaId;
IF @PorscheId IS NULL SET @PorscheId = @ToyotaId;
IF @MazdaId IS NULL SET @MazdaId = @ToyotaId;
IF @HyundaiId IS NULL SET @HyundaiId = @ToyotaId;
IF @VinFastId IS NULL SET @VinFastId = @ToyotaId;
IF @KiaId IS NULL SET @KiaId = @ToyotaId;

-- 3. Insert 50 xe chi tiết
INSERT INTO Cars (BrandId, CarName, Model, [Year], Color, Mileage, FuelType, Transmission, Price, Description, ImageUrl, Status, CreatedAt)
VALUES 
-- Toyota (1-5)
(@ToyotaId, N'Toyota Camry 2.5Q 2023', N'Camry', 2023, N'Trắng Ngọc Trai', 12000, N'Gasoline', N'Automatic', 1220000000, N'Xe sedan hạng D sang trọng, nội thất da cao cấp, trang bị Toyota Safety Sense.', N'https://images.unsplash.com/photo-1621007947382-bb3c3994e3fb?w=800', N'Available', GETDATE()),
(@ToyotaId, N'Toyota Corolla Cross 1.8V 2024', N'Corolla Cross', 2024, N'Đỏ đô', 5000, N'Gasoline', N'Automatic', 860000000, N'SUV đô thị tiết kiệm nhiên liệu, cửa số trời panorama, phanh tay điện tử.', N'https://images.unsplash.com/photo-1590362891991-f776e747a588?w=800', N'Available', GETDATE()),
(@ToyotaId, N'Toyota Fortuner Legender 2.8AT 4x4', N'Fortuner', 2023, N'Đen', 18000, N'Diesel', N'Automatic', 1420000000, N'SUV 7 chỗ mạnh mẽ, 2 cầu chủ động, trang bị JBL 11 loa.', N'https://images.unsplash.com/photo-1533473359331-0135ef1b58bf?w=800', N'Available', GETDATE()),
(@ToyotaId, N'Toyota Raize 1.0 Turbo 2023', N'Raize', 2023, N'Ngọc Xanh', 9000, N'Gasoline', N'Automatic', 550000000, N'Xe gầm cao nhỏ gọn, động cơ Turbo tiết kiệm, thiết kế trẻ trung.', N'https://images.unsplash.com/photo-1549399542-7e3f8b79c341?w=800', N'Available', GETDATE()),
(@ToyotaId, N'Toyota Land Cruiser Prado 2022', N'Land Cruiser Prado', 2022, N'Trắng', 25000, N'Gasoline', N'Automatic', 2650000000, N'Mẫu SUV địa hình đẳng cấp, phù hợp di chuyển mọi địa hình.', N'https://images.unsplash.com/photo-1541899481282-d53bffe3c35d?w=800', N'Available', GETDATE()),

-- Honda (6-10)
(@HondaId, N'Honda CR-V L AWD Hybrid 2024', N'CR-V', 2024, N'Xám Ghi', 3000, N'Hybrid', N'Automatic', 1250000000, N'SUV 7 chỗ động cơ Hybrid e:HEV siêu tiết kiệm, gói an toàn Honda SENSING.', N'https://images.unsplash.com/photo-1568605117036-5fe5e7bab0b7?w=800', N'Available', GETDATE()),
(@HondaId, N'Honda Civic RS 1.5 Turbo 2023', N'Civic', 2023, N'Đỏ Đua', 11000, N'Gasoline', N'Automatic', 870000000, N'Sedan thể thao đậm chất ngầu, động cơ VTEC Turbo 176 mã lực.', N'https://images.unsplash.com/photo-1605559424843-9e4c228bf1c2?w=800', N'Available', GETDATE()),
(@HondaId, N'Honda City RS 2024', N'City', 2024, N'Trắng', 2000, N'Gasoline', N'Automatic', 609000000, N'Sedan hạng B rộng nhất phân khúc, có Honda Sensing toàn diện.', N'https://images.unsplash.com/photo-1580273916550-e323be2ae537?w=800', N'Available', GETDATE()),
(@HondaId, N'Honda HR-V G 2023', N'HR-V', 2023, N'Đen', 14000, N'Gasoline', N'Automatic', 699000000, N'Crossover đô thị thiết kế Coupé đẹp mắt, ghế Magic Seat linh hoạt.', N'https://images.unsplash.com/photo-1552519507-da3b142c6e3d?w=800', N'Available', GETDATE()),
(@HondaId, N'Honda Accord 1.5 Turbo 2022', N'Accord', 2022, N'Ghi Xám', 22000, N'Gasoline', N'Automatic', 1050000000, N'Sedan doanh nhân cao cấp, cách âm vượt trội, khung gầm chắc chắn.', N'https://images.unsplash.com/photo-1542282088-72c9c27ed0cd?w=800', N'Available', GETDATE()),

-- Ford (11-15)
(@FordId, N'Ford Everest Titanium+ 2.0L 4x4 2023', N'Everest', 2023, N'Nâu Đất', 15000, N'Diesel', N'Automatic', 1468000000, N'SUV 7 chỗ cơ bắp Mỹ, động cơ Bi-Turbo, màn hình 12 inch dọc.', N'https://images.unsplash.com/photo-1533473359331-0135ef1b58bf?w=800', N'Available', GETDATE()),
(@FordId, N'Ford Ranger Raptor 2.0L Bi-Turbo 2023', N'Ranger', 2023, N'Cam Cyber', 16000, N'Diesel', N'Automatic', 1299000000, N'Bán tải hiệu năng cao, phuộc Fox Racing, chế độ lái Baja chuyên nghiệp.', N'https://images.unsplash.com/photo-1559416523-140ddc3d238c?w=800', N'Available', GETDATE()),
(@FordId, N'Ford Territory Titanium X 2024', N'Territory', 2024, N'Trắng', 4000, N'Gasoline', N'Automatic', 929000000, N'SUV đô thị rộng trãi, trang bị lùi xe tự động, cửa cốp điện thông minh.', N'https://images.unsplash.com/photo-1503376780353-7e6692767b70?w=800', N'Available', GETDATE()),
(@FordId, N'Ford Explorer Limited 2.3 EcoBoost 2022', N'Explorer', 2022, N'Đen', 21000, N'Gasoline', N'Automatic', 1999000000, N'SUV hạng E nhập Mỹ, động cơ EcoBoost 300 hp, âm thanh Bang & Olufsen 12 loa.', N'https://images.unsplash.com/photo-1502877338535-766e1452684a?w=800', N'Available', GETDATE()),
(@FordId, N'Ford Mustang EcoBoost Premium 2022', N'Mustang', 2022, N'Vàng Rực', 10000, N'Gasoline', N'Automatic', 2490000000, N'Xe thể thao thể hiện phong cách Mỹ, thiết kế Coupé 2 cửa hấp dẫn.', N'https://images.unsplash.com/photo-1584345604476-8ec5e12e42dd?w=800', N'Available', GETDATE()),

-- BMW (16-20)
(@BmwId, N'BMW 320i Sport Line 2023', N'3 Series', 2023, N'Xanh Phytonic', 8000, N'Gasoline', N'Automatic', 1399000000, N'Sedan thể thao đẳng cấp, màn hình cong BMW Curved Display HD.', N'https://images.unsplash.com/photo-1555215695-3004980ad54e?w=800', N'Available', GETDATE()),
(@BmwId, N'BMW 520i M Sport 2023', N'5 Series', 2023, N'Đen Sapphire', 12000, N'Gasoline', N'Automatic', 2199000000, N'Sedan sang trọng trang bị gói M Sport, phanh M thể thao.', N'https://images.unsplash.com/photo-1523983388277-336a66bf9bcd?w=800', N'Available', GETDATE()),
(@BmwId, N'BMW X5 xDrive40i M Sport 2024', N'X5', 2024, N'Trắng Mineral', 3500, N'Gasoline', N'Automatic', 3899000000, N'SUV hạng sang gầm cao, động cơ I6 3.0L mạnh mẽ, đèn Laserlight.', N'https://images.unsplash.com/photo-1549399542-7e3f8b79c341?w=800', N'Available', GETDATE()),
(@BmwId, N'BMW X3 xDrive20i M Sport 2023', N'X3', 2023, N'Xanh Brooklyn', 9500, N'Gasoline', N'Automatic', 2159000000, N'SAV đa dụng, dẫn động 4 bánh toàn thời gian xDrive thông minh.', N'https://images.unsplash.com/photo-1556189250-72ba968cf38a?w=800', N'Available', GETDATE()),
(@BmwId, N'BMW 730Li Pure Excellence 2022', N'7 Series', 2022, N'Đen', 19000, N'Gasoline', N'Automatic', 3999000000, N'Flagship sedan đỉnh cao cao cấp, ghế thương gia massage, âm thanh Bowers & Wilkins.', N'https://images.unsplash.com/photo-1503376780353-7e6692767b70?w=800', N'Available', GETDATE()),

-- Mercedes-Benz (21-25)
(@MercId, N'Mercedes-Benz C200 Avantgarde 2023', N'C-Class', 2023, N'Trắng Polar', 7000, N'Gasoline', N'Automatic', 1599000000, N'Sedan sang trọng trẻ trung, màn hình trung tâm 11.9 inch MBUX.', N'https://images.unsplash.com/photo-1618843479313-40f8afb4b4d8?w=800', N'Available', GETDATE()),
(@MercId, N'Mercedes-Benz E300 AMG 2023', N'E-Class', 2023, N'Đen Obsidian', 10000, N'Gasoline', N'Automatic', 2850000000, N'Sedan doanh nhân đậm chất AMG, vô-lăng D-Cut, đèn Multibeam LED.', N'https://images.unsplash.com/photo-1617814076367-b759c7d7e738?w=800', N'Available', GETDATE()),
(@MercId, N'Mercedes-Benz GLC 300 4MATIC 2024', N'GLC', 2024, N'Đỏ Hyacinth', 2000, N'Gasoline', N'Automatic', 2799000000, N'SUV hạng sang bán chạy nhất, thiết kế mới thế hệ X254.', N'https://images.unsplash.com/photo-1563720223185-11003d516935?w=800', N'Available', GETDATE()),
(@MercId, N'Mercedes-AMG G63 2022', N'G-Class', 2022, N'Đen Mờ', 15000, N'Gasoline', N'Automatic', 10900000000, N'Ông vua địa hình V8 4.0L Bi-Turbo, âm thanh Burmester, ống xả đôi bên hông.', N'https://images.unsplash.com/photo-1520050206274-a1ae44613e6d?w=800', N'Available', GETDATE()),
(@MercId, N'Mercedes-Benz S450 Luxury 2023', N'S-Class', 2023, N'Bạc High-tech', 8500, N'Gasoline', N'Automatic', 5199000000, N'Biểu tượng xe sang sang trọng, đánh lái bánh sau, đèn Digital Light.', N'https://images.unsplash.com/photo-1541899481282-d53bffe3c35d?w=800', N'Available', GETDATE()),

-- Tesla (26-30)
(@TeslaId, N'Tesla Model 3 Long Range 2023', N'Model 3', 2023, N'Trắng Pearl', 11000, N'Electric', N'Automatic', 1450000000, N'Xe điện hiện đại, tầm hoạt động 550 km/lần sạc, tính năng Autopilot.', N'https://images.unsplash.com/photo-1560958089-b8a1929cea89?w=800', N'Available', GETDATE()),
(@TeslaId, N'Tesla Model Y Performance 2023', N'Model Y', 2023, N'Đỏ Multi-Coat', 9000, N'Electric', N'Automatic', 1750000000, N'Crossover điện 5 chỗ gia tốc 0-100 km/h trong 3.7 giây.', N'https://images.unsplash.com/photo-1536700503339-1e4b06520771?w=800', N'Available', GETDATE()),
(@TeslaId, N'Tesla Model S Plaid 2022', N'Model S', 2022, N'Đen Solid', 13000, N'Electric', N'Automatic', 3200000000, N'Siêu xe điện 3 động cơ 1.020 mã lực, tăng tốc 0-100km/h chỉ 2.1s.', N'https://images.unsplash.com/photo-1617788138017-80ad40651399?w=800', N'Available', GETDATE()),
(@TeslaId, N'Tesla Model X Long Range 2023', N'Model X', 2023, N'Xanh Deep Blue', 6000, N'Electric', N'Automatic', 3500000000, N'SUV điện cửa cánh chim Falcon Wing độc đáo, 6 chỗ gia đình.', N'https://images.unsplash.com/photo-1571127236794-81c0bbfe1ce3?w=800', N'Available', GETDATE()),
(@TeslaId, N'Tesla Cybertruck Dual Motor 2024', N'Cybertruck', 2024, N'Thép Thô', 1000, N'Electric', N'Automatic', 3900000000, N'Bán tải điện tương lai với vỏ thép không gỉ chống đạn, thiết kế góc cạnh.', N'https://images.unsplash.com/photo-1617788138017-80ad40651399?w=800', N'Available', GETDATE()),

-- Audi (31-35)
(@AudiId, N'Audi A4 45 TFSI quattro 2023', N'A4', 2023, N'Trắng Ibis', 10000, N'Gasoline', N'Automatic', 1650000000, N'Sedan hạng sang dẫn động quattro trứ danh, màn hình Virtual Cockpit.', N'https://images.unsplash.com/photo-1603584173870-7f23fdae1b7a?w=800', N'Available', GETDATE()),
(@AudiId, N'Audi Q5 Sportback 45 TFSI 2023', N'Q5', 2023, N'Xám Daytona', 8000, N'Gasoline', N'Automatic', 2450000000, N'SUV Coupé thể thao sang trọng, đèn OLED phía sau tùy chỉnh giao diện.', N'https://images.unsplash.com/photo-1542282088-72c9c27ed0cd?w=800', N'Available', GETDATE()),
(@AudiId, N'Audi Q7 45 TFSI quattro 2022', N'Q7', 2022, N'Đen Mythos', 18000, N'Gasoline', N'Automatic', 3290000000, N'SUV 7 chỗ rộng trãi cho gia đình, treo khí nến thích ứng.', N'https://images.unsplash.com/photo-1502877338535-766e1452684a?w=800', N'Available', GETDATE()),
(@AudiId, N'Audi A6 45 TFSI 2023', N'A6', 2023, N'Nâu Firmament', 7500, N'Gasoline', N'Automatic', 2290000000, N'Sedan sang trọng cao cấp 2 màn hình cảm ứng trung tâm hiện đại.', N'https://images.unsplash.com/photo-1555215695-3004980ad54e?w=800', N'Available', GETDATE()),
(@AudiId, N'Audi e-tron GT quattro 2023', N'e-tron GT', 2023, N'Xanh Tactical', 5000, N'Electric', N'Automatic', 4990000000, N'Gran Turismo điện thể thao kiệt tác thiết kế, sạc siêu nhanh 800V.', N'https://images.unsplash.com/photo-1617788138017-80ad40651399?w=800', N'Available', GETDATE()),

-- Porsche (36-40)
(@PorscheId, N'Porsche Macan 2.0 2023', N'Macan', 2023, N'Xanh Papaya', 9000, N'Gasoline', N'Automatic', 3350000000, N'SUV compact thể thao cảm giác lái tốt nhất phân khúc, hộp số PDK 7 cấp.', N'https://images.unsplash.com/photo-1503376780353-7e6692767b70?w=800', N'Available', GETDATE()),
(@PorscheId, N'Porsche Cayenne Coupe 2023', N'Cayenne', 2023, N'Trắng Carrara', 7000, N'Gasoline', N'Automatic', 5100000000, N'SUV Coupé thể thao đậm chất Porsche, gói Sport Chrono mạnh mẽ.', N'https://images.unsplash.com/photo-1614162692292-7ac56d7f7f1e?w=800', N'Available', GETDATE()),
(@PorscheId, N'Porsche Panamera 4 2022', N'Panamera', 2022, N'Ghi Volcano', 14000, N'Gasoline', N'Automatic', 5500000000, N'Saloon thể thao 4 cửa đẳng cấp, nội thất bọc da cao cấp.', N'https://images.unsplash.com/photo-1611245141662-817d7b420054?w=800', N'Available', GETDATE()),
(@PorscheId, N'Porsche Taycan 4S 2023', N'Taycan', 2023, N'Xanh Frozen', 6000, N'Electric', N'Automatic', 5700000000, N'Xe thể thao thuần điện, gia tốc tức thì, tăng tốc 0-100km/h trong 4.0s.', N'https://images.unsplash.com/photo-1617788138017-80ad40651399?w=800', N'Available', GETDATE()),
(@PorscheId, N'Porsche 911 Carrera S 2022', N'911', 2022, N'Vàng Racing', 8000, N'Gasoline', N'Automatic', 8900000000, N'Huyền thoại xe thể thao động cơ Boxer 6 xi-lanh tăng tốc việt dắt.', N'https://images.unsplash.com/photo-1614162692292-7ac56d7f7f1e?w=800', N'Available', GETDATE()),

-- Hyundai (41-44)
(@HyundaiId, N'Hyundai Santa Fe 2.2D Cao Cấp 2023', N'Santa Fe', 2023, N'Đen', 13000, N'Diesel', N'Automatic', 1219000000, N'SUV 7 chỗ máy dầu siêu êm, dẫn động HTRAC, thiết kế mặt ca-lăng đặc trưng.', N'https://images.unsplash.com/photo-1549399542-7e3f8b79c341?w=800', N'Available', GETDATE()),
(@HyundaiId, N'Hyundai Tucson 1.6T 2024', N'Tucson', 2024, N'Đỏ Đô', 2500, N'Gasoline', N'Automatic', 959000000, N'SUV thiết kế tương lai Parametric Dynamics, đèn định vị ẩn sáng.', N'https://images.unsplash.com/photo-1580273916550-e323be2ae537?w=800', N'Available', GETDATE()),
(@HyundaiId, N'Hyundai Creta 1.5L Cao Cấp 2023', N'Creta', 2023, N'Trắng-Đen', 10000, N'Gasoline', N'Automatic', 740000000, N'B-SUV đô thị trẻ trung, loa Bose cao cấp, gói an toàn Hyundai SmartSense.', N'https://images.unsplash.com/photo-1552519507-da3b142c6e3d?w=800', N'Available', GETDATE()),
(@HyundaiId, N'Hyundai Ioniq 5 Long Range 2023', N'Ioniq 5', 2023, N'Ghi Nhám', 4500, N'Electric', N'Automatic', 1450000000, N'Xe điện thiết kế Retro-futuristic đạt giải Xe của Năm thế giới.', N'https://images.unsplash.com/photo-1536700503339-1e4b06520771?w=800', N'Available', GETDATE()),

-- VinFast (45-48)
(@VinFastId, N'VinFast VF8 Plus 2023', N'VF8', 2023, N'Trắng Brahminy', 9000, N'Electric', N'Automatic', 1270000000, N'SUV điện thông minh hạng D, công suất 402 hp, trợ lý việt Vivi AI.', N'https://images.unsplash.com/photo-1563720223185-11003d516935?w=800', N'Available', GETDATE()),
(@VinFastId, N'VinFast VF9 Plus 6 Chỗ 2023', N'VF9', 2023, N'Xanh VinFast Blue', 5000, N'Electric', N'Automatic', 2170000000, N'SUV điện hạng E full-size cao cấp, ghế cơ trưởng massage sưởi thông gió.', N'https://images.unsplash.com/photo-1549399542-7e3f8b79c341?w=800', N'Available', GETDATE()),
(@VinFastId, N'VinFast VF7 Plus AWD 2024', N'VF7', 2024, N'Đỏ Crimson', 1500, N'Electric', N'Automatic', 1199000000, N'SUV điện hạng C thể thao thiết kế phi thuyền vũ trụ, công suất 349 hp.', N'https://images.unsplash.com/photo-1617814076367-b759c7d7e738?w=800', N'Available', GETDATE()),
(@VinFastId, N'VinFast VF5 Plus 2023', N'VF5', 2023, N'Cam VinFast', 12000, N'Electric', N'Automatic', 468000000, N'Xe điện gầm cao hạng A nhỏ gọn, chi phí vận hành siêu tiết kiệm.', N'https://images.unsplash.com/photo-1549399542-7e3f8b79c341?w=800', N'Available', GETDATE()),

-- Kia & Mazda (49-50)
(@KiaId, N'Kia Carnival 2.2D Signature 7 Chỗ 2023', N'Carnival', 2023, N'Đen', 14000, N'Diesel', N'Automatic', 1389000000, N'Mẫu xe SUV đô thị đa dụng 7 chỗ rộng nhất phân khúc, ghế Ottoman cao cấp.', N'https://images.unsplash.com/photo-1533473359331-0135ef1b58bf?w=800', N'Available', GETDATE()),
(@MazdaId, N'Mazda CX-5 2.0 Premium 2024', N'CX-5', 2024, N'Đỏ Soul Red Crystal', 3000, N'Gasoline', N'Automatic', 829000000, N'SUV 5 chỗ thiết kế KODO tinh tế, loa Bose 10 loa, trang bị i-Activesense.', N'https://images.unsplash.com/photo-1580273916550-e323be2ae537?w=800', N'Available', GETDATE());

GO

PRINT N'Thêm thành công 50 xe vào cơ sở dữ liệu CarShowroomDB!';
