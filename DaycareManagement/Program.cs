using DaycareManagement.Menus;
using DaycareManagement.Services;

Console.OutputEncoding = System.Text.Encoding.UTF8;

var store      = new DataStore(AppDomain.CurrentDomain.BaseDirectory);
var attendance = new AttendanceService(store);
var staffing   = new StaffingService(store);
var allergy    = new AllergyService(store);
var pickup     = new PickupService(store);
var summary    = new DailySummaryService(store);

var attendanceMenu = new AttendanceMenu(attendance, allergy, staffing);
var staffingMenu   = new StaffingMenu(staffing);
var allergyMenu    = new AllergyMenu(allergy, attendance);
var pickupMenu     = new PickupMenu(pickup, attendance);
var summaryMenu    = new DailySummaryMenu(summary, attendance, staffing, allergy);

UI.PrintBanner();

bool running = true;
while (running)
{
    UI.PrintHeader("Main Menu");
    Console.WriteLine("  1. Children & Attendance   [FR2, FR3]");
    Console.WriteLine("  2. Staffing Management     [FR4, FR5, FR6]");
    Console.WriteLine("  3. Allergy Management      [FR7, FR8]");
    Console.WriteLine("  4. Pickup Management       [FR9, FR10]");
    Console.WriteLine("  5. Daily Summary           [FR11, FR12]");
    Console.WriteLine("  0. Exit");
    Console.Write("\n  Select: ");
    switch (Console.ReadLine()?.Trim())
    {
        case "1": attendanceMenu.Run(); break;
        case "2": staffingMenu.Run();   break;
        case "3": allergyMenu.Run();    break;
        case "4": pickupMenu.Run();     break;
        case "5": summaryMenu.Run();    break;
        case "0": running = false;      break;
        default:  UI.PrintError("Invalid option."); break;
    }
}

Console.WriteLine("\n  Goodbye!\n");
