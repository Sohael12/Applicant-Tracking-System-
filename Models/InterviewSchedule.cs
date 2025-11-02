using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
/// <summary>
/// Represents the scheduling details for an interview related to a specific job application.
/// This class stores the interview date, the interviewer assigned, and links to the associated application.
/// </summary>
public class InterviewSchedule
{
    [Key]
    public int Id { get; set; }

    [ForeignKey("Application")]
    public int ApplicationId { get; set; }
    public Application Application { get; set; }

    public DateTime InterviewDate { get; set; }

    public string Interviewer { get; set; } 
}
