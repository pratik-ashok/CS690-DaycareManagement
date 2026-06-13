using DaycareManagement.Services;

namespace DaycareManagement.Tests;

public class DailySummaryServiceTests : IDisposable
{
    private readonly string _tempDir;
    private readonly DataStore _store;
    private readonly DailySummaryService _service;

    public DailySummaryServiceTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(_tempDir);
        _store   = new DataStore(_tempDir);
        _service = new DailySummaryService(_store);
    }

    public void Dispose() => Directory.Delete(_tempDir, recursive: true);

    [Fact]
    public void AddNote_StoresNoteForDate()
    {
        _service.AddNote("2024-01-15", "Fire drill at 10am");
        var notes = _service.GetNotes("2024-01-15");
        Assert.Single(notes);
        Assert.Equal("Fire drill at 10am", notes[0]);
    }

    [Fact]
    public void AddNote_MultipleNotes_AllStored()
    {
        _service.AddNote("2024-01-15", "First note");
        _service.AddNote("2024-01-15", "Second note");
        _service.AddNote("2024-01-15", "Third note");
        var notes = _service.GetNotes("2024-01-15");
        Assert.Equal(3, notes.Count);
    }

    [Fact]
    public void GetNotes_NoNotesForDate_ReturnsEmpty()
    {
        var notes = _service.GetNotes("2024-01-01");
        Assert.Empty(notes);
    }

    [Fact]
    public void AddNote_DifferentDates_StoredSeparately()
    {
        _service.AddNote("2024-01-15", "Note for Jan 15");
        _service.AddNote("2024-01-16", "Note for Jan 16");
        Assert.Single(_service.GetNotes("2024-01-15"));
        Assert.Single(_service.GetNotes("2024-01-16"));
        Assert.Equal("Note for Jan 15", _service.GetNotes("2024-01-15")[0]);
        Assert.Equal("Note for Jan 16", _service.GetNotes("2024-01-16")[0]);
    }

    [Fact]
    public void GetSummary_NoSummary_ReturnsNull()
    {
        var result = _service.GetSummary("2024-01-01");
        Assert.Null(result);
    }

    [Fact]
    public void GetSummary_AfterAddNote_ReturnsSummary()
    {
        _service.AddNote("2024-01-15", "Test note");
        var result = _service.GetSummary("2024-01-15");
        Assert.NotNull(result);
        Assert.Equal("2024-01-15", result.Date);
    }

    [Fact]
    public void AddNote_PersistedAcrossInstances()
    {
        _service.AddNote("2024-01-15", "Persisted note");
        var service2 = new DailySummaryService(_store);
        var notes = service2.GetNotes("2024-01-15");
        Assert.Single(notes);
        Assert.Equal("Persisted note", notes[0]);
    }

    [Fact]
    public void AddNote_PreservesOrderOfNotes()
    {
        _service.AddNote("2024-01-15", "Alpha");
        _service.AddNote("2024-01-15", "Beta");
        _service.AddNote("2024-01-15", "Gamma");
        var notes = _service.GetNotes("2024-01-15");
        Assert.Equal("Alpha", notes[0]);
        Assert.Equal("Beta",  notes[1]);
        Assert.Equal("Gamma", notes[2]);
    }
}
