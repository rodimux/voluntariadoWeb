using QRCoder;
using Volun.Core.Services;

namespace Volun.Web.Services;

public class QrCodeGeneratorService : IQrCodeGenerator
{
    public byte[] GenerateQr(string payload)
    {
        using var generator = new QRCodeGenerator();
        using var data = generator.CreateQrCode(payload, QRCodeGenerator.ECCLevel.Q);
        var pngQRCode = new PngByteQRCode(data);
        return pngQRCode.GetGraphic(20);
    }
}
