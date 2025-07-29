using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PeerMarking.Data;
using PeerMarking.Models;

namespace PeerMarking.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StudentsController : ControllerBase
    {
        private readonly UniversityDbContext _context;

        public StudentsController(UniversityDbContext context)
        {
            _context = context;
        }

        // GET: api/Students
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Student>>> GetStudents()
        {
            return await _context.Students.ToListAsync();
        }

        // GET: api/Students/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Student>> GetStudent(int id)
        {
            var student = await _context.Students.FindAsync(id);
            if (student == null)
                return NotFound();

            return student;
        }

        // POST: api/Students
        [HttpPost]
        public async Task<ActionResult<Student>> PostStudent(Student student)
        {
            var existingStudent = await _context.Students
                .FirstOrDefaultAsync(s => s.StudentId == student.StudentId);

            if (existingStudent != null)
            {
                return Conflict(new { message = "StudentId already exists." });
            }

            _context.Students.Add(student);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetStudent), new { id = student.Id }, student);
        }


        // PUT: api/Students/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutStudent(int id, Student student)
        {
            if (id != student.Id)
                return BadRequest();

            _context.Entry(student).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!StudentExists(id))
                    return NotFound();
                else
                    throw;
            }

            return NoContent();
        }

        // DELETE: api/Students/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteStudent(int id)
        {
            var student = await _context.Students.FindAsync(id);
            if (student == null)
                return NotFound();

            _context.Students.Remove(student);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool StudentExists(int id)
        {
            return _context.Students.Any(e => e.Id == id);
        }

        [HttpPost("upload")]
        public async Task<IActionResult> UploadCsv(IFormFile csvFile)
        {
            if (csvFile == null || csvFile.Length == 0)
            {
                return BadRequest("No file uploaded.");
            }

            using var reader = new StreamReader(csvFile.OpenReadStream());
            var content = await reader.ReadToEndAsync();

            var lines = content.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);
            var students = new List<Student>();

            foreach (var line in lines.Skip(1)) // Skip CSV header
            {
                var columns = line.Split(',');

                if (columns.Length < 3)
                    continue;

                var studentId = columns[0].Trim();
                var fullName = columns[1].Trim();
                var email = columns[2].Trim();

                if (string.IsNullOrWhiteSpace(studentId) || string.IsNullOrWhiteSpace(fullName) || string.IsNullOrWhiteSpace(email))
                    continue;

                students.Add(new Student
                {
                    StudentId = studentId,
                    FullName = fullName,
                    Email = email
                });
            }
            // Logging connection string being used
            Console.WriteLine(_context.Database.GetConnectionString());


            _context.Students.AddRange(students);
            await _context.SaveChangesAsync();

            return Ok("Students uploaded successfully.");
        }
    }
}
