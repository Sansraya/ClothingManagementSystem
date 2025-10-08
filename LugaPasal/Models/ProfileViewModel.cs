using LugaPasal.Entities;
using LugaPasal.Validation;
using System.ComponentModel.DataAnnotations;

namespace LugaPasal.Models
{
    public class ProfileViewModel
    {
        public string Id { get; set; }
        [RegularExpression(@"^[a-zA-Z]+$", ErrorMessage = "First Name can only contain letters")]
        public required string FirstName { get; set; }
        [RegularExpression(@"^[a-zA-Z]+$", ErrorMessage = "Last Name can only contain letters")]
        public required string LastName { get; set; }
        [RegularExpression(@"^[a-zA-Z0-9]*$", ErrorMessage = "Username can only contain letters and numbers")]
        public string Username { get; set; }
        [EmailAddress(ErrorMessage = "Email is Required")]
        public string Email { get; set; }
        [RegularExpression(@"^98\d{8}$", ErrorMessage = "Phone number must start with 98 and contain 10 digits")]
        public string Phone { get; set; }
        [MinimumAge(18, ErrorMessage = "You must be atleast 18 years old")]
        public required DateOnly DateOfBirth { get; set; }
        public string? ProfilePicturePath { get; set; }
        public IFormFile? ProfilePicture { get; set; }

        public List<Products> ?ProductsOfUser { get; set; }


    }
}
