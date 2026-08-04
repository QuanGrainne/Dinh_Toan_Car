using Microsoft.AspNetCore.Mvc;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using Services;
using BusinessObjects.Models;
using Microsoft.Extensions.Configuration;

using System.Text.Json.Serialization;

namespace CarSalesManagementSystemAPI.Controllers
{
    // ╔══════════════════════════════════════════════════════════════╗
    // ║  ARCHITECTURE INTENT
    // ╠══════════════════════════════════════════════════════════════╣
    // ║  Problem Domain   : Car image upload detection and search
    // ║  Design Pattern   : Gateway / Proxy pattern to Python microservice
    // ║  Data Flow        : Frontend -> API Gateway (C#) -> Microservice (Python) -> C# SQL DB lookup -> Client
    // ║  Boundary         : API Gateway delegates heavy ViT inference & Vector DB lookup to Python, returns hydrated C# Entities
    // ║  Failure Modes    : Fallback on connection errors, logs details, surfaces typed errors
    // ╚══════════════════════════════════════════════════════════════╝

    [ApiController]
    [Route("api/[controller]")]
    public class CarDetectionController : ControllerBase
    {
        private readonly ICarService _carService;
        private readonly HttpClient _httpClient;
        private readonly string _pythonServiceUrl;

        public CarDetectionController(ICarService carService, IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _carService = carService ?? throw new ArgumentNullException(nameof(carService));
            _httpClient = httpClientFactory.CreateClient("CarDetectionClient");
            _pythonServiceUrl = (configuration["PythonService:BaseUrl"] ?? "http://localhost:5005").TrimEnd('/');
        }

        // ─── FLOW ────────────────────────────────────────────────────────────────────
        //
        //  [Request POST] → DetectCar(file)
        //      │
        //      ├─ [Validation] → Check if file is null or empty
        //      │
        //      ├─ [Proxy HTTP] → Stream file to PythonService /detect
        //      │                    └─ Get JSON (predicted_label, matched_car_ids)
        //      │
        //      ├─ [Hydration] → Query SQL Server for matching Car records by IDs
        //      │
        //      └─ [Response] → Return predicted details and hydrated list of Cars
        //
        //  Invariants:
        //    - Does not store uploaded files permanently on API server to save space
        //    - Handles Python service outage gracefully (returns 503 Service Unavailable)
        //
        // ─────────────────────────────────────────────────────────────────────────────
        [HttpPost("detect")]
        public async Task<IActionResult> DetectCar(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest(new { message = "Vui lòng chọn một file ảnh ô tô hợp lệ." });
            }

            try
            {
                using var form = new MultipartFormDataContent();
                using var fileStream = file.OpenReadStream();
                using var streamContent = new StreamContent(fileStream);
                streamContent.Headers.ContentType = MediaTypeHeaderValue.Parse(file.ContentType);
                form.Add(streamContent, "file", file.FileName);

                var pythonDetectUrl = $"{_pythonServiceUrl}/detect";
                var response = await _httpClient.PostAsync(pythonDetectUrl, form);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    return StatusCode((int)response.StatusCode, new { message = "Lỗi từ dịch vụ nhận diện: " + errorContent });
                }

                var jsonResponse = await response.Content.ReadAsStringAsync();
                var detectionResult = JsonSerializer.Deserialize<DetectionResultDto>(jsonResponse, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (detectionResult == null)
                {
                    return StatusCode(502, new { message = "Dịch vụ nhận diện trả về dữ liệu không hợp lệ." });
                }

                var matchedCars = new List<Car>();
                if (detectionResult.MatchedCarIds != null && detectionResult.MatchedCarIds.Any())
                {
                    var allCars = _carService.GetAllCars().ToList();
                    matchedCars = allCars
                        .Where(c => detectionResult.MatchedCarIds.Contains(c.CarId) && c.Status != "Inactive")
                        .ToList();
                }

                return Ok(new
                {
                    predictedLabel = detectionResult.PredictedLabel,
                    confidence = detectionResult.Confidence,
                    matchedCars = matchedCars
                });
            }
            catch (HttpRequestException ex)
            {
                return StatusCode(503, new { message = "Không thể kết nối tới dịch vụ nhận diện AI. Chi tiết: " + ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi hệ thống khi phân tích ảnh: " + ex.Message });
            }
        }

        // ─── FLOW ────────────────────────────────────────────────────────────────────
        //
        //  [Request POST] → SyncDatabase()
        //      │
        //      ├─ [Proxy HTTP] → Call PythonService /sync
        //      │
        //      └─ [Response] → Return status of synchronization
        //
        // ─────────────────────────────────────────────────────────────────────────────
        [HttpPost("sync")]
        public async Task<IActionResult> SyncDatabase()
        {
            try
            {
                var pythonSyncUrl = $"{_pythonServiceUrl}/sync";
                var response = await _httpClient.PostAsync(pythonSyncUrl, null);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    return StatusCode((int)response.StatusCode, new { message = "Lỗi khi đồng bộ Vector DB: " + errorContent });
                }

                var jsonResponse = await response.Content.ReadAsStringAsync();
                return Ok(JsonSerializer.Deserialize<object>(jsonResponse));
            }
            catch (HttpRequestException ex)
            {
                return StatusCode(503, new { message = "Không thể kết nối tới dịch vụ nhận diện AI để đồng bộ. Chi tiết: " + ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi hệ thống khi đồng bộ dữ liệu: " + ex.Message });
            }
        }
    }

    public class DetectionResultDto
    {
        [JsonPropertyName("predicted_label")]
        public string PredictedLabel { get; set; } = string.Empty;

        [JsonPropertyName("confidence")]
        public double Confidence { get; set; }

        [JsonPropertyName("matched_car_ids")]
        public List<int> MatchedCarIds { get; set; } = new();
    }
}
