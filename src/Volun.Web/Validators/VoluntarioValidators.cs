using FluentValidation;
using Volun.Web.Dtos;

namespace Volun.Web.Validators;

public class CreateVoluntarioRequestValidator : AbstractValidator<CreateVoluntarioRequest>
{
    public CreateVoluntarioRequestValidator()
    {
        RuleFor(x => x.Nombre).NotEmpty().MaximumLength(128);
        RuleFor(x => x.Apellidos).NotEmpty().MaximumLength(256);
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(256);
        RuleFor(x => x.FechaNacimiento).LessThan(DateTimeOffset.UtcNow.AddYears(-10));
        RuleFor(x => x.DniNie).MaximumLength(32);
        RuleFor(x => x.Preferencias)
            .Must(list => list == null || list.All(p => p.Length <= 64))
            .WithMessage("Cada preferencia debe tener menos de 64 caracteres.");
        RuleFor(x => x.Habilidades)
            .Must(list => list == null || list.All(p => p.Length <= 64))
            .WithMessage("Cada habilidad debe tener menos de 64 caracteres.");
    }
}

public class UpdateVoluntarioRequestValidator : AbstractValidator<UpdateVoluntarioRequest>
{
    public UpdateVoluntarioRequestValidator()
    {
        RuleFor(x => x.Nombre).NotEmpty().MaximumLength(128);
        RuleFor(x => x.Apellidos).NotEmpty().MaximumLength(256);
        RuleFor(x => x.Preferencias)
            .Must(list => list == null || list.All(p => p.Length <= 64))
            .WithMessage("Cada preferencia debe tener menos de 64 caracteres.");
        RuleFor(x => x.Habilidades)
            .Must(list => list == null || list.All(p => p.Length <= 64))
            .WithMessage("Cada habilidad debe tener menos de 64 caracteres.");
    }
}
