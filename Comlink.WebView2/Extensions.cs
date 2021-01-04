namespace Comlink.WebView2
{
    public static class Extensions
    {
        public static void Expose<T>(this Microsoft.Web.WebView2.Wpf.WebView2 window, T target) => Core.Comlink.Expose(target, ToEndpoint(window));
        public static Core.IEndpoint ToEndpoint(this Microsoft.Web.WebView2.Wpf.WebView2 window) => Core.Comlink.WindowEndpoint(new WebView2Window(window));
    }
}