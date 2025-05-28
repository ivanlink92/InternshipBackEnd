using System;

namespace PeerMarking.Models
{
    public class CourseEnrollment
    {
        public int Id { get; set; }
        public int CourseId { get; set; }
        public int StudentId { get; set; }
        public DateTime EnrolledAt { get; set; }
    }
}
