namespace Domain.DTO.Requests;
public class CreateJournalFromProjectRequest
{
    public string JournalName { get; set; }
    public string PublisherName { get; set; }
    public string Description { get; set; }
    public DateTime SubmissionDate { get; set; }
    public string DoiNumber { get; set; }
    public string Methodlogy { get; set; }
}
