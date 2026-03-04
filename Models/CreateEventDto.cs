using SideProject0303.Models;

namespace SideProject0303.Models;

public class CreateEventDto
{
    public string CoupleCode { get; set; } = "";
    public CalendarEventType Type { get; set; }
    public string Title { get; set; } = "";
    public string? Body { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly? EndDate { get; set; }
    public TimeOnly? StartTime { get; set; }
    public TimeOnly? EndTime { get; set; }
    public bool IsAllDay { get; set; } = true;
    public string? Color { get; set; }
}
