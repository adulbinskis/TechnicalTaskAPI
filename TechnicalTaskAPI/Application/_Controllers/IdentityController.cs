using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TechnicalTaskAPI.Application.Identity.Commands;
using TechnicalTaskAPI.Application.Identity.Models;

namespace TechnicalTaskAPI.Application._Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class IdentityController : MediatRController
    {
        [HttpPost("register")]
        public async Task<ActionResult<RegistrationResponse>> Register(RegistrationRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var response = await Mediator.Send(new Register.Command
            {
                Username = request.Username,
                Email = request.Email,
                Password = request.Password
            });

            if (response == null)
            {
                return BadRequest("Registration failed. Please check the provided details and try again.");
            }

            return CreatedAtAction(nameof(Register), new { email = response.Email }, response);
        }

        [HttpPost("login")]
        public async Task<ActionResult<AuthResponse>> Authenticate([FromBody] AuthRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var response = await Mediator.Send(new Authenticate.Command
            {
                Email = request.Email,
                Password = request.Password
            });

            if (response == null)
            {
                return BadRequest("Bad credentials");
            }
            else 
            {
                var cookieOptions = new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.None,
                    Expires = response.RefreshTokenExpirationDate,
                };

                Response.Cookies.Append("refreshToken", response.RefreshToken, cookieOptions);

                var authResponse = new AuthResponse
                {
                    UserId = response.UserId,
                    Email = response.Email,
                    Token = response.Token,
                    UserName = response.UserName,
                    Role = response.Role
                };

                return Ok(authResponse);
            }
        }

        [HttpPost("logout")]
        public async Task<ActionResult<AuthResponse>> Logout()
        {
            var refreshToken = Request.Cookies["refreshToken"];

            var response = await Mediator.Send(new Logout.Command
            {
                RefreshToken = refreshToken
            });

            if (refreshToken != null)
            {
                Response.Cookies.Delete("refreshToken");
            }

            return Ok();
        }


        [HttpGet("refresh")]
        public async Task<ActionResult<AuthResponse>> Refresh()
        {
            var refreshToken = Request.Cookies["refreshToken"];

            var response = await Mediator.Send(new Refresh.Command
            { 
                RefreshToken = refreshToken
            });

            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.None,
                Expires = response.RefreshTokenExpirationDate,
            };

            Response.Cookies.Append("refreshToken", response.RefreshToken, cookieOptions);

            var authResponse = new AuthResponse
            {
                UserId = response.UserId,
                Email = response.Email,
                Token = response.Token,
                UserName = response.UserName,
                Role = response.Role,
            };

            return authResponse;
        }
    }
}
