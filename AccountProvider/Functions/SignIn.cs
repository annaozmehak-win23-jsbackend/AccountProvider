using AccountProvider.Models;
using Data.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace AccountProvider.Functions;

public class SignIn(ILogger<SignIn> logger, SignInManager<UserAccount> signInManager, UserManager<UserAccount> userManager)
{
    private readonly ILogger<SignIn> _logger = logger;
    private readonly SignInManager<UserAccount> _signInManager = signInManager;
    private readonly UserManager<UserAccount> _userManager = userManager;


    [Function("SignIn")]
    public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequest req)
    {
        string body;

        try
        {
            body = await new StreamReader(req.Body).ReadToEndAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError($"StreamReader :: {ex.Message}");
            return new StatusCodeResult(StatusCodes.Status500InternalServerError);
        }

        UserLoginRequest ulr;

         try
         {
            ulr = JsonConvert.DeserializeObject<UserLoginRequest>(body)!;
         }
         catch (Exception ex)
         {
            _logger.LogError($"JsonConvert.DeserializeObject<UserLoginRequest> :: {ex.Message}");
            return new BadRequestResult();
         }

         if (ulr != null && !string.IsNullOrEmpty(ulr.Email) && !string.IsNullOrEmpty(ulr.Password))
         {
            try
            {
                var user = await _userManager.FindByEmailAsync(ulr.Email);
                if (user == null)
                {
                    _logger.LogError($"No user found with email: {ulr.Email}");
                    return new UnauthorizedResult();
                }

                var isPasswordValid = await _userManager.CheckPasswordAsync(user, ulr.Password);
                if (!isPasswordValid)
                {
                    _logger.LogError($"Invalid password for user: {ulr.Email}");
                    return new UnauthorizedResult();
                }

                _logger.LogInformation($"Attempting to sign in user: {ulr.Email}");
                var result = await _signInManager.PasswordSignInAsync(ulr.Email, ulr.Password, isPersistent: false, lockoutOnFailure: false);
                _logger.LogInformation($"Sign in result: {result.Succeeded}");
                if (result.Succeeded)
                {
                    return new OkResult();
                }

                return new UnauthorizedResult();
            }
            catch (Exception ex)
            {
                _logger.LogError($"await _signInManager.PasswordSignInAsync :: {ex.Message}");
                return new StatusCodeResult(StatusCodes.Status500InternalServerError);
            }
         }

        return new BadRequestResult();
    }
}
