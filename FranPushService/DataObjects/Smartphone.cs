using Microsoft.WindowsAzure.Mobile.Service;

namespace FranPushService.DataObjects
{
    public class Smartphone : EntityData
    {
        public string Modelo { get; set; }
        public string Fabricante { get; set; }
        public decimal Precio { get; set; }
    }
}