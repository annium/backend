using System.ComponentModel.DataAnnotations;
using Annium.AspNetCore.Extensions;
using Annium.Core.Mediator;
using Annium.Data.Operations;
using Microsoft.AspNetCore.Mvc;

namespace Demo.AspNetCore.Controllers
{
    [Route("/")]
    public class IndexController : ServerController
    {
        public IndexController(IMediator mediator) : base(mediator)
        {

        }

        [HttpGet]
        public IActionResult Index()
        {
            return Ok("Hello World from Demo.AspNetCore");
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

        public class Payload
        {
            [Required]
            public string Name { get; set; } = string.Empty;

            [Required]
            public string Email { get; set; } = string.Empty;
        }
    }
}