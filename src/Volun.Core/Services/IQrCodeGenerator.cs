namespace Volun.Core.Services;

public interface IQrCodeGenerator
{
    byte[] GenerateQr(string payload);
}
