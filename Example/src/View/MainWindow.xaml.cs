using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using CefSharp;
using CefSharp.SchemeHandler;
using CefSharp.Wpf;
using Comlink.Core;
using Comlink.CefSharp;
using Comlink.WebView2;
using Microsoft.Web.WebView2.Core;
using static Comlink.Core.Comlink;

namespace Example.View
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            PreInitialize();
            InitializeComponent();
            Logic();
        }

        private void PreInitialize()
        {
            CefSettings settings = new();
            settings.RegisterScheme(new CefCustomScheme
            {
                SchemeName = "https",
                DomainName = "myapp",
                SchemeHandlerFactory = new FolderSchemeHandlerFactory(
                    rootFolder: ".",
                    hostName: "myapp",
                    defaultPage: "index.html"
                )
            });

            Cef.Initialize(settings);
        }

        private void Logic()
        {
            Kaas kaas = new();
            
            WebView2Browser.CoreWebView2InitializationCompleted += (e, v) =>
            {
                WebView2Browser.CoreWebView2.SetVirtualHostNameToFolderMapping(
                    "myapp", 
                    Directory.GetCurrentDirectory(), 
                    CoreWebView2HostResourceAccessKind.Allow
                );
            };

            WebView2Browser.ContentLoading += async (_, _) =>
            {
                await WebView2Browser.ExecuteScriptAsync("console.log('C# noticed me!')");
                await WebView2Browser.ExecuteScriptAsync(@"window.chrome.webview.addEventListener('message', e => console.log(e))");
                
                WebView2Browser.Expose(kaas);
                var endpoint = WebView2Browser.ToEndpoint();
                dynamic remote = Wrap<IClientSchema>(endpoint);
                // remote.SomeMethod("a string value");
                
                endpoint.PostMessage(new Message
                {
                    Id = "KAAS IS AWESOME",
                    Type = MessageType.Apply,
                    Path = new[]{ "someMethod" },
                    ArgumentList = new []{ new WireValue{ Type = WireValueType.Raw, Value = "some message" } }
                });
            };

            CefSharpBrowser.Loaded += (_, _) =>
            {
                // CefSharpBrowser.ShowDevTools();
                CefSharpBrowser.Expose(kaas);
                var endpoint = CefSharpBrowser.ToEndpoint();
                dynamic remote2 = Wrap<IClientSchema>(endpoint);
                // remote2.SomeMethod("a string value");
                
                
                endpoint.PostMessage(new Message
                {
                    Id = "KAAS IS AWESOME",
                    Type = MessageType.Apply,
                    Path = new[]{ "someMethod" },
                    ArgumentList = new []{ new WireValue{ Type = WireValueType.Raw, Value = "some message" } }
                });
            };
        }
    }

    public class Kaas
    {
        public void SomeCSharpMethod(String message)
        {
            Console.WriteLine(message);
        }
    }

    public interface IClientSchema
    {
        public void SomeMethod(String message);
    }
}
