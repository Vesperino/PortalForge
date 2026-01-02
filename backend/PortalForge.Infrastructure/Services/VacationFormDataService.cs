using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using PortalForge.Application.Common.Interfaces;
using PortalForge.Domain.Enums;

namespace PortalForge.Infrastructure.Services;

public class VacationFormDataService : IVacationFormDataService
{
    private readonly ILogger<VacationFormDataService> _logger;

    private static readonly string[] AnnualKeywords = { "wypocz", "annual" };
    private static readonly string[] OnDemandKeywords = { "żąd", "zadanie", "on demand", "ondemand" };
    private static readonly string[] CircumstantialKeywords = { "okolicz", "circumstantial" };
    private static readonly string[] SickKeywords = { "zwolnienie", "l4", "sick" };

    public VacationFormDataService(ILogger<VacationFormDataService> logger)
    {
        _logger = logger;
    }

    public VacationFormData? ExtractVacationData(string formDataJson)
    {
        try
        {
            var dict = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(formDataJson);
            if (dict == null)
            {
                return null;
            }

            string? leaveTypeStr = null;
            DateTime? startDate = null;
            DateTime? endDate = null;

            foreach (var kv in dict)
            {
                if (kv.Value.ValueKind != JsonValueKind.String)
                {
                    continue;
                }

                var str = kv.Value.GetString();
                if (string.IsNullOrWhiteSpace(str))
                {
                    continue;
                }

                leaveTypeStr ??= TryMatchLeaveType(str);
                TryParseDateValue(str, ref startDate, ref endDate);
            }

            leaveTypeStr ??= TryMatchLeaveTypeByKeywords(dict);

            if (leaveTypeStr == null || !startDate.HasValue || !endDate.HasValue)
            {
                _logger.LogDebug(
                    "Incomplete vacation data: LeaveType={LeaveType}, StartDate={StartDate}, EndDate={EndDate}",
                    leaveTypeStr, startDate, endDate);
                return null;
            }

            if (!Enum.TryParse<LeaveType>(leaveTypeStr, out var leaveType))
            {
                _logger.LogDebug("Failed to parse leave type: {LeaveTypeStr}", leaveTypeStr);
                return null;
            }

            return new VacationFormData(startDate.Value, endDate.Value, leaveType);
        }
        catch (JsonException ex)
        {
            _logger.LogWarning(ex, "Failed to parse form data JSON");
            return null;
        }
    }

    public int CalculateBusinessDays(DateTime startDate, DateTime endDate)
    {
        var days = 0;
        var current = startDate.Date;
        var last = endDate.Date;

        while (current <= last)
        {
            if (current.DayOfWeek != DayOfWeek.Saturday && current.DayOfWeek != DayOfWeek.Sunday)
            {
                days++;
            }
            current = current.AddDays(1);
        }

        return days;
    }

    private static string? TryMatchLeaveType(string value)
    {
        return value switch
        {
            "Annual" or "OnDemand" or "Circumstantial" or "Sick" => value,
            _ => null
        };
    }

    private static void TryParseDateValue(string value, ref DateTime? startDate, ref DateTime? endDate)
    {
        if (!Regex.IsMatch(value, @"^\d{4}-\d{2}-\d{2}$"))
        {
            return;
        }

        if (!DateTime.TryParse(value, out var parsedDate))
        {
            return;
        }

        if (!startDate.HasValue)
        {
            startDate = parsedDate;
        }
        else if (!endDate.HasValue)
        {
            endDate = parsedDate;
        }
    }

    private static string? TryMatchLeaveTypeByKeywords(Dictionary<string, JsonElement> dict)
    {
        foreach (var kv in dict)
        {
            if (kv.Value.ValueKind != JsonValueKind.String)
            {
                continue;
            }

            var str = kv.Value.GetString()?.ToLowerInvariant();
            if (string.IsNullOrWhiteSpace(str))
            {
                continue;
            }

            if (AnnualKeywords.Any(k => str.Contains(k)))
            {
                return "Annual";
            }

            if (OnDemandKeywords.Any(k => str.Contains(k)))
            {
                return "OnDemand";
            }

            if (CircumstantialKeywords.Any(k => str.Contains(k)))
            {
                return "Circumstantial";
            }

            if (SickKeywords.Any(k => str.Contains(k)))
            {
                return "Sick";
            }
        }

        return null;
    }
}
