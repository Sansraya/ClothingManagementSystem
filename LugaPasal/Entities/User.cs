using Microsoft.AspNetCore.Identity;

namespace LugaPasal.Entities
{
    public class User : IdentityUser
    {
        public required string FirstName { get; set; }
        public required string LastName { get; set; }
        public required DateOnly DateOfBirth { get; set; }
        public string? ProfilePicturePath { get; set; }

    }
}
