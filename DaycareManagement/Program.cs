using DaycareManagement.Menus;
using DaycareManagement.Services;

namespace DaycareManagement;

class Program
{
    static void Main(string[] args)
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;
        UI.PrintBanner();

        var dataStore  = new DataStore(AppDomain.CurrentDomain.BaseDirectory);
        var attendance = new AttendanceService(dataStore);
        var allergy    = new AllergyService(dataStore);
        var pickup     = new PickupService(dataStore);
        var staffing   = new StaffingService(dataStore);

        var attendanceMenu = new AttendanceMenu(attendance, allergy, staffing);
        var allergyMenu    = new AllergyMenu(allergy, attendance);
        var pickupMenu     = new PickupMenu(pickup, attendance);
        var staffingMenu   = new StaffingMenu(staffing);

        bool running = true;
        while (running)
        {
            UI.PrintHeader("Main Menu");
            Console.WriteLine("  1. Children & Attendance  [FR2, FR3]");
            Console.WriteLine("  2. Allergy Management     [FR7, FR8]");
            Console.WriteLine("  3. Pickup Management      [FR9, FR10]");
            Console.WriteLine("  4. Staffing & Ratio       [FR4, FR5, FR6]");
            Console.WriteLine("  0. Exit");
            Console.Write("\n  Select: ");
            switch (Console.ReadLine()?.Trim())
            {
                case "1": attendanceMenu.Run(); break;
                case "2": allergyMenu.Run();    break;
                case "3": pickupMenu.Run();     break;
                case "4": staffingMenu.Run();   break;
                case "0": running = false;      break;
                default:  UI.PrintError("Invalid option."); break;
            }
        }
        Console.WriteLine("\n  Goodbye!\n");
    }
}
