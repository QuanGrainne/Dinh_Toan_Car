using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Text.Json;
using CarSalesManagementSystemClient.Models;
using Microsoft.AspNetCore.Authorization;

namespace CarSalesManagementSystemClient.Controllers
{
    public class PartsController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly string _partsApiUrl = "http://localhost:5084/api/Parts";
        private readonly string _categoriesApiUrl = "http://localhost:5084/api/PartCategories";

        public PartsController(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient();
        }

        private void AppendAuthorizationHeader()
        {
            var token = User.FindFirst("jwt_token")?.Value;
            if (!string.IsNullOrEmpty(token))
            {
                _httpClient.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }
        }

        // GET: Parts (Customer catalog)
        public async Task<IActionResult> Index(PartSearchViewModel filter)
        {
            try
            {
                var categories = await _httpClient.GetFromJsonAsync<IEnumerable<PartCategoryViewModel>>(_categoriesApiUrl);
                ViewBag.Categories = categories ?? new List<PartCategoryViewModel>();

                var odataParams = new List<string>();
                var filters = new List<string>();
                filters.Add("Status ne 'Inactive'");

                if (filter.CategoryId.HasValue)
                    filters.Add($"CategoryId eq {filter.CategoryId.Value}");
                if (filter.MinPrice.HasValue)
                    filters.Add($"Price ge {filter.MinPrice.Value}");
                if (filter.MaxPrice.HasValue)
                    filters.Add($"Price le {filter.MaxPrice.Value}");
                if (!string.IsNullOrEmpty(filter.SearchTerm))
                {
                    var term = Uri.EscapeDataString(filter.SearchTerm.ToLower());
                    filters.Add($"(contains(tolower(PartName), '{term}') or contains(tolower(PartCode), '{term}') or contains(tolower(Brand), '{term}'))");
                }

                odataParams.Add($"$filter={string.Join(" and ", filters)}");

                var sortExpr = filter.SortBy?.ToLower() switch
                {
                    "priceasc" => "Price asc",
                    "pricedesc" => "Price desc",
                    "nameasc" => "PartName asc",
                    _ => "CreatedAt desc"
                };
                odataParams.Add($"$orderby={sortExpr}");

                var skip = (filter.PageNumber - 1) * filter.PageSize;
                odataParams.Add($"$skip={skip}");
                odataParams.Add($"$top={filter.PageSize}");
                odataParams.Add("$count=true");
                odataParams.Add("$expand=Category");

                var requestUri = "http://localhost:5084/odata/Parts?" + string.Join("&", odataParams);
                var odataResponse = await _httpClient.GetFromJsonAsync<ODataResponse<PartViewModel>>(requestUri);

                var totalCount = (int)(odataResponse?.Count ?? 0);
                var parts = odataResponse?.Value ?? new List<PartViewModel>();
                var totalPages = (int)Math.Ceiling((double)totalCount / filter.PageSize);

                var pagedResult = new PagedResultViewModel<PartViewModel>
                {
                    Items = parts,
                    TotalItems = totalCount,
                    PageNumber = filter.PageNumber,
                    PageSize = filter.PageSize,
                    TotalPages = totalPages,
                    HasPreviousPage = filter.PageNumber > 1,
                    HasNextPage = filter.PageNumber < totalPages
                };

                ViewBag.Filter = filter;
                return View(pagedResult);
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = "Không thể tải danh sách phụ tùng: " + ex.Message;
                return View(new PagedResultViewModel<PartViewModel>());
            }
        }

        // GET: Parts/Details/5
        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var requestUri = $"http://localhost:5084/odata/Parts?$filter=PartId eq {id}&$expand=Category&$top=1";
                var odataResponse = await _httpClient.GetFromJsonAsync<ODataResponse<PartViewModel>>(requestUri);
                var part = odataResponse?.Value?.FirstOrDefault();

                if (part == null)
                {
                    var directPart = await _httpClient.GetFromJsonAsync<PartViewModel>($"{_partsApiUrl}/{id}");
                    if (directPart == null) return NotFound();
                    return View(directPart);
                }
                return View(part);
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = "Khong the tai thong tin phu tung: " + ex.Message;
                return RedirectToAction("Index");
            }
        }

        // GET: Parts/Manage (Admin CRUD)
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Manage()
        {
            try
            {
                var categories = await _httpClient.GetFromJsonAsync<IEnumerable<PartCategoryViewModel>>(_categoriesApiUrl);
                ViewBag.Categories = categories ?? new List<PartCategoryViewModel>();

                var requestUri = "http://localhost:5084/odata/Parts?$expand=Category&$orderby=CreatedAt desc";
                var odataResponse = await _httpClient.GetFromJsonAsync<ODataResponse<PartViewModel>>(requestUri);

                return View(odataResponse?.Value ?? new List<PartViewModel>());
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = "Loi khi tai trang quan tri phu tung: " + ex.Message;
                ViewBag.Categories = new List<PartCategoryViewModel>();
                return View(new List<PartViewModel>());
            }
        }

        // POST: Parts/UploadMultipleImages
        [HttpPost]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> UploadMultipleImages(List<IFormFile> files)
        {
            if (files == null || !files.Any())
                return Json(new { success = false, message = "Vui long chon it nhat 1 tap tin anh." });

            try
            {
                var uploadsFolder = System.IO.Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "parts");
                if (!Directory.Exists(uploadsFolder))
                    Directory.CreateDirectory(uploadsFolder);

                var imageUrls = new List<string>();
                foreach (var file in files)
                {
                    if (file != null && file.Length > 0)
                    {
                        var fileName = Guid.NewGuid().ToString("N") + System.IO.Path.GetExtension(file.FileName);
                        var filePath = System.IO.Path.Combine(uploadsFolder, fileName);
                        using var stream = new System.IO.FileStream(filePath, System.IO.FileMode.Create);
                        await file.CopyToAsync(stream);
                        imageUrls.Add("/uploads/parts/" + fileName);
                    }
                }
                return Json(new { success = true, imageUrls = imageUrls, joinedUrl = string.Join(",", imageUrls) });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Loi khi tai anh len: " + ex.Message });
            }
        }

        // GET: Parts/GetPartJson/5
        [HttpGet]
        public async Task<IActionResult> GetPartJson(int id)
        {
            try
            {
                var part = await _httpClient.GetFromJsonAsync<PartViewModel>($"{_partsApiUrl}/{id}");
                if (part == null) return NotFound();

                return Json(new {
                    partId = part.PartId,
                    partName = part.PartName,
                    partCode = part.PartCode,
                    categoryId = part.CategoryId,
                    brand = part.Brand,
                    price = part.Price,
                    description = part.Description,
                    imageUrl = part.ImageUrl,
                    status = part.Status
                });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // POST: Parts/Save (AJAX POST - Create or Update)
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Save(UpdatePartViewModel model)
        {
            try
            {
                AppendAuthorizationHeader();
                HttpResponseMessage response;

                var partPayload = new {
                    PartId = model.PartId,
                    PartName = model.PartName?.Trim(),
                    PartCode = model.PartCode?.Trim(),
                    CategoryId = model.CategoryId,
                    Brand = model.Brand?.Trim(),
                    Price = model.Price,
                    Quantity = 100,
                    MinStockLevel = 5,
                    MaxStockLevel = 500,
                    UnitOfMeasure = "Cai",
                    Description = model.Description?.Trim(),
                    ImageUrl = model.ImageUrl?.Trim(),
                    Status = "Available"
                };

                if (model.PartId == 0)
                    response = await _httpClient.PostAsJsonAsync(_partsApiUrl, partPayload);
                else
                    response = await _httpClient.PutAsJsonAsync($"{_partsApiUrl}/{model.PartId}", partPayload);

                if (response.IsSuccessStatusCode)
                {
                    string msg = model.PartId == 0 ? "Them phu tung moi thanh cong!" : "Cap nhat thong tin phu tung thanh cong!";
                    return Json(new { success = true, message = msg });
                }

                string errMsg = await ExtractErrorMessageAsync(response, "Da xay ra loi tren Server.");
                return Json(new { success = false, message = errMsg });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Loi ket noi: " + ex.Message });
            }
        }

        // POST: Parts/Delete/5
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                AppendAuthorizationHeader();
                var response = await _httpClient.DeleteAsync($"{_partsApiUrl}/{id}");

                if (response.IsSuccessStatusCode)
                    return Json(new { success = true, message = "Xoa phu tung thanh cong!" });

                string errMsg = await ExtractErrorMessageAsync(response, "Khong the xoa phu tung.");
                return Json(new { success = false, message = errMsg });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Loi ket noi: " + ex.Message });
            }
        }

        // POST: Parts/CreateCategory
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateCategory([FromBody] object payload)
        {
            try
            {
                AppendAuthorizationHeader();
                var response = await _httpClient.PostAsJsonAsync("http://localhost:5084/api/PartCategories", payload);
                if (response.IsSuccessStatusCode)
                    return Json(new { success = true, data = await response.Content.ReadFromJsonAsync<object>() });

                var errMsg = await ExtractErrorMessageAsync(response, "Them danh muc that bai.");
                return BadRequest(new { message = errMsg });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Loi ket noi: " + ex.Message });
            }
        }

        // POST: Parts/UpdateCategory/5
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateCategory(int id, [FromBody] object payload)
        {
            try
            {
                AppendAuthorizationHeader();
                var response = await _httpClient.PutAsJsonAsync($"http://localhost:5084/api/PartCategories/{id}", payload);
                if (response.IsSuccessStatusCode)
                    return Json(new { success = true, data = await response.Content.ReadFromJsonAsync<object>() });

                var errMsg = await ExtractErrorMessageAsync(response, "Cap nhat danh muc that bai.");
                return BadRequest(new { message = errMsg });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Loi ket noi: " + ex.Message });
            }
        }

        private async Task<string> ExtractErrorMessageAsync(HttpResponseMessage response, string defaultMessage)
        {
            try
            {
                var errContent = await response.Content.ReadFromJsonAsync<JsonElement>();
                if (errContent.TryGetProperty("message", out var msgProp))
                    return msgProp.GetString()!;
                if (errContent.TryGetProperty("errors", out var errorsProp) && errorsProp.ValueKind == JsonValueKind.Object)
                {
                    var errorsList = new List<string>();
                    foreach (var prop in errorsProp.EnumerateObject())
                        foreach (var err in prop.Value.EnumerateArray())
                            errorsList.Add(err.GetString()!);
                    if (errorsList.Any()) return string.Join("<br/>", errorsList);
                }
            }
            catch
            {
                try
                {
                    var rawStr = await response.Content.ReadAsStringAsync();
                    if (!string.IsNullOrWhiteSpace(rawStr) && rawStr.Length < 200)
                        return rawStr;
                }
                catch { }
            }
            return defaultMessage;
        }
    }
}
