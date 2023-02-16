using System.ComponentModel.DataAnnotations;

namespace LibraryManagement.Dto.Request
{
    public class UserRegistration
    {
        [Required(ErrorMessage = "User Name is required")]
        public string UserName { get; set; }
        [EmailAddress]
        [Required(ErrorMessage = "Email is required")]
        public string Email { get; set; }
        [Required(ErrorMessage = "Password is required")]
        public string Password { get; set; }
        public string LibraryCardNumber { get; set; }
    }
}
