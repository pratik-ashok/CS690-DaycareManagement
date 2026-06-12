using DaycareManagement.Models;
namespace DaycareManagement.Services;
public class AllergyService
{
    private readonly DataStore _store;
    public AllergyService(DataStore store) => _store = store;
    public void AddOrUpdateAllergy(int childId, string allergen, string severity, string notes)
    {
        var allergies = _store.LoadAllergies();
        var existing = allergies.FirstOrDefault(a =>
            a.ChildId == childId && a.Allergen.Equals(allergen, StringComparison.OrdinalIgnoreCase));
        if (existing != null) { existing.Severity = severity; existing.Notes = notes; }
        else allergies.Add(new AllergyRecord { ChildId = childId, Allergen = allergen, Severity = severity, Notes = notes });
        _store.SaveAllergies(allergies);
    }
    public List<AllergyRecord> GetAllergies(int childId) =>
        _store.LoadAllergies().Where(a => a.ChildId == childId).ToList();
    public Dictionary<Child, List<AllergyRecord>> PreMealScan()
    {
        var presentChildren = _store.LoadChildren().Where(c => c.IsCheckedIn).ToList();
        var allAllergies    = _store.LoadAllergies();
        var result          = new Dictionary<Child, List<AllergyRecord>>();
        foreach (var child in presentChildren)
        {
            var childAllergies = allAllergies.Where(a => a.ChildId == child.Id).ToList();
            if (childAllergies.Any()) result[child] = childAllergies;
        }
        return result;
    }
}
