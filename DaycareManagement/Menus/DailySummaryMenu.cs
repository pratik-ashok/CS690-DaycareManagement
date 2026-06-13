using DaycareManagement.Services;

namespace DaycareManagement.Menus;

public class DailySummaryMenu
{
    private readonly DailySummaryService _summary;
    private readonly AttendanceService   _attendance;
    private readonly StaffingService     _staffing;
    private readonly AllergyService      _allergy;

    public DailySummaryMenu(DailySummaryService summary, AttendanceService attendance,
                             StaffingService staffing, AllergyService allergy)
    {
        _summary    = summary;
        _attendance = attendance;
        _staffing   = staffing;
        _allergy    = allergy;
    }

    public void Run()
    {
        bool back = false;
        while (!back)
        {
            UI.PrintHeader("Daily Summary  [FR11, FR12]");
            Console.WriteLine("  1. View Today's Summary    [FR11]");
            Console.WriteLine("  2. Add Note to Summary     [FR12]");
            Console.WriteLine("  0. Back");
            Console.Write("\n  Select: ");
            switch (Console.ReadLine()?.Trim())
            {
                case "1": ViewSummary(); break;
                case "2": AddNote();     break;
                case "0": back = true;  break;
                default:  UI.PrintError("Invalid option."); break;
            }
        }
    }

    private void ViewSummary()
    {
        string today = DateTime.Today.ToString("yyyy-MM-dd");
        UI.PrintHeader($"Daily Summary -- {today}  [FR11]");

        // Attendance
        Console.WriteLine("  === ATTENDANCE ===");
        var children  = _attendance.GetChildren();
        var todayRecs = _attendance.GetTodayRecords(today);
        if (!todayRecs.Any())
        {
            Console.WriteLine("  No attendance records for today.");
        }
        else
        {
            Console.WriteLine(string.Format("  {0,-22} {1,-10} {2,-12} Status", "Name", "Check-In", "Check-Out"));
            Console.WriteLine("  " + new string('-', 55));
            foreach (var child in children)
            {
                var rec = todayRecs.FirstOrDefault(r => r.ChildId == child.Id);
                if (rec == null) continue;
                string status = child.IsCheckedIn ? "Present" : "Left";
                Console.WriteLine(string.Format("  {0,-22} {1,-10} {2,-12} {3}",
                    child.Name, rec.CheckInTime ?? "-", rec.CheckOutTime ?? "-", status));
            }
        }

        // Staffing ratio
        Console.WriteLine("\n  === STAFFING RATIO ===");
        int childCount = _staffing.GetPresentChildrenCount();
        int staffCount = _staffing.GetOnDutyStaffCount();
        Console.WriteLine($"  Children present : {childCount}");
        Console.WriteLine($"  Staff on duty    : {staffCount}");
        var ratio = _staffing.GetRatio();
        if (ratio == null)
        {
            Console.WriteLine("  Ratio            : N/A (no children present)");
        }
        else
        {
            string ratioDisplay = staffCount == 0
                ? "No staff on duty"
                : $"1 : {Math.Ceiling(childCount / (double)staffCount):0}";
            Console.WriteLine($"  Ratio            : {ratioDisplay}");
            if (_staffing.IsRatioBelowThreshold())
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("  Status           : BELOW MINIMUM (1:8 required)");
                Console.ResetColor();
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("  Status           : Safe");
                Console.ResetColor();
            }
        }

        // Allergy alerts
        Console.WriteLine("\n  === ALLERGY ALERTS (children currently present) ===");
        var alerts = _allergy.PreMealScan();
        if (!alerts.Any())
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("  No allergy alerts.");
            Console.ResetColor();
        }
        else
        {
            foreach (var kvp in alerts)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.Write($"  {kvp.Key.Name}: ");
                Console.ResetColor();
                Console.WriteLine(string.Join(", ", kvp.Value.Select(a => $"{a.Allergen} [{a.Severity}]")));
            }
        }

        // Staff notes
        Console.WriteLine("\n  === STAFF NOTES ===");
        var notes = _summary.GetNotes(today);
        if (!notes.Any())
        {
            Console.WriteLine("  No notes for today.");
        }
        else
        {
            for (int i = 0; i < notes.Count; i++)
                Console.WriteLine($"  {i + 1}. {notes[i]}");
        }

        UI.Pause();
    }

    private void AddNote()
    {
        string today = DateTime.Today.ToString("yyyy-MM-dd");
        UI.PrintHeader("Add Note to Daily Summary  [FR12]");
        Console.WriteLine($"  Date: {today}");
        Console.Write("\n  Enter note: ");
        string note = Console.ReadLine()?.Trim() ?? "";
        if (string.IsNullOrEmpty(note)) { UI.PrintError("Note cannot be empty."); return; }
        _summary.AddNote(today, note);
        UI.PrintSuccess("Note added to today's summary.");
    }
}
