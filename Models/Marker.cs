using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PeerMarking.Models
{
    public class Marker
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey("PresentationSlot")]
        public int PresentationSlotId { get; set; }

        [ForeignKey("Student")]
        public int? MarkerStudentId { get; set; }  // Nullable for Lecturer Markers

        [MaxLength(100)]
        public string TemporaryPassword { get; set; }

        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }

        public bool IsLecturer { get; set; }

        // Navigation Properties
        public virtual PresentationSlot PresentationSlot { get; set; }
        public virtual Student MarkerStudent { get; set; }
    }
}
