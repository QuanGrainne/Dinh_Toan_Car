using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Text.Json;
using System.Text;
using CarSalesManagementSystemClient.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace CarSalesManagementSystemClient.Controllers
{
    public class PartOrdersController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly string _ordersApiUrl = "http://localhost:5084/api/PartOrders";


        public PartOrdersController(IHttpClientFactory httpClientFactory)
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



        // GET: PartOrders/MyOrders
        [Authorize]
        public IActionResult MyOrders()
        {
            return RedirectToAction("Index", "Orders");
        }

        // GET: PartOrders/Manage (Admin only)
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Manage()
        {
            try
            {
                AppendAuthorizationHeader();
                var requestUri = $"{_ordersApiUrl}?$expand=Customer,PartOrderDetails($expand=Part)&$orderby=CreatedAt desc";
                var response = await _httpClient.GetFromJsonAsync<List<PartOrderViewModel>>(requestUri);

                return View(response ?? new List<PartOrderViewModel>());
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = "Không thể tải danh sách đơn hàng quản trị: " + ex.Message;
                return View(new List<PartOrderViewModel>());
            }
        }

        // POST: PartOrders/UpdateStatus (Admin / Customer Cancel)
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> UpdateStatus(int id, string status)
        {
            try
            {
                AppendAuthorizationHeader();
                var payload = new { OrderId = id, Status = status };
                
                var response = await _httpClient.PutAsJsonAsync($"{_ordersApiUrl}/{id}", payload);
                if (response.IsSuccessStatusCode)
                {
                    string message = status == "Cancelled" ? "Đã hủy đơn hàng thành công!" : $"Cập nhật trạng thái đơn hàng sang '{status}' thành công!";
                    return Json(new { success = true, message });
                }

                string errMsg = await ExtractErrorMessageAsync(response, "Không thể cập nhật đơn hàng.");
                return Json(new { success = false, message = errMsg });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi kết nối: " + ex.Message });
            }
        }

        private async Task<string> ExtractErrorMessageAsync(HttpResponseMessage response, string defaultMessage)
        {
            try
            {
                var errContent = await response.Content.ReadFromJsonAsync<JsonElement>();
                if (errContent.TryGetProperty("message", out var msgProp))
                {
                    return msgProp.GetString()!;
                }
                if (errContent.TryGetProperty("errors", out var errorsProp) && errorsProp.ValueKind == JsonValueKind.Object)
                {
                    var errorsList = new List<string>();
                    foreach (var prop in errorsProp.EnumerateObject())
                    {
                        foreach (var err in prop.Value.EnumerateArray())
                        {
                            errorsList.Add(err.GetString()!);
                        }
                    }
                    if (errorsList.Any())
                    {
                        return string.Join("<br/>", errorsList);
                    }
                }
            }
            catch
            {
                try
                {
                    var rawStr = await response.Content.ReadAsStringAsync();
                    if (!string.IsNullOrWhiteSpace(rawStr) && rawStr.Length < 200)
                    {
                        return rawStr;
                    }
                }
                catch { }
            }
            return defaultMessage;
        }
    }
}
