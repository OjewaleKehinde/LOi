using FluentValidation;
using LOi.Models;

namespace LOi.Validation
{
    public class OrderValidator : AbstractValidator<Order>
    {
        public OrderValidator()
        {
            List<string> sizes = new()
            {
                "100mL",
                "200mL",
                "500mL",
                "1000mL",
                "2000mL",
            };

            List<string> orderStatus = new()
            {
                "Pending",
                "InTransit",
                "Delivered",
            };
            RuleFor(o => o.Cost).GreaterThan(0);
            RuleFor(o => o.Location).NotNull().NotEmpty().Length(2, 100).WithMessage("Delivery location must be between 2 and 100 characters");
            RuleFor(o => o.Quantity).GreaterThan(0).WithMessage("You cannot order less than 1 ice cream");
            RuleFor(o => o.Size).Must(o => sizes.Contains(o)).WithMessage("Ice cream size can only be 100mL, 200mL, 500mL, 1000mL or 2000mL");
            RuleFor(o => o.OrderStatus).Must(o => orderStatus.Contains(o)).WithMessage("Order status can only be Pending, InTransit or Delivered");
        }
    }
}