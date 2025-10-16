using Volun.Core.Entities;

namespace Volun.Core.Services;

public interface ICertificateService
{
    Task<byte[]> GenerateCertificatePdfAsync(Certificado certificado, CancellationToken cancellationToken = default);
}
