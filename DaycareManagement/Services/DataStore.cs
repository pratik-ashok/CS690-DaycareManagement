using System.Text.Json;
using DaycareManagement.Models;

namespace DaycareManagement.Services;

public class DataStore
{
    private readonly string _dataDir;
    private static readonly JsonSerializerOptions Opts = new() { WriteIndented = true };

    public DataStore(string baseDirectory)
    {
        _dataDir = Path.Combine(baseDirectory, "data");
        Directory.CreateDirectory(_dataDir);
    }

    private string ChildrenFile    => Path.Combine(_dataDir, "children.json");
    private string AllergiesFile   => Path.Combine(_dataDir, "allergies.json");
    private string PickupFile      => Path.Combine(_dataDir, "pickup_contacts.json");
    private string AttendanceFile  => Path.Combine(_dataDir, "attendance.json");
    private string StaffFile       => Path.Combine(_dataDir, "staff.json");
    private string ScheduleFile    => Path.Combine(_dataDir, "schedule.json");
    private string SummariesFile   => Path.Combine(_dataDir, "daily_summaries.json");

    public List<Child>            LoadChildren()         => Load<List<Child>>(ChildrenFile)             ?? new();
    public void                   SaveChildren(List<Child> d)           => Save(ChildrenFile, d);

    public List<AllergyRecord>    LoadAllergies()        => Load<List<AllergyRecord>>(AllergiesFile)    ?? new();
    public void                   SaveAllergies(List<AllergyRecord> d)  => Save(AllergiesFile, d);

    public List<PickupContact>    LoadPickupContacts()   => Load<List<PickupContact>>(PickupFile)       ?? new();
    public void                   SavePickupContacts(List<PickupContact> d) => Save(PickupFile, d);

    public List<AttendanceRecord> LoadAttendance()       => Load<List<AttendanceRecord>>(AttendanceFile) ?? new();
    public void                   SaveAttendance(List<AttendanceRecord> d) => Save(AttendanceFile, d);

    public List<StaffMember>      LoadStaff()            => Load<List<StaffMember>>(StaffFile)          ?? new();
    public void                   SaveStaff(List<StaffMember> d)        => Save(StaffFile, d);

    public List<ScheduleEntry>    LoadSchedule()         => Load<List<ScheduleEntry>>(ScheduleFile)     ?? new();
    public void                   SaveSchedule(List<ScheduleEntry> d)   => Save(ScheduleFile, d);

    public List<DailySummary>     LoadDailySummaries()   => Load<List<DailySummary>>(SummariesFile)     ?? new();
    public void                   SaveDailySummaries(List<DailySummary> d) => Save(SummariesFile, d);

    private T? Load<T>(string path)
    {
        if (!File.Exists(path)) return default;
        return JsonSerializer.Deserialize<T>(File.ReadAllText(path));
    }

    private void Save<T>(string path, T data) =>
        File.WriteAllText(path, JsonSerializer.Serialize(data, Opts));
}
