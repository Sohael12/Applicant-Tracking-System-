using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
/// <summary>
/// Represents the history record of status changes for a specific job application.
/// Each record tracks the status, the date when the status was changed, and who made the change.
/// </summary
public class ApplicationHistory
{
    [Key]
    public int Id { get; set; }

    [ForeignKey("Application")]
    public int ApplicationId { get; set; }
    public virtual Application Application { get; set; }

    public string Status { get; set; }
    public DateTime DateChanged { get; set; }
    public string ChangedBy { get; set; }
}
