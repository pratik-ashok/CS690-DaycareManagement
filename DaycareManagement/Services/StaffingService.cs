using DaycareManagement.Models;
namespace DaycareManagement.Services;
public class StaffingService
{
    private readonly DataStore _store;
    public const double MinStaffToChildRatio = 1.0 / 8.0;
    public StaffingService(DataStore store) => _store = store;
    public StaffMember AddStaff(string name)
    {
        var staff = _store.LoadStaff();
        int newId = staff.Count > 0 ? staff.Max(s => s.Id) + 1 : 1;
        var member = new StaffMember { Id = newId, Name = name };
        staff.Add(member);
        _store.SaveStaff(staff);
        return member;
    }
    public List<StaffMember> GetStaff() => _store.LoadStaff();
    public bool CheckInStaff(int staffId)
    {
        var staff = _store.LoadStaff();
        var member = staff.FirstOrDefault(s => s.Id == staffId);
        if (member == null || member.IsOnDuty) return false;
        member.IsOnDuty = true;
        _store.SaveStaff(staff);
        return true;
    }
    public bool CheckOutStaff(int staffId)
    {
        var staff = _store.LoadStaff();
        var member = staff.FirstOrDefault(s => s.Id == staffId);
        if (member == null || !member.IsOnDuty) return false;
        member.IsOnDuty = false;
        _store.SaveStaff(staff);
        return true;
    }
    public int GetPresentChildrenCount() => _store.LoadChildren().Count(c => c.IsCheckedIn);
    public int GetOnDutyStaffCount() => _store.LoadStaff().Count(s => s.IsOnDuty);
    public double? GetRatio()
    {
        int children = GetPresentChildrenCount();
        if (children == 0) return null;
        return (double)GetOnDutyStaffCount() / children;
    }
    public bool IsRatioBelowThreshold()
    {
        var ratio = GetRatio();
        if (ratio == null) return false;
        return ratio < MinStaffToChildRatio;
    }
}
