using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
/// <summary>
/// Represents a note related to a job interview,
/// including the interview date, time, candidate name,
/// related application, and metadata like who created it.
/// </summary>
public class InterviewNote
{
    [Key]
[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [ForeignKey("Application")]
    public int ApplicationId { get; set; }
    public Application Application { get; set; }

    public string Comment { get; set; }

    public string CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    [Required]
    public DateOnly InterviewDate { get; set; }
    [Required]
    public TimeSpan InterviewTime { get; set; }
    public string FullName { get; set; }

    public ICollection<string> InvitedUserEmails { get; set; } = new List<string>();

}


