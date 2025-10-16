using FluentValidation;
using Volun.Web.Dtos;

namespace Volun.Web.Validators;

public class CreateAccionRequestValidator : AbstractValidator<CreateAccionRequest>
{
    public CreateAccionRequestValidator()
    {
        RuleFor(x => x.Titulo).NotEmpty().MaximumLength(256);
        RuleFor(x => x.Descripcion).NotEmpty();
        RuleFor(x => x.Organizador).NotEmpty().MaximumLength(256);
        RuleFor(x => x.Categoria).NotEmpty().MaximumLength(128);
        RuleFor(x => x.FechaFin).GreaterThan(x => x.FechaInicio);
        RuleFor(x => x.CupoMaximo).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Latitud).InclusiveBetween(-90, 90).When(x => x.Latitud.HasValue);
        RuleFor(x => x.Longitud).InclusiveBetween(-180, 180).When(x => x.Longitud.HasValue);
    }
}

public class UpdateAccionRequestValidator : AbstractValidator<UpdateAccionRequest>
{
    public UpdateAccionRequestValidator()
    {
        RuleFor(x => x.Titulo).NotEmpty().MaximumLength(256);
        RuleFor(x => x.Descripcion).NotEmpty();
        RuleFor(x => x.Categoria).NotEmpty().MaximumLength(128);
        RuleFor(x => x.FechaFin).GreaterThan(x => x.FechaInicio);
        RuleFor(x => x.CupoMaximo).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Latitud).InclusiveBetween(-90, 90).When(x => x.Latitud.HasValue);
        RuleFor(x => x.Longitud).InclusiveBetween(-180, 180).When(x => x.Longitud.HasValue);
    }
}

public class CreateTurnoRequestValidator : AbstractValidator<CreateTurnoRequest>
{
    public CreateTurnoRequestValidator()
    {
        RuleFor(x => x.Titulo).NotEmpty().MaximumLength(256);
        RuleFor(x => x.FechaFin).GreaterThan(x => x.FechaInicio);
        RuleFor(x => x.Cupo).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Notas).MaximumLength(512);
    }
}
