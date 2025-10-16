using FluentValidation;
using Volun.Web.Dtos;

namespace Volun.Web.Validators;

public class CreateInscripcionRequestValidator : AbstractValidator<CreateInscripcionRequest>
{
    public CreateInscripcionRequestValidator()
    {
        RuleFor(x => x.VoluntarioId).NotEmpty();
        RuleFor(x => x.AccionId).NotEmpty();
        RuleFor(x => x.Notas).MaximumLength(1024);
    }
}

public class UpdateEstadoInscripcionRequestValidator : AbstractValidator<UpdateEstadoInscripcionRequest>
{
    public UpdateEstadoInscripcionRequestValidator()
    {
        RuleFor(x => x.Estado).IsInEnum();
        RuleFor(x => x.Comentarios).MaximumLength(1024);
    }
}
