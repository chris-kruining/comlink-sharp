using Microsoft.Web.WebView2.Wpf;

namespace Comlink
{
    public class MessagePort : Endpoint, ITransferable
    {
        public MessagePort(WebView2 window) : base(window)
        {
        }
    }
}