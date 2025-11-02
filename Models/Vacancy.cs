using System.ComponentModel.DataAnnotations;
using Stageproject_ATS_AP2025Q2.Models;
namespace Stageproject_ATS_AP2025Q2.Models;
/// <summary>
/// Represents a job vacancy in the internship project,
/// including title, description, department, deadline,
/// creator info, and associated applications.
/// </summary> 
/// 
public class Vacancy
{
    [Key]
    public int Id { get; set; }

    public string Title { get; set; }

    public string Description { get; set; }
  [Required]
  [DataType(DataType.Date)]

    public DateTime?  Deadline { get; set; }

    public string Department { get; set; }

    public string CreatedBy { get; set; } 
    public DateTime StartDate { get; set; }

      

    public List<Application> Applications { get; set; }
}
