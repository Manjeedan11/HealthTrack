using System.ComponentModel.DataAnnotations;

namespace HealthTrack.ViewModels.Admin
{
    public class CreateTrainerViewModel
    {
        [Required(ErrorMessage = "Name is required")]
        public string name { get; set; }

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email address")]
        public string email { get; set; }

        [Required(ErrorMessage = "Password is required")]
        [MinLength(6, ErrorMessage = "Password must be at least 6 characters")]
        public string password { get; set; }

        public string specialization { get; set; }
        public string bio { get; set; }
    }
}