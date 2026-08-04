# Module Ô Tô + Hóa đơn tổng dùng chung — Nâng cấp theo CarShowroomDB v2 (Production)

Phạm vi: module ô tô, AI detect ô tô, chatbot tư vấn, **và tầng hóa đơn/thanh toán dùng chung**
trên `MasterInvoice` cho cả 3 module. Không viết lại logic kho (phụ tùng) hay lịch hẹn (dịch vụ).

## 1. Triết lý: MasterInvoice là trung tâm

Mọi giao dịch — mua lẻ 1 module hay mua GỘP nhiều module — đều tạo **một `MasterInvoice`**.
Các hóa đơn con (`CarInvoice`, `PartInvoice`, `ServiceInvoice`) trỏ về master đó.
Nghiệp vụ **đặt cọc (Deposit)** và **mua đứt (Buyout)** xác thực bằng **mã captcha do nhân viên sinh**,
khách hàng nhập để xác thực — **dùng chung cho cả 3 module**.

```
                         ┌────────────── MasterInvoice (hồ sơ thanh toán trung tâm) ──────────────┐
   CarInvoice (xe) ──────┤  PurchaseType: Deposit | Buyout                                         │
   PartInvoice (phụ tùng)┤  PaymentStatus: Unpaid → Deposited → Paid                               │
   ServiceInvoice (dịch vụ)  InvoiceStatus: PendingVerification → Confirmed → Completed            │
                         │  DepositCaptchaCode / FinalCaptchaCode (nhân viên sinh, khách xác thực) │
                         └────────────────────────────────────────────────────────────────────────┘
```

### Vòng đời thanh toán (chung mọi module)
1. Nhân viên tạo hóa đơn tổng qua **`POST /api/checkout`** (gộp xe + đơn phụ tùng + lịch dịch vụ của 1 khách).
   - `Deposit` → sinh `DepositCaptchaCode`, `DepositAmount` (mặc định 10% tổng), `DepositExpiresAt`.
   - `Buyout`  → sinh `FinalCaptchaCode`.
   - Xe được giữ chỗ (`Reserved`), phụ tùng bị trừ kho + ghi `InventoryTransaction`, lịch dịch vụ gắn master.
2. Khách nhập mã đặt cọc (`POST /api/invoices/verify/deposit`) → `PaymentStatus=Deposited`, `InvoiceStatus=Confirmed`.
3. Nhân viên sinh mã tất toán (`POST /api/invoices/{id}/final-captcha`).
4. Khách nhập mã tất toán/mua đứt (`POST /api/invoices/verify/final`) → `Paid`/`Completed`.
   - Xe → `Sold`, yêu cầu mua → `Completed`; đơn phụ tùng → đã thanh toán; lịch dịch vụ → `IsPaid=true`.
5. Cọc quá hạn: `POST /api/invoices/deposits/release-expired` (trả xe về `Available`, hủy hóa đơn).

Mua đứt trực tiếp: bỏ qua bước 2–3, chỉ cần bước 1 (`Buyout`) rồi bước 4.

## 2. API

### Yêu cầu mua xe (khách) — `/api/car-sales`
| Method | Route | Quyền | Mô tả |
|---|---|---|---|
| POST | `/api/car-sales/requests` | Đăng nhập | Khách gửi yêu cầu mua xe |
| GET  | `/api/car-sales/requests` | Đăng nhập | Admin/Staff xem tất cả; khách xem của mình |

### Lập hóa đơn tổng (nhân viên) — `/api/checkout`
| Method | Route | Quyền | Mô tả |
|---|---|---|---|
| POST | `/api/checkout` | Admin, Staff | Tạo 1 master gộp `Cars[]` (theo `PurchaseRequestId` + phí) + `PartOrderIds[]` + `AppointmentIds[]`, chọn `Deposit`/`Buyout`, sinh captcha |

### Hóa đơn & thanh toán dùng chung — `/api/invoices`
| Method | Route | Quyền | Mô tả |
|---|---|---|---|
| POST | `/api/invoices/deposit-captcha` | Admin, Staff | Sinh mã đặt cọc cho 1 master có sẵn |
| POST | `/api/invoices/{id}/final-captcha` | Admin, Staff | Sinh mã tất toán/mua đứt |
| POST | `/api/invoices/verify/deposit` | Đăng nhập | Khách xác thực đặt cọc |
| POST | `/api/invoices/verify/final` | Đăng nhập | Khách xác thực tất toán/mua đứt |
| POST | `/api/invoices/deposits/release-expired` | Admin, Staff | Giải phóng cọc hết hạn |
| GET  | `/api/invoices?type=Car\|Part\|Service\|Combined` | Đăng nhập | Danh sách (mã captcha chỉ hiện cho Admin/Staff) |
| GET  | `/api/invoices/{id}` | Đăng nhập | Chi tiết hóa đơn tổng + dòng chi tiết mọi module |

AI detect: `POST /api/CarDetection/detect`, `POST /api/CarDetection/sync`.
Chatbot: `POST /api/Chat/message` — chỉ tư vấn, không tạo đơn.

## 3. Kiến trúc mã nguồn (3-tier + Singleton DAO)

**Tầng dùng chung (mới)**
- `BusinessObjects/Common/MasterInvoiceDtos.cs` — `MasterInvoiceViewDto`, `InvoiceLineDto`, `CheckoutDto`, `GenerateDepositCaptchaDto`…
- `DataAccessObjects/MasterInvoicePaymentDAO.cs` — sinh/xác thực captcha + **dispatcher side-effect** theo loại hóa đơn con.
- `DataAccessObjects/CheckoutDAO.cs` — tạo 1 master gộp nhiều module (giữ chỗ xe, trừ kho phụ tùng, gắn lịch dịch vụ).
- `DataAccessObjects/InvoiceHelpers.cs` — `CaptchaHelper`, `InvoiceNumberHelper`.
- `Repositories` + `Services`: `MasterInvoicePayment*`, `Checkout*`.
- `Controllers`: `InvoicesController`, `CheckoutController`.

**Module ô tô**
- `CarSales*` chỉ còn lo **yêu cầu mua xe** (tạo & tra cứu). Phần hóa đơn/đặt cọc/mua đứt đã chuyển sang tầng chung.

**Chỉnh tối thiểu module khác**
- `Services/PartOrderService.cs` — thêm guard: nếu đơn đã gắn `MasterInvoiceId` (qua checkout) thì không tạo master/trừ kho lần hai khi admin xác nhận. Logic kho, giá, giao hàng giữ nguyên.
- `MaintenanceAppointmentService` — không đổi: nó vốn chỉ tạo master khi `MasterInvoiceId` chưa có, nên checkout gộp không gây trùng.

## 4. Database

1. `database/CarShowroomDB_v2.sql` (DB tên `CarShowroomDB`).
2. `database/CarShowroomDB_v2_shared_columns_patch.sql` — **bắt buộc**: thêm `InvoiceType, PaymentMethod, PaymentReference, PaidAt` vào `MasterInvoices` và `IsPaid` vào `MaintenanceAppointments` (các cột code của cả 3 module cần).
   - `InvoiceType` nhận thêm giá trị `Combined` cho hóa đơn gộp.

## 5. Quy ước trạng thái

- `MasterInvoices.PurchaseType`: `Deposit` | `Buyout`.
- `PaymentStatus`: `Unpaid` → `Deposited` → `Paid` (hoặc `Refunded` khi hủy cọc).
- `InvoiceStatus`: `PendingVerification` → `Confirmed` → `Completed` (hoặc `Cancelled`).
- Xe: `Available` → `Reserved` (đặt cọc/giữ chỗ) → `Sold` (tất toán).

## 6. Ghi chú production còn tồn (khuyến nghị)

- `appsettings.json` đang lộ mật khẩu SMTP + JWT Secret → chuyển sang User Secrets / biến môi trường.
- Thư mục scaffold `BusinessObjects/ModelsTemp/`, `BusinessObjects/ContextTemp/` dư thừa → nên xóa.
- Có thể bổ sung audit fields cho entity `Car` để dùng đủ cột v2.
- Phí trước bạ/biển số/bảo hiểm của xe hiện do nhân viên nhập khi checkout; có thể chuẩn hóa thành cấu hình theo % giá xe nếu muốn.
