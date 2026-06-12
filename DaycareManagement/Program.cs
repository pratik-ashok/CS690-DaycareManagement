using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace DaycareManagement
{
    class Child
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public string DateOfBirth { get; set; } = "";
        public bool IsCheckedIn { get; set; } = false;
    }

    class AllergyRecord
    {
        public int ChildId { get; set; }
        public string Allergen { get; set; } = "";
        public string Severity { get; set; } = "Mild";
        public string Notes { get; set; } = "";
    }

    class PickupContact
    {
        public int ChildId { get; set; }
        public string Name { get; set; } = "";
        public string Relationship { get; set; } = "";
        public string Phone { get; set; } = "";
    }

    class AttendanceRecord
    {
        public int ChildId { get; set; }
        public string Date { get; set; } = "";
        public string? CheckInTime { get; set; }
        public string? CheckOutTime { get; set; }
    }

    static class DataStore
    {
        private static readonly string DataDir = Path.Combine(
            AppDomain.CurrentDomain.BaseDirectory, "data");
        private static string ChildrenFile   => Path.Combine(DataDir, "children.json");
        private static string AllergiesFile  => Path.Combine(DataDir, "allergies.json");
        private static string PickupFile     => Path.Combine(DataDir, "pickup_contacts.json");
        private static string AttendanceFile => Path.Combine(DataDir, "attendance.json");
        private static readonly JsonSerializerOptions Opts = new() { WriteIndented = true };
        static DataStore() { Directory.CreateDirectory(DataDir); }
        public static List<Child>            LoadChildren()       => Load<List<Child>>(ChildrenFile)            ?? new();
        public static void                   SaveChildren(List<Child> d)       => Save(ChildrenFile, d);
        public static List<AllergyRecord>    LoadAllergies()      => Load<List<AllergyRecord>>(AllergiesFile)   ?? new();
        public static void                   SaveAllergies(List<AllergyRecord> d)  => Save(AllergiesFile, d);
        public static List<PickupContact>    LoadPickupContacts() => Load<List<PickupContact>>(PickupFile)      ?? new();
        public static void                   SavePickupContacts(List<PickupContact> d) => Save(PickupFile, d);
        public static List<AttendanceRecord> LoadAttendance()     => Load<List<AttendanceRecord>>(AttendanceFile) ?? new();
        public static void                   SaveAttendance(List<AttendanceRecord> d) => Save(AttendanceFile, d);
        private static T? Load<T>(string path) { if (!File.Exists(path)) return default; return JsonSerializer.Deserialize<T>(File.ReadAllText(path)); }
        private static void Save<T>(string path, T data) => File.WriteAllText(path, JsonSerializer.Serialize(data, Opts));
    }

    class Program
    {
        static void Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            PrintBanner();
            bool running = true;
            while (running)
            {
                PrintHeader("Main Menu");
                Console.WriteLine("  1. Children & Attendance");
                Console.WriteLine("  2. Allergy Management");
                Console.WriteLine("  3. Pickup Management");
                Console.WriteLine("  0. Exit");
                Console.Write("\n  Select: ");
                switch (Console.ReadLine()?.Trim())
                {
                    case "1": AttendanceMenu(); break;
                    case "2": AllergyMenu();    break;
                    case "3": PickupMenu();     break;
                    case "0": running = false;  break;
                    default:  PrintError("Invalid option."); break;
                }
            }
            Console.WriteLine("\n  Goodbye!\n");
        }

        static void AttendanceMenu()
        {
            bool back = false;
            while (!back)
            {
                PrintHeader("Children & Attendance  [FR2]");
                Console.WriteLine("  1. Add Child");
                Console.WriteLine("  2. List All Children");
                Console.WriteLine("  3. Check In Child");
                Console.WriteLine("  4. Check Out Child");
                Console.WriteLine("  5. View Today's Attendance");
                Console.WriteLine("  0. Back");
                Console.Write("\n  Select: ");
                switch (Console.ReadLine()?.Trim())
                {
                    case "1": AddChild();            break;
                    case "2": ListChildren();        break;
                    case "3": CheckInChild();        break;
                    case "4": CheckOutChild();       break;
                    case "5": ViewTodayAttendance(); break;
                    case "0": back = true;           break;
                    default:  PrintError("Invalid option."); break;
                }
            }
        }

        static void AddChild()
        {
            PrintHeader("Add Child");
            Console.Write("  Name: ");
            string name = Console.ReadLine()?.Trim() ?? "";
            if (string.IsNullOrEmpty(name)) { PrintError("Name cannot be empty."); return; }
            Console.Write("  Date of birth (YYYY-MM-DD): ");
            string dob = Console.ReadLine()?.Trim() ?? "";
            var children = DataStore.LoadChildren();
            int newId = children.Count > 0 ? children.Max(c => c.Id) + 1 : 1;
            children.Add(new Child { Id = newId, Name = name, DateOfBirth = dob });
            DataStore.SaveChildren(children);
            PrintSuccess($"Child '{name}' added with ID {newId}.");
        }

        static void ListChildren()
        {
            var children = DataStore.LoadChildren();
            PrintHeader("All Registered Children");
            if (!children.Any()) { Console.WriteLine("  No children registered yet."); }
            else
            {
                Console.WriteLine($"  {\"ID\",-5} {\"Name\",-22} {\"Date of Birth\",-14} Status");
                Console.WriteLine("  " + new string('-', 55));
                foreach (var c in children)
                    Console.WriteLine($"  {c.Id,-5} {c.Name,-22} {c.DateOfBirth,-14} {(c.IsCheckedIn ? \"Present\" : \"Absent\")}");
            }
            Pause();
        }

        static void CheckInChild()
        {
            PrintHeader("Check In Child  [FR2]");
            var children = DataStore.LoadChildren();
            var absent = children.Where(c => !c.IsCheckedIn).ToList();
            if (!absent.Any()) { PrintError("No absent children to check in."); return; }
            Console.WriteLine("  Absent children:");
            foreach (var c in absent) Console.WriteLine($"    [{c.Id}] {c.Name}");
            Console.Write("\n  Enter child ID: ");
            if (!int.TryParse(Console.ReadLine(), out int id)) { PrintError("Invalid ID."); return; }
            var child = children.FirstOrDefault(c => c.Id == id);
            if (child == null) { PrintError("Child not found."); return; }
            if (child.IsCheckedIn) { PrintError($"{child.Name} is already checked in."); return; }
            child.IsCheckedIn = true;
            DataStore.SaveChildren(children);
            var attendance = DataStore.LoadAttendance();
            string today = DateTime.Today.ToString("yyyy-MM-dd");
            var rec = attendance.FirstOrDefault(a => a.ChildId == id && a.Date == today);
            if (rec == null) attendance.Add(new AttendanceRecord { ChildId = id, Date = today, CheckInTime = DateTime.Now.ToString("HH:mm") });
            else rec.CheckInTime = DateTime.Now.ToString("HH:mm");
            DataStore.SaveAttendance(attendance);
            var allergies = DataStore.LoadAllergies().Where(a => a.ChildId == id).ToList();
            if (allergies.Any())
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"\n  *** ALLERGY ALERT for {child.Name} ***");
                foreach (var a in allergies) Console.WriteLine($"    - {a.Allergen} [{a.Severity}]: {a.Notes}");
                Console.ResetColor();
            }
            PrintSuccess($"{child.Name} checked in at {DateTime.Now:HH:mm}.");
        }

        static void CheckOutChild()
        {
            PrintHeader("Check Out Child  [FR2]");
            var children = DataStore.LoadChildren();
            var present = children.Where(c => c.IsCheckedIn).ToList();
            if (!present.Any()) { PrintError("No children currently checked in."); return; }
            Console.WriteLine("  Present children:");
            foreach (var c in present) Console.WriteLine($"    [{c.Id}] {c.Name}");
            Console.Write("\n  Enter child ID: ");
            if (!int.TryParse(Console.ReadLine(), out int id)) { PrintError("Invalid ID."); return; }
            var child = children.FirstOrDefault(c => c.Id == id);
            if (child == null) { PrintError("Child not found."); return; }
            if (!child.IsCheckedIn) { PrintError($"{child.Name} is not checked in."); return; }
            child.IsCheckedIn = false;
            DataStore.SaveChildren(children);
            var attendance = DataStore.LoadAttendance();
            string today = DateTime.Today.ToString("yyyy-MM-dd");
            var rec = attendance.FirstOrDefault(a => a.ChildId == id && a.Date == today);
            if (rec != null) rec.CheckOutTime = DateTime.Now.ToString("HH:mm");
            DataStore.SaveAttendance(attendance);
            PrintSuccess($"{child.Name} checked out at {DateTime.Now:HH:mm}.");
        }

        static void ViewTodayAttendance()
        {
            PrintHeader("Today's Attendance");
            var children = DataStore.LoadChildren();
            var attendance = DataStore.LoadAttendance();
            string today = DateTime.Today.ToString("yyyy-MM-dd");
            var todayRecs = attendance.Where(a => a.Date == today).ToList();
            Console.WriteLine($"  Date: {today}\n");
            Console.WriteLine($"  {\"Name\",-22} {\"Check-In\",-10} {\"Check-Out\",-12} Status");
            Console.WriteLine("  " + new string('-', 58));
            foreach (var child in children)
            {
                var rec = todayRecs.FirstOrDefault(a => a.ChildId == child.Id);
                string status = child.IsCheckedIn ? "Present" : (rec != null ? "Left" : "Absent");
                Console.WriteLine($"  {child.Name,-22} {rec?.CheckInTime ?? \"-\",-10} {rec?.CheckOutTime ?? \"-\",-12} {status}");
            }
            Pause();
        }

        static void AllergyMenu()
        {
            bool back = false;
            while (!back)
            {
                PrintHeader("Allergy Management  [FR7, FR8]");
                Console.WriteLine("  1. Add / Update Allergy for Child");
                Console.WriteLine("  2. View Allergies for Child");
                Console.WriteLine("  3. Pre-Meal Allergy Alert Check");
                Console.WriteLine("  0. Back");
                Console.Write("\n  Select: ");
                switch (Console.ReadLine()?.Trim())
                {
                    case "1": AddAllergy();          break;
                    case "2": ViewAllergies();       break;
                    case "3": PreMealAllergyCheck(); break;
                    case "0": back = true;           break;
                    default:  PrintError("Invalid option."); break;
                }
            }
        }

        static void AddAllergy()
        {
            PrintHeader("Add / Update Allergy  [FR7]");
            var children = DataStore.LoadChildren();
            if (!children.Any()) { PrintError("No children registered. Add children first."); return; }
            foreach (var c in children) Console.WriteLine($"    [{c.Id}] {c.Name}");
            Console.Write("\n  Enter child ID: ");
            if (!int.TryParse(Console.ReadLine(), out int id) || children.All(c => c.Id != id)) { PrintError("Invalid child ID."); return; }
            var child = children.First(c => c.Id == id);
            Console.Write("  Allergen (e.g. Peanuts, Dairy, Gluten): ");
            string allergen = Console.ReadLine()?.Trim() ?? "";
            if (string.IsNullOrEmpty(allergen)) { PrintError("Allergen cannot be empty."); return; }
            Console.Write("  Severity (Mild / Moderate / Severe): ");
            string severity = Console.ReadLine()?.Trim() ?? "Mild";
            Console.Write("  Notes / Action to take: ");
            string notes = Console.ReadLine()?.Trim() ?? "";
            var allergies = DataStore.LoadAllergies();
            var existing = allergies.FirstOrDefault(a => a.ChildId == id && a.Allergen.Equals(allergen, StringComparison.OrdinalIgnoreCase));
            if (existing != null) { existing.Severity = severity; existing.Notes = notes; }
            else allergies.Add(new AllergyRecord { ChildId = id, Allergen = allergen, Severity = severity, Notes = notes });
            DataStore.SaveAllergies(allergies);
            PrintSuccess($"Allergy '{allergen}' saved for {child.Name}.");
        }

        static void ViewAllergies()
        {
            PrintHeader("View Allergies  [FR7]");
            var children = DataStore.LoadChildren();
            if (!children.Any()) { PrintError("No children registered."); return; }
            foreach (var c in children) Console.WriteLine($"    [{c.Id}] {c.Name}");
            Console.Write("\n  Enter child ID: ");
            if (!int.TryParse(Console.ReadLine(), out int id) || children.All(c => c.Id != id)) { PrintError("Invalid child ID."); return; }
            var child = children.First(c => c.Id == id);
            var allergies = DataStore.LoadAllergies().Where(a => a.ChildId == id).ToList();
            PrintHeader($"Allergies for {child.Name}");
            if (!allergies.Any()) Console.WriteLine("  No allergies recorded.");
            else foreach (var a in allergies) Console.WriteLine($"  - {a.Allergen} [{a.Severity}]: {a.Notes}");
            Pause();
        }

        static void PreMealAllergyCheck()
        {
            PrintHeader("Pre-Meal Allergy Alert  [FR8]");
            Console.WriteLine("  Scanning all present children for known allergies...\n");
            var present = DataStore.LoadChildren().Where(c => c.IsCheckedIn).ToList();
            var allergies = DataStore.LoadAllergies();
            bool anyAlert = false;
            foreach (var child in present)
            {
                var ca = allergies.Where(a => a.ChildId == child.Id).ToList();
                if (!ca.Any()) continue;
                anyAlert = true;
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"  *** ALLERGY ALERT -- {child.Name.ToUpper()} ***");
                Console.ResetColor();
                foreach (var a in ca) Console.WriteLine($"    - {a.Allergen} [{a.Severity}]: {a.Notes}");
                Console.WriteLine();
            }
            if (!anyAlert) { Console.ForegroundColor = ConsoleColor.Green; Console.WriteLine("  All clear -- no allergy alerts for present children."); Console.ResetColor(); }
            Pause();
        }

        static void PickupMenu()
        {
            bool back = false;
            while (!back)
            {
                PrintHeader("Pickup Management  [FR9, FR10]");
                Console.WriteLine("  1. Add Pickup Contact for Child");
                Console.WriteLine("  2. View Pickup Contacts for Child");
                Console.WriteLine("  3. Verify Pickup Authorization");
                Console.WriteLine("  0. Back");
                Console.Write("\n  Select: ");
                switch (Console.ReadLine()?.Trim())
                {
                    case "1": AddPickupContact();   break;
                    case "2": ViewPickupContacts(); break;
                    case "3": VerifyPickup();       break;
                    case "0": back = true;          break;
                    default:  PrintError("Invalid option."); break;
                }
            }
        }

        static void AddPickupContact()
        {
            PrintHeader("Add Pickup Contact  [FR9]");
            var children = DataStore.LoadChildren();
            if (!children.Any()) { PrintError("No children registered. Add children first."); return; }
            foreach (var c in children) Console.WriteLine($"    [{c.Id}] {c.Name}");
            Console.Write("\n  Enter child ID: ");
            if (!int.TryParse(Console.ReadLine(), out int id) || children.All(c => c.Id != id)) { PrintError("Invalid child ID."); return; }
            var child = children.First(c => c.Id == id);
            Console.Write("  Contact name: ");
            string name = Console.ReadLine()?.Trim() ?? "";
            if (string.IsNullOrEmpty(name)) { PrintError("Name cannot be empty."); return; }
            Console.Write("  Relationship (e.g. Parent, Grandparent, Sibling): ");
            string rel = Console.ReadLine()?.Trim() ?? "";
            Console.Write("  Phone number: ");
            string phone = Console.ReadLine()?.Trim() ?? "";
            var contacts = DataStore.LoadPickupContacts();
            contacts.Add(new PickupContact { ChildId = id, Name = name, Relationship = rel, Phone = phone });
            DataStore.SavePickupContacts(contacts);
            PrintSuccess($"Contact '{name}' added for {child.Name}.");
        }

        static void ViewPickupContacts()
        {
            PrintHeader("View Pickup Contacts  [FR9]");
            var children = DataStore.LoadChildren();
            if (!children.Any()) { PrintError("No children registered."); return; }
            foreach (var c in children) Console.WriteLine($"    [{c.Id}] {c.Name}");
            Console.Write("\n  Enter child ID: ");
            if (!int.TryParse(Console.ReadLine(), out int id) || children.All(c => c.Id != id)) { PrintError("Invalid child ID."); return; }
            var child = children.First(c => c.Id == id);
            var contacts = DataStore.LoadPickupContacts().Where(p => p.ChildId == id).ToList();
            PrintHeader($"Authorized Contacts -- {child.Name}");
            if (!contacts.Any()) Console.WriteLine("  No pickup contacts on file.");
            else
            {
                Console.WriteLine($"  {\"Name\",-22} {\"Relationship\",-16} Phone");
                Console.WriteLine("  " + new string('-', 55));
                foreach (var p in contacts) Console.WriteLine($"  {p.Name,-22} {p.Relationship,-16} {p.Phone}");
            }
            Pause();
        }

        static void VerifyPickup()
        {
            PrintHeader("Verify Pickup Authorization  [FR10]");
            var children = DataStore.LoadChildren().Where(c => c.IsCheckedIn).ToList();
            if (!children.Any()) { PrintError("No children currently checked in."); return; }
            Console.WriteLine("  Present children:");
            foreach (var c in children) Console.WriteLine($"    [{c.Id}] {c.Name}");
            Console.Write("\n  Enter child ID: ");
            if (!int.TryParse(Console.ReadLine(), out int id) || children.All(c => c.Id != id)) { PrintError("Invalid child ID."); return; }
            var child = children.First(c => c.Id == id);
            Console.Write("  Person requesting pickup (full name): ");
            string personName = Console.ReadLine()?.Trim() ?? "";
            var contacts = DataStore.LoadPickupContacts().Where(p => p.ChildId == id).ToList();
            var match = contacts.FirstOrDefault(p => p.Name.Equals(personName, StringComparison.OrdinalIgnoreCase));
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
                Console.WriteLine("  Do NOT release the child. Contact parent/guardian immediately.");
                Console.ResetColor();
            }
            Pause();
        }

        static void PrintBanner()
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine();
            Console.WriteLine("  ============================================");
            Console.WriteLine("    Daycare Management System  v1.0.0");
            Console.WriteLine("    CS690 Final Project -- Claire's Daycare");
            Console.WriteLine("  ============================================");
            Console.ResetColor();
            Console.WriteLine();
        }

        static void PrintHeader(string title)
        {
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine($"  --- {title} ---");
            Console.ResetColor();
        }

        static void PrintSuccess(string msg)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"\n  [OK] {msg}");
            Console.ResetColor();
            Pause();
        }

        static void PrintError(string msg)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"\n  [ERROR] {msg}");
            Console.ResetColor();
            Pause();
        }

        static void Pause()
        {
            Console.WriteLine("\n  Press any key to continue...");
            Console.ReadKey(true);
        }
    }
}
