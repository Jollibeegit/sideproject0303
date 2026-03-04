using SideProject0303.Models;

namespace SideProject0303.Components;

public sealed class EventModalSaveRequest
{
    public CalendarEventType Type { get; set; } = CalendarEventType.Schedule;
    public DateOnly StartDate { get; set; }
    public DateOnly? EndDate { get; set; }
    public string Title { get; set; } = "";
    public string? Body { get; set; }
}
