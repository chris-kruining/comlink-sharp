using System.Windows;
using Comlink.Core;
using Comlink.CefSharp;
using Comlink.WebView2;

// NOTE(Chris Kruining)
// - You wouldn't have to refer to `Comlink` as `Core.Comlink`, this is just a side effect of C# namespacing

namespace Comlink.Example.View
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Initialize()
        {
            Kaas kaas = new Kaas();
            Proxy<Kaas> proxied = Core.Comlink.Proxy(kaas);
            
            WebView2Browser.Expose(kaas);
            dynamic remote1 = Core.Comlink.Wrap<ClientSchema>(WebView2Browser.ToEndpoint());
            remote1.SomeMethod("a string value");
            
            CefSharpBrowser.Expose(kaas);
            dynamic remote2 = Core.Comlink.Wrap<ClientSchema>(CefSharpBrowser.ToEndpoint());
            remote2.SomeMethod("a string value");
        }
    }

    public class Kaas
    {

    }

    public class ClientSchema
    {

    }
}
