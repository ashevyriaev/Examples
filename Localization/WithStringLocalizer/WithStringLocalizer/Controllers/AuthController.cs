using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using WithStringLocalizer.DTO;

namespace WithStringLocalizer.Controllers {
  [ApiController]
  [Route("[controller]")]
  public class AuthController : ControllerBase {
    private IStringLocalizer<AuthController> _stringLocalizer;

    public AuthController(IStringLocalizer<AuthController> stringLocalizer) =>
      _stringLocalizer = stringLocalizer;

    [HttpPost("login")]
    public string Login(LoginDTO loginDTO) {
      return _stringLocalizer["messageUserLoggedin"].ToString().Replace("{0}", loginDTO.Email);
    }
  }
}
