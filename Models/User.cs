using System.ComponentModel.DataAnnotations;

namespace SideProject0303.Models;

public class User
{
    public long Id { get; set; }

    public long CoupleId { get; set; }
    public Couple Couple { get; set; } = null!;

    [MaxLength(50)]
    public string DisplayName { get; set; } = "";

    [MaxLength(120)]
    public string Email { get; set; } = ""; // 지금은 선택. 로그인 붙일 때 활용

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}
