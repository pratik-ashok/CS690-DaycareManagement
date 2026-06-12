using DaycareManagement.Services;

namespace DaycareManagement.Tests;

/// <summary>Tests for AttendanceService (FR2, FR3).</summary>
public class AttendanceServiceTests : IDisposable
{
    private readonly string            _tempDir;
    private readonly DataStore         _store;
    private readonly AttendanceService _service;

    public AttendanceServiceTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        _store   = new DataStore(_tempDir);
        _service = new AttendanceService(_store);
    }

    public void Dispose() => Directory.Delete(_tempDir, recursive: true);

    [Fact]
    public void AddChild_AssignsIncrementingIds()
    {
        var c1 = _service.AddChild("Alice", "2019-01-01");
        var c2 = _service.AddChild("Bob",   "2020-02-02");
        Assert.Equal(1, c1.Id);
        Assert.Equal(2, c2.Id);
    }

    [Fact]
    public void CheckIn_SetsIsCheckedIn()
    {
        _service.AddChild("Alice", "2019-01-01");
        string today = DateTime.Today.ToString("yyyy-MM-dd");
        bool result = _service.CheckIn(1, today);
        Assert.True(result);
        Assert.True(_service.GetChildren().First(c => c.Id == 1).IsCheckedIn);
    }

    [Fact]
    public void CheckIn_ReturnsFalse_WhenAlreadyCheckedIn()
    {
        _service.AddChild("Alice", "2019-01-01");
        string today = DateTime.Today.ToString("yyyy-MM-dd");
        _service.CheckIn(1, today);
        bool result = _service.CheckIn(1, today);
        Assert.False(result);
    }

    [Fact]
    public void CheckIn_ReturnsFalse_WhenChildNotFound()
    {
        string today = DateTime.Today.ToString("yyyy-MM-dd");
        bool result = _service.CheckIn(99, today);
        Assert.False(result);
    }

    [Fact]
    public void CheckOut_ClearsIsCheckedIn()
    {
        _service.AddChild("Alice", "2019-01-01");
        string today = DateTime.Today.ToString("yyyy-MM-dd");
        _service.CheckIn(1, today);
        bool result = _service.CheckOut(1, today);
        Assert.True(result);
        Assert.False(_service.GetChildren().First(c => c.Id == 1).IsCheckedIn);
    }

    [Fact]
    public void CheckOut_ReturnsFalse_WhenNotCheckedIn()
    {
        _service.AddChild("Alice", "2019-01-01");
        string today = DateTime.Today.ToString("yyyy-MM-dd");
        bool result = _service.CheckOut(1, today);
        Assert.False(result);
    }

    [Fact]
    public void IsScheduled_ReturnsFalse_WhenNotScheduled()
    {
        _service.AddChild("Alice", "2019-01-01");
        Assert.False(_service.IsScheduled(1, "2026-06-12"));
    }

    [Fact]
    public void AddToSchedule_ThenIsScheduled_ReturnsTrue()
    {
        _service.AddChild("Alice", "2019-01-01");
        _service.AddToSchedule(1, "2026-06-12");
        Assert.True(_service.IsScheduled(1, "2026-06-12"));
    }

    [Fact]
    public void AddToSchedule_IsIdempotent()
    {
        _service.AddChild("Alice", "2019-01-01");
        _service.AddToSchedule(1, "2026-06-12");
        _service.AddToSchedule(1, "2026-06-12");
        var schedule = _store.LoadSchedule();
        Assert.Single(schedule);
    }

    [Fact]
    public void GetTodayRecords_ReturnsOnlyTodayEntries()
    {
        _service.AddChild("Alice", "2019-01-01");
        _service.CheckIn(1, "2026-06-11");
        _service.CheckIn(1, "2026-06-12");
        var recs = _service.GetTodayRecords("2026-06-12");
        Assert.All(recs, r => Assert.Equal("2026-06-12", r.Date));
    }
}
