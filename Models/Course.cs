namespace PeerMarking.Models
{
    public class Course
    {
        public int Id { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public string Semester { get; set; }

        // Foreign Key
        public int LecturerId { get; set; }
    }
}
