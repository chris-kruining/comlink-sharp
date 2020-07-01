using System;
using System.Text.Json;
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.Wpf;

namespace Comlink
{
    public class Endpoint
    {
        private readonly WebView2 _window;

        public Endpoint(WebView2 window)
        {
            _window = window;
            _window.WebMessageReceived += (s, a) => Message?.Invoke(s, a);
        }

        public void PostMessage(dynamic message, params ITransferable[] transferables)
        {
            _window.CoreWebView2.PostWebMessageAsJson(JsonSerializer.Serialize(message, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            }));
        }

        public event EventHandler<CoreWebView2WebMessageReceivedEventArgs> Message;
    }
}