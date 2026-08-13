# 🚗 ĐỊNH TOÀN AUTO - CAR SHOWROOM MANAGEMENT SYSTEM
> **Dự án Môn học PRN232** | Hệ thống Quản lý Showroom Ô tô, Phụ tùng và Gói Dịch vụ Bảo dưỡng

---

## 📌 Giới thiệu Tổng quan
**Định Toàn Auto** là hệ thống quản lý và giới thiệu xe ô tô hiện đại, hỗ trợ khách hàng tra cứu xe, phụ tùng, bảng giá dịch vụ bảo dưỡng, đồng thời cung cấp giao diện quản trị (Admin Dashboard) mạnh mẽ để quản lý toàn bộ danh mục sản phẩm của showroom.

---

## 🛠️ Công nghệ Sử dụng

### 1. Backend (`prn232-be`)
- **Framework:** .NET 8 (ASP.NET Core Web API)
- **Truy vấn dữ liệu:** Microsoft.AspNetCore.OData (v8)
- **ORM:** Entity Framework Core 8 (Code-First / Script-First)
- **Kiến trúc:** 3-Tier Architecture / Repository & Service Pattern
- **Bảo mật & Xác thực:** JWT Bearer Token, BCrypt Password Hashing, Role-based Authorization (`Admin`, `Customer`)
- **Tài liệu API:** Swagger / OpenAPI UI

### 2. Frontend (`prn232-fe`)
- **Framework:** ASP.NET Core 8 MVC (Razor Views)
- **Giao diện:** Bootstrap 5, FontAwesome Icons, Vanilla CSS
- **Kết nối API:** `HttpClient`, OData Client Queries

### 3. Database
- **Hệ quản trị CSDL:** Microsoft SQL Server 2019+
- **File kịch bản:** `database/dinh_toan_car.sql`

---

## ✨ Tính năng Chính

### 🚗 1. Quản lý Xe Ô tô (Cars Management)
- **Khách hàng / Khách vãng lai:**
  - Xem danh sách xe ô tô kèm hình ảnh, thông số kỹ thuật (hộp số, nhiên liệu, số km, dung tích động cơ...).
  - Bộ lọc thông minh theo Hãng xe, Khoảng giá, Loại nhiên liệu, Hộp số.
  - Phân trang dữ liệu và sắp xếp theo giá tăng/giảm, mới nhất.
  - Nhận diện xe thông qua hình ảnh bằng công nghệ AI.
- **Quản trị viên (Admin):**
  - CRUD thông tin xe (Thêm mới, Chỉnh sửa, Xóa, Cập nhật trạng thái `Available`/`Sold`).
  - Quản lý danh sách Hãng xe (Car Brands).

### ⚙️ 2. Quản lý Phụ tùng & Danh mục (Parts Management)
- **Khách hàng:**
  - Tra cứu danh mục phụ tùng theo nhóm (Lốp xe, Dầu nhớt, Hệ thống điện, Phanh...).
  - Tìm kiếm theo tên phụ tùng, mã sản phẩm (Part Code), thương hiệu.
- **Quản trị viên (Admin):**
  - Quản lý danh mục phụ tùng (Part Categories).
  - CRUD thông tin chi tiết phụ tùng (Mã, Tên, Giá, Đơn vị tính, Thương hiệu, Trạng thái).

### 🔧 3. Dịch vụ & Gói Bảo dưỡng (Maintenance Services & Packages)
- Giới thiệu các **Gói bảo dưỡng định kỳ** trọn gói (Cơ bản, Tiêu chuẩn, Cao cấp).
- Bảng giá các **Dịch vụ kỹ thuật lẻ** (Thay dầu động cơ, Cân chỉnh thước lái, Vệ sinh khoang máy, Kiểm tra toàn diện...).
- Chi tiết các hạng mục công việc và thời gian ước tính thực hiện.

### 👤 4. Tài khoản & Phân quyền (Authentication)
- Đăng ký, Đăng nhập hệ thống cấp phát mã thông báo JWT.
- Phân quyền nghiêm ngặt:
  - **Khách hàng / Khách:** Chỉ xem và tra cứu thông tin.
  - **Admin:** Toàn quyền quản trị CRUD danh mục và sản phẩm.

---

## 📂 Cấu trúc Thư mục

```text
project_25_7/
│
├── database/
│   └── dinh_toan_car.sql          # Script tạo Database & Dữ liệu mẫu
│
├── prn232-be/                     # BACKEND SOLUTION (.NET 8 Web API)
│   ├── BusinessObjects/           # Entities, DTOs, ViewModels
│   ├── DataAccessObjects/         # DbContext, DAOs
│   ├── Repositories/              # Interface & Repository implementations
│   ├── Services/                  # Business Logic Services
│   └── CarSalesManagementSystemAPI/# API Controllers & OData Endpoints
│
├── prn232-fe/                     # FRONTEND SOLUTION (.NET 8 MVC)
│   └── CarSalesManagementSystemClient/
│       ├── Controllers/           # MVC Controllers
│       ├── Models/                # ViewModels
│       └── Views/                 # Razor Views (Cars, Parts, Maintenance, Admin...)
│
└── README.md
```

---

## 🚀 Hướng dẫn Cài đặt & Khởi chạy

### Yêu cầu Tiên quyết
- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Microsoft SQL Server](https://www.microsoft.com/en-us/sql-server/sql-server-downloads) & [SSMS](https://learn.microsoft.com/en-us/sql/ssms/download-sql-server-management-studio-ssms)

---

### Bước 1: Khởi tạo Cơ sở Dữ liệu
1. Mở **SQL Server Management Studio (SSMS)** và kết nối tới SQL Server của bạn.
2. Mở file [`database/dinh_toan_car.sql`](database/dinh_toan_car.sql).
3. Nhấn **Execute (F5)** để tự động tạo CSDL `CarShowroomDB` và nạp dữ liệu mẫu ban đầu.

---

### Bước 2: Cấu hình Kết nối (Backend)
Mở file `prn232-be/CarSalesManagementSystemAPI/appsettings.json`, kiểm tra chuỗi kết nối:

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=.;Database=CarShowroomDB;Trusted_Connection=True;Encrypt=False;TrustServerCertificate=True;"
}
```
*(Thay đổi `Server=.` phù hợp với tên SQL Server Instance của máy bạn nếu cần)*

---

### Bước 3: Khởi chạy Backend API
Mở Terminal / PowerShell tại thư mục `prn232-be/CarSalesManagementSystemAPI`:

```bash
dotnet restore
dotnet run
```
👉 **Backend API sẽ chạy tại:** `http://localhost:5084`  
👉 **Swagger UI:** `http://localhost:5084/swagger`

---

### Bước 4: Khởi chạy Frontend Client
Mở cửa sổ Terminal / PowerShell khác tại thư mục `prn232-fe/CarSalesManagementSystemClient`:

```bash
dotnet restore
dotnet run
```
👉 **Website sẽ chạy tại:** `https://localhost:7117` hoặc cổng hiển thị trên console.

---

## 🔑 Tài khoản Đăng nhập Mặc định

| Vai trò | Email | Mật khẩu | Quyền hạn |
|---|---|---|---|
| **Admin** | `admin@dinhtoancar.vn` | `Admin@123` | Toàn quyền quản trị CRUD hệ thống |
| **Admin (Dev)** | `admin@gmail.com` | `admin` | Tài khoản phụ test hệ thống |
| **Customer** | `customer@gmail.com` | `customer` | Xem & tra cứu thông tin |

---

## 🌐 Danh sách API Endpoints Chính

| Phương thức | Tuyến đường (Route) | Mô tả |
|---|---|---|
| `GET` | `/odata/Cars` | Lấy danh sách xe ô tô (OData: `$filter`, `$expand`, `$top`, `$skip`...) |
| `GET` | `/odata/CarBrands` | Lấy danh sách hãng xe |
| `GET` | `/odata/Parts` | Lấy danh sách phụ tùng (kèm thông tin danh mục) |
| `GET` | `/api/PartCategories` | Lấy danh sách danh mục phụ tùng |
| `GET` | `/api/Services` | Danh sách dịch vụ bảo dưỡng lẻ |
| `GET` | `/api/MaintenancePackages` | Danh sách gói bảo dưỡng combo |
| `POST`| `/api/Auth/login` | Đăng nhập tài khoản & nhận JWT token |

---

## 👨‍💻 Nhóm Phát triển
- **Đề tài:** Hệ thống Quản lý Showroom Ô tô (PRN232)
- **Năm học:** 2025 - 2026
