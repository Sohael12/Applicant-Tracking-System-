using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Stageproject_ATS_AP2025Q2.Models;
using Stageproject_ATS_AP2025Q2.Services;
using System.Security.Claims;

namespace Stageproject_ATS_AP2025Q2.Controllers
{
    
    /// <summary>
    /// This controller handles job applications submitted by users.
    /// Users must be authorized (logged in) to submit or view applications.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ApplicationController : ControllerBase 
    {
        private readonly ApplicationService _applicationService;

        public ApplicationController(ApplicationService applicationService)
        {
            _applicationService = applicationService;
        }

        /// <summary>
        /// Submit a new application for the current logged-in user.
        /// </summary>
        [HttpPost("submit")]
        public async Task<IActionResult> Submit([FromBody] Application application)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Get current user Id from claims
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized("User not logged in.");

            application.UserId = userId;
            await _applicationService.SaveApplicationAsync(application);

            return Ok(new { message = "Application saved successfully." });
        }

        /// <summary>
        /// Get the application of the logged-in user for a specific vacancy.
        /// </summary>
        [HttpGet("{vacancyId}")]
        public async Task<IActionResult> Get(int vacancyId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var application = await _applicationService.GetByVacancyAndUserAsync(vacancyId, userId);
            if (application == null)
                return NotFound();

            return Ok(application);
        }
    }
}
