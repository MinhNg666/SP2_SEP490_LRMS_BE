namespace Domain.DTO.Requests;
public class CreateConferenceFromProjectRequest
{
    public string ConferenceName { get; set; }
    public string Location { get; set; }
    public int ConferenceRanking { get; set; }
    public DateTime PresentationDate { get; set; }
    public int PresentationType { get; set; }
    public string Description { get; set; }
}
