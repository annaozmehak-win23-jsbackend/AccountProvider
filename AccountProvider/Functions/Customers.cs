using Data.Contexts;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace AccountProvider.Functions;

public class Customers(ILogger<Customers> logger, DataContext context)
{
    private readonly ILogger<Customers> _logger = logger;
    private readonly DataContext _context = context;


    [Function("Customers")]
    public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "get", "post")] HttpRequest req)
    {
        try
        {
            var customers = await _context.Users.OrderByDescending(u => u.Email).ToListAsync();
            return new OkObjectResult(new { Status = 200, customers });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while fetching customers");
            return new BadRequestObjectResult(new { Status = 500, Message = "Unable to get all customers right now" });
        }
    }
}
