using DaycareManagement.Models;
using DaycareManagement.Services;

namespace DaycareManagement.Tests;

/// <summary>Tests for the DataStore persistence module.</summary>
public class DataStoreTests : IDisposable
{
    private readonly string    _tempDir;
    private readonly DataStore _store;

    public DataStoreTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        _store   = new DataStore(_tempDir);
    }

    public void Dispose() => Directory.Delete(_tempDir, recursive: true);

    [Fact]
    public void LoadChildren_WhenFileNotExists_ReturnsEmptyList()
    {
        var result = _store.LoadChildren();
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public void SaveAndLoadChildren_RoundTrips()
    {
        var children = new List<Child>
        {
            new() { Id = 1, Name = "Alice", DateOfBirth = "2019-04-01" }
        };
        _store.SaveChildren(children);
        var loaded = _store.LoadChildren();
        Assert.Single(loaded);
        Assert.Equal("Alice", loaded[0].Name);
        Assert.Equal("2019-04-01", loaded[0].DateOfBirth);
    }

    [Fact]
    public void SaveAndLoadAllergies_RoundTrips()
    {
        var allergies = new List<AllergyRecord>
        {
            new() { ChildId = 1, Allergen = "Peanuts", Severity = "Severe", Notes = "Use EpiPen" }
        };
        _store.SaveAllergies(allergies);
        var loaded = _store.LoadAllergies();
        Assert.Single(loaded);
        Assert.Equal("Peanuts", loaded[0].Allergen);
        Assert.Equal("Severe",  loaded[0].Severity);
    }

    [Fact]
    public void SaveAndLoadStaff_RoundTrips()
    {
        var staff = new List<StaffMember>
        {
            new() { Id = 1, Name = "Jane", IsOnDuty = true }
        };
        _store.SaveStaff(staff);
        var loaded = _store.LoadStaff();
        Assert.Single(loaded);
        Assert.Equal("Jane", loaded[0].Name);
        Assert.True(loaded[0].IsOnDuty);
    }

    [Fact]
    public void SaveAndLoadSchedule_RoundTrips()
    {
        var schedule = new List<ScheduleEntry>
        {
            new() { ChildId = 1, Date = "2026-06-12" }
        };
        _store.SaveSchedule(schedule);
        var loaded = _store.LoadSchedule();
        Assert.Single(loaded);
        Assert.Equal(1,            loaded[0].ChildId);
        Assert.Equal("2026-06-12", loaded[0].Date);
    }

    [Fact]
    public void SaveAndLoadPickupContacts_RoundTrips()
    {
        var contacts = new List<PickupContact>
        {
            new() { ChildId = 1, Name = "Jane Smith", Relationship = "Mother", Phone = "416-555-0101" }
        };
        _store.SavePickupContacts(contacts);
        var loaded = _store.LoadPickupContacts();
        Assert.Single(loaded);
        Assert.Equal("Jane Smith", loaded[0].Name);
    }
}
