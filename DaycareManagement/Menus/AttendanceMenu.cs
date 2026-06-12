using DaycareManagement.Services;

namespace DaycareManagement.Menus;

public class AttendanceMenu
{
    private readonly AttendanceService _attendance;
    private readonly AllergyService    _allergy;
    private readonly StaffingService   _staffing;

    public AttendanceMenu(AttendanceService attendance, AllergyService allergy, StaffingService staffing)
    {
        _attendance = attendance;
        _allergy    = allergy;
        _staffing   = staffing;
    }

    public void Run()
    {
        bool back = false;
        while (!back)
        {
            UI.PrintHeader("Children & Attendance  [FR2, FR3]");
            Console.WriteLine("  1. Add Child");
            Console.WriteLine("  2. List All Children");
            Console.WriteLine("  3. Schedule Child for Today    [FR3]");
            Console.WriteLine("  4. Check In Child");
            Console.WriteLine("  5. Check Out Child");
            Console.WriteLine("  6. View Today's Attendance");
            Console.WriteLine("  0. Back");
            Console.Write("\n  Select: ");
            switch (Console.ReadLine()?.Trim())
            {
                case "1": AddChild();            break;
                case "2": ListChildren();        break;
                case "3": ScheduleChild();       break;
                case "4": CheckInChild();        break;
                case "5": CheckOutChild();       break;
                case "6": ViewTodayAttendance(); break;
                case "0": back = true;           break;
                default:  UI.PrintError("Invalid option."); break;
            }
        }
    }

    private void AddChild()
    {
        UI.PrintHeader("Add Child");
        Console.Write("  Name: ");
        string name = Console.ReadLine()?.Trim() ?? "";
        if (string.IsNullOrEmpty(name)) { UI.PrintError("Name cannot be empty."); return; }
        Console.Write("  Date of birth (YYYY-MM-DD): ");
        string dob = Console.ReadLine()?.Trim() ?? "";
        var child = _attendance.AddChild(name, dob);
        UI.PrintSuccess($"Child '{child.Name}' added with ID {child.Id}.");
    }

    private void ListChildren()
    {
        var children = _attendance.GetChildren();
        UI.PrintHeader("All Registered Children");
        if (!children.Any()) { Console.WriteLine("  No children registered yet."); UI.Pause(); return; }
        Console.WriteLine(string.Format("  {0,-5} {1,-22} {2,-14} Status", "ID", "Name", "Date of Birth"));
        Console.WriteLine("  " + new string('-', 55));
        foreach (var c in children)
        {
            string status = c.IsCheckedIn ? "Present" : "Absent";
            Console.WriteLine(string.Format("  {0,-5} {1,-22} {2,-14} {3}", c.Id, c.Name, c.DateOfBirth, status));
        }
        UI.Pause();
    }

    private void ScheduleChild()
    {
        UI.PrintHeader("Schedule Child for Today  [FR3]");
        var children = _attendance.GetChildren();
        if (!children.Any()) { UI.PrintError("No children registered."); return; }
        foreach (var c in children)
            Console.WriteLine($"    [{c.Id}] {c.Name}");
        Console.Write("\n  Enter child ID: ");
        if (!int.TryParse(Console.ReadLine(), out int id) || children.All(c => c.Id != id))
        { UI.PrintError("Invalid child ID."); return; }
        string today = DateTime.Today.ToString("yyyy-MM-dd");
        _attendance.AddToSchedule(id, today);
        var child = children.First(c => c.Id == id);
        UI.PrintSuccess($"{child.Name} has been added to today's schedule.");
    }

    private void CheckInChild()
    {
        UI.PrintHeader("Check In Child  [FR2, FR3]");
        var children = _attendance.GetChildren();
        var absent   = children.Where(c => !c.IsCheckedIn).ToList();
        if (!absent.Any()) { UI.PrintError("No absent children to check in."); return; }

        Console.WriteLine("  Absent children:");
        foreach (var c in absent) Console.WriteLine($"    [{c.Id}] {c.Name}");
        Console.Write("\n  Enter child ID: ");
        if (!int.TryParse(Console.ReadLine(), out int id)) { UI.PrintError("Invalid ID."); return; }
        var child = children.FirstOrDefault(c => c.Id == id);
        if (child == null)     { UI.PrintError("Child not found."); return; }
        if (child.IsCheckedIn) { UI.PrintError($"{child.Name} is already checked in."); return; }

        string today = DateTime.Today.ToString("yyyy-MM-dd");

        // FR3: Alert if child is not on today's schedule
        if (!_attendance.IsScheduled(id, today))
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"\n  *** SCHEDULE ALERT *** {child.Name} is NOT on today's scheduled list!");
            Console.WriteLine("  This is an unscheduled arrival. Please notify management.");
            Console.ResetColor();
        }

        _attendance.CheckIn(id, today);

        // FR7: Show allergy alert on check-in
        var alerts = _allergy.GetAllergies(id);
        if (alerts.Any())
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"\n  *** ALLERGY ALERT for {child.Name} ***");
            foreach (var a in alerts)
                Console.WriteLine($"    - {a.Allergen} [{a.Severity}]: {a.Notes}");
            Console.ResetColor();
        }

        // FR6: Ratio warning after check-in
        if (_staffing.IsRatioBelowThreshold())
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("\n  *** RATIO WARNING *** Staff-to-child ratio is below the safe minimum (1:8)!");
            Console.WriteLine("  Please ensure additional staff coverage immediately.");
            Console.ResetColor();
        }

        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"\n  [OK] {child.Name} checked in at {DateTime.Now:HH:mm}.");
        Console.ResetColor();
        UI.Pause();
    }

    private void CheckOutChild()
    {
        UI.PrintHeader("Check Out Child  [FR2]");
        var children = _attendance.GetChildren();
        var present  = children.Where(c => c.IsCheckedIn).ToList();
        if (!present.Any()) { UI.PrintError("No children currently checked in."); return; }

        Console.WriteLine("  Present children:");
        foreach (var c in present) Console.WriteLine($"    [{c.Id}] {c.Name}");
        Console.Write("\n  Enter child ID: ");
        if (!int.TryParse(Console.ReadLine(), out int id)) { UI.PrintError("Invalid ID."); return; }
        var child = children.FirstOrDefault(c => c.Id == id);
        if (child == null)      { UI.PrintError("Child not found."); return; }
        if (!child.IsCheckedIn) { UI.PrintError($"{child.Name} is not currently checked in."); return; }

        string today = DateTime.Today.ToString("yyyy-MM-dd");
        _attendance.CheckOut(id, today);
        UI.PrintSuccess($"{child.Name} checked out at {DateTime.Now:HH:mm}.");
    }

    private void ViewTodayAttendance()
    {
        UI.PrintHeader("Today's Attendance");
        var children  = _attendance.GetChildren();
        string today  = DateTime.Today.ToString("yyyy-MM-dd");
        var todayRecs = _attendance.GetTodayRecords(today);
        Console.WriteLine($"  Date: {today}\n");
        Console.WriteLine(string.Format("  {0,-22} {1,-10} {2,-12} Status", "Name", "Check-In", "Check-Out"));
        Console.WriteLine("  " + new string('-', 58));
        foreach (var child in children)
        {
            var rec    = todayRecs.FirstOrDefault(a => a.ChildId == child.Id);
            string status = child.IsCheckedIn ? "Present" : (rec != null ? "Left" : "Absent");
            Console.WriteLine(string.Format("  {0,-22} {1,-10} {2,-12} {3}",
                child.Name, rec?.CheckInTime ?? "-", rec?.CheckOutTime ?? "-", status));
        }
        UI.Pause();
    }
}
