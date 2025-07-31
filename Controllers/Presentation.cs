using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PeerMarking.Data;
using PeerMarking.Models;

namespace PeerMarking.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PresentationsController : ControllerBase
    {
        private readonly UniversityDbContext _context;

        public PresentationsController(UniversityDbContext context)
        {
            _context = context;
        }

        // GET: api/Presentations
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Presentation>>> GetPresentations()
        {
            return await _context.Presentations.ToListAsync();
        }

        // GET: api/Presentations/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Presentation>> GetPresentation(int id)
        {
            var presentation = await _context.Presentations.FindAsync(id);
            if (presentation == null)
                return NotFound();

            return presentation;
        }

        // POST: api/Presentations
        [HttpPost]
        public async Task<ActionResult<Presentation>> PostPresentation(Presentation presentation)
        {
            _context.Presentations.Add(presentation);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetPresentation), new { id = presentation.Id }, presentation);
        }

        // PUT: api/Presentations/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutPresentation(int id, Presentation presentation)
        {
            if (id != presentation.Id)
                return BadRequest();

            _context.Entry(presentation).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!PresentationExists(id))
                    return NotFound();
                else
                    throw;
            }

            return NoContent();
        }

        // DELETE: api/Presentations/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePresentation(int id)
        {
            var presentation = await _context.Presentations.FindAsync(id);
            if (presentation == null)
                return NotFound();

            _context.Presentations.Remove(presentation);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool PresentationExists(int id)
        {
            return _context.Presentations.Any(e => e.Id == id);
        }

        [HttpPost("presentations")]
        public async Task<IActionResult> CreatePresentation([FromBody] Presentation presentation)
        {
            _context.Presentations.Add(presentation);
            await _context.SaveChangesAsync();
            return Ok(new { presentation.Id });
        }

        [HttpPost("presentations/{presentationId}/upload-students")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> UploadStudents(int presentationId, IFormFile csvFile)
        {
            if (csvFile == null || csvFile.Length == 0)
                return BadRequest("No file uploaded.");

            var presentation = await _context.Presentations.FindAsync(presentationId);
            if (presentation == null)
                return NotFound("Presentation not found.");

            var students = new List<Student>();

            using (var reader = new StreamReader(csvFile.OpenReadStream()))
            {
                await reader.ReadLineAsync();
                string? line;
                while ((line = await reader.ReadLineAsync()) != null)
                {
                    var values = line.Split(',');

                    string studentId = values[0].Trim();
                    string fullName = values[1].Trim();
                    string email = values[2].Trim();

                    var student = await _context.Students
                        .FirstOrDefaultAsync(s => s.StudentId == studentId);

                    if (student == null)
                    {
                        student = new Student
                        {
                            StudentId = studentId,
                            FullName = fullName,
                            Email = email
                        };
                        _context.Students.Add(student);
                        await _context.SaveChangesAsync();
                    }

                    students.Add(student);
                }
            }

            var random = new Random();
            var randomizedStudents = students.OrderBy(s => random.Next()).ToList();

            DateTime slotTime = presentation.PresentationDate;
            var slots = new List<PresentationSlot>();

            foreach (var student in randomizedStudents)
            {
                slots.Add(new PresentationSlot
                {
                    PresentationId = presentation.Id,
                    StudentId = student.Id,
                    SlotDateTime = slotTime
                });

                slotTime = slotTime.AddMinutes(presentation.DurationMin);
            }

            _context.PresentationSlots.AddRange(slots);
            await _context.SaveChangesAsync();

            return Ok("Students uploaded and slots assigned.");
        }

    }
}
