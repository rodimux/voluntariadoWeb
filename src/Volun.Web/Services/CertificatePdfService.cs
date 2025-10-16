using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using Volun.Core.Entities;
using Volun.Core.Services;

namespace Volun.Web.Services;

public class CertificatePdfService : ICertificateService
{
    public Task<byte[]> GenerateCertificatePdfAsync(Certificado certificado, CancellationToken cancellationToken = default)
    {
        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Margin(40);
                page.Header()
                    .AlignCenter()
                    .Text("Certificado de Voluntariado")
                    .FontSize(24)
                    .SemiBold();

                page.Content().Column(column =>
                {
                    column.Spacing(16);

                    column.Item().Text(text =>
                    {
                        text.Span("Código de verificación: ").SemiBold();
                        text.Span(certificado.CodigoVerificacion);
                    });

                    column.Item().Text(text =>
                    {
                        text.Span("Horas acreditadas: ").SemiBold();
                        text.Span($"{certificado.Horas:F2}");
                    });

                    column.Item().Text(text =>
                    {
                        text.Span("Emitido en: ").SemiBold();
                        text.Span(certificado.EmitidoEn.ToString("dd/MM/yyyy"));
                    });

                    if (certificado.Voluntario is not null)
                    {
                        column.Item().Text(text =>
                        {
                            text.Span("Voluntario/a: ").SemiBold();
                            text.Span($"{certificado.Voluntario.Nombre} {certificado.Voluntario.Apellidos}");
                        });
                    }

                    if (certificado.Accion is not null)
                    {
                        column.Item().Text(text =>
                        {
                            text.Span("Acción: ").SemiBold();
                            text.Span(certificado.Accion.Titulo);
                        });
                    }

                    column.Item().Text("Agradecemos su compromiso y dedicación.").Italic();
                });

                page.Footer()
                    .AlignCenter()
                    .Text("Emitido por la plataforma Volun")
                    .FontSize(10)
                    .Light();
            });
        });

        var bytes = document.GeneratePdf();
        return Task.FromResult(bytes);
    }
}
