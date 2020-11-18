using System;
using Comlink.Core;

namespace Comlink.WebView2
{
    public class WebView2Window : IWindow
    {
        private readonly Microsoft.Web.WebView2.Wpf.WebView2 _window;
        public WebView2Window(Microsoft.Web.WebView2.Wpf.WebView2 window)
        {
            _window = window;
            _window.WebMessageReceived += (s, a) => Message?.Invoke(a.WebMessageAsJson);
        }

        public void Start()
        {

        }

        public event Action<String>? Message;
        public void PostMessage(String message, params ITransferable[]? transferables)
        {
            _window.CoreWebView2.PostWebMessageAsJson(message);
        }
    }
}