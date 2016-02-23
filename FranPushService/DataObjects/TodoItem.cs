using Microsoft.WindowsAzure.Mobile.Service;

namespace FranPushService.DataObjects
{
    public class TodoItem : EntityData
    {
        public string Text { get; set; }
        public bool Completed { get; set; }
    }
}