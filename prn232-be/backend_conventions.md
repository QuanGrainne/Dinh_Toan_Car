# QUY ƯỚC LẬP TRÌNH BACKEND 🖥️ (BACKEND CODING CONVENTIONS)

Tài liệu này quy định các nguyên tắc thiết kế, quy chuẩn đặt tên và cấu trúc code áp dụng riêng cho phần **Backend (ASP.NET Core Web API)**.

---

## I. Tiêu Chuẩn Kiến Trúc & SOLID Principles

Hệ thống tuân thủ kiến trúc **3-Tier (Presentation - Business - Data)** và 5 nguyên lý SOLID:

1.  **S - Single Responsibility (Đơn nhiệm)**:
    *   **Controller**: Chỉ chịu trách nhiệm tiếp nhận HTTP Request, gọi tầng Service và trả về HTTP Response. Không chứa logic nghiệp vụ hay truy vấn SQL.
    *   **Service (Business Logic)**: Chứa toàn bộ nghiệp vụ của hệ thống (ví dụ: tự động tính cọc, gửi email xác nhận, cập nhật trạng thái xe).
    *   **Repository**: Lớp trung gian chuyển tiếp từ Service xuống DAO.
    *   **DAO**: Thực hiện tương tác trực tiếp với Database qua EF Core.
2.  **O - Open/Closed (Mở/Đóng)**: Thiết kế các Service mở rộng được thông qua kế thừa hoặc Interface mà không phải sửa trực tiếp lớp đang chạy ổn định.
3.  **L - Liskov Substitution (Thay thế Liskov)**: Các lớp triển khai cụ thể có khả năng thay thế Interface cha mà không làm hỏng tính đúng đắn của chương trình.
4.  **I - Interface Segregation (Phân tách Giao diện)**: Chia nhỏ interface theo chức năng chuyên biệt (ví dụ: `IAuthService`, `ICarService`, `IEmailService`).
5.  **D - Dependency Inversion (Đảo ngược phụ thuộc)**: Các tầng giao tiếp với nhau thông qua Interface. Sử dụng **Constructor Dependency Injection** để tiêm dependencies. Không sử dụng từ khóa `new` để khởi tạo các lớp dịch vụ trong Controller.

---

## II. Quy Chuẩn Lập Trình C# (Code Style)

### 1. Quy tắc Đặt tên (Naming Conventions)
*   **PascalCase**: Cho tên Lớp (Class), Interface, Method, Thuộc tính (Property), Namespace.
    *   *Ví dụ*: `CarsController`, `ICarService`, `GetCarById`, `Price`.
    *   *Lưu ý*: Tên Interface luôn bắt đầu bằng chữ `I` viết hoa (`ICarRepository`).
*   **camelCase**: Cho Tham số truyền vào (Parameter) và Biến cục bộ (Local Variable).
    *   *Ví dụ*: `carId`, `requestBody`, `tempPrice`.
*   **Underscore camelCase (`_camelCase`)**: Cho các trường Private Readonly (thường là các đối tượng được DI vào class).
    *   *Ví dụ*: `private readonly ICarRepository _carRepository;`

### 2. Thiết kế DAO & Singleton Pattern
Tất cả các lớp DAO ở tầng Data Access Layer phải được thiết kế dạng **Singleton Pattern** để tối ưu hóa bộ nhớ và quản lý kết nối hiệu quả:
```csharp
public class CarDAO
{
    private static CarDAO instance = null;
    private static readonly object instanceLock = new object();

    // Constructor private để chặn khởi tạo trực tiếp qua 'new'
    private CarDAO() { }

    public static CarDAO Instance
    {
        get
        {
            lock (instanceLock)
            {
                if (instance == null)
                {
                    instance = new CarDAO();
                }
                return instance;
            }
        }
    }

    // Mỗi truy vấn bắt buộc sử dụng khối lệnh 'using' để giải phóng Connection ngay lập tức
    public IEnumerable<Car> GetAllCars()
    {
        using var context = new CarShowroomContext();
        return context.Cars.Include(c => c.Brand).ToList();
    }
}
```

### 3. Quy chuẩn Dữ liệu (DTO vs Entity)
*   **Tuyệt đối không trả trực tiếp các Entity của EF Core (như Car, AppUser)** về Client thông qua API. Điều này giúp ngăn chặn lỗi lặp vòng tròn JSON (`Json Loop References`) và tăng tính bảo mật thông tin.
*   Sử dụng **DTO (Data Transfer Object)** hoặc **ViewModel** để nhận đầu vào từ API và trả dữ liệu ra ngoài Client.
*   Ràng buộc chặt chẽ kiểu dữ liệu đầu vào bằng `DataAnnotations` trong DTO:
    ```csharp
    public class CarDto
    {
        [Required(ErrorMessage = "Tên xe không được trống")]
        [StringLength(150, ErrorMessage = "Tên xe không quá 150 ký tự")]
        public string CarName { get; set; } = null!;

        [Range(1000000, 10000000000, ErrorMessage = "Giá bán không hợp lệ")]
        public decimal Price { get; set; }
    }
    ```

### 4. Cấu hình OData API
*   Các API hỗ trợ tìm kiếm/lọc nâng cao phải được tích hợp **OData**.
*   Đăng ký OData Entity Set trong `Program.cs` và gắn Attribute `[EnableQuery]` ở các phương thức `Get()` của Controller để cho phép Client sử dụng các tham số truy vấn nâng cao (`$filter`, `$expand`, `$select`, `$orderby`).

### 5. Quản lý Lỗi và HTTP Response
Mọi API trả về phải tuân thủ cấu trúc JSON thống nhất và phản ánh đúng trạng thái HTTP:
*   **200 OK / 201 Created**: Cho các tác vụ thành công. Trả về cấu trúc:
    ```json
    { "success": true, "message": "Thông báo thành công", "data": { ... } }
    ```
*   **400 Bad Request**: Khi dữ liệu đầu vào không hợp lệ hoặc lỗi Validate.
*   **401 Unauthorized / 403 Forbidden**: Khi không đăng nhập hoặc Token JWT không có quyền truy cập vai trò tương ứng (ví dụ: User gọi API xóa xe của Admin).
*   **404 Not Found**: Không tìm thấy thực thể cần truy vấn.
*   **500 Internal Server Error**: Lỗi hệ thống ngầm. Thực hiện log lỗi ra server và chỉ trả thông báo lỗi chung về Client, tránh để lộ thông tin bảo mật hệ thống.
