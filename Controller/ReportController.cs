using Microsoft.AspNetCore.Mvc;

[Route("api/reports")]
[ApiController]
public class ReportsController : ControllerBase
{
    private readonly ReportService _reportService;

    // Dependency injection of ReportService
    public ReportsController(ReportService reportService)
    {
        _reportService = reportService;
    }

    [HttpGet("vacancy")]
    public async Task<IActionResult> GetVacancyReport()
    {
        var fileBytes = await _reportService.GenerateVacancyReportAsync();
        var fileName = $"VacancyReport_{DateTime.Now:yyyyMMdd}.xlsx";

        return File(fileBytes,
                    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    fileName);
    }

    [HttpGet("applications")]
    public async Task<IActionResult> GetApplicationReport()
    {
        var fileBytes = await _reportService.GenerateApplicationReportAsync();
        var fileName = $"SollicitatieRapport_{DateTime.Now:yyyyMMdd}.xlsx";

        return File(fileBytes,
                    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    fileName);
    }

    [HttpGet("training")]
    public async Task<IActionResult> GetTrainingReport()
    {
        var report = await _reportService.GenerateTrainingReportAsync();
        return File(report,
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            $"TrainingReport_{DateTime.Now:yyyyMMddHHmm}.xlsx");
    }

    // ✅ Users Report
    [HttpGet("users")]
    public async Task<IActionResult> GetUsersReport()
    {
        var reportBytes = await _reportService.GenerateUsersReportAsync();
        var fileName = $"UsersReport_{DateTime.Now:yyyyMMdd}.xlsx";

        return File(reportBytes,
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            fileName);
    }

[HttpGet("user/{userId}")]
public async Task<IActionResult> GetUserReport(string userId)
{
    try
    {
        var reportBytes = await _reportService.GenerateUserReportAsync(userId);
        var fileName = $"User_{userId}_Report_{DateTime.Now:yyyyMMdd}.xlsx";

        return File(reportBytes,
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            fileName);
    }
    catch (Exception ex)
    {
        return BadRequest($"Error generating user report: {ex.Message}");
    }
}

}

