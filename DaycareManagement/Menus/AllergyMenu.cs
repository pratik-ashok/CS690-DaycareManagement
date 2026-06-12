using DaycareManagement.Services;

namespace DaycareManagement.Menus;

public class AllergyMenu
{
    private readonly AllergyService    _allergy;
    private readonly AttendanceService _attendance;

    public AllergyMenu(AllergyService allergy, AttendanceService attendance)
    {
        _allergy    = allergy;
        _attendance = attendance;
    }

    public void Run()
    {
        bool back = false;
        while (!back)
        {
            UI.PrintHeader("Allergy Management  [FR7, FR8]");
            Console.WriteLine("  1. Add / Update Allergy for Child  [FR7]");
            Console.WriteLine("  2. View Allergies for Child        [FR7]");
            Console.WriteLine("  3. Pre-Meal Allergy Alert Check    [FR8]");
            Console.WriteLine("  0. Back");
            Console.Write("\n  Select: ");
            switch (Console.ReadLine()?.Trim())
            {
                case "1": AddAllergy();          break;
                case "2": ViewAllergies();       break;
                case "3": PreMealAllergyCheck(); break;
                case "0": back = true;           break;
                default:  UI.PrintError("Invalid option."); break;
            }
        }
    }

    private void AddAllergy()
    {
        UI.PrintHeader("Add / Update Allergy  [FR7]");
        var children = _attendance.GetChildren();
        if (!children.Any()) { UI.PrintError("No children registered. Add children first."); return; }
        foreach (var c in children) Console.WriteLine($"    [{c.Id}] {c.Name}");
        Console.Write("\n  Enter child ID: ");
        if (!int.TryParse(Console.ReadLine(), out int id) || children.All(c => c.Id != id))
        { UI.PrintError("Invalid child ID."); return; }
        var child = children.First(c => c.Id == id);
        Console.Write("  Allergen (e.g. Peanuts, Dairy, Gluten): ");
        string allergen = Console.ReadLine()?.Trim() ?? "";
        if (string.IsNullOrEmpty(allergen)) { UI.PrintError("Allergen cannot be empty."); return; }
        Console.Write("  Severity (Mild / Moderate / Severe): ");
        string severity = Console.ReadLine()?.Trim() ?? "Mild";
        Console.Write("  Notes / Action to take: ");
        string notes = Console.ReadLine()?.Trim() ?? "";
        _allergy.AddOrUpdateAllergy(id, allergen, severity, notes);
        UI.PrintSuccess($"Allergy '{allergen}' saved for {child.Name}.");
    }

    private void ViewAllergies()
    {
        UI.PrintHeader("View Allergies  [FR7]");
        var children = _attendance.GetChildren();
        if (!children.Any()) { UI.PrintError("No children registered."); return; }
        foreach (var c in children) Console.WriteLine($"    [{c.Id}] {c.Name}");
        Console.Write("\n  Enter child ID: ");
        if (!int.TryParse(Console.ReadLine(), out int id) || children.All(c => c.Id != id))
        { UI.PrintError("Invalid child ID."); return; }
        var child = children.First(c => c.Id == id);
        var allergies = _allergy.GetAllergies(id);
        UI.PrintHeader($"Allergies for {child.Name}");
        if (!allergies.Any())
            Console.WriteLine("  No allergies on record.");
        else
            foreach (var a in allergies)
                Console.WriteLine($"  - {a.Allergen} [{a.Severity}]: {a.Notes}");
        UI.Pause();
    }

    private void PreMealAllergyCheck()
    {
        UI.PrintHeader("Pre-Meal Allergy Alert  [FR8]");
        Console.WriteLine("  Scanning all present children for known allergies...\n");
        var results = _allergy.PreMealScan();
        if (!results.Any())
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("  All clear -- no allergy alerts for present children.");
            Console.ResetColor();
        }
        else
        {
            foreach (var kvp in results)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"  *** ALLERGY ALERT -- {kvp.Key.Name.ToUpper()} ***");
                Console.ResetColor();
                foreach (var a in kvp.Value)
                    Console.WriteLine($"    - {a.Allergen} [{a.Severity}]: {a.Notes}");
                Console.WriteLine();
            }
        }
        UI.Pause();
    }
}
