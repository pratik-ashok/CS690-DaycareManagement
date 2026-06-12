namespace DaycareManagement.Models;

public class Child
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string DateOfBirth { get; set; } = "";
    public bool IsCheckedIn { get; set; } = false;
}
