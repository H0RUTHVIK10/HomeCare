namespace HomeCare.Services
{
    public interface IQrCodeService
    {
        string GenerateBase64QrCode(string text);
        byte[] GenerateQrCodePngBytes(string text);
    }
}
