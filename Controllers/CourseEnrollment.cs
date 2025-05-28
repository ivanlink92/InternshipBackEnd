using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PeerMarking.Data;
using PeerMarking.Models;

namespace PeerMarking.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CourseEnrollmentsController : ControllerBase
    {
        private readonly UniversityDbContext _context;

        public CourseEnrollmentsController(UniversityDbContext context)
        {
            _context = context;
        }

        // GET: api/CourseEnrollments
        [HttpGet]
        public async Task<ActionResult<IEnumerable<CourseEnrollment>>> GetEnrollments()
        {
            return await _context.CourseEnrollments.ToListAsync();
        }

        // GET: api/CourseEnrollments/5
        [HttpGet("{id}")]
        public async Task<ActionResult<CourseEnrollment>> GetEnrollment(int id)
        {
            var enrollment = await _context.CourseEnrollments.FindAsync(id);
            if (enrollment == null)
                return NotFound();

            return enrollment;
        }

        // POST: api/CourseEnrollments
        [HttpPost]
        public async Task<ActionResult<CourseEnrollment>> PostEnrollment(CourseEnrollment enrollment)
        {
            enrollment.EnrolledAt = DateTime.UtcNow;
            _context.CourseEnrollments.Add(enrollment);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetEnrollment), new { id = enrollment.Id }, enrollment);
        }

        // DELETE: api/CourseEnrollments/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteEnrollment(int id)
        {
            var enrollment = await _context.CourseEnrollments.FindAsync(id);
            if (enrollment == null)
                return NotFound();

            _context.CourseEnrollments.Remove(enrollment);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool EnrollmentExists(int id)
        {
            return _context.CourseEnrollments.Any(e => e.Id == id);
        }
    }
}
