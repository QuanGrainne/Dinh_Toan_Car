using System;
using System.Collections.Generic;
using System.Linq;
using BusinessObjects.Models;
using BusinessObjects.DTOs;
using DataAccessObjects;
using Microsoft.EntityFrameworkCore;
using Repositories;

namespace Services
{
    public class MaintenanceAppointmentService : IMaintenanceAppointmentService
    {
        private readonly IMaintenanceAppointmentRepository _repository;
        private readonly IMaintenancePackageRepository _packageRepository;
        private readonly IServiceRepository _serviceRepository;
        private readonly ICustomerCarRepository _customerCarRepo;
        private readonly IPartRepository _partRepository;

        public MaintenanceAppointmentService(
            IMaintenanceAppointmentRepository repository,
            IMaintenancePackageRepository packageRepository,
            IServiceRepository serviceRepository,
            ICustomerCarRepository customerCarRepo,
            IPartRepository partRepository)
        {
            _repository = repository;
            _packageRepository = packageRepository;
            _serviceRepository = serviceRepository;
            _customerCarRepo = customerCarRepo;
            _partRepository = partRepository;
        }

        public IEnumerable<MaintenanceAppointmentDTO> GetAllAppointments()
        {
            var appointments = _repository.GetAllAppointments();
            return appointments.Select(MapToDTO);
        }

        public IEnumerable<MaintenanceAppointmentDTO> GetAppointmentsByCustomerId(int customerId)
        {
            var appointments = _repository.GetAppointmentsByCustomerId(customerId);
            return appointments.Select(MapToDTO);
        }

        public MaintenanceAppointmentDTO GetAppointmentById(int appointmentId)
        {
            var appointment = _repository.GetAppointmentById(appointmentId);
            if (appointment == null) return null;
            return MapToDTO(appointment);
        }

        public MaintenanceAppointmentDTO CreateAppointment(int customerId, CreateAppointmentDTO createDto)
        {
            int carId = createDto.CustomerCarId;
            if (carId <= 0 && !string.IsNullOrWhiteSpace(createDto.LicensePlate))
            {
                var existingCar = _customerCarRepo.GetByLicensePlate(createDto.LicensePlate);
                if (existingCar != null)
                {
                    carId = existingCar.CustomerCarId;
                }
                else
                {
                    var newCar = new CustomerCar
                    {
                        CustomerId = customerId,
                        Model = string.IsNullOrWhiteSpace(createDto.CarName) ? "Chưa rõ" : createDto.CarName,
                        LicensePlate = createDto.LicensePlate,
                        VIN = "VIN-" + Guid.NewGuid().ToString().Substring(0, 13).ToUpper(),
                        BrandId = 1 // Default or placeholder
                    };
                    _customerCarRepo.AddCustomerCar(newCar);
                    var savedCar = _customerCarRepo.GetByLicensePlate(createDto.LicensePlate);
                    carId = savedCar.CustomerCarId;
                }
            }

            var appointment = new MaintenanceAppointment
            {
                CustomerId = customerId,
                CustomerCarId = carId,
                CustomerName = createDto.CustomerName,
                CustomerPhone = createDto.CustomerPhone,
                CustomerEmail = createDto.CustomerEmail,
                AppointmentDate = createDto.AppointmentDate,
                AppointmentTime = createDto.AppointmentTime,
                Note = createDto.Note,
                Status = "Pending",
                CreatedAt = DateTime.Now
            };

            var details = new List<AppointmentDetail>();
            var consumedParts = new List<AppointmentConsumedPart>();

            // Xử lý các gói bảo dưỡng
            if (createDto.PackageIds != null)
            {
                foreach (var packageId in createDto.PackageIds)
                {
                    var package = _packageRepository.GetPackageById(packageId);
                    if (package != null)
                    {
                        details.Add(new AppointmentDetail
                        {
                            PackageId = packageId,
                            UnitPrice = package.PackagePrice,
                            Quantity = 1,
                            CreatedAt = DateTime.Now
                        });
                    }
                }
            }

            // Xử lý các dịch vụ lẻ
            if (createDto.ServiceIds != null)
            {
                foreach (var serviceId in createDto.ServiceIds)
                {
                    var service = _serviceRepository.GetServiceById(serviceId);
                    if (service != null)
                    {
                        details.Add(new AppointmentDetail
                        {
                            ServiceId = serviceId,
                            UnitPrice = service.BasePrice,
                            Quantity = 1,
                            CreatedAt = DateTime.Now
                        });
                    }
                }
            }
            
            // Xử lý các phụ tùng mua kèm
            if (createDto.PartItems != null)
            {
                foreach (var partItem in createDto.PartItems)
                {
                    var part = _partRepository.GetPartById(partItem.PartId);
                    if (part != null)
                    {
                        consumedParts.Add(new AppointmentConsumedPart
                        {
                            PartId = partItem.PartId,
                            Quantity = partItem.Quantity,
                            UnitPrice = part.Price, // Lấy giá hiện tại
                            CreatedAt = DateTime.Now
                        });
                    }
                }
            }

            if (!details.Any() && !consumedParts.Any())
            {
                throw new Exception("Vui long chon it nhat mot goi, dich vu le hoac phu tung.");
            }

            var createdAppointment = _repository.CreateAppointmentWithDetails(appointment, details, consumedParts);
            
            // Lấy lại từ DB để có đầy đủ navigation properties cho DTO
            var fullAppointment = _repository.GetAppointmentById(createdAppointment.AppointmentId);
            return MapToDTO(fullAppointment);
        }

        public void UpdateAppointmentStatus(int appointmentId, string status, string? reason = null)
        {
            var appointment = _repository.GetAppointmentById(appointmentId);
            if (appointment != null)
            {
                appointment.Status = status;
                if (!string.IsNullOrEmpty(reason))
                {
                    appointment.Note = string.IsNullOrEmpty(appointment.Note) ? $"[Lý do hủy: {reason}]" : $"{appointment.Note}\n[Lý do hủy: {reason}]";
                }
                appointment.UpdatedAt = DateTime.Now;
                _repository.UpdateAppointment(appointment);

                // Auto-generate MasterInvoice & ServiceInvoice when status transitions to Completed
                if (status == "Completed" && !appointment.MasterInvoiceId.HasValue)
                {
                    using var context = new CarShowroomContext();
                    var dbAppointment = context.MaintenanceAppointments
                        .Include(a => a.AppointmentDetails)
                        .Include(a => a.ConsumedParts)
                        .FirstOrDefault(a => a.AppointmentId == appointmentId);

                    if (dbAppointment != null && !dbAppointment.MasterInvoiceId.HasValue)
                    {
                        decimal detailsTotal = dbAppointment.AppointmentDetails?.Sum(d => d.UnitPrice * d.Quantity) ?? 0;
                        decimal partsTotal = dbAppointment.ConsumedParts?.Where(p => p.ApprovedByCustomer).Sum(p => p.UnitPrice * p.Quantity) ?? 0;

                        decimal extraFee = 0;
                        if (!string.IsNullOrEmpty(dbAppointment.Note))
                        {
                            var match = System.Text.RegularExpressions.Regex.Match(dbAppointment.Note, @"\[PhiPhatSinh:\s*(\d+)\]");
                            if (match.Success)
                            {
                                decimal.TryParse(match.Groups[1].Value, out extraFee);
                            }
                        }

                        decimal totalAmount = detailsTotal + partsTotal + extraFee;

                        var masterInvoice = new MasterInvoice
                        {
                            InvoiceNumber = $"INV-SRV-{DateTime.Now:yyyyMMdd}-{appointmentId:D4}",
                            InvoiceType = BusinessObjects.Common.InvoiceTypes.Service,
                            CustomerId = dbAppointment.CustomerId,
                            TotalSubTotal = detailsTotal + partsTotal,
                            DiscountAmount = 0,
                            TaxAmount = 0,
                            TotalAmount = totalAmount,
                            PaymentStatus = dbAppointment.IsPaid ? BusinessObjects.Common.PaymentStatuses.Paid : BusinessObjects.Common.PaymentStatuses.Unpaid,
                            InvoiceStatus = BusinessObjects.Common.InvoiceStatuses.Confirmed,
                            PurchaseType = "Buyout",
                            CreatedAt = DateTime.Now
                        };
                        context.MasterInvoices.Add(masterInvoice);
                        context.SaveChanges();

                        var serviceInvoice = new ServiceInvoice
                        {
                            MasterInvoiceId = masterInvoice.MasterInvoiceId,
                            AppointmentId = appointmentId,
                            SubTotal = detailsTotal + partsTotal,
                            LaborDiscount = 0,
                            TotalAmount = totalAmount,
                            CreatedAt = DateTime.Now
                        };
                        context.ServiceInvoices.Add(serviceInvoice);

                        dbAppointment.MasterInvoiceId = masterInvoice.MasterInvoiceId;
                        dbAppointment.UpdatedAt = DateTime.Now;
                        context.SaveChanges();
                    }
                }
            }
        }

        public void UpdateAppointmentPaymentStatus(int appointmentId, bool isPaid)
        {
            var appointment = _repository.GetAppointmentById(appointmentId);
            if (appointment != null)
            {
                appointment.IsPaid = isPaid;
                appointment.UpdatedAt = DateTime.Now;
                _repository.UpdateAppointment(appointment);

                if (appointment.MasterInvoiceId.HasValue)
                {
                    using var context = new CarShowroomContext();
                    var masterInvoice = context.MasterInvoices.Find(appointment.MasterInvoiceId.Value);
                    if (masterInvoice != null)
                    {
                        masterInvoice.PaymentStatus = isPaid ? BusinessObjects.Common.PaymentStatuses.Paid : BusinessObjects.Common.PaymentStatuses.Unpaid;
                        if (isPaid)
                        {
                            masterInvoice.InvoiceStatus = BusinessObjects.Common.InvoiceStatuses.Completed;
                            masterInvoice.PaidAt = DateTime.Now;
                        }
                        masterInvoice.UpdatedAt = DateTime.Now;
                        context.SaveChanges();
                    }
                }
            }
        }

        public void UpdateAppointmentExtraFee(int appointmentId, decimal fee)
        {
            var appointment = _repository.GetAppointmentById(appointmentId);
            if (appointment != null)
            {
                string note = appointment.Note ?? "";
                // Xóa tag cũ nếu có
                note = System.Text.RegularExpressions.Regex.Replace(note, @"\[PhiPhatSinh:\s*\d+\]\n?", "").Trim();
                if (fee > 0)
                {
                    note += (note.Length > 0 ? "\n" : "") + $"[PhiPhatSinh: {fee}]";
                }
                appointment.Note = note;
                appointment.UpdatedAt = DateTime.Now;
                _repository.UpdateAppointment(appointment);
            }
        }

        public void DeleteAppointment(int appointmentId) => _repository.DeleteAppointment(appointmentId);

        private MaintenanceAppointmentDTO MapToDTO(MaintenanceAppointment appointment)
        {
            return new MaintenanceAppointmentDTO
            {
                AppointmentId = appointment.AppointmentId,
                CustomerId = appointment.CustomerId,
                CustomerCarId = appointment.CustomerCarId,
                CustomerName = appointment.CustomerName,
                CustomerPhone = appointment.CustomerPhone,
                CustomerEmail = appointment.CustomerEmail,
                AppointmentDate = appointment.AppointmentDate,
                AppointmentTime = appointment.AppointmentTime,
                Note = appointment.Note,
                Status = appointment.Status,
                IsPaid = appointment.IsPaid,
                CreatedAt = appointment.CreatedAt,
                CustomerCar = appointment.CustomerCar != null ? new CustomerCarDTO
                {
                    CustomerCarId = appointment.CustomerCar.CustomerCarId,
                    CustomerId = appointment.CustomerCar.CustomerId,
                    BrandId = appointment.CustomerCar.BrandId,
                    BrandName = appointment.CustomerCar.Brand?.BrandName,
                    Model = appointment.CustomerCar.Model,
                    Year = appointment.CustomerCar.Year,
                    VIN = appointment.CustomerCar.VIN,
                    LicensePlate = appointment.CustomerCar.LicensePlate,
                    Color = appointment.CustomerCar.Color
                } : null,
                Details = appointment.AppointmentDetails?.Select(d => new AppointmentDetailDTO
                {
                    AppointmentDetailId = d.AppointmentDetailId,
                    PackageId = d.PackageId,
                    PackageName = d.Package?.PackageName,
                    ServiceId = d.ServiceId,
                    ServiceName = d.Service?.ServiceName,
                    UnitPrice = d.UnitPrice,
                    Quantity = d.Quantity,
                    PackageServices = d.Package?.PackageServices?.Select(ps => new ServiceSummaryDTO
                    {
                        ServiceId = ps.Service.ServiceId,
                        ServiceName = ps.Service.ServiceName,
                        BasePrice = ps.Service.BasePrice,
                        EstimatedDurationMinutes = ps.Service.EstimatedDurationMinutes
                    }).ToList() ?? new List<ServiceSummaryDTO>()
                }).ToList() ?? new List<AppointmentDetailDTO>(),
                ConsumedParts = appointment.ConsumedParts?.Select(cp => new ConsumedPartDTO
                {
                    ConsumedPartId = cp.ConsumedPartId,
                    PartId = cp.PartId,
                    PartName = cp.Part?.PartName,
                    Quantity = cp.Quantity,
                    UnitPrice = cp.UnitPrice,
                    IsIncurred = cp.IsIncurred,
                    ApprovedByCustomer = cp.ApprovedByCustomer,
                    Notes = cp.Notes
                }).ToList() ?? new List<ConsumedPartDTO>()
            };
        }
    }
}
