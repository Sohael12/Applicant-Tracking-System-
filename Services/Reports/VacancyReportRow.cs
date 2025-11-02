public class VacancyReportRow
{
    public string VacancyTitle { get; set; } = "";
    public int TotalApplications { get; set; }
    public int InitialScreening { get; set; }
    public int InterviewWithHR { get; set; }
    public int InterviewWithManager { get; set; }
    public int SecondOpinion { get; set; }
    public int Accepted { get; set; }
    public int TrainingPhase { get; set; }
    public int Rejected { get; set; }
}
