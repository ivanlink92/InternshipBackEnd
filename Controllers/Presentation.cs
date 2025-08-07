using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PeerMarking.Data;
using PeerMarking.Models;
using PeerMarking.Services;

namespace PeerMarking.Controllers
{

    [Route("api/[controller]")]
    [ApiController]
    public class PresentationsController : ControllerBase
    {
        private readonly UniversityDbContext _context;

        private readonly EmailService _emailService;

        public PresentationsController(UniversityDbContext context, EmailService emailService)
        {
            _context = context;
            _emailService = emailService;
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
                await reader.ReadLineAsync(); // skip header
                string? line;
                while ((line = await reader.ReadLineAsync()) != null)
                {
                    var values = line.Split(',');

                    if (values.Length < 3)
                        continue; // skip malformed line

                    string studentId = values[0].Trim();
                    string fullName = values[1].Trim();
                    string email = values[2].Trim();

                    var student = await _context.Students.FirstOrDefaultAsync(s => s.StudentId == studentId);

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

            // Shuffle students for slot assignment
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

            // Reload slots with IDs
            var savedSlots = await _context.PresentationSlots
                .Where(s => s.PresentationId == presentation.Id)
                .ToListAsync();

            // Generate Markers
            // Number of peer markers per presentation
            int markersPerPresentation = 3;

            var studentIds = students.Select(s => s.Id).ToList();
            int n = studentIds.Count;

            if (markersPerPresentation >= n)
                return BadRequest("Markers per presentation must be less than total students.");

            // Initialize marker workload count for each student
            var markerLoad = studentIds.ToDictionary(id => id, id => 0);

            // Prepare assignments dictionary: Key = presenterStudentId, Value = list of markerStudentIds
            var assignments = studentIds.ToDictionary(id => id, id => new List<int>());

            var rand = new Random();

            // Assign peer markers randomly but balanced
            foreach (var presenterId in studentIds)
            {
                // Get eligible candidates (exclude self)
                var candidates = studentIds.Where(id => id != presenterId)
                                           .OrderBy(id => markerLoad[id]) // Prioritize students with least load
                                           .ThenBy(_ => rand.Next())      // Randomize among same load
                                           .ToList();

                assignments[presenterId] = candidates.Take(markersPerPresentation).ToList();

                // Update workload count
                foreach (var markerId in assignments[presenterId])
                {
                    markerLoad[markerId]++;
                }
            }


            // Create Marker entities
            var markers = new List<Marker>();

            foreach (var slot in savedSlots)
            {
                int presenterId = slot.StudentId;

                foreach (var markerStudentId in assignments[presenterId])
                {
                    markers.Add(new Marker
                    {
                        PresentationSlotId = slot.Id,
                        MarkerStudentId = markerStudentId,
                        TemporaryPassword = LecturersController.GenerateRandomPassword(20),
                        StartTime = slot.SlotDateTime,
                        EndTime = slot.SlotDateTime.AddMinutes(presentation.DurationMin),
                        IsLecturer = false
                    });
                }

                // Add lecturer marker
                markers.Add(new Marker
                {
                    PresentationSlotId = slot.Id,
                    MarkerStudentId = null, // or lecturer ID if exists
                    TemporaryPassword = "LECTURER",
                    StartTime = slot.SlotDateTime,
                    EndTime = slot.SlotDateTime.AddMinutes(presentation.DurationMin),
                    IsLecturer = true
                });
            }

            _context.Markers.AddRange(markers);
            await _context.SaveChangesAsync();

            // Send email notifications
            var peerMarkers = markers.Where(m => !m.IsLecturer).ToList();

            foreach (var marker in peerMarkers)
            {
                var student = await _context.Students.FindAsync(marker.MarkerStudentId);
                if (student != null && !string.IsNullOrWhiteSpace(student.Email) && student.Email.Contains("@"))
                {
                    string subject = "Peer Marking Assignment - Temporary Password";

                    string body = $"Hello {student.FullName},\n\n" +
                                  $"You have been assigned as a peer marker for an upcoming presentation.\n\n" +
                                  $"Your temporary login password is: {marker.TemporaryPassword}\n\n" +
                                  $"You can access the marking portal **only during this time window**:\n" +
                                  $"Start Time: {marker.StartTime:yyyy-MM-dd HH:mm}\n" +
                                  $"End Time: {marker.EndTime:yyyy-MM-dd HH:mm}\n\n" +
                                  $"Please make sure to log in and complete your peer marking within this timeframe.\n\n" +
                                  $"Portal Link: https://your-marking-portal-link.com\n\n" +
                                  $"Regards,\nPeer Marking System";
                    Console.WriteLine(student.Email);
                    await _emailService.SendEmailAsync(student.Email, subject, body);
                }
                else
                {
                    Console.WriteLine($"Invalid Email for StudentId: {student?.StudentId}, Email: {student?.Email}");
                }
            }

            return Ok("Students uploaded, slots assigned, and markers generated.");

        }


    }
}
