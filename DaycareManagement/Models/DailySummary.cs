namespace DaycareManagement.Models;

public class DailySummary
{
    public string Date { get; set; } = "";
    public List<string> Notes { get; set; } = new();
}
