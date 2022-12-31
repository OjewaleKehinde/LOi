using FluentValidation;
using LOi.Models;

namespace LOi.Validation
{
    public class AdminValidator :  AbstractValidator<Admin>
    {
       public AdminValidator()
       {
            RuleFor(a => a.Name).NotNull().NotEmpty();
       }
    }
}