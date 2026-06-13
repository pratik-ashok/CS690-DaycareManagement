using DaycareManagement.Models;

namespace DaycareManagement.Services;

public class DailySummaryService
{
    private readonly DataStore _store;

    public DailySummaryService(DataStore store) => _store = store;

    public void AddNote(string date, string note)
    {
        var summaries = _store.LoadDailySummaries();
        var summary = summaries.FirstOrDefault(s => s.Date == date);
        if (summary == null)
        {
            summary = new DailySummary { Date = date };
            summaries.Add(summary);
        }
        summary.Notes.Add(note);
        _store.SaveDailySummaries(summaries);
    }

    public List<string> GetNotes(string date)
    {
        var summary = _store.LoadDailySummaries().FirstOrDefault(s => s.Date == date);
        return summary?.Notes ?? new List<string>();
    }

    public DailySummary? GetSummary(string date) =>
        _store.LoadDailySummaries().FirstOrDefault(s => s.Date == date);
}
