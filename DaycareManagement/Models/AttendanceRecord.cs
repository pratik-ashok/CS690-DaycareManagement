namespace DaycareManagement.Models;

public class AttendanceRecord
{
    public int ChildId { get; set; }
    public string Date { get; set; } = "";           // yyyy-MM-dd
    public string? CheckInTime { get; set; }          // HH:mm
    public string? CheckOutTime { get; set; }         // HH:mm
}
