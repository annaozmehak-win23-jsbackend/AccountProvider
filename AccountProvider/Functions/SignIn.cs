using AccountProvider.Models;
using Data.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;

namespace AccountProvider.Functions;

public class SignIn(ILogger<SignIn> logger, SignInManager<UserAccount> signInManager, UserManager<UserAccount> userManager)
{
    private readonly ILogger<SignIn> _logger = logger;
    private readonly SignInManager<UserAccount> _signInManager = signInManager;
    private readonly UserManager<UserAccount> _userManager = userManager;


    [Function("SignIn")]
    public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequest req)
    {
        string body = null!;

        try
        {
            body = await new StreamReader(req.Body).ReadToEndAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError($"StreamReader :: {ex.Message}");
        }

        if (body != null)
        {
            UserLoginRequest ulr = null!;

            try
            {
                ulr = JsonConvert.DeserializeObject<UserLoginRequest>(body)!;
            }
            catch (Exception ex)
            {
                _logger.LogError($"JsonConvert.DeserializeObject<UserLoginRequest> :: {ex.Message}");
            }

            if (ulr != null && !string.IsNullOrEmpty(ulr.Email) && !string.IsNullOrEmpty(ulr.Password))
            {
                try
                {
                    var userAccount = await _userManager.FindByEmailAsync(ulr.Email);
                    var result = await _signInManager.CheckPasswordSignInAsync(userAccount!, ulr.Password, false);
                    if (result.Succeeded)
                    {
                        //Get token from TokenProvider

                        return new OkObjectResult("accesstoken");
                    }

                    return new UnauthorizedResult();
                }
                catch (Exception ex)
                {
                    _logger.LogError($"await _signInManager.PasswordSignInAsync :: {ex.Message}");
                }
            }
        }

        return new BadRequestResult();
    }
}
