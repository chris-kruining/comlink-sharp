using System;

namespace Comlink.CefSharp
{
    public class CefSharpWindow : IWindow
    {
        private readonly Object _window;
        public CefSharpWindow(Object window)
        {
            _window = window;
            _window.WebMessageReceived += (s, a) => Message?.Invoke(a.WebMessageAsJson);
        }

        public event Action<String> Message;
        public void PostMessage(String message)
        {
            throw new NotImplementedException();
        }
    }
}