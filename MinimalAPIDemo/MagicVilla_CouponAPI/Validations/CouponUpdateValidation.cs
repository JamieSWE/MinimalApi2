using FluentValidation;
using MagicVilla_CouponAPI.Models.DTOs;

namespace MagicVilla_CouponAPI.Validations;

public class CouponUpdateValidation : AbstractValidator<CouponUpdateDTO>
{
    public CouponUpdateValidation()
    {
        RuleFor(r => r.Id).NotEmpty().GreaterThan(0);
        RuleFor(r => r.Name).NotEmpty();
        RuleFor(r => r.Percent).InclusiveBetween(1, 100);
    }
}
