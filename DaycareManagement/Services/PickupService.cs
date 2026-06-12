using DaycareManagement.Models;
namespace DaycareManagement.Services;
public class PickupService
{
    private readonly DataStore _store;
    public PickupService(DataStore store) => _store = store;
    public void AddContact(int childId, string name, string relationship, string phone)
    {
        var contacts = _store.LoadPickupContacts();
        contacts.Add(new PickupContact { ChildId = childId, Name = name, Relationship = relationship, Phone = phone });
        _store.SavePickupContacts(contacts);
    }
    public List<PickupContact> GetContacts(int childId) =>
        _store.LoadPickupContacts().Where(p => p.ChildId == childId).ToList();
    public PickupContact? VerifyPickup(int childId, string personName) =>
        _store.LoadPickupContacts().FirstOrDefault(p =>
            p.ChildId == childId && p.Name.Equals(personName, StringComparison.OrdinalIgnoreCase));
}
