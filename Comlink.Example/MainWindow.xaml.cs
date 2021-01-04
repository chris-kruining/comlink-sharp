using System.Windows;
using Comlink.Core;
using Comlink.WebView2;

// NOTE(Chris Kruining)
// - You wouldn't have to refer to `Comlink` as `Core.Comlink`, this is just a side effect of C# namespacing

namespace Comlink.Example
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
            WebView2.Expose(kaas);

            Proxy<Kaas> proxied = Core.Comlink.Proxy(kaas);
            dynamic remote = Core.Comlink.Wrap<ClientSchema>(WebView2.ToEndpoint());

            remote.SomeMethod("a string value");
        }
    }

    public class Kaas
    {

    }

    public class ClientSchema
    {

    }
}
