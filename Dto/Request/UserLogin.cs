using System.ComponentModel.DataAnnotations;

namespace LibraryManagement.Dto.Request
{
    public class UserLogin
    {
        [Required(ErrorMessage = "User Name is required")]
        public string UserName { get; set; }
        [Required(ErrorMessage = "Password is required")]
        public string Password { get; set; }
    }
}
