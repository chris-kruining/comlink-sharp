using System;
using CefSharp;
using Comlink.Core;

namespace Comlink.CefSharp
{
    public class CefSharpWindow : IWindow
    {
        private readonly IWebBrowser _window;
        public CefSharpWindow(IWebBrowser window)
        {
            _window = window;
            _window.JavascriptMessageReceived += (s, a) => Message?.Invoke(a.ConvertMessageTo<String>());
        }

        public void Start()
        {
        }

        public event Action<String> Message;
        public void PostMessage(String message, params ITransferable[]? transferables)
        {
            // I bet this doesn't work, but it should do the gist
            _window.ExecuteScriptAsync($"window.parent.dispatchEvent(new MessageEvent('message', {{ data: '{message}' }}))");
        }
    }
}