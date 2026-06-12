namespace DaycareManagement;

/// <summary>Shared console UI helpers.</summary>
public static class UI
{
    public static void PrintBanner()
    {
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine();
        Console.WriteLine("  =============================================");
        Console.WriteLine("    Daycare Management System  v2.0.0");
        Console.WriteLine("    CS690 Final Project -- Claire's Daycare");
        Console.WriteLine("  =============================================");
        Console.ResetColor();
        Console.WriteLine();
    }

    public static void PrintHeader(string title)
    {
        Console.WriteLine();
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine($"  --- {title} ---");
        Console.ResetColor();
    }

    public static void PrintSuccess(string msg)
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"\n  [OK] {msg}");
        Console.ResetColor();
        Pause();
    }

    public static void PrintError(string msg)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"\n  [ERROR] {msg}");
        Console.ResetColor();
        Pause();
    }

    public static void Pause()
    {
        Console.WriteLine("\n  Press any key to continue...");
        Console.ReadKey(true);
    }
}
