using System.ComponentModel.DataAnnotations;

namespace PeerMarking.Models
{
    public class PresentationSlot
    {
        public int Id { get; set; }
        public int PresentationId { get; set; }
        public int StudentId { get; set; }
        public DateTime SlotDateTime { get; set; }
    }
}