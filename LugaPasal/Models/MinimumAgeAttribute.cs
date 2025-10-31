using System.ComponentModel.DataAnnotations;

namespace LugaPasal.Validation
{
    public class MinimumAgeAttribute : ValidationAttribute
    {
        private readonly int _minimumAge;

        public MinimumAgeAttribute(int minimumAge)
        {
            _minimumAge = minimumAge;
        }
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value is not DateOnly dateofBirth)
            {
                return new ValidationResult("Invalid date of birth");
            }
            var today = DateOnly.FromDateTime(DateTime.Today);
            var age = today.Year - dateofBirth.Year;
            if ( dateofBirth> today.AddYears(-age))
            {
                age--;
            }
            return age >= _minimumAge? ValidationResult.Success
                : new ValidationResult("You must be atleast 18 years old");
        }
    }
}
