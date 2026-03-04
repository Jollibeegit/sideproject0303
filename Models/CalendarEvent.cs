using System.ComponentModel.DataAnnotations;

namespace SideProject0303.Models;

public enum CalendarEventType
{
    Memo = 0,
    Anniversary = 1,
    Schedule = 2
}

public class CalendarEvent
{
    public long Id { get; set; }

    public long CoupleId { get; set; }
    public Couple Couple { get; set; } = null!;

    public CalendarEventType Type { get; set; } = CalendarEventType.Schedule;

    [MaxLength(80)]
    public string Title { get; set; } = "";   // 달력에 보이는 짧은 텍스트(예: 영화 보기)

    [MaxLength(2000)]
    public string? Body { get; set; }         // 모달의 긴 내용(선택)

    // 달력 기준 날짜(최소 필요)
    public DateOnly StartDate { get; set; }

    // 여러날 일정이면 사용(선택)
    public DateOnly? EndDate { get; set; }

    // 시간 있는 일정이면 사용(선택)
    public TimeOnly? StartTime { get; set; }
    public TimeOnly? EndTime { get; set; }
    public bool IsAllDay { get; set; } = true;

    // UI 바 색(선택) 예: "#7C3AED"
    [MaxLength(16)]
    public string? Color { get; set; }

    // 누가 만들었는지(로그인 붙일 때 살림) - 지금은 nullable
    public long? CreatedByUserId { get; set; }

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;
}
