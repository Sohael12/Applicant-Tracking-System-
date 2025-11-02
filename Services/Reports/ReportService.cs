using ClosedXML.Excel;
using Stageproject_ATS_AP2025Q2.Services;
using Stageproject_ATS_AP2025Q2.Models;
using Stageproject_ATS_AP2025Q2.Data;
using Microsoft.EntityFrameworkCore;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

public class ReportService
{
    private readonly VacancyService _vacancyService;
    private readonly ApplicationService _applicationService;
    private readonly AppDbContext _context;

    public ReportService(
        VacancyService vacancyService,
        ApplicationService applicationService,
        AppDbContext context)
    {
        _vacancyService = vacancyService;
        _applicationService = applicationService;
        _context = context;
    }

    // ------------------------------
    // 📊 VACANCY REPORT
    // ------------------------------
    public async Task<byte[]> GenerateVacancyReportAsync()
    {
        var vacancies = await _vacancyService.GetVacanciesAsync();
        var applications = await _applicationService.GetAllAsync();

        var rows = vacancies.Select(v => new VacancyReportRow
        {
            VacancyTitle = v.Title,
            TotalApplications = applications.Count(a => a.Vacancy.Id == v.Id),
            InitialScreening = applications.Count(a => a.Vacancy.Id == v.Id && a.LatestStatus() == StatusHistory.ApplicationStatus.InitialScreening),
            InterviewWithHR = applications.Count(a => a.Vacancy.Id == v.Id && a.LatestStatus() == StatusHistory.ApplicationStatus.InterviewWithHR),
            InterviewWithManager = applications.Count(a => a.Vacancy.Id == v.Id && a.LatestStatus() == StatusHistory.ApplicationStatus.InterviewWithManager),
            SecondOpinion = applications.Count(a => a.Vacancy.Id == v.Id && a.LatestStatus() == StatusHistory.ApplicationStatus.SecondOpinion),
            Accepted = applications.Count(a => a.Vacancy.Id == v.Id && a.LatestStatus() == StatusHistory.ApplicationStatus.Accepted),
            TrainingPhase = applications.Count(a => a.Vacancy.Id == v.Id && a.LatestStatus() == StatusHistory.ApplicationStatus.TrainingPhase),
            Rejected = applications.Count(a => a.Vacancy.Id == v.Id && a.LatestStatus() == StatusHistory.ApplicationStatus.Rejected),
        }).ToList();

        using var workbook = new XLWorkbook();
        var ws = workbook.Worksheets.Add("Vacancy Report");

        // Titel
        ws.Cell("A1").Value = "Vacancy Report – Overview";
        ws.Cell("A1").Style.Font.Bold = true;
        ws.Cell("A1").Style.Font.FontSize = 18;
        ws.Range("A1:I1").Merge().Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

        ws.Cell("A2").Value = $"Generated on: {DateTime.Now:dd-MM-yyyy HH:mm}";
        ws.Range("A2:I2").Merge().Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
        ws.Cell("A2").Style.Font.Italic = true;

        // Samenvatting
        int totalApps = rows.Sum(r => r.TotalApplications);
        ws.Cell("A4").Value = "Total Vacancies:";
        ws.Cell("B4").Value = rows.Count;
        ws.Cell("A5").Value = "Total Applications:";
        ws.Cell("B5").Value = totalApps;
        ws.Cell("A6").Value = "Average per Vacancy:";
        ws.Cell("B6").Value = rows.Count > 0 ? (totalApps / rows.Count) : 0;
        ws.Range("A4:B6").Style.Fill.BackgroundColor = XLColor.LightYellow;
        ws.Range("A4:B6").Style.Font.Bold = true;

        // Header
        int headerRow = 8;
        string[] headers = {
            "Vacancy", "Total Apps", "Initial Screening", "Interview HR",
            "Interview Manager", "Second Opinion", "Accepted", "Training", "Rejected"
        };

        for (int i = 0; i < headers.Length; i++)
        {
            var cell = ws.Cell(headerRow, i + 1);
            cell.Value = headers[i];
            cell.Style.Font.Bold = true;
            cell.Style.Fill.BackgroundColor = XLColor.LightSteelBlue;
            cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            cell.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
        }

        // Data
        for (int i = 0; i < rows.Count; i++)
        {
            var row = rows[i];
            int excelRow = headerRow + 1 + i;

            ws.Cell(excelRow, 1).Value = row.VacancyTitle;
            ws.Cell(excelRow, 2).Value = row.TotalApplications;
            ws.Cell(excelRow, 3).Value = row.InitialScreening;
            ws.Cell(excelRow, 4).Value = row.InterviewWithHR;
            ws.Cell(excelRow, 5).Value = row.InterviewWithManager;
            ws.Cell(excelRow, 6).Value = row.SecondOpinion;
            ws.Cell(excelRow, 7).Value = row.Accepted;
            ws.Cell(excelRow, 8).Value = row.TrainingPhase;
            ws.Cell(excelRow, 9).Value = row.Rejected;

            var bg = (i % 2 == 0) ? XLColor.White : XLColor.LightGray;
            ws.Range(excelRow, 1, excelRow, 9).Style.Fill.BackgroundColor = bg;
        }

        // Total
        int summaryRow = headerRow + 1 + rows.Count;
        ws.Cell(summaryRow, 1).Value = "TOTAL";
        ws.Range(summaryRow, 1, summaryRow, 9).Style.Font.Bold = true;
        ws.Range(summaryRow, 1, summaryRow, 9).Style.Fill.BackgroundColor = XLColor.LightGoldenrodYellow;
        for (int c = 2; c <= 9; c++)
            ws.Cell(summaryRow, c).FormulaA1 = $"=SUM({ws.Cell(headerRow + 1, c).Address}:{ws.Cell(summaryRow - 1, c).Address})";

        ws.Columns().AdjustToContents();
        ws.SheetView.FreezeRows(headerRow);
        ws.RangeUsed().SetAutoFilter();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }

    // ------------------------------
    // 📄 APPLICATION REPORT
    // ------------------------------
    public async Task<byte[]> GenerateApplicationReportAsync()
    {
        var applications = await _applicationService.GetAllAsync();

        using var workbook = new XLWorkbook();
        var ws = workbook.Worksheets.Add("Applications");

        ws.Cell("A1").Value = "Application Report – Overview";
        ws.Cell("A1").Style.Font.Bold = true;
        ws.Cell("A1").Style.Font.FontSize = 16;
        ws.Range("A1:E1").Merge().Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

        string[] headers = { "Vacancy", "Applicant", "Status", "Last Update", "Motivation" };
        for (int i = 0; i < headers.Length; i++)
        {
            var cell = ws.Cell(3, i + 1);
            cell.Value = headers[i];
            cell.Style.Font.Bold = true;
            cell.Style.Fill.BackgroundColor = XLColor.LightSteelBlue;
            cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            cell.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
        }

        for (int i = 0; i < applications.Count; i++)
        {
            var app = applications[i];
            var status = app.StatusHistories?.OrderByDescending(s => s.Date).FirstOrDefault();
            int row = 4 + i;

            ws.Cell(row, 1).Value = app.Vacancy?.Title ?? "Unknown";
            ws.Cell(row, 2).Value = $"{app.User?.FirstName} {app.User?.LastName}".Trim();
            ws.Cell(row, 3).Value = status?.Status.ToString() ?? "No status";
            ws.Cell(row, 4).Value = status?.Date.ToString("dd-MM-yyyy HH:mm") ?? "";
            ws.Cell(row, 5).Value = app.Motivation ?? "";

            var bg = (row % 2 == 0) ? XLColor.White : XLColor.LightGray;
            ws.Range(row, 1, row, 5).Style.Fill.BackgroundColor = bg;
        }

        ws.Columns().AdjustToContents();
        ws.SheetView.FreezeRows(3);
        ws.RangeUsed().SetAutoFilter();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }

    // ------------------------------
    // 🧑‍🏫 TRAINING REPORT
    // ------------------------------
    public async Task<byte[]> GenerateTrainingReportAsync()
    {
        var applications = await _applicationService.GetAllAsync();
        var trainingApps = applications.Where(a => a.LatestStatus() == StatusHistory.ApplicationStatus.TrainingPhase).ToList();

        using var workbook = new XLWorkbook();
        var ws = workbook.Worksheets.Add("Training Report");

        ws.Cell("A1").Value = "Training & Onboarding Report";
        ws.Cell("A1").Style.Font.Bold = true;
        ws.Cell("A1").Style.Font.FontSize = 16;
        ws.Range("A1:E1").Merge().Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

        string[] headers = { "Applicant", "Email", "Vacancy", "Last Status Date", "Motivation" };
        for (int i = 0; i < headers.Length; i++)
        {
            var cell = ws.Cell(3, i + 1);
            cell.Value = headers[i];
            cell.Style.Font.Bold = true;
            cell.Style.Fill.BackgroundColor = XLColor.LightSteelBlue;
            cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            cell.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
        }

        for (int i = 0; i < trainingApps.Count; i++)
        {
            var app = trainingApps[i];
            var status = app.StatusHistories?.OrderByDescending(s => s.Date).FirstOrDefault();
            int row = 4 + i;

            ws.Cell(row, 1).Value = $"{app.User?.FirstName} {app.User?.LastName}".Trim();
            ws.Cell(row, 2).Value = app.User?.Email ?? "";
            ws.Cell(row, 3).Value = app.Vacancy?.Title ?? "Unknown";
            ws.Cell(row, 4).Value = status?.Date.ToString("dd-MM-yyyy HH:mm") ?? "";
            ws.Cell(row, 5).Value = app.Motivation ?? "";

            var bg = (row % 2 == 0) ? XLColor.White : XLColor.LightGray;
            ws.Range(row, 1, row, 5).Style.Fill.BackgroundColor = bg;
        }

        ws.Columns().AdjustToContents();
        ws.SheetView.FreezeRows(3);
        ws.RangeUsed().SetAutoFilter();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }

    // ------------------------------
    // 👥 USERS REPORT
    // ------------------------------
    public async Task<byte[]> GenerateUsersReportAsync()
    {
        var users = await _context.Users.ToListAsync();

        using var workbook = new XLWorkbook();
        var ws = workbook.Worksheets.Add("Users Report");

        ws.Cell("A1").Value = "Users Report – Overview";
        ws.Cell("A1").Style.Font.Bold = true;
        ws.Cell("A1").Style.Font.FontSize = 16;
        ws.Range("A1:D1").Merge().Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

        string[] headers = { "Email", "Full Name", "Role", "Status" };
        for (int i = 0; i < headers.Length; i++)
        {
            var cell = ws.Cell(3, i + 1);
            cell.Value = headers[i];
            cell.Style.Font.Bold = true;
            cell.Style.Fill.BackgroundColor = XLColor.LightSteelBlue;
            cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            cell.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
        }

        int row = 4;
        foreach (var u in users)
        {
            ws.Cell(row, 1).Value = u.Email;
            ws.Cell(row, 2).Value = $"{u.FirstName} {u.LastName}";
            ws.Cell(row, 3).Value = u.Role ?? "User";
            ws.Cell(row, 4).Value = u.IsActive ? "Active" : "Inactive";

            var bg = (row % 2 == 0) ? XLColor.White : XLColor.LightGray;
            ws.Range(row, 1, row, 4).Style.Fill.BackgroundColor = bg;

            row++;
        }

        ws.Columns().AdjustToContents();
        ws.SheetView.FreezeRows(3);
        ws.RangeUsed().SetAutoFilter();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }

    // ------------------------------
    // 👤 USER DETAIL REPORT
    // ------------------------------
    public async Task<byte[]> GenerateUserReportAsync(string userId)
    {
        var user = await _context.Users
            .Include(u => u.Applications)
                .ThenInclude(a => a.Vacancy)
            .Include(u => u.Applications)
                .ThenInclude(a => a.StatusHistories) // ✅ Include status histories
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user == null)
            throw new Exception("User not found.");

        using var workbook = new XLWorkbook();
        var ws = workbook.Worksheets.Add("User Report");

        ws.Cell("A1").Value = "User Report – Detailed Overview";
        ws.Cell("A1").Style.Font.Bold = true;
        ws.Cell("A1").Style.Font.FontSize = 16;
        ws.Range("A1:E1").Merge().Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

        ws.Cell("A2").Value = $"Generated on: {DateTime.Now:dd-MM-yyyy HH:mm}";
        ws.Range("A2:E2").Merge().Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
        ws.Cell("A2").Style.Font.Italic = true;

        // Gebruikersinfo
        ws.Cell("A4").Value = "Name:";
        ws.Cell("B4").Value = $"{user.FirstName} {user.LastName}";
        ws.Cell("A5").Value = "Email:";
        ws.Cell("B5").Value = user.Email;
        ws.Cell("A6").Value = "Address:";
        ws.Cell("B6").Value = user.Address ?? "N/A";
        ws.Cell("A7").Value = "Applications:";
        ws.Cell("B7").Value = user.Applications?.Count.ToString() ?? "0";
        ws.Range("A4:A7").Style.Font.Bold = true;
        ws.Range("A4:B7").Style.Fill.BackgroundColor = XLColor.LightYellow;

        int headerRow = 9;
        string[] headers = { "Vacancy Title", "Status", "Last Updated", "Motivation" };
        for (int i = 0; i < headers.Length; i++)
        {
            var cell = ws.Cell(headerRow, i + 1);
            cell.Value = headers[i];
            cell.Style.Font.Bold = true;
            cell.Style.Fill.BackgroundColor = XLColor.LightSteelBlue;
            cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            cell.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
        }

        int row = headerRow + 1;
        if (user.Applications != null && user.Applications.Any())
        {
            foreach (var app in user.Applications)
            {
                var status = app.StatusHistories?.OrderByDescending(s => s.Date).FirstOrDefault();

                ws.Cell(row, 1).Value = app.Vacancy?.Title ?? "N/A";
                ws.Cell(row, 2).Value = status?.Status.ToString() ?? "N/A";
                ws.Cell(row, 3).Value = status?.Date.ToString("dd-MM-yyyy HH:mm") ?? "";
                ws.Cell(row, 4).Value = app.Motivation ?? "";

                var bg = (row % 2 == 0) ? XLColor.White : XLColor.LightGray;
                ws.Range(row, 1, row, 4).Style.Fill.BackgroundColor = bg;

                row++;
            }
        }
        else
        {
            ws.Cell(row, 1).Value = "No applications found.";
            ws.Range(row, 1, row, 4).Merge();
            ws.Cell(row, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            ws.Cell(row, 1).Style.Font.Italic = true;
        }

        ws.Columns().AdjustToContents();
        ws.SheetView.FreezeRows(headerRow);
        ws.RangeUsed().SetAutoFilter();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }
}

// ------------------------------
// Helper extension
// ------------------------------
public static class ApplicationExtensions
{
    public static StatusHistory.ApplicationStatus? LatestStatus(this Application app)
        => app.StatusHistories?.OrderByDescending(s => s.Date).FirstOrDefault()?.Status;
}
