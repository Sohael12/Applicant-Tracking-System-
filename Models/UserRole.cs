using System.ComponentModel.DataAnnotations;
using Stageproject_ATS_AP2025Q2.Models; 
namespace Stageproject_ATS_AP2025Q2.Models
{
    public class UserRole
    {
        [Key]
        public int Id { get; set; }

        public string UserId { get; set; }
        public AppUser User { get; set; }

        public string Role { get; set; } 
    }
}
