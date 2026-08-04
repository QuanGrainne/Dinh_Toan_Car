using System;
using Microsoft.EntityFrameworkCore;

namespace DataAccessObjects;

public static class ExceptionMessageHelper
{
    public static string GetDetailedMessage(Exception ex)
    {
        if (ex is DbUpdateException dbUpdateException)
        {
            return dbUpdateException.InnerException?.Message ?? dbUpdateException.Message;
        }

        return ex.InnerException?.Message ?? ex.Message;
    }
}
