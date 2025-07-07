namespace PeerMarking.Models
{
    public class Presentation
    {
        public int Id { get; set; }
        public int CourseId { get; set; }
        public string Title { get; set; } = string.Empty;
        public int DurationMin { get; set; }
        public DateTime PresentationDate { get; set; }
    }
}