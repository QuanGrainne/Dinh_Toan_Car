using System;
using System.Threading;
using System.Threading.Tasks;
using BusinessObjects.ViewModels;

namespace Services
{
    public sealed class ServiceResult<T>
    {
        public bool Success { get; init; }

        public string Message { get; init; } = string.Empty;

        public T? Data { get; init; }

        public static ServiceResult<T> Ok(T data, string message = "")
            => new()
            {
                Success = true,
                Data = data,
                Message = message
            };

        public static ServiceResult<T> Fail(string message)
            => new()
            {
                Success = false,
                Message = message
            };
    }

    public interface IInventoryReceiptService
    {
        Task<ServiceResult<int>> CreateReceiptAsync(
            InventoryReceiptCreateViewModel request,
            int currentAdminId,
            CancellationToken cancellationToken = default
        );
    }
}
