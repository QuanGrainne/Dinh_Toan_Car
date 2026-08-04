using System.Threading.Tasks;
using BusinessObjects.DTOs;

namespace Services;

public interface IChatProxyService
{
    /// <summary>
    /// Proxies a chat message to the Python RAG service and returns the response.
    /// Never throws — returns a graceful error reply on failure.
    /// </summary>
    Task<ChatResponseDto> SendMessageAsync(ChatRequestDto request);

    /// <summary>Returns raw history messages for a session from Python service.</summary>
    Task<object> GetHistoryAsync(string sessionId);
}
