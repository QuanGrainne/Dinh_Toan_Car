using System;
using System.Threading.Tasks;
using BusinessObjects.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Services;

namespace CarSalesManagementSystemAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ChatController : ControllerBase
{
    private readonly IChatProxyService _chatProxy;
    private readonly ILogger<ChatController> _logger;

    public ChatController(IChatProxyService chatProxy, ILogger<ChatController> logger)
    {
        _chatProxy = chatProxy;
        _logger = logger;
    }

    /// <summary>
    /// Sends a message to the RAG chatbot. Anonymous access allowed —
    /// customerId extracted from JWT if present, otherwise null.
    /// </summary>
    [HttpPost("message")]
    public async Task<IActionResult> SendMessage([FromBody] ChatRequestDto request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var userIdStr = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (int.TryParse(userIdStr, out var userId))
        {
            request.CustomerId = userId;
        }

        _logger.LogInformation("Chat message received. Session={SessionId} CustomerId={CustomerId}", request.SessionId, request.CustomerId);

        var result = await _chatProxy.SendMessageAsync(request);
        return Ok(new ApiResponse<ChatResponseDto>(true, "OK", result));
    }

    /// <summary>
    /// Returns conversation history for a session.
    /// </summary>
    [HttpGet("history/{sessionId}")]
    public async Task<IActionResult> GetHistory(string sessionId)
    {
        if (string.IsNullOrWhiteSpace(sessionId))
            return BadRequest(new { message = "SessionId không hợp lệ." });

        var history = await _chatProxy.GetHistoryAsync(sessionId);
        return Ok(history);
    }

    /// <summary>
    /// Generates a new session ID for a fresh conversation.
    /// </summary>
    [HttpPost("session")]
    public IActionResult CreateSession()
    {
        var sessionId = Guid.NewGuid().ToString("N");
        return Ok(new { session_id = sessionId });
    }
}
