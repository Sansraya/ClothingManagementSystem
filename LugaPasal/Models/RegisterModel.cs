using LugaPasal.Validation;
using System.ComponentModel.DataAnnotations;

namespace LugaPasal.Models
{
    public class RegisterModel
    {
        [Required(ErrorMessage = "First Name is Required")]
        [RegularExpression(@"^[a-zA-Z]+$", ErrorMessage = "First Name can only contain letters")]
        public required string FirstName { get; set; }

        [Required(ErrorMessage = "last Name is Required")]
        [RegularExpression(@"^[a-zA-Z]+$", ErrorMessage = "Last Name can only contain letters")]
        public required string LastName { get; set; }

        [Required(ErrorMessage = "Username is Required")]
        [RegularExpression(@"^[a-zA-Z0-9]*$", ErrorMessage = "Username can only contain letters and numbers")]
        public required string Username { get; set; }

        [Required(ErrorMessage = "DOB is Required")]
        [MinimumAge(18,ErrorMessage ="You must be atleast 18 years old")]
        public required DateOnly DateOfBirth { get; set; }

        [Phone(ErrorMessage = "Invalid Phone Number")]
        [RegularExpression(@"^98\d{8}$", ErrorMessage = "Phone number must start with 98 and contain 10 digits")]
        public required string PhoneNumber { get; set; }

        [EmailAddress(ErrorMessage = "Email is Required")]
        public required string Email { get; set; }

        [Required(ErrorMessage = "Password is Required")]
        public required string Password { get; set; }

        [Required(ErrorMessage = "Confirm Password is Required")]
        [Compare("Password",ErrorMessage="Passwords do not match")]
        public required string ConfirmPassword { get; set; }
        public string? ProfilePicturePath { get; set; }
        public IFormFile? ProfilePicture { get; set; }

    }
}
