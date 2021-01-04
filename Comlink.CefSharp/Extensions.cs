using CefSharp;

namespace Comlink.CefSharp
{
    public static class Extensions
    {
        public static void Expose<T>(this IWebBrowser window, T target) => Core.Comlink.Expose(target, ToEndpoint(window));
        public static Core.IEndpoint ToEndpoint(this IWebBrowser window) => Core.Comlink.WindowEndpoint(new CefSharpWindow(window));
    }
}
