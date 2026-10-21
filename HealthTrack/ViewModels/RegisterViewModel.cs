using System.ComponentModel.DataAnnotations;

namespace HealthTrack.ViewModels;

public class RegisterViewModel
{
    public string name { get; set; }
    public string email { get; set; }
    public string password { get; set; }
    
    [Compare("password", ErrorMessage = "The password and confirmation password do not match.")]
    public string confirmPassword { get; set; }
    
    public string role { get; set; }
}