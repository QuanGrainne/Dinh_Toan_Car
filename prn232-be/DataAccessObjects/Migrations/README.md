# EF Migrations — Baseline theo CarShowroomDB v2

Bộ migrations cũ (tạo trước v2) đã được **gỡ bỏ** vì:

- Chúng tham chiếu các bảng không còn tồn tại trong schema v2 (`ComboOrders`, `DepositCaptchas`)
  và thiếu nhiều bảng mới của v2 → không dùng làm baseline được.
- Ứng dụng **không** gọi `context.Database.Migrate()` khi khởi động (xem `Program.cs`).
  **Nguồn schema chuẩn là file SQL:** `database/CarShowroomDB_v2.sql`.

## Cách khởi tạo database (khuyến nghị)

```sql
-- 1) Tạo schema + seed
:r database/CarShowroomDB_v2.sql
-- 2) Bổ sung cột dùng chung mà code cần (additive, an toàn)
:r database/CarShowroomDB_v2_shared_columns_patch.sql
```

Chuỗi kết nối mặc định trỏ tới database `CarShowroomDB` (xem `appsettings.json`).

## Nếu muốn quay lại dùng EF Migrations

Sau khi DB đã khớp code, sinh lại **một** baseline sạch từ model hiện tại:

```bash
dotnet ef migrations add InitialV2Baseline \
  --project DataAccessObjects \
  --startup-project CarSalesManagementSystemAPI
```

Lưu ý: giữ SQL script và EF migrations đồng bộ, chỉ chọn **một** làm nguồn chuẩn để tránh lệch schema.
