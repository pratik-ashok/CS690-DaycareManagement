using DaycareManagement.Services;

namespace DaycareManagement.Menus;

public class PickupMenu
{
    private readonly PickupService     _pickup;
    private readonly AttendanceService _attendance;

    public PickupMenu(PickupService pickup, AttendanceService attendance)
    {
        _pickup     = pickup;
        _attendance = attendance;
    }

    public void Run()
    {
        bool back = false;
        while (!back)
        {
            UI.PrintHeader("Pickup Management  [FR9, FR10]");
            Console.WriteLine("  1. Add Pickup Contact for Child       [FR9]");
            Console.WriteLine("  2. View Pickup Contacts for Child     [FR9]");
            Console.WriteLine("  3. Verify Pickup Authorization        [FR10]");
            Console.WriteLine("  0. Back");
            Console.Write("\n  Select: ");
            switch (Console.ReadLine()?.Trim())
            {
                case "1": AddPickupContact();   break;
                case "2": ViewPickupContacts(); break;
                case "3": VerifyPickup();       break;
                case "0": back = true;          break;
                default:  UI.PrintError("Invalid option."); break;
            }
        }
    }

    private void AddPickupContact()
    {
        UI.PrintHeader("Add Pickup Contact  [FR9]");
        var children = _attendance.GetChildren();
        if (!children.Any()) { UI.PrintError("No children registered. Add children first."); return; }
        foreach (var c in children) Console.WriteLine($"    [{c.Id}] {c.Name}");
        Console.Write("\n  Enter child ID: ");
        if (!int.TryParse(Console.ReadLine(), out int id) || children.All(c => c.Id != id))
        { UI.PrintError("Invalid child ID."); return; }
        var child = children.First(c => c.Id == id);
        Console.Write("  Contact name: ");
        string name = Console.ReadLine()?.Trim() ?? "";
        if (string.IsNullOrEmpty(name)) { UI.PrintError("Name cannot be empty."); return; }
        Console.Write("  Relationship (e.g. Parent, Grandparent, Sibling): ");
        string rel = Console.ReadLine()?.Trim() ?? "";
        Console.Write("  Phone number: ");
        string phone = Console.ReadLine()?.Trim() ?? "";
        _pickup.AddContact(id, name, rel, phone);
        UI.PrintSuccess($"Contact '{name}' added for {child.Name}.");
    }

    private void ViewPickupContacts()
    {
        UI.PrintHeader("View Pickup Contacts  [FR9]");
        var children = _attendance.GetChildren();
        if (!children.Any()) { UI.PrintError("No children registered."); return; }
        foreach (var c in children) Console.WriteLine($"    [{c.Id}] {c.Name}");
        Console.Write("\n  Enter child ID: ");
        if (!int.TryParse(Console.ReadLine(), out int id) || children.All(c => c.Id != id))
        { UI.PrintError("Invalid child ID."); return; }
        var child = children.First(c => c.Id == id);
        var contacts = _pickup.GetContacts(id);
        UI.PrintHeader($"Authorized Contacts -- {child.Name}");
        if (!contacts.Any())
            Console.WriteLine("  No pickup contacts on file.");
        else
        {
            Console.WriteLine(string.Format("  {0,-22} {1,-16} Phone", "Name", "Relationship"));
            Console.WriteLine("  " + new string('-', 55));
            foreach (var p in contacts)
                Console.WriteLine(string.Format("  {0,-22} {1,-16} {2}", p.Name, p.Relationship, p.Phone));
        }
        UI.Pause();
    }

    private void VerifyPickup()
    {
        UI.PrintHeader("Verify Pickup Authorization  [FR10]");
        var children = _attendance.GetChildren().Where(c => c.IsCheckedIn).ToList();
        if (!children.Any()) { UI.PrintError("No children currently checked in."); return; }
        Console.WriteLine("  Present children:");
        foreach (var c in children) Console.WriteLine($"    [{c.Id}] {c.Name}");
        Console.Write("\n  Enter child ID: ");
        if (!int.TryParse(Console.ReadLine(), out int id) || children.All(c => c.Id != id))
        { UI.PrintError("Invalid child ID."); return; }
        var child = children.First(c => c.Id == id);
        Console.Write("  Person requesting pickup (full name): ");
        string personName = Console.ReadLine()?.Trim() ?? "";
        var match = _pickup.VerifyPickup(id, personName);
        Console.WriteLine();
        if (match != null)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"  AUTHORIZED -- {match.Name} ({match.Relationship}) is on the approved pickup list for {child.Name}.");
            Console.ResetColor();
        }
        else
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"  NOT AUTHORIZED -- '{personName}' is NOT on the approved pickup list for {child.Name}.");
            Console.WriteLine("  Do NOT release the child. Contact the parent or guardian immediately.");
            Console.ResetColor();
        }
        UI.Pause();
    }
}
