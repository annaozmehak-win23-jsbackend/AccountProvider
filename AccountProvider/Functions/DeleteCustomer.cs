using Data.Contexts;
using Data.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace AccountProvider.Functions;

public class DeleteCustomer(ILogger<DeleteCustomer> logger, DataContext context, UserManager<UserAccount> userManager)
{
    private readonly ILogger<DeleteCustomer> _logger = logger;
    private readonly DataContext _context = context;
    private readonly UserManager<UserAccount> _userManager = userManager;


    [Function("DeleteCustomer")]
    public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequest req)
    {
        string userId = req.Query["userId"]!;

        try
        {
            var user = await _userManager.FindByIdAsync(userId!);
            if (user == null)
            {
                return new NotFoundObjectResult($"User with ID = {userId} cannot be found");
            }
            else
            {
                var result = await _userManager.DeleteAsync(user);
                if (result.Succeeded)
                {
                    return new OkObjectResult("User deleted successfully");
                }
                else
                {
                    return new BadRequestObjectResult("User could not be deleted");
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"Exception thrown: {ex.Message}");
            return new StatusCodeResult(StatusCodes.Status500InternalServerError);
        }
    }
}
