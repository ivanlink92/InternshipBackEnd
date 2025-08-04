using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PeerMarking.Data;
using PeerMarking.Models;

namespace PeerMarking.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MarkersController : ControllerBase
    {
        private readonly UniversityDbContext _context;

        public MarkersController(UniversityDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Marker>>> GetMarkers()
        {
            return await _context.Markers
                .Include(m => m.MarkerStudent)
                .Include(m => m.PresentationSlot)
                .ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Marker>> GetMarker(int id)
        {
            var marker = await _context.Markers
                .Include(m => m.MarkerStudent)
                .Include(m => m.PresentationSlot)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (marker == null)
                return NotFound();

            return marker;
        }
    }
}
