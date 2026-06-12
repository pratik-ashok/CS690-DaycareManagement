namespace DaycareManagement.Models;

public class AllergyRecord
{
    public int ChildId { get; set; }
    public string Allergen { get; set; } = "";
    public string Severity { get; set; } = "Mild"; // Mild / Moderate / Severe
    public string Notes { get; set; } = "";
}
