using BudgetTracker.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace BudgetTracker.Controllers
{
    [AllowAnonymous]
    [Route("check")]
    public class CheckController : Controller
    {
        private readonly BudgettrackerdbContext _context;
        private readonly ILogger<CheckController> _logger;

        public CheckController(BudgettrackerdbContext context, ILogger<CheckController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: /check
        [HttpGet("")]
        public async Task<IActionResult> Index()
        {
            try
            {
                var connection = _context.Database.GetDbConnection();
                if (connection.State != ConnectionState.Open)
                    await connection.OpenAsync();

                await using var command = connection.CreateCommand();
                command.CommandText = "SELECT 1";
                var result = await command.ExecuteScalarAsync();

                if (result is null)
                    return ServerWasntReached();

                return Content("OK");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Check de base de datos fallido. TraceId: {TraceId}", HttpContext.TraceIdentifier);
                return ServerWasntReached();
            }
        }

        private ContentResult ServerWasntReached()
        {
            return new ContentResult
            {
                StatusCode = StatusCodes.Status500InternalServerError,
                Content = "server wasn't reached",
                ContentType = "text/plain"
            };
        }
    }
}
