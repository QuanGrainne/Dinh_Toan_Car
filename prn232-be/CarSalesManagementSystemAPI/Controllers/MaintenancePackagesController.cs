using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using BusinessObjects.Models;
using BusinessObjects.DTOs;
using Services;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using Microsoft.AspNetCore.OData.Formatter;

namespace CarSalesManagementSystemAPI.Controllers
{
    [Route("api/[controller]")]
    public class MaintenancePackagesController : ODataController
    {
        private readonly IMaintenancePackageService _service;

        public MaintenancePackagesController(IMaintenancePackageService service)
        {
            _service = service;
        }

        [HttpGet]
        public ActionResult<ApiResponse<IEnumerable<MaintenancePackageDTO>>> Get()
        {
            var packages = _service.GetAllPackages().ToList();
            return Ok(new ApiResponse<IEnumerable<MaintenancePackageDTO>>(true, "Lấy danh sách thành công", packages));
        }

        [HttpGet("available")]
        public ActionResult<ApiResponse<IEnumerable<MaintenancePackageDTO>>> GetAvailable()
        {
            var packages = _service.GetAvailablePackages().ToList();
            return Ok(new ApiResponse<IEnumerable<MaintenancePackageDTO>>(true, "Lấy danh sách thành công", packages));
        }

        [HttpGet("{id}")]
        public ActionResult<ApiResponse<MaintenancePackageDTO>> Get(int id)
        {
            var package = _service.GetPackageById(id);
            if (package == null)
            {
                return NotFound(new ApiResponse<MaintenancePackageDTO>(false, "Không tìm thấy gói bảo dưỡng"));
            }
            return Ok(new ApiResponse<MaintenancePackageDTO>(true, "Lấy chi tiết thành công", package));
        }

        [HttpGet("/odata/MaintenancePackages")]
        [EnableQuery]
        public ActionResult<IQueryable<MaintenancePackage>> GetOData()
        {
            return Ok(DataAccessObjects.MaintenancePackageDAO.Instance.GetAllPackages().AsQueryable());
        }

        [HttpPost]
        public ActionResult<ApiResponse<MaintenancePackageDTO>> Post([FromBody] MaintenancePackageDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new ApiResponse<object>(false, "Dữ liệu không hợp lệ", ModelState));

            _service.AddPackage(dto);
            return Ok(new ApiResponse<string>(true, "Thêm thành công"));
        }

        [HttpPut("{id}")]
        public ActionResult<ApiResponse<string>> Put(int id, [FromBody] MaintenancePackageDTO dto)
        {
            if (id != dto.PackageId)
            {
                return BadRequest(new ApiResponse<string>(false, "ID không hợp lệ"));
            }

            var package = _service.GetPackageById(id);
            if (package == null) return NotFound(new ApiResponse<string>(false, "Không tìm thấy"));

            _service.UpdatePackage(dto);
            return Ok(new ApiResponse<string>(true, "Cập nhật thành công"));
        }

        [HttpDelete("{id}")]
        public ActionResult<ApiResponse<string>> Delete(int id)
        {
            try
            {
                _service.DeletePackage(id);
                return Ok(new ApiResponse<string>(true, "Xóa thành công"));
            }
            catch (Microsoft.EntityFrameworkCore.DbUpdateException)
            {
                return BadRequest(new ApiResponse<string>(false, "Không thể xóa gói bảo dưỡng này vì đã có khách hàng đặt lịch. Hãy cân nhắc chuyển trạng thái sang 'Ngừng cung cấp'."));
            }
            catch (System.Exception ex)
            {
                return BadRequest(new ApiResponse<string>(false, ex.Message));
            }
        }
    }
}
