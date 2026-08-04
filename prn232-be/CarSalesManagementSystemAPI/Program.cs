using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Repositories;
using Services;
using Microsoft.AspNetCore.OData;
using Microsoft.OData.ModelBuilder;
using Microsoft.EntityFrameworkCore;

namespace CarSalesManagementSystemAPI
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddHttpClient();
            builder.Services.AddScoped<IAppUserRepository, AppUserRepository>();
            builder.Services.AddScoped<IAppRoleRepository, AppRoleRepository>();
            builder.Services.AddScoped<IEmailService, EmailService>();
            builder.Services.AddScoped<IAuthService, AuthService>();
            builder.Services.AddScoped<IMaintenancePackageRepository, MaintenancePackageRepository>();
            builder.Services.AddScoped<IMaintenancePackageService, MaintenancePackageService>();
            builder.Services.AddScoped<IMaintenanceAppointmentRepository, MaintenanceAppointmentRepository>();
            builder.Services.AddScoped<IMaintenanceAppointmentService, MaintenanceAppointmentService>();
            builder.Services.AddScoped<IServiceRepository, ServiceRepository>();
            builder.Services.AddScoped<IServiceService, ServiceService>();
            builder.Services.AddScoped<ICustomerCarRepository, CustomerCarRepository>();
            builder.Services.AddScoped<IAppointmentConsumedPartRepository, AppointmentConsumedPartRepository>();
            builder.Services.AddScoped<IAppointmentConsumedPartService, AppointmentConsumedPartService>();

            // Car Showroom flow registrations
            builder.Services.AddScoped<ICarRepository, CarRepository>();
            builder.Services.AddScoped<ICarService, CarService>();
            builder.Services.AddScoped<ICarBrandRepository, CarBrandRepository>();
            builder.Services.AddScoped<ICarBrandService, CarBrandService>();

            // Part flow registrations
            builder.Services.AddScoped<IPartCategoryRepository, PartCategoryRepository>();
            builder.Services.AddScoped<IPartCategoryService, PartCategoryService>();
            builder.Services.AddScoped<IPartRepository, PartRepository>();
            builder.Services.AddScoped<IPartService, PartService>();
            builder.Services.AddScoped<IPartOrderRepository, PartOrderRepository>();
            builder.Services.AddScoped<IPartOrderService, PartOrderService>();
            
            // New parts logistics flow registrations
            builder.Services.AddScoped<ISupplierRepository, SupplierRepository>();
            builder.Services.AddScoped<ISupplierService, SupplierService>();
            builder.Services.AddScoped<IPartCompatibilityRepository, PartCompatibilityRepository>();
            builder.Services.AddScoped<IPartCompatibilityService, PartCompatibilityService>();
            builder.Services.AddScoped<IInventoryRepository, InventoryRepository>();
            builder.Services.AddScoped<IInventoryService, InventoryService>();
            builder.Services.AddScoped<IInventoryReceiptService, InventoryReceiptService>();
            // Car sales flow (yêu cầu mua xe)
            builder.Services.AddScoped<ICarSalesRepository, CarSalesRepository>();
            builder.Services.AddScoped<ICarSalesService, CarSalesService>();

            // Hóa đơn tổng dùng chung: checkout (mua lẻ/gộp) + thanh toán đặt cọc/mua đứt cho cả 3 module
            builder.Services.AddScoped<ICheckoutRepository, CheckoutRepository>();
            builder.Services.AddScoped<ICheckoutService, CheckoutService>();
            builder.Services.AddScoped<IMasterInvoicePaymentRepository, MasterInvoicePaymentRepository>();
            builder.Services.AddScoped<IMasterInvoicePaymentService, MasterInvoicePaymentService>();

            // Chat proxy — delegates to Python RAG service (chatbot chỉ tư vấn, không tạo đơn)
            builder.Services.AddHttpClient<IChatProxyService, ChatProxyService>();

            var modelBuilder = new ODataConventionModelBuilder();
            var cars = modelBuilder.EntitySet<BusinessObjects.Models.Car>("Cars");
            cars.EntityType.HasKey(c => c.CarId);

            var carBrands = modelBuilder.EntitySet<BusinessObjects.Models.CarBrand>("CarBrands");
            carBrands.EntityType.HasKey(cb => cb.BrandId);

            var packages = modelBuilder.EntitySet<BusinessObjects.Models.MaintenancePackage>("MaintenancePackages");
            packages.EntityType.HasKey(mp => mp.PackageId);

            var parts = modelBuilder.EntitySet<BusinessObjects.Models.Part>("Parts");
            parts.EntityType.HasKey(p => p.PartId);

            var partCategories = modelBuilder.EntitySet<BusinessObjects.Models.PartCategory>("PartCategories");
            partCategories.EntityType.HasKey(pc => pc.CategoryId);

            var partOrders = modelBuilder.EntitySet<BusinessObjects.Models.PartOrder>("PartOrders");
            partOrders.EntityType.HasKey(po => po.OrderId);

            // PurchaseRequest được phục vụ qua REST (/api/car-sales), nhưng vẫn khai báo khóa cho OData
            // vì Car.PurchaseRequests là navigation — tránh mọi rủi ro khi dựng EDM.
            var purchaseRequests = modelBuilder.EntitySet<BusinessObjects.Models.PurchaseRequest>("PurchaseRequests");
            purchaseRequests.EntityType.HasKey(pr => pr.RequestId);

            var odataSuppliers = modelBuilder.EntitySet<BusinessObjects.Models.Supplier>("Suppliers");
            odataSuppliers.EntityType.HasKey(s => s.SupplierId);

            builder.Services.AddControllers(options =>
            {
                options.SuppressImplicitRequiredAttributeForNonNullableReferenceTypes = true;
            })
                .AddOData(options => options
                    .Select()
                    .Filter()
                    .OrderBy()
                    .Expand()
                    .Count()
                    .SetMaxTop(100)
                    .AddRouteComponents("odata", modelBuilder.GetEdmModel())
                )
                .AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
                });
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(options =>
            {
                options.ResolveConflictingActions(apiDescriptions => apiDescriptions.First());
            });

            // JWT Authentication Configuration
            var jwtSettings = builder.Configuration.GetSection("JwtSettings");
            var secretKey = jwtSettings["Secret"];

            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtSettings["Issuer"],
                    ValidAudience = jwtSettings["Audience"],
                    IssuerSigningKey = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(secretKey))
                };
            });

            // Configure CORS
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowAll", policy =>
                {
                    policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod();
                });
            });

            var app = builder.Build();

            // Automatically apply EF migrations and custom schemas on startup
            using (var scope = app.Services.CreateScope())
            {
                try
                {
                    System.Console.WriteLine("Entity Framework Migrations are managed manually via SQL script (CarShowroomDB_v2.sql).");
                    using var context = new DataAccessObjects.CarShowroomContext();

                    // Kiểm tra kết nối DB ngay khi khởi động để phát hiện sớm lỗi (slow + 500 thường do DB).
                    System.Console.WriteLine("Checking database connectivity...");
                    if (context.Database.CanConnect())
                        System.Console.WriteLine("[OK] Database connection successful.");
                    else
                        System.Console.WriteLine("[ERROR] KHÔNG kết nối được database. Kiểm tra ConnectionStrings:DefaultConnection trong appsettings.json và đảm bảo SQL Server đang chạy + database CarShowroomDB đã tạo.");
                    // Schema chuẩn được tạo từ database/CarShowroomDB_v2.sql.
                    // Module ô tô (đặt cọc/mua đứt) dùng MasterInvoices + CarInvoices + PurchaseRequests của v2,
                    // không còn tạo bảng DepositCaptchas hay cột deposit trên PurchaseRequests khi khởi động.

                    // Check and apply custom delivery management schema adjustments
                    try
                    {
                        System.Console.WriteLine("Checking delivery and payment schema columns in PartOrders...");
                        context.Database.ExecuteSqlRaw(@"
                            IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('PartOrders') AND name = 'DeliveryMethod')
                            BEGIN
                                ALTER TABLE PartOrders
                                    ADD DeliveryMethod   NVARCHAR(50)   NOT NULL DEFAULT 'Pickup',
                                        ShippingFee      DECIMAL(18,2)  NOT NULL DEFAULT 0;
                            END
                            IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('PartOrders') AND name = 'PaymentMethod')
                            BEGIN
                                ALTER TABLE PartOrders
                                    ADD PaymentMethod NVARCHAR(50) NULL;
                            END
                        ");
                        context.Database.ExecuteSqlRaw(@"
                            ALTER TABLE PartOrders
                                ALTER COLUMN ShippingAddress NVARCHAR(255) NULL;
                        ");
                        
                        System.Console.WriteLine("Checking IsPaid column in MaintenanceAppointments...");
                        context.Database.ExecuteSqlRaw(@"
                            IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('MaintenanceAppointments') AND name = 'IsPaid')
                            BEGIN
                                ALTER TABLE MaintenanceAppointments
                                    ADD IsPaid BIT NOT NULL DEFAULT 0;
                            END
                        ");

                        // Cột phục vụ xác thực OTP / Reset mật khẩu trên AppUsers
                        System.Console.WriteLine("Checking OTP verification columns on AppUsers...");
                        context.Database.ExecuteSqlRaw(@"
                            IF COL_LENGTH('AppUsers','VerificationCode') IS NULL
                                ALTER TABLE AppUsers ADD VerificationCode NVARCHAR(100) NULL;
                            IF COL_LENGTH('AppUsers','CodeExpiryTime') IS NULL
                                ALTER TABLE AppUsers ADD CodeExpiryTime DATETIME NULL;
                        ");

                        // Cột dùng chung trên hóa đơn tổng (cả 3 module cần) — tự bổ sung nếu chưa chạy patch.
                        System.Console.WriteLine("Checking shared columns on MasterInvoices...");
                        context.Database.ExecuteSqlRaw(@"
                            IF COL_LENGTH('MasterInvoices','InvoiceType') IS NULL
                                ALTER TABLE MasterInvoices ADD InvoiceType VARCHAR(20) NOT NULL CONSTRAINT DF_MasterInvoices_InvoiceType DEFAULT 'Car';
                            IF COL_LENGTH('MasterInvoices','PaymentMethod') IS NULL
                                ALTER TABLE MasterInvoices ADD PaymentMethod NVARCHAR(50) NULL;
                            IF COL_LENGTH('MasterInvoices','PaymentReference') IS NULL
                                ALTER TABLE MasterInvoices ADD PaymentReference NVARCHAR(100) NULL;
                            IF COL_LENGTH('MasterInvoices','PaidAt') IS NULL
                                ALTER TABLE MasterInvoices ADD PaidAt DATETIME NULL;
                        ");

                        System.Console.WriteLine("Schema checks completed.");
                    }
                    catch (System.Exception ex)
                    {
                        System.Console.WriteLine($"[WARN] Schema sync skipped: {ex.InnerException?.Message ?? ex.Message}");
                    }
                }
                catch (System.Exception ex)
                {
                    // KHÔNG để lỗi đồng bộ schema làm sập ứng dụng — chỉ ghi log để chẩn đoán.
                    System.Console.WriteLine($"[WARN] Startup schema check failed (app vẫn khởi động): {ex.InnerException?.Message ?? ex.Message}");
                }
            }

            // Bắt & ghi log mọi exception chưa xử lý (đầu tiên trong pipeline).
            app.UseMiddleware<Middleware.ExceptionHandlingMiddleware>();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            if (!app.Environment.IsDevelopment())
            {
                app.UseHttpsRedirection();
            }
            app.UseCors("AllowAll");

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}
