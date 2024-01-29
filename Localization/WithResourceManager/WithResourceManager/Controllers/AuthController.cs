using Microsoft.AspNetCore.Mvc;
using WithResourceManager.DTO;

namespace WithResourceManager.Controllers {
  [ApiController]
  [Route("[controller]")]
  public class AuthController : ControllerBase {
    public AuthController() { }

    [HttpPost("login")]
    public string Login(LoginDTO loginDTO) {
      return Resources
        .Controllers
        .AuthController
        .messageUserLoggedin
        .Replace("{0}", loginDTO.Email);
    }
  }
}
