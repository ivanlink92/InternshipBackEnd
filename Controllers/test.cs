using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.IO;
using System.Threading.Tasks;

namespace YourNamespace.Controllers
{
    [Route("upload")]
    public class UploadController : Controller
    {
        [HttpPost]
        public async Task<IActionResult> UploadCsv(IFormFile csvFile)
        {
            if (csvFile == null || csvFile.Length == 0)
            {
                return BadRequest("No file uploaded.");
            }

            using (var stream = new StreamReader(csvFile.OpenReadStream()))
            {
                // Read the first line (header or first row)
                var firstRow = await stream.ReadLineAsync();

                // Print to console
                Console.WriteLine("First row of CSV:");
                Console.WriteLine(firstRow);
                Console.WriteLine("good");
            }

            return Ok("File uploaded and first row printed to console.");
        }

    }
}
