using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace WithResourceManager.DTO {
  public class LoginDTO {
    [Required(ErrorMessageResourceType = typeof(Resources.SharedResources), ErrorMessageResourceName = "errorRequired")]
    [EmailAddress(ErrorMessageResourceType = typeof(Resources.SharedResources), ErrorMessageResourceName = "errorEmail")]
    [DefaultValue("")]
    [Display(Name = "email", ResourceType = typeof(Resources.SharedResources))]
    public string Email { get; set; } = "";

    [Required(ErrorMessageResourceType = typeof(Resources.SharedResources), ErrorMessageResourceName = "errorRequired")]
    [Length(8, 32, ErrorMessageResourceType = typeof(Resources.SharedResources), ErrorMessageResourceName = "errorStringLength")]
    [RegularExpression("^[a-zA-Z0-9]*$", ErrorMessageResourceType = typeof(Resources.SharedResources), ErrorMessageResourceName = "errorRegularExpression")]
    [DefaultValue("")]
    [Display(Name = "password", ResourceType = typeof(Resources.SharedResources))]
    public string Password { get; set; } = "";
  }
}
