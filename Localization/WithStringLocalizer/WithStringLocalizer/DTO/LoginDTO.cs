using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace WithStringLocalizer.DTO {
  public class LoginDTO {
    [Required(ErrorMessage = "errorRequired")]
    [EmailAddress(ErrorMessage = "errorEmail")]
    [DefaultValue("")]
    [Display(Name = "email")]
    public string Email { get; set; } = "";

    [Required(ErrorMessage = "errorRequired")]
    [Length(8, 32, ErrorMessage = "errorStringLength")]
    [RegularExpression("^[a-zA-Z0-9]*$", ErrorMessage = "errorRegularExpression")]
    [DefaultValue("")]
    [Display(Name = "password")]
    public string Password { get; set; } = "";
  }
}
