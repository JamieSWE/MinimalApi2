using FluentValidation;
using MagicVilla_CouponAPI.Models.DTOs;

namespace MagicVilla_CouponAPI.Validations;

public class CouponCreateValidation : AbstractValidator<CouponCreateDTO>
{
    public CouponCreateValidation()
    {
        RuleFor(r => r.Name).NotEmpty();
        RuleFor(r => r.Percent).InclusiveBetween(1, 100);
    }
}
