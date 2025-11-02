using Microsoft.EntityFrameworkCore;
using Stageproject_ATS_AP2025Q2.Data;
using Stageproject_ATS_AP2025Q2.Models;

public class TemplateService
{
    private readonly AppDbContext _context;

    public TemplateService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<string> RenderTemplateAsync(string templateName, Dictionary<string, string> replacements)
    {
        var template = await _context.Templates
            .FirstOrDefaultAsync(t => t.Name == templateName);

        if (template == null)
            throw new Exception($"Template '{templateName}' not found.");

        string body = template.HtmlContent;

        // Vervang alle placeholders in de template
        foreach (var kvp in replacements)
        {
            body = body.Replace("{" + kvp.Key + "}", kvp.Value);
        }

        return body;
    }

        public async Task<Template?> GetTemplateByNameAsync(string name)
        {
            return await _context.Templates
                .FirstOrDefaultAsync(t => t.Name == name);
        }
}
