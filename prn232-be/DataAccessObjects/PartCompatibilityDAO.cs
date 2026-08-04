using System;
using System.Collections.Generic;
using System.Linq;
using BusinessObjects.Models;
using BusinessObjects.DTOs;
using Microsoft.EntityFrameworkCore;

namespace DataAccessObjects
{
    public class PartCompatibilityDAO
    {
        private static PartCompatibilityDAO? instance = null;
        private static readonly object instanceLock = new object();

        private PartCompatibilityDAO() { }

        public static PartCompatibilityDAO Instance
        {
            get
            {
                lock (instanceLock)
                {
                    if (instance == null)
                    {
                        instance = new PartCompatibilityDAO();
                    }
                    return instance;
                }
            }
        }

        public CompatibilityResultDto CheckCompatibility(string licensePlate, string partCode)
        {
            using var context = new CarShowroomContext();
            
            // 1. Get Part details
            var part = context.Parts.SingleOrDefault(p => p.PartCode.ToLower() == partCode.ToLower());
            if (part == null)
            {
                return new CompatibilityResultDto
                {
                    PartCode = partCode,
                    PartName = "Không xác định",
                    IsCompatible = false,
                    Message = $"Phụ tùng với mã '{partCode}' không tồn tại trong hệ thống."
                };
            }

            // 2. Get Customer Car details
            var car = context.CustomerCars
                .Include(c => c.Brand)
                .SingleOrDefault(c => c.LicensePlate.ToLower() == licensePlate.ToLower());
                
            if (car == null)
            {
                return new CompatibilityResultDto
                {
                    PartCode = partCode,
                    PartName = part.PartName,
                    IsCompatible = false,
                    Message = $"Không tìm thấy xe của khách hàng với biển số '{licensePlate}'."
                };
            }

            // 3. Find if there is any matching compatibility configuration
            var compatibilities = context.PartCompatibilities
                .Where(pc => pc.PartId == part.PartId && pc.BrandId == car.BrandId)
                .ToList();

            bool isCompatible = false;
            string matchMessage = "";

            foreach (var comp in compatibilities)
            {
                // Check model name match (Vios matches Toyota Vios 1.5G, Camry matches Camry, etc.)
                bool modelMatch = car.Model.Contains(comp.ModelName, StringComparison.OrdinalIgnoreCase) 
                               || comp.ModelName.Contains(car.Model, StringComparison.OrdinalIgnoreCase);

                if (modelMatch)
                {
                    // Check year range
                    bool yearMatch = true;
                    int carYear = car.Year ?? 0;
                    if (carYear > 0)
                    {
                        if (comp.YearFrom.HasValue && carYear < comp.YearFrom.Value)
                            yearMatch = false;
                        if (comp.YearTo.HasValue && carYear > comp.YearTo.Value)
                            yearMatch = false;
                    }

                    if (yearMatch)
                    {
                        isCompatible = true;
                        matchMessage = $"Phụ tùng tương thích hoàn toàn với dòng xe {car.Brand.BrandName} {car.Model} {car.Year} của khách hàng.";
                        break;
                    }
                }
            }

            if (!isCompatible)
            {
                matchMessage = $"Phụ tùng không tương thích hoặc chưa được cấu hình cho dòng xe {car.Brand.BrandName} {car.Model} {car.Year} của khách hàng.";
            }

            return new CompatibilityResultDto
            {
                PartCode = part.PartCode,
                PartName = part.PartName,
                IsCompatible = isCompatible,
                Message = matchMessage
            };
        }
    }
}
