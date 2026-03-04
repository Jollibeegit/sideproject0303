using System.Net.Http.Json;
using SideProject0303.Models;

namespace SideProject0303;

public sealed class ApiClient
{
    private readonly HttpClient _http;

    public ApiClient(HttpClient http)
    {
        _http = http;
    }

    public async Task<SummaryResponse?> GetSummaryAsync(string coupleCode, CancellationToken ct = default)
    {
        var url = $"/api/summary?coupleCode={Uri.EscapeDataString(coupleCode)}";
        return await GetOrThrowAsync<SummaryResponse>(url, ct);
    }

    public async Task<List<CalendarEventItem>> GetEventsAsync(
        string coupleCode,
        string month, // "yyyy-MM"
        CalendarEventType? type = null,
        CancellationToken ct = default)
    {
        var url = $"/api/events?coupleCode={Uri.EscapeDataString(coupleCode)}&month={Uri.EscapeDataString(month)}";
        if (type.HasValue) url += $"&type={type.Value}";

        var list = await GetOrThrowAsync<List<CalendarEventItem>>(url, ct);
        return list ?? new();
    }

    private async Task<T?> GetOrThrowAsync<T>(string url, CancellationToken ct)
    {
        using var res = await _http.GetAsync(url, ct);
        if (!res.IsSuccessStatusCode)
        {
            var text = await res.Content.ReadAsStringAsync(ct);
            throw new InvalidOperationException($"GET {url} failed: {(int)res.StatusCode} {res.ReasonPhrase}\n{text}");
        }

        return await res.Content.ReadFromJsonAsync<T>(cancellationToken: ct);
    }

    public async Task<CalendarEventItem?> CreateEventAsync(CreateEventDto dto, CancellationToken ct = default)
    {
        var res = await _http.PostAsJsonAsync("/api/events", dto, ct);
        if (!res.IsSuccessStatusCode)
        {
            var text = await res.Content.ReadAsStringAsync(ct);
            throw new InvalidOperationException($"CreateEvent failed: {(int)res.StatusCode} {res.ReasonPhrase}\n{text}");
        }

        return await res.Content.ReadFromJsonAsync<CalendarEventItem>(cancellationToken: ct);
    }
}

/// <summary>
/// 서버 /api/events 가 내려주는 형태(Program.cs의 Select/anonymous type)와 맞춘 DTO
/// </summary>
public sealed class CalendarEventItem
{
    public long Id { get; set; }
    public long CoupleId { get; set; }
    public CalendarEventType Type { get; set; }
    public string Title { get; set; } = "";
    public string? Body { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly? EndDate { get; set; }
    public TimeOnly? StartTime { get; set; }
    public TimeOnly? EndTime { get; set; }
    public bool IsAllDay { get; set; }
    public string? Color { get; set; }
    public long? CreatedByUserId { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public DateTime UpdatedAtUtc { get; set; }
}

public sealed class SummaryResponse
{
    public DateOnly StartedOn { get; set; }
    public int DPlusDays { get; set; }
    public int UpcomingSchedules { get; set; }
    public int TotalMemos { get; set; }
    public int TotalAnniversaries { get; set; }
}
