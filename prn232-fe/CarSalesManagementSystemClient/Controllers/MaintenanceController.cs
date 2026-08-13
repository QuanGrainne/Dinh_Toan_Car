using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using CarSalesManagementSystemClient.Models;

namespace CarSalesManagementSystemClient.Controllers
{
    public class MaintenanceController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiUrl = "http://localhost:5084/api";

        public MaintenanceController(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient();
        }

        // Helper classes to deserialize API wrapper
        private class ApiResponse<T>
        {
            public bool Success { get; set; }
            public string Message { get; set; }
            public T Data { get; set; }
        }

        // GET: /Maintenance/
        public async Task<IActionResult> Index(MaintenancePackageSearchViewModel filter)
        {
            var viewModel = new MaintenanceIndexViewModel 
            { 
                CurrentPage = filter.PageNumber,
                Filter = filter 
            };

            try
            {
                var requestUri = $"{_apiUrl}/maintenancepackages/available";
                var apiResponse = await _httpClient.GetFromJsonAsync<ApiResponse<List<MaintenancePackageViewModel>>>(requestUri);

                var allPackages = apiResponse?.Data ?? new List<MaintenancePackageViewModel>();

                // Apply filters
                if (filter.MinPrice.HasValue)
                    allPackages = allPackages.Where(p => p.PackagePrice >= filter.MinPrice.Value).ToList();
                if (filter.MaxPrice.HasValue)
                    allPackages = allPackages.Where(p => p.PackagePrice <= filter.MaxPrice.Value).ToList();
                if (filter.MaxDuration.HasValue)
                    allPackages = allPackages.Where(p => p.TotalDurationMinutes <= filter.MaxDuration.Value).ToList();
                if (!string.IsNullOrEmpty(filter.SearchTerm))
                    allPackages = allPackages.Where(p => p.PackageName?.Contains(filter.SearchTerm, StringComparison.OrdinalIgnoreCase) == true).ToList();

                // Apply sorting
                if (!string.IsNullOrEmpty(filter.SortBy))
                {
                    allPackages = filter.SortBy.ToLower() switch
                    {
                        "priceasc" => allPackages.OrderBy(p => p.PackagePrice).ToList(),
                        "pricedesc" => allPackages.OrderByDescending(p => p.PackagePrice).ToList(),
                        "durationasc" => allPackages.OrderBy(p => p.TotalDurationMinutes).ToList(),
                        _ => allPackages
                    };
                }

                int totalItems = allPackages.Count;
                var skip = (filter.PageNumber - 1) * filter.PageSize;
                viewModel.Packages = allPackages.Skip(skip).Take(filter.PageSize).ToList();
                int totalItemsForPagination = totalItems;
                viewModel.TotalPages = (int)Math.Ceiling((double)totalItemsForPagination / filter.PageSize);
                if (viewModel.TotalPages == 0) viewModel.TotalPages = 1;
                if (viewModel.CurrentPage > viewModel.TotalPages) viewModel.CurrentPage = viewModel.TotalPages;
                if (viewModel.CurrentPage < 1) viewModel.CurrentPage = 1;
            }
            catch
            {
                viewModel.Packages = new List<MaintenancePackageViewModel>();
            }

            return View(viewModel);
        }

        // GET: /Maintenance/Services
        public async Task<IActionResult> Services(string? searchTerm, decimal? minPrice, decimal? maxPrice, int pageNumber = 1)
        {
            int pageSize = 12;
            var viewModel = new ServicesIndexViewModel
            {
                SearchTerm = searchTerm,
                MinPrice = minPrice,
                MaxPrice = maxPrice,
                PageNumber = pageNumber,
                PageSize = pageSize
            };

            try
            {
                var response = await _httpClient.GetAsync($"{_apiUrl}/Services/available");
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var apiResult = JsonSerializer.Deserialize<ApiResponse<List<ServiceSummaryViewModel>>>(content,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                    if (apiResult?.Success == true && apiResult.Data != null)
                    {
                        var allServices = apiResult.Data;

                        // Filter
                        if (!string.IsNullOrEmpty(searchTerm))
                            allServices = allServices.Where(s => s.ServiceName.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)).ToList();
                        if (minPrice.HasValue)
                            allServices = allServices.Where(s => s.BasePrice >= minPrice.Value).ToList();
                        if (maxPrice.HasValue)
                            allServices = allServices.Where(s => s.BasePrice <= maxPrice.Value).ToList();

                        int totalItems = allServices.Count;
                        viewModel.TotalPages = (int)Math.Ceiling(totalItems / (double)pageSize);
                        if (viewModel.TotalPages == 0) viewModel.TotalPages = 1;
                        if (pageNumber > viewModel.TotalPages) pageNumber = viewModel.TotalPages;
                        if (pageNumber < 1) pageNumber = 1;
                        viewModel.PageNumber = pageNumber;

                        viewModel.Services = allServices.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();
                    }
                }
            }
            catch
            {
                viewModel.Services = new List<ServiceSummaryViewModel>();
            }

            return View(viewModel);
        }
    }
}
