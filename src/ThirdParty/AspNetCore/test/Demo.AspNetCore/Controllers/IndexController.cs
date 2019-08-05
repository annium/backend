using System.ComponentModel.DataAnnotations;
using Annium.AspNetCore.Extensions;
using Annium.Data.Operations;
using Microsoft.AspNetCore.Mvc;

namespace Demo.AspNetCore.Controllers
{
    [Route("/")]
    public class IndexController : ServerController
    {
        public IndexController()
        {

        }

        [HttpGet]
        public IActionResult Index()
        {
            return Ok("Hello World from Demo.AspNetCore");
        }

        [HttpGet("created")]
        public IActionResult Created()
        {
            return Created("created");
        }

        [HttpPost("bad-request")]
        public IActionResult BadRequest([FromBody] Payload payload)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            return NoContent();
        }

        [HttpGet("forbidden")]
        public IActionResult Forbidden()
        {
            return Forbidden(Result.Failure());
        }

        [HttpGet("conflict")]
        public new IActionResult Conflict()
        {
            return Conflict(Result.Failure());
        }

        [HttpGet("not-found")]
        public new IActionResult NotFound()
        {
            return NotFound(Result.Failure());
        }

        [HttpGet("server-error")]
        public IActionResult ServerError()
        {
            return ServerError(Result.Failure());
        }

        public class Payload
        {
            [Required]
            public string Name { get; set; }

            [Required]
            public string Email { get; set; }
        }
    }
}