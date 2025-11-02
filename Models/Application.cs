using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Stageproject_ATS_AP2025Q2.Models;
using static StatusHistory;
/// <summary>
/// Represents a job application linked to a vacancy and a user.
/// Includes resume info, save status, interview notes, and schedule.
/// </summary>
public class Application
{
    [Key]
    public int Id { get; set; }

    [ForeignKey("Vacancy")]
    public int VacancyId { get; set; }
    public virtual Vacancy Vacancy { get; set; }

    [ForeignKey("User")]
    public string UserId { get; set; }
    public virtual AppUser User { get; set; }
    [Column(TypeName = "LONGBLOB")] 
    [MaxLength(16777215)] // Max 16 MB (voor LONGBLOB)
    public byte[] cv { get; set; }

    public string ResumeFilePath { get; set; }
    public bool IsSaved { get; set; } = false;
    public string Motivation { get; set; }
    public string AboutYourself { get; set; }
    public virtual List<InterviewNote> InterviewNotes { get; set; }
    public virtual InterviewSchedule Interview { get; set; }
    public List<StatusHistory> StatusHistories { get; set; } = new List<StatusHistory>();



}
