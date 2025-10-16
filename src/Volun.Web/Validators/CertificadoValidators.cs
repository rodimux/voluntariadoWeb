using FluentValidation;
using Volun.Web.Dtos;

namespace Volun.Web.Validators;

public class CreateCertificadoRequestValidator : AbstractValidator<CreateCertificadoRequest>
{
    public CreateCertificadoRequestValidator()
    {
        RuleFor(x => x.VoluntarioId).NotEmpty();
        RuleFor(x => x.AccionId).NotEmpty();
        RuleFor(x => x.Horas).GreaterThan(0);
    }
}
