namespace DaycareManagement.Models;

public class StaffMember
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public bool IsOnDuty { get; set; } = false;
}
