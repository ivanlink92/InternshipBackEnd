using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using PeerMarking.Data;
using PeerMarking.Models;

namespace PeerMarking.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LecturersController : ControllerBase
    {
        private readonly UniversityDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;


        public LecturersController(UniversityDbContext context, UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager)
        {
            _context = context;
            _userManager = userManager;
            _signInManager = signInManager;
        }

        // GET: api/Lecturers
        [HttpGet]
        [Authorize]
        public async Task<ActionResult<IEnumerable<Lecturer>>> GetLecturers()
        {
            return await _context.Lecturers.ToListAsync();
        }

        // GET: api/Lecturers/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Lecturer>> GetLecturer(int id)
        {
            var lecturer = await _context.Lecturers.FindAsync(id);

            if (lecturer == null)
            {
                return NotFound();
            }

            return lecturer;
        }

        // POST: api/Lecturers
        [HttpPost]
        public async Task<ActionResult<Lecturer>> PostLecturer(Lecturer lecturer)
        {
            var user = new IdentityUser
            {
                UserName = lecturer.Username,
                Email = lecturer.Email,
                EmailConfirmed = true
            };

            var result = await _userManager.CreateAsync(user, lecturer.Password);
            lecturer.Password = "";
            _context.Lecturers.Add(lecturer);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetLecturer), new { id = lecturer.Id }, lecturer);
        }

        // PUT: api/Lecturers/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutLecturer(int id, Lecturer lecturer)
        {
            if (id != lecturer.Id)
            {
                return BadRequest();
            }

            _context.Entry(lecturer).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!LecturerExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // DELETE: api/Lecturers/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteLecturer(int id)
        {
            var lecturer = await _context.Lecturers.FindAsync(id);
            if (lecturer == null)
            {
                return NotFound();
            }

            _context.Lecturers.Remove(lecturer);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool LecturerExists(int id)
        {
            return _context.Lecturers.Any(e => e.Id == id);
        }

        // POST: api/Lecturers
        [HttpPost("Login")]
        public async Task<ActionResult<Lecturer>> Login(SignInRequest signInRequest)
        {
            var result = await _signInManager.PasswordSignInAsync(signInRequest.user, signInRequest.pass, isPersistent: false, lockoutOnFailure: false);
            var user = await _userManager.FindByNameAsync(signInRequest.user);

            if (result.Succeeded)
            {
                var token = GenerateJwtToken(user, null);
                return Ok(new { token });
            }
            else
            {
                return Unauthorized("Invalid login attempt");
            }

        }

        private string GenerateJwtToken(IdentityUser user, IList<string> roles = null)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("Jwt:KeySuperSecret1234567890987654321!!!!!!!!!!!!!!!!!!!!"));
            var creds = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Email ?? ""),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.NameIdentifier, user.Id)
            };

            if (roles != null)
            {
                claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));
            }

            var token = new JwtSecurityToken(
                issuer: "Jwt:IssuerSuperSecret!!",
                audience: "Jwt:AudienceSuperSecret!!",
                claims: claims,
                expires: DateTime.UtcNow.AddHours(1),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        [HttpGet("secureapi")]
        [Authorize]
        public async Task<ActionResult> SecureAPI()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return Ok(new { userId });
        }
    }
}
