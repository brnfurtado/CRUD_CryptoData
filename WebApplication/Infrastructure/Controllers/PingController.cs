using Microsoft.AspNetCore.Mvc;
using DomainDefinitions.Data;
using DomainDefinitions.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace WebApplication1.Infrastructure.Controllers
{
    [Route("/")]
    [ApiController]
    public class PingController
    {
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            return new OkObjectResult("Connected");
        }
    }
}
