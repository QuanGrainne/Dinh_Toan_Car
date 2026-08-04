using System;
using System.Collections.Generic;

namespace BusinessObjects.DTOs
{
    public class ChatRequestDto
    {
        public string? SessionId { get; set; }
        public string Message { get; set; } = string.Empty;
        public int? CustomerId { get; set; }
    }

    public class ChatResponseDto
    {
        public string Reply { get; set; } = string.Empty;
        public string SessionId { get; set; } = string.Empty;
        public List<ComboOrderItemDto> SuggestedItems { get; set; } = new List<ComboOrderItemDto>();
        public string? Action { get; set; }
        public bool HasOrderSuggestion { get; set; }
    }
}
