using Microsoft.AspNetCore.Mvc;
using DomainDefinitions.Data;
using DomainDefinitions.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace WebApplication1.Infrastructure.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ValidRequestParams
    {
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            return new OkObjectResult(JsonSerializer.Serialize(new ValidParamsObject()));
        }
    }
}
