using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
/// <summary>
/// Represents an application user, extending IdentityUser with
/// additional properties such as role, first name, last name,
/// and a list of job applications linked to the user.
/// </summary>
public class AppUser : IdentityUser
{
    public string Role { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Address { get; set; }
    public List<Application> Applications { get; set; }
    public bool IsActive { get; set; } = true;
        



}
