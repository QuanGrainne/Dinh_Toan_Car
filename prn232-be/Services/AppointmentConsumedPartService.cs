using System;
using BusinessObjects.Models;
using BusinessObjects.DTOs;
using Repositories;

namespace Services
{
    public class AppointmentConsumedPartService : IAppointmentConsumedPartService
    {
        private readonly IAppointmentConsumedPartRepository _consumedPartRepository;
        private readonly IPartRepository _partRepository;
        private readonly IMaintenanceAppointmentRepository _appointmentRepository;

        public AppointmentConsumedPartService(
            IAppointmentConsumedPartRepository consumedPartRepository,
            IPartRepository partRepository,
            IMaintenanceAppointmentRepository appointmentRepository)
        {
            _consumedPartRepository = consumedPartRepository;
            _partRepository = partRepository;
            _appointmentRepository = appointmentRepository;
        }

        private void ValidateAppointmentActive(int appointmentId)
        {
            var appointment = _appointmentRepository.GetAppointmentById(appointmentId);
            if (appointment != null && (appointment.IsPaid || appointment.Status == "Completed" || appointment.Status == "Cancelled"))
            {
                throw new Exception("Đơn hàng/lịch hẹn đã hoàn thành, thanh toán hoặc bị hủy. Không thể chỉnh sửa danh sách phụ tùng.");
            }
        }

        public void ReportIncurredPart(IncurredPartReportDto dto)
        {
            ValidateAppointmentActive(dto.AppointmentId);

            var part = _partRepository.GetPartById(dto.PartId);
            if (part == null) throw new Exception("Không tìm thấy phụ tùng.");

            var consumedPart = new AppointmentConsumedPart
            {
                AppointmentId = dto.AppointmentId,
                AppointmentDetailId = dto.AppointmentDetailId,
                PartId = dto.PartId,
                Quantity = dto.Quantity,
                UnitPrice = part.Price, // Khóa giá lúc báo cáo
                IsIncurred = true,
                ApprovedByCustomer = false, // Chờ khách duyệt
                Notes = dto.Notes,
                CreatedAt = DateTime.Now
            };

            _consumedPartRepository.AddConsumedPart(consumedPart);
        }

        public void AddPart(IncurredPartReportDto dto)
        {
            ValidateAppointmentActive(dto.AppointmentId);

            var part = _partRepository.GetPartById(dto.PartId);
            if (part == null) throw new Exception("Không tìm thấy phụ tùng.");
            if (part.Quantity < dto.Quantity) throw new Exception("Không đủ số lượng tồn kho.");

            // Trừ kho ngay lập tức
            part.Quantity -= dto.Quantity;
            if (part.Quantity == 0 || part.Quantity < part.MinStockLevel)
            {
                part.Status = "OutOfStock";
            }
            _partRepository.UpdatePart(part);

            var consumedPart = new AppointmentConsumedPart
            {
                AppointmentId = dto.AppointmentId,
                AppointmentDetailId = dto.AppointmentDetailId,
                PartId = dto.PartId,
                Quantity = dto.Quantity,
                UnitPrice = part.Price, // Khóa giá lúc báo cáo
                IsIncurred = false,
                ApprovedByCustomer = true, // Khách đã duyệt/Admin thêm vào
                Notes = dto.Notes,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };

            _consumedPartRepository.AddConsumedPart(consumedPart);

            // Ghi nhận InventoryTransaction Export
            using var context = new DataAccessObjects.CarShowroomContext();
            context.InventoryTransactions.Add(new InventoryTransaction
            {
                PartId = part.PartId,
                TransactionType = BusinessObjects.Common.InventoryTransactionTypes.Export,
                Quantity = -dto.Quantity,
                ReferenceType = BusinessObjects.Common.InventoryReferenceTypes.MaintenanceAppointment,
                ReferenceId = dto.AppointmentId,
                Notes = $"Xuất phụ tùng cho lịch bảo dưỡng #{dto.AppointmentId}",
                TransactionDate = DateTime.Now,
                CreatedAt = DateTime.Now
            });
            context.SaveChanges();
        }

        public void ApproveIncurredPart(IncurredPartApprovalDto dto)
        {
            var consumedPart = _consumedPartRepository.GetById(dto.ConsumedPartId);
            if (consumedPart == null) throw new Exception("Không tìm thấy bản ghi phụ tùng phát sinh.");
            
            ValidateAppointmentActive(consumedPart.AppointmentId);

            if (!consumedPart.IsIncurred) throw new Exception("Đây là phụ tùng định mức, không thể duyệt/từ chối.");
            if (consumedPart.ApprovedByCustomer) throw new Exception("Phụ tùng này đã được duyệt trước đó.");

            if (dto.IsApproved)
            {
                // Khách duyệt -> cập nhật trạng thái và trừ kho
                var part = _partRepository.GetPartById(consumedPart.PartId);
                if (part == null) throw new Exception("Không tìm thấy phụ tùng trong kho.");
                if (part.Quantity < consumedPart.Quantity) throw new Exception("Không đủ tồn kho cho phụ tùng này.");

                part.Quantity -= consumedPart.Quantity;
                if (part.Quantity == 0 || part.Quantity < part.MinStockLevel)
                {
                    part.Status = "OutOfStock";
                }
                _partRepository.UpdatePart(part);

                consumedPart.ApprovedByCustomer = true;
                consumedPart.UpdatedAt = DateTime.Now;
                _consumedPartRepository.UpdateConsumedPart(consumedPart);

                // Ghi nhận InventoryTransaction Export
                using var context = new DataAccessObjects.CarShowroomContext();
                context.InventoryTransactions.Add(new InventoryTransaction
                {
                    PartId = part.PartId,
                    TransactionType = BusinessObjects.Common.InventoryTransactionTypes.Export,
                    Quantity = -consumedPart.Quantity,
                    ReferenceType = BusinessObjects.Common.InventoryReferenceTypes.MaintenanceAppointment,
                    ReferenceId = consumedPart.AppointmentId,
                    Notes = $"Xuất phụ tùng phát sinh đã duyệt cho lịch bảo dưỡng #{consumedPart.AppointmentId}",
                    TransactionDate = DateTime.Now,
                    CreatedAt = DateTime.Now
                });
                context.SaveChanges();
            }
            else
            {
                // Khách từ chối -> xóa bản ghi phát sinh này
                _consumedPartRepository.DeleteConsumedPart(dto.ConsumedPartId);
            }
        }

        public void RemoveIncurredPart(int consumedPartId)
        {
            var consumedPart = _consumedPartRepository.GetById(consumedPartId);
            if (consumedPart == null) throw new Exception("Không tìm thấy phụ tùng phát sinh.");

            ValidateAppointmentActive(consumedPart.AppointmentId);

            // Nếu đã được duyệt (đã trừ kho), cần hoàn lại số lượng vào kho
            if (consumedPart.ApprovedByCustomer)
            {
                var part = _partRepository.GetPartById(consumedPart.PartId);
                if (part != null)
                {
                    part.Quantity += consumedPart.Quantity;
                    if ((part.Status == "OutOfStock" || part.Status == "Out of Stock") && part.Quantity > 0)
                    {
                        part.Status = "Available";
                    }
                    _partRepository.UpdatePart(part);

                    using var context = new DataAccessObjects.CarShowroomContext();
                    context.InventoryTransactions.Add(new InventoryTransaction
                    {
                        PartId = part.PartId,
                        TransactionType = BusinessObjects.Common.InventoryTransactionTypes.Return,
                        Quantity = consumedPart.Quantity,
                        ReferenceType = BusinessObjects.Common.InventoryReferenceTypes.MaintenanceAppointment,
                        ReferenceId = consumedPart.AppointmentId,
                        Notes = $"Hoàn kho phụ tùng từ lịch bảo dưỡng #{consumedPart.AppointmentId}",
                        TransactionDate = DateTime.Now,
                        CreatedAt = DateTime.Now
                    });
                    context.SaveChanges();
                }
            }

            _consumedPartRepository.DeleteConsumedPart(consumedPartId);
        }
    }
}
