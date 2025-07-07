using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PeerMarking.Data;
using PeerMarking.Models;

namespace PeerMarking.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PresentationSlotsController : ControllerBase
    {
        private readonly UniversityDbContext _context;

        public PresentationSlotsController(UniversityDbContext context)
        {
            _context = context;
        }

        // GET: api/PresentationSlots
        [HttpGet]
        public async Task<ActionResult<IEnumerable<PresentationSlot>>> GetPresentationSlots()
        {
            return await _context.PresentationSlots.ToListAsync();
        }

        // GET: api/PresentationSlots/5
        [HttpGet("{id}")]
        public async Task<ActionResult<PresentationSlot>> GetPresentationSlot(int id)
        {
            var slot = await _context.PresentationSlots.FindAsync(id);
            if (slot == null)
                return NotFound();

            return slot;
        }

        // POST: api/PresentationSlots
        [HttpPost]
        public async Task<ActionResult<PresentationSlot>> PostPresentationSlot(PresentationSlot slot)
        {
            _context.PresentationSlots.Add(slot);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetPresentationSlot), new { id = slot.Id }, slot);
        }

        // PUT: api/PresentationSlots/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutPresentationSlot(int id, PresentationSlot slot)
        {
            if (id != slot.Id)
                return BadRequest();

            _context.Entry(slot).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!PresentationSlotExists(id))
                    return NotFound();
                else
                    throw;
            }

            return NoContent();
        }

        // DELETE: api/PresentationSlots/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePresentationSlot(int id)
        {
            var slot = await _context.PresentationSlots.FindAsync(id);
            if (slot == null)
                return NotFound();

            _context.PresentationSlots.Remove(slot);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool PresentationSlotExists(int id)
        {
            return _context.PresentationSlots.Any(e => e.Id == id);
        }
    }
}
