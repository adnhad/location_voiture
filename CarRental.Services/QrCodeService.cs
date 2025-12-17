using QRCoder;
using System.Drawing;
using System.IO;

namespace CarRental.Services
{
    public class QrCodeService
    {
        public byte[] GenerateQrCode(string data)
        {
            using (var qrGenerator = new QRCodeGenerator())
            {
                var qrCodeData = qrGenerator.CreateQrCode(data, QRCodeGenerator.ECCLevel.Q);
                var qrCode = new BitmapByteQRCode(qrCodeData);
                var qrCodeImage = qrCode.GetGraphic(20);
                return qrCodeImage;
            }
        }

        public Image GenerateQrCodeImage(string data)
        {
            var bytes = GenerateQrCode(data);
            using (var stream = new MemoryStream(bytes))
            {
                return Image.FromStream(stream);
            }
        }

        public string GenerateRentalQrCodeData(int rentalId, string clientName, string vehicleInfo)
        {
            return $"RENTAL:{rentalId}|CLIENT:{clientName}|VEHICLE:{vehicleInfo}|DATE:{DateTime.Now:yyyyMMdd}";
        }
    }
}