using System.Collections.Generic;
using BusinessObjects.Models;
using Repositories;

namespace Services
{
    public class PartService : IPartService
    {
        private readonly IPartRepository _repository;

        public PartService(IPartRepository repository)
        {
            _repository = repository;
        }

        public IEnumerable<Part> GetAllParts() => _repository.GetAllParts();

        public Part? GetPartById(int partId) => _repository.GetPartById(partId);

        public void AddPart(Part part)
        {
            if (string.IsNullOrWhiteSpace(part.PartName))
            {
                throw new InvalidOperationException("Tên phụ tùng không được để trống.");
            }
            if (string.IsNullOrWhiteSpace(part.PartCode))
            {
                throw new InvalidOperationException("Mã phụ tùng không được để trống.");
            }

            var name = part.PartName.Trim();
            var code = part.PartCode.Trim();
            var parts = _repository.GetAllParts();

            if (parts.Any(p => p.PartName.Trim().Equals(name, StringComparison.OrdinalIgnoreCase)))
            {
                throw new InvalidOperationException("Tên phụ tùng đã tồn tại trong hệ thống. Vui lòng nhập tên khác.");
            }
            if (parts.Any(p => p.PartCode.Trim().Equals(code, StringComparison.OrdinalIgnoreCase)))
            {
                throw new InvalidOperationException("Mã phụ tùng đã tồn tại trong hệ thống. Vui lòng nhập mã khác.");
            }

            part.PartName = name;
            part.PartCode = code;
            _repository.AddPart(part);
        }

        public bool HasTransactions(int partId)
        {
            return false;
        }

        public void UpdatePartMetadata(BusinessObjects.ViewModels.UpdatePartViewModel model, int? adminId)
        {
            using var context = new DataAccessObjects.CarShowroomContext();
            var part = context.Parts.SingleOrDefault(p => p.PartId == model.PartId);
            if (part == null)
            {
                throw new InvalidOperationException("Không tìm thấy phụ tùng cần cập nhật.");
            }

            if (string.IsNullOrWhiteSpace(model.PartName))
            {
                throw new InvalidOperationException("Tên phụ tùng không được để trống.");
            }
            var name = model.PartName.Trim();
            if (context.Parts.Any(p => p.PartId != model.PartId && p.PartName.Trim().ToLower() == name.ToLower()))
            {
                throw new InvalidOperationException("Tên phụ tùng đã tồn tại trong hệ thống. Vui lòng nhập tên khác.");
            }

            if (model.CategoryId <= 0 || !context.PartCategories.Any(c => c.CategoryId == model.CategoryId))
            {
                throw new InvalidOperationException("Danh mục phụ tùng không tồn tại.");
            }

            if (model.Price < 0)
            {
                throw new InvalidOperationException("Giá bán không được âm.");
            }

            bool hasTx = false;
            if (!hasTx && !string.IsNullOrWhiteSpace(model.PartCode))
            {
                var code = model.PartCode.Trim().ToUpperInvariant();
                if (context.Parts.Any(p => p.PartId != model.PartId && p.PartCode.Trim().ToUpper() == code))
                {
                    throw new InvalidOperationException("Mã phụ tùng đã tồn tại trong hệ thống. Vui lòng nhập mã khác.");
                }
                part.PartCode = code;
            }

            // Simple status: Available or Inactive only
            part.Status = model.Status == "Inactive" ? "Inactive" : "Available";

            part.PartName = name;
            part.CategoryId = model.CategoryId;
            part.Brand = string.IsNullOrWhiteSpace(model.Brand) ? null : model.Brand.Trim();
            part.Price = model.Price;
            part.Description = string.IsNullOrWhiteSpace(model.Description) ? null : model.Description.Trim();
            part.ImageUrl = string.IsNullOrWhiteSpace(model.ImageUrl) ? null : model.ImageUrl.Trim();
            part.UpdatedAt = DateTime.Now;
            if (adminId.HasValue && adminId.Value > 0)
            {
                part.UpdatedUser = adminId.Value;
            }

            context.SaveChanges();
        }

        public void UpdatePart(Part part)
        {
            if (string.IsNullOrWhiteSpace(part.PartName))
            {
                throw new InvalidOperationException("Tên phụ tùng không được để trống.");
            }
            if (string.IsNullOrWhiteSpace(part.PartCode))
            {
                throw new InvalidOperationException("Mã phụ tùng không được để trống.");
            }

            var name = part.PartName.Trim();
            var code = part.PartCode.Trim();
            var parts = _repository.GetAllParts();

            if (parts.Any(p => p.PartId != part.PartId && p.PartName.Trim().Equals(name, StringComparison.OrdinalIgnoreCase)))
            {
                throw new InvalidOperationException("Tên phụ tùng đã tồn tại trong hệ thống. Vui lòng nhập tên khác.");
            }
            if (parts.Any(p => p.PartId != part.PartId && p.PartCode.Trim().Equals(code, StringComparison.OrdinalIgnoreCase)))
            {
                throw new InvalidOperationException("Mã phụ tùng đã tồn tại trong hệ thống. Vui lòng nhập mã khác.");
            }

            part.PartName = name;
            part.PartCode = code;
            _repository.UpdatePart(part);
        }

        public void DeletePart(int partId)
        {
            var part = _repository.GetPartById(partId);
            if (part == null)
            {
                throw new InvalidOperationException("Không tìm thấy phụ tùng cần xóa.");
            }
            try
            {
                _repository.DeletePart(partId);
            }
            catch (Exception)
            {
                throw new InvalidOperationException("Không thể xóa phụ tùng này vì đã tồn tại trong lịch sử đơn hàng.");
            }
        }

        public IEnumerable<Part> GetPartsFiltered(int categoryId) => _repository.GetPartsFiltered(categoryId);
    }
}

