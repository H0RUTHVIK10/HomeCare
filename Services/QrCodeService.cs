using QRCoder;

namespace HomeCare.Services
{
    public class QrCodeService : IQrCodeService
    {
        public byte[] GenerateQrCodePngBytes(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                text = "/";
            }

            using var qrGenerator = new QRCodeGenerator();
            using var qrCodeData = qrGenerator.CreateQrCode(text, QRCodeGenerator.ECCLevel.Q);
            var pngQrCode = new PngByteQRCode(qrCodeData);
            return pngQrCode.GetGraphic(10);
        }

        public string GenerateBase64QrCode(string text)
        {
            var bytes = GenerateQrCodePngBytes(text);
            return $"data:image/png;base64,{Convert.ToBase64String(bytes)}";
        }
    }
}
