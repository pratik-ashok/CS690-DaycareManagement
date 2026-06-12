using DaycareManagement.Services;

namespace DaycareManagement.Tests;

/// <summary>Tests for PickupService (FR9, FR10).</summary>
public class PickupServiceTests : IDisposable
{
    private readonly string        _tempDir;
    private readonly DataStore     _store;
    private readonly PickupService _service;

    public PickupServiceTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        _store   = new DataStore(_tempDir);
        _service = new PickupService(_store);
    }

    public void Dispose() => Directory.Delete(_tempDir, recursive: true);

    [Fact]
    public void AddContact_CanBeRetrieved()
    {
        _service.AddContact(1, "Jane Smith", "Mother", "416-555-0101");
        var contacts = _service.GetContacts(1);
        Assert.Single(contacts);
        Assert.Equal("Jane Smith", contacts[0].Name);
        Assert.Equal("Mother",     contacts[0].Relationship);
    }

    [Fact]
    public void GetContacts_ReturnsOnlyForSpecifiedChild()
    {
        _service.AddContact(1, "Jane Smith", "Mother", "416-555-0101");
        _service.AddContact(2, "Bob Jones",  "Father", "647-555-0202");
        Assert.Single(_service.GetContacts(1));
        Assert.Single(_service.GetContacts(2));
        Assert.Equal("Bob Jones", _service.GetContacts(2)[0].Name);
    }

    [Fact]
    public void GetContacts_ReturnsEmpty_WhenNoContactsForChild()
    {
        Assert.Empty(_service.GetContacts(99));
    }

    [Fact]
    public void VerifyPickup_ReturnsContact_WhenAuthorized()
    {
        _service.AddContact(1, "Jane Smith", "Mother", "416-555-0101");
        var result = _service.VerifyPickup(1, "Jane Smith");
        Assert.NotNull(result);
        Assert.Equal("Mother", result.Relationship);
    }

    [Fact]
    public void VerifyPickup_ReturnsNull_WhenNotAuthorized()
    {
        _service.AddContact(1, "Jane Smith", "Mother", "416-555-0101");
        Assert.Null(_service.VerifyPickup(1, "Unknown Person"));
    }

    [Fact]
    public void VerifyPickup_IsCaseInsensitive()
    {
        _service.AddContact(1, "Jane Smith", "Mother", "416-555-0101");
        Assert.NotNull(_service.VerifyPickup(1, "jane smith"));
        Assert.NotNull(_service.VerifyPickup(1, "JANE SMITH"));
    }

    [Fact]
    public void VerifyPickup_ReturnsNull_WhenWrongChild()
    {
        _service.AddContact(1, "Jane Smith", "Mother", "416-555-0101");
        Assert.Null(_service.VerifyPickup(2, "Jane Smith"));
    }
}
