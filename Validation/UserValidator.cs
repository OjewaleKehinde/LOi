using FluentValidation;
using LOi.Models;
using System.Linq;

namespace LOi.Validation
{
    public class UserValidator : AbstractValidator<User>
    {
        public UserValidator()
        {
            // decimal n;
            RuleFor(u => u.LastName).NotNull().NotEmpty().Length(4, 30).WithMessage("Last name cannot be less than 4 characters or exceed 30 characters");
            RuleFor(u => u.FirstName).NotNull().NotEmpty().Length(4, 30).WithMessage("First name cannot be less than 4 characters or exceed 30 characters");
            RuleFor(u => u.PhoneNumber).NotNull().NotEmpty().Must(u => u.Length == 13).Must(u => decimal.TryParse(u, out _) == true).WithMessage("Phone number should contain 13 digits alone");
            RuleFor(u => u.Email).EmailAddress().NotNull().NotEmpty();

        }
    }
}