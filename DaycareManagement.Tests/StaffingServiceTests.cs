using DaycareManagement.Models;
using DaycareManagement.Services;

namespace DaycareManagement.Tests;

/// <summary>Tests for StaffingService (FR4, FR5, FR6).</summary>
public class StaffingServiceTests : IDisposable
{
    private readonly string          _tempDir;
    private readonly DataStore       _store;
    private readonly StaffingService _service;

    public StaffingServiceTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        _store   = new DataStore(_tempDir);
        _service = new StaffingService(_store);
    }

    public void Dispose() => Directory.Delete(_tempDir, recursive: true);

    [Fact]
    public void AddStaff_AssignsIncrementingIds()
    {
        var s1 = _service.AddStaff("Alice");
        var s2 = _service.AddStaff("Bob");
        Assert.Equal(1, s1.Id);
        Assert.Equal(2, s2.Id);
    }

    [Fact]
    public void AddStaff_IsNotOnDutyByDefault()
    {
        var member = _service.AddStaff("Alice");
        Assert.False(member.IsOnDuty);
    }

    [Fact]
    public void CheckInStaff_SetsOnDuty()
    {
        _service.AddStaff("Alice");
        bool result = _service.CheckInStaff(1);
        Assert.True(result);
        Assert.Equal(1, _service.GetOnDutyStaffCount());
    }

    [Fact]
    public void CheckInStaff_ReturnsFalse_WhenAlreadyOnDuty()
    {
        _service.AddStaff("Alice");
        _service.CheckInStaff(1);
        Assert.False(_service.CheckInStaff(1));
    }

    [Fact]
    public void CheckOutStaff_ClearsOnDuty()
    {
        _service.AddStaff("Alice");
        _service.CheckInStaff(1);
        bool result = _service.CheckOutStaff(1);
        Assert.True(result);
        Assert.Equal(0, _service.GetOnDutyStaffCount());
    }

    [Fact]
    public void CheckOutStaff_ReturnsFalse_WhenNotOnDuty()
    {
        _service.AddStaff("Alice");
        Assert.False(_service.CheckOutStaff(1));
    }

    [Fact]
    public void GetPresentChildrenCount_ReturnsOnlyCheckedIn()
    {
        _store.SaveChildren(new List<Child>
        {
            new() { Id = 1, IsCheckedIn = true  },
            new() { Id = 2, IsCheckedIn = false },
            new() { Id = 3, IsCheckedIn = true  },
        });
        Assert.Equal(2, _service.GetPresentChildrenCount());
    }

    [Fact]
    public void GetOnDutyStaffCount_ReturnsOnlyOnDuty()
    {
        _service.AddStaff("Alice");
        _service.AddStaff("Bob");
        _service.CheckInStaff(1);
        Assert.Equal(1, _service.GetOnDutyStaffCount());
    }

    [Fact]
    public void GetRatio_ReturnsNull_WhenNoChildren()
    {
        _service.AddStaff("Alice");
        _service.CheckInStaff(1);
        Assert.Null(_service.GetRatio());
    }

    [Fact]
    public void GetRatio_ReturnsZero_WhenNoStaffButChildrenPresent()
    {
        _store.SaveChildren(new List<Child>
        {
            new() { Id = 1, IsCheckedIn = true }
        });
        Assert.Equal(0.0, _service.GetRatio());
    }

    [Fact]
    public void GetRatio_CalculatesCorrectly_OneToFour()
    {
        _store.SaveChildren(Enumerable.Range(1, 4)
            .Select(i => new Child { Id = i, IsCheckedIn = true }).ToList());
        _service.AddStaff("Alice");
        _service.CheckInStaff(1);
        Assert.Equal(0.25, _service.GetRatio());
    }

    [Fact]
    public void IsRatioBelowThreshold_ReturnsFalse_WhenNoChildren()
    {
        Assert.False(_service.IsRatioBelowThreshold());
    }

    [Fact]
    public void IsRatioBelowThreshold_ReturnsFalse_WhenRatioIsSafe()
    {
        _store.SaveChildren(Enumerable.Range(1, 4)
            .Select(i => new Child { Id = i, IsCheckedIn = true }).ToList());
        _service.AddStaff("Alice");
        _service.CheckInStaff(1);
        Assert.False(_service.IsRatioBelowThreshold());
    }

    [Fact]
    public void IsRatioBelowThreshold_ReturnsTrue_WhenRatioIsUnsafe()
    {
        _store.SaveChildren(Enumerable.Range(1, 9)
            .Select(i => new Child { Id = i, IsCheckedIn = true }).ToList());
        _service.AddStaff("Alice");
        _service.CheckInStaff(1);
        Assert.True(_service.IsRatioBelowThreshold());
    }

    [Fact]
    public void IsRatioBelowThreshold_ReturnsTrue_WhenNoStaffAndChildrenPresent()
    {
        _store.SaveChildren(new List<Child>
        {
            new() { Id = 1, IsCheckedIn = true }
        });
        Assert.True(_service.IsRatioBelowThreshold());
    }
}
