using Microsoft.EntityFrameworkCore;
using Stageproject_ATS_AP2025Q2.Data;

public class InterviewNoteService
{
    private readonly AppDbContext _context;

    public InterviewNoteService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<InterviewNote>> GetAllNotesAsync()
    {
        return await _context.InterviewNotes
            .Include(n => n.Application)
            .ThenInclude(a => a.User)
            .ToListAsync();
    }

    public async Task AddNoteAsync(InterviewNote note)
    {
        // Controleer of de ApplicationId geldig is
        var applicationExists = await _context.Applications
            .AnyAsync(a => a.Id == note.ApplicationId);

        if (!applicationExists)
        {
            throw new Exception("De ApplicationId bestaat niet.");
        }

        // Voeg de InterviewNote toe aan de database
        _context.InterviewNotes.Add(note);
        await _context.SaveChangesAsync();
    }

    public async Task<InterviewNote> GetNoteByIdAsync(int id)
    {
        return await _context.InterviewNotes
            .Include(i => i.Application)
            .FirstOrDefaultAsync(i => i.Id == id);
    }

    public async Task UpdateNoteAsync(InterviewNote updatedNote)
    {
        var existingNote = await _context.InterviewNotes.FindAsync(updatedNote.Id);
        if (existingNote != null)
        {
            existingNote.FullName = updatedNote.FullName;
            existingNote.ApplicationId = updatedNote.ApplicationId;
            existingNote.Comment = updatedNote.Comment;
            existingNote.CreatedAt = updatedNote.CreatedAt;
            existingNote.CreatedBy = updatedNote.CreatedBy;

            await _context.SaveChangesAsync();
        }
    }

    // Toevoegen: Verkrijg notities voor een specifieke sollicitatie
    public async Task<List<InterviewNote>> GetNotesByApplicationIdAsync(int applicationId)
    {
        return await _context.InterviewNotes
            .Where(n => n.ApplicationId == applicationId)
            .Include(n => n.Application)
            .ThenInclude(a => a.User)
            .ToListAsync();
    }

    // Toevoegen: Verwijder een interviewnotitie
    public async Task DeleteNoteAsync(int id)
    {
        var noteToDelete = await _context.InterviewNotes.FindAsync(id);
        if (noteToDelete != null)
        {
            _context.InterviewNotes.Remove(noteToDelete);
            await _context.SaveChangesAsync();
        }
        else
        {
            throw new Exception("Notitie niet gevonden.");
        }


    }

    public async Task<List<InterviewNote>> GetInterviewNotesByApplicationIdAsync(int applicationId)
    {
        return await _context.InterviewNotes
            .Where(note => note.ApplicationId == applicationId)
            .ToListAsync();
    }

public async Task DeleteInterviewNoteAsync(int noteId)
{
    // Delete logic here, e.g., EF Core:
    var note = await _context.InterviewNotes.FindAsync(noteId);
    if (note != null)
    {
        _context.InterviewNotes.Remove(note);
        await _context.SaveChangesAsync();
    }
}


}
