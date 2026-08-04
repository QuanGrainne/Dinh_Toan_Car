# TÀI LIỆU KỸ THUẬT & HƯỚNG DẪN LẬP TRÌNH: MODULE QUẢN LÝ PHỤ TÙNG
*(Tài liệu Kỹ thuật Chuẩn Production - Dành cho Lập trình viên)*

Tài liệu này cung cấp chi tiết thiết kế cơ sở dữ liệu, nghiệp vụ Logistics kho, danh sách API, quy tắc nghiệp vụ (Business Rules), và mã nguồn mẫu cho module **Quản lý phụ tùng (Spare Parts Management)**.

---

## 1. PHẠM VI NGHIỆP VỤ & THỰC THỂ CƠ SỞ DỮ LIỆU

Module Phụ tùng phụ trách quản lý danh mục vật tư phụ tùng, kiểm tra tính tương thích với đời xe, quản lý luồng nhập kho từ nhà cung cấp, và lưu trữ lịch sử biến động kho hàng (Nhật ký giao dịch kho).

### Các bảng dữ liệu liên quan:
1. **`Suppliers`**: Nhà cung cấp phụ tùng chính hãng.
2. **`PartCategories`**: Phân loại phụ tùng (Ắc quy, Lốp xe, Dầu máy...).
3. **`Parts`**: Danh mục chi tiết các phụ tùng trong kho kèm vị trí kệ.
4. **`PartCompatibilities`**: Danh sách cấu hình dòng xe tương thích của phụ tùng.
5. **`InventoryReceipts` & `InventoryReceiptDetails`**: Hóa đơn nhập hàng từ NCC.
6. **`InventoryTransactions`**: Nhật ký ghi nhận mọi biến động xuất/nhập kho vật lý.

---

## 2. THIẾT KẾ CƠ SỞ DỮ LIỆU CHUYÊN SÂU

```sql
-- Cấu trúc bảng Parts (Phụ tùng)
CREATE TABLE Parts (
    PartId INT IDENTITY(1,1) PRIMARY KEY,
    CategoryId INT NOT NULL,
    PartName NVARCHAR(150) NOT NULL,
    PartCode VARCHAR(50) NOT NULL UNIQUE, 
    Brand NVARCHAR(100) NULL,
    Price DECIMAL(18,2) NOT NULL,            -- Giá bán ra cho khách
    Quantity INT NOT NULL DEFAULT 0,          -- Tồn kho hiện tại
    MinStockLevel INT NOT NULL DEFAULT 5,     -- Điểm đặt hàng lại (Reorder Point)
    MaxStockLevel INT NOT NULL DEFAULT 100,   -- Sức chứa tối đa
    UnitOfMeasure NVARCHAR(20) NOT NULL DEFAULT N'Cái', -- Cái, Lít, Cặp...
    WarehouseLocation NVARCHAR(100) NULL,    -- Vị trí kho (Ví dụ: Khu A - Kệ 3 - Ngăn 1)
    WarrantyMonths INT NOT NULL DEFAULT 0,
    Status NVARCHAR(50) NOT NULL DEFAULT 'Available'
        CONSTRAINT CK_Parts_Status CHECK (Status IN ('Available', 'OutOfStock', 'Inactive')),
    ExpiredAt DATETIME NULL,                 -- Hạn sử dụng phụ tùng (Dầu nhớt, Ắc quy...)
    
    -- Audit fields
    CreatedAt DATETIME NOT NULL DEFAULT GETDATE(),
    CreatedUser INT NULL,
    UpdatedAt DATETIME NULL,
    UpdatedUser INT NULL,
    
    CONSTRAINT FK_Parts_PartCategories FOREIGN KEY(CategoryId) REFERENCES PartCategories(CategoryId)
);
```

### Các ràng buộc & Chỉ mục (Constraints & Indexes):
* **`IX_Parts_PartCode`**: Đảm bảo mã phụ tùng là duy nhất và tăng tốc tìm kiếm phụ tùng bằng đầu đọc mã vạch (Barcode Scanner).
* **`IX_PartCompatibilities_PartId`**: Tăng tốc truy vấn khi lọc danh sách phụ tùng tương thích với xe của khách hàng.

---

## 3. THIẾT KẾ DTOs (DATA TRANSFER OBJECTS) C#

```csharp
namespace BusinessObjects.DTOs;

// DTO kiểm tra độ tương thích của phụ tùng với xe khách
public class PartCompatibilityCheckDto
{
    public string LicensePlate { get; set; } = null!;
    public string PartCode { get; set; } = null!;
}

// DTO phản hồi khi kiểm tra tương thích
public class CompatibilityResultDto
{
    public string PartCode { get; set; } = null!;
    public string PartName { get; set; } = null!;
    public bool IsCompatible { get; set; }
    public string Message { get; set; } = null!;
}
```

---

## 4. CHI TIẾT DANH SÁCH API CỐT LÕI (API SPECIFICATIONS)

### A. Kiểm tra độ tương thích của phụ tùng với xe khách hàng
* **API:** `POST /api/parts/check-compatibility`
* **Mô tả:** Kiểm tra phụ tùng có lắp đặt vừa vặn với dòng xe và năm sản xuất của xe khách hay không.
* **Request Body:**
  ```json
  {
    "licensePlate": "30G-888.88",
    "partCode": "PT-BOS-AERO"
  }
  ```
* **Response (200 OK):**
  ```json
  {
    "partCode": "PT-BOS-AERO",
    "partName": "Gạt mưa Bosch Aerotwin",
    "isCompatible": true,
    "message": "Phụ tùng tương thích hoàn toàn với dòng xe Toyota Vios 2021 của khách hàng."
  }
  ```

### B. Cập nhật giao dịch nhập kho (Nhập hàng từ Nhà cung cấp)
* **API:** `POST /api/inventory/receipt`
* **Mô tả:** Lập phiếu nhập kho phụ tùng.
* **Headers:** `Authorization: Bearer <token>`
* **Request Body:**
  ```json
  {
    "supplierId": 1,
    "notes": "Nhập kho lô hàng phụ tùng Bosch tháng 7",
    "items": [
      {
        "partId": 4,
        "quantity": 50,
        "importPrice": 320000.00
      }
    ]
  }
  ```
* **Response (200 OK):**
  ```json
  {
    "receiptId": 12,
    "totalAmount": 16000000.00,
    "status": "Success",
    "message": "Đã nhập kho 50 cặp Gạt mưa Bosch và tự động cộng dồn tồn kho."
  }
  ```

---

## 5. QUY TẮC NGHIỆP VỤ & LƯU Ý KHI CODE (BUSINESS RULES)

1. **Ràng buộc thay đổi tồn kho (Audit Trail):**
   * **Bắt buộc:** Tuyệt đối không viết code update trực tiếp cột `Quantity` trong bảng `Parts` mà không có bản ghi nhật ký tương ứng trong bảng `InventoryTransactions`.
   * Mọi hành động làm thay đổi tồn kho (Nhập NCC, Xuất bán phụ tùng, Thay thế trong lịch hẹn dịch vụ, Kiểm kho) đều phải ghi nhận vào `InventoryTransactions` kèm mã tham chiếu (`ReferenceId`) để phục vụ đối soát.
2. **Cảnh báo tồn kho tối thiểu (MinStockLevel):**
   * Khi thực hiện bán phụ tùng hoặc xuất kho lắp đặt, hệ thống cần kiểm tra nếu số lượng tồn kho sau xuất nhỏ hơn `MinStockLevel`, hệ thống phải kích hoạt cờ cảnh báo nhập thêm hàng (`OutOfStock` hoặc gửi mail cho quản trị viên kho).
3. **Quản lý hạn sử dụng phụ tùng (`ExpiredAt`):**
   * Phụ tùng thuộc loại dầu máy, hóa chất, hoặc ắc quy cần bắt buộc truyền giá trị `ExpiredAt`. Khi lập phiếu xuất kho, hệ thống ưu tiên xuất các lô hàng có `ExpiredAt` gần nhất (Quy tắc FIFO/FEFO).
