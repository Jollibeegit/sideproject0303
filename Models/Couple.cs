using System.ComponentModel.DataAnnotations;

namespace SideProject0303.Models;

public class Couple
{
    public long Id { get; set; }

    [MaxLength(16)]
    public string Code { get; set; } = Guid.NewGuid().ToString("N")[..8];

    // 사귄날(없으면 커플 생성일 기준으로 기본 세팅 가능)
    public DateOnly StartedOn { get; set; } = DateOnly.FromDateTime(DateTime.UtcNow);

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;

    public List<User> Users { get; set; } = new();
    public List<CalendarEvent> Events { get; set; } = new();
}
