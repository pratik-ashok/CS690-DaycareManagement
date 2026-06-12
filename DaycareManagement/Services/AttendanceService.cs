using DaycareManagement.Models;
namespace DaycareManagement.Services;
public class AttendanceService
{
    private readonly DataStore _store;
    public AttendanceService(DataStore store) => _store = store;
    public List<Child> GetChildren() => _store.LoadChildren();
    public Child AddChild(string name, string dateOfBirth)
    {
        var children = _store.LoadChildren();
        int newId = children.Count > 0 ? children.Max(c => c.Id) + 1 : 1;
        var child = new Child { Id = newId, Name = name, DateOfBirth = dateOfBirth };
        children.Add(child);
        _store.SaveChildren(children);
        return child;
    }
    public bool CheckIn(int childId, string today)
    {
        var children = _store.LoadChildren();
        var child = children.FirstOrDefault(c => c.Id == childId);
        if (child == null || child.IsCheckedIn) return false;
        child.IsCheckedIn = true;
        _store.SaveChildren(children);
        var attendance = _store.LoadAttendance();
        var rec = attendance.FirstOrDefault(a => a.ChildId == childId && a.Date == today);
        if (rec == null)
            attendance.Add(new AttendanceRecord { ChildId = childId, Date = today, CheckInTime = DateTime.Now.ToString("HH:mm") });
        else
            rec.CheckInTime = DateTime.Now.ToString("HH:mm");
        _store.SaveAttendance(attendance);
        return true;
    }
    public bool CheckOut(int childId, string today)
    {
        var children = _store.LoadChildren();
        var child = children.FirstOrDefault(c => c.Id == childId);
        if (child == null || !child.IsCheckedIn) return false;
        child.IsCheckedIn = false;
        _store.SaveChildren(children);
        var attendance = _store.LoadAttendance();
        var rec = attendance.FirstOrDefault(a => a.ChildId == childId && a.Date == today);
        if (rec != null) rec.CheckOutTime = DateTime.Now.ToString("HH:mm");
        _store.SaveAttendance(attendance);
        return true;
    }
    public List<AttendanceRecord> GetTodayRecords(string today) =>
        _store.LoadAttendance().Where(a => a.Date == today).ToList();
    public bool IsScheduled(int childId, string today) =>
        _store.LoadSchedule().Any(s => s.ChildId == childId && s.Date == today);
    public void AddToSchedule(int childId, string today)
    {
        var schedule = _store.LoadSchedule();
        if (!schedule.Any(s => s.ChildId == childId && s.Date == today))
        {
            schedule.Add(new ScheduleEntry { ChildId = childId, Date = today });
            _store.SaveSchedule(schedule);
        }
    }
}
