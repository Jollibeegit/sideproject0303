using Microsoft.EntityFrameworkCore;
using SideProject0303.Components;
using SideProject0303;
using SideProject0303.Models;

var builder = WebApplication.CreateBuilder(args);

// 🔹 Render PORT 대응 (로컬에서는 8080 사용)
var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
builder.WebHost.UseUrls($"http://0.0.0.0:{port}");

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();
builder.Services.AddDbContextFactory<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("Default")));
builder.Services.AddHttpClient<ApiClient>(client =>
{
    client.BaseAddress = new Uri($"http://localhost:{port}");
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseStatusCodePagesWithReExecute("/not-found");
app.UseStaticFiles();
app.UseAntiforgery();

// API Endpoints
app.MapPost("/api/events", CreateEventAsync);
app.MapGet("/api/events", GetEventsAsync);
app.MapGet("/api/summary", GetSummaryAsync);

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

// 시드 데이터 추가
try
{
    using (var scope = app.Services.CreateScope())
    {
        var dbFactory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<AppDbContext>>();
        await using var db = await dbFactory.CreateDbContextAsync();
        
        if (!await db.Couples.AnyAsync(c => c.Code == "ABCD1234"))
        {
            var couple = new Couple
            {
                Code = "ABCD1234",
                StartedOn = new DateOnly(2024, 7, 14)
            };
            db.Couples.Add(couple);
            await db.SaveChangesAsync();
        }
    }
}
catch { }

await app.RunAsync();

// Endpoint Handlers
async Task<IResult> CreateEventAsync(CreateEventDto dto, IDbContextFactory<AppDbContext> dbFactory)
{
    if (string.IsNullOrWhiteSpace(dto.CoupleCode) || string.IsNullOrWhiteSpace(dto.Title))
        return Results.BadRequest("CoupleCode과 Title은 필수입니다.");

    await using var db = await dbFactory.CreateDbContextAsync();

    var couple = await db.Couples.FirstOrDefaultAsync(c => c.Code == dto.CoupleCode);
    if (couple is null)
        return Results.NotFound("Couple을 찾을 수 없습니다.");

    var @event = new CalendarEvent
    {
        CoupleId = couple.Id,
        Type = (CalendarEventType)dto.Type,
        Title = dto.Title.Trim(),
        Body = dto.Body,
        StartDate = dto.StartDate,
        EndDate = dto.EndDate,
        StartTime = dto.StartTime,
        EndTime = dto.EndTime,
        IsAllDay = dto.IsAllDay,
        Color = dto.Color,
        CreatedAtUtc = DateTime.UtcNow,
        UpdatedAtUtc = DateTime.UtcNow
    };

    db.CalendarEvents.Add(@event);
    await db.SaveChangesAsync();

    var result = new
    {
        @event.Id,
        @event.CoupleId,
        Type = (int)@event.Type,
        @event.Title,
        @event.Body,
        @event.StartDate,
        @event.EndDate,
        @event.StartTime,
        @event.EndTime,
        @event.IsAllDay,
        @event.Color,
        @event.CreatedByUserId,
        @event.CreatedAtUtc,
        @event.UpdatedAtUtc
    };

    return Results.Created($"/api/events/{@event.Id}", result);
}

async Task<IResult> GetEventsAsync(
    string coupleCode,
    string month,
    int? type,
    IDbContextFactory<AppDbContext> dbFactory)
{
    if (!DateTime.TryParseExact(month, "yyyy-MM", null, System.Globalization.DateTimeStyles.None, out var monthDate))
        return Results.BadRequest("month은 'yyyy-MM' 형식이어야 합니다.");

    await using var db = await dbFactory.CreateDbContextAsync();

    var couple = await db.Couples.FirstOrDefaultAsync(c => c.Code == coupleCode);
    if (couple is null)
        return Results.NotFound("Couple을 찾을 수 없습니다.");

    var start = new DateOnly(monthDate.Year, monthDate.Month, 1);
    var end = start.AddMonths(1).AddDays(-1);

    var query = db.CalendarEvents
        .AsNoTracking()
        .Where(e => e.CoupleId == couple.Id && e.StartDate >= start && e.StartDate <= end);

    if (type.HasValue && Enum.IsDefined(typeof(CalendarEventType), type.Value))
    {
        query = query.Where(e => (int)e.Type == type.Value);
    }

    var events = await query
        .Select(e => new
        {
            e.Id,
            e.CoupleId,
            Type = (int)e.Type,
            e.Title,
            e.Body,
            e.StartDate,
            e.EndDate,
            e.StartTime,
            e.EndTime,
            e.IsAllDay,
            e.Color,
            e.CreatedByUserId,
            e.CreatedAtUtc,
            e.UpdatedAtUtc
        })
        .OrderBy(e => e.StartDate)
        .ToListAsync();

    return Results.Ok(events);
}

async Task<IResult> GetSummaryAsync(string coupleCode, IDbContextFactory<AppDbContext> dbFactory)
{
    await using var db = await dbFactory.CreateDbContextAsync();

    var couple = await db.Couples
        .AsNoTracking()
        .FirstOrDefaultAsync(c => c.Code == coupleCode);

    if (couple is null)
        return Results.NotFound("Couple을 찾을 수 없습니다.");

    var dPlusDays = (DateTime.Today.Date - couple.StartedOn.ToDateTime(TimeOnly.MinValue).Date).Days + 1;

    var upcomingSchedules = await db.CalendarEvents
        .AsNoTracking()
        .CountAsync(e => e.CoupleId == couple.Id && (int)e.Type == (int)CalendarEventType.Schedule && e.StartDate >= DateOnly.FromDateTime(DateTime.Today));

    var totalMemos = await db.CalendarEvents
        .AsNoTracking()
        .CountAsync(e => e.CoupleId == couple.Id && (int)e.Type == (int)CalendarEventType.Memo);

    var totalAnniversaries = await db.CalendarEvents
        .AsNoTracking()
        .CountAsync(e => e.CoupleId == couple.Id && (int)e.Type == (int)CalendarEventType.Anniversary);

    var result = new
    {
        couple.StartedOn,
        DPlusDays = dPlusDays,
        UpcomingSchedules = upcomingSchedules,
        TotalMemos = totalMemos,
        TotalAnniversaries = totalAnniversaries
    };

    return Results.Ok(result);
}
