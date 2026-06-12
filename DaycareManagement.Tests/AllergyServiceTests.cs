using DaycareManagement.Models;
using DaycareManagement.Services;

namespace DaycareManagement.Tests;

/// <summary>Tests for AllergyService (FR7, FR8).</summary>
public class AllergyServiceTests : IDisposable
{
    private readonly string        _tempDir;
    private readonly DataStore     _store;
    private readonly AllergyService _service;

    public AllergyServiceTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        _store   = new DataStore(_tempDir);
        _service = new AllergyService(_store);
    }

    public void Dispose() => Directory.Delete(_tempDir, recursive: true);

    [Fact]
    public void AddAllergy_CanBeRetrieved()
    {
        _service.AddOrUpdateAllergy(1, "Peanuts", "Severe", "Use EpiPen");
        var result = _service.GetAllergies(1);
        Assert.Single(result);
        Assert.Equal("Peanuts",    result[0].Allergen);
        Assert.Equal("Severe",     result[0].Severity);
        Assert.Equal("Use EpiPen", result[0].Notes);
    }

    [Fact]
    public void UpdateAllergy_OverwritesExistingEntry()
    {
        _service.AddOrUpdateAllergy(1, "Peanuts", "Mild",   "Monitor");
        _service.AddOrUpdateAllergy(1, "Peanuts", "Severe", "Use EpiPen");
        var result = _service.GetAllergies(1);
        Assert.Single(result);
        Assert.Equal("Severe",     result[0].Severity);
        Assert.Equal("Use EpiPen", result[0].Notes);
    }

    [Fact]
    public void GetAllergies_IsCaseInsensitiveForAllergen()
    {
        _service.AddOrUpdateAllergy(1, "Peanuts", "Severe", "Use EpiPen");
        _service.AddOrUpdateAllergy(1, "peanuts", "Mild",   "Monitor");
        var result = _service.GetAllergies(1);
        Assert.Single(result);
    }

    [Fact]
    public void GetAllergies_ReturnsOnlyForSpecifiedChild()
    {
        _service.AddOrUpdateAllergy(1, "Peanuts", "Severe", "");
        _service.AddOrUpdateAllergy(2, "Dairy",   "Mild",   "");
        Assert.Single(_service.GetAllergies(1));
        Assert.Equal("Dairy", _service.GetAllergies(2)[0].Allergen);
    }

    [Fact]
    public void GetAllergies_ReturnsEmpty_WhenNoAllergies()
    {
        Assert.Empty(_service.GetAllergies(99));
    }

    [Fact]
    public void PreMealScan_ReturnsOnlyPresentChildrenWithAllergies()
    {
        _store.SaveChildren(new List<Child>
        {
            new() { Id = 1, Name = "Alice", IsCheckedIn = true  },
            new() { Id = 2, Name = "Bob",   IsCheckedIn = false }
        });
        _service.AddOrUpdateAllergy(1, "Peanuts", "Severe", "");
        _service.AddOrUpdateAllergy(2, "Dairy",   "Mild",   "");

        var results = _service.PreMealScan();
        Assert.Single(results);
        Assert.Equal("Alice", results.Keys.First().Name);
    }

    [Fact]
    public void PreMealScan_ReturnsEmpty_WhenNoAllergiesForPresentChildren()
    {
        _store.SaveChildren(new List<Child>
        {
            new() { Id = 1, Name = "Alice", IsCheckedIn = true }
        });
        Assert.Empty(_service.PreMealScan());
    }

    [Fact]
    public void PreMealScan_ReturnsEmpty_WhenNoChildrenPresent()
    {
        _store.SaveChildren(new List<Child>
        {
            new() { Id = 1, Name = "Alice", IsCheckedIn = false }
        });
        _service.AddOrUpdateAllergy(1, "Peanuts", "Severe", "");
        Assert.Empty(_service.PreMealScan());
    }
}
