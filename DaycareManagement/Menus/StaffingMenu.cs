using DaycareManagement.Services;

namespace DaycareManagement.Menus;

public class StaffingMenu
{
    private readonly StaffingService _staffing;

    public StaffingMenu(StaffingService staffing) => _staffing = staffing;

    public void Run()
    {
        bool back = false;
        while (!back)
        {
            UI.PrintHeader("Staffing Management  [FR4, FR5, FR6]");
            Console.WriteLine("  1. Add Staff Member");
            Console.WriteLine("  2. Staff Check In");
            Console.WriteLine("  3. Staff Check Out");
            Console.WriteLine("  4. View Occupancy & Ratio  [FR4, FR5, FR6]");
            Console.WriteLine("  0. Back");
            Console.Write("\n  Select: ");
            switch (Console.ReadLine()?.Trim())
            {
                case "1": AddStaff();      break;
                case "2": StaffCheckIn();  break;
                case "3": StaffCheckOut(); break;
                case "4": ViewRatio();     break;
                case "0": back = true;     break;
                default:  UI.PrintError("Invalid option."); break;
            }
        }
    }

    private void AddStaff()
    {
        UI.PrintHeader("Add Staff Member");
        Console.Write("  Staff name: ");
        string name = Console.ReadLine()?.Trim() ?? "";
        if (string.IsNullOrEmpty(name)) { UI.PrintError("Name cannot be empty."); return; }
        var member = _staffing.AddStaff(name);
        UI.PrintSuccess($"Staff member '{member.Name}' added with ID {member.Id}.");
    }

    private void StaffCheckIn()
    {
        UI.PrintHeader("Staff Check In");
        var staff   = _staffing.GetStaff();
        var offDuty = staff.Where(s => !s.IsOnDuty).ToList();
        if (!offDuty.Any()) { UI.PrintError("All staff are already on duty."); return; }
        foreach (var s in offDuty) Console.WriteLine($"    [{s.Id}] {s.Name}");
        Console.Write("\n  Enter staff ID: ");
        if (!int.TryParse(Console.ReadLine(), out int id)) { UI.PrintError("Invalid ID."); return; }
        if (_staffing.CheckInStaff(id))
            UI.PrintSuccess("Staff member is now on duty.");
        else
            UI.PrintError("Could not check in: staff member not found or already on duty.");
    }

    private void StaffCheckOut()
    {
        UI.PrintHeader("Staff Check Out");
        var staff  = _staffing.GetStaff();
        var onDuty = staff.Where(s => s.IsOnDuty).ToList();
        if (!onDuty.Any()) { UI.PrintError("No staff currently on duty."); return; }
        foreach (var s in onDuty) Console.WriteLine($"    [{s.Id}] {s.Name}");
        Console.Write("\n  Enter staff ID: ");
        if (!int.TryParse(Console.ReadLine(), out int id)) { UI.PrintError("Invalid ID."); return; }
        if (_staffing.CheckOutStaff(id))
            UI.PrintSuccess("Staff member has been checked out.");
        else
            UI.PrintError("Could not check out: staff member not found or already off duty.");
    }

    private void ViewRatio()
    {
        UI.PrintHeader("Occupancy & Ratio  [FR4, FR5, FR6]");
        int children   = _staffing.GetPresentChildrenCount();
        int staffCount = _staffing.GetOnDutyStaffCount();

        // FR4: Display count
        Console.WriteLine($"  Children currently present : {children}");
        Console.WriteLine($"  Staff currently on duty    : {staffCount}");
        Console.WriteLine();

        // FR5: Calculate and display ratio
        var ratio = _staffing.GetRatio();
        if (ratio == null)
        {
            Console.WriteLine("  Staff-to-child ratio : N/A  (no children present)");
        }
        else
        {
            string display = staffCount == 0
                ? "0 staff (critical -- no coverage!)"
                : $"1 : {Math.Ceiling(children / (double)staffCount):0}";
            Console.WriteLine($"  Staff-to-child ratio : {display}");

            // FR6: Warn if below threshold
            if (_staffing.IsRatioBelowThreshold())
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("\n  *** WARNING *** Ratio is below the required minimum of 1:8.");
                Console.WriteLine("  Please ensure adequate staff coverage immediately.");
                Console.ResetColor();
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("\n  Ratio is within safe limits (minimum 1:8 met).");
                Console.ResetColor();
            }
        }
        UI.Pause();
    }
}
