using FluentValidation;
using Volun.Web.Dtos;

namespace Volun.Web.Validators;

public class CheckInRequestValidator : AbstractValidator<CheckInRequest>
{
    public CheckInRequestValidator()
    {
        RuleFor(x => x.InscripcionId).NotEmpty();
    }
}

public class CheckOutRequestValidator : AbstractValidator<CheckOutRequest>
{
    public CheckOutRequestValidator()
    {
        RuleFor(x => x.InscripcionId).NotEmpty();
        RuleFor(x => x.CheckOut).NotEmpty();
    }
}
