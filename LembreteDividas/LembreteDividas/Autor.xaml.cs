using System;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace LembreteDividas
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class Autor : ContentPage
    {
        public Autor()
        {
            InitializeComponent();
        }
        private void GitHub_Tapped(object sender, EventArgs e)
        {
            Browser.OpenAsync("https://github.com/rayanralile", BrowserLaunchMode.SystemPreferred);
        }

        private void License_Tapped(object sender, EventArgs e)
        {
            Browser.OpenAsync("https://creativecommons.org/licenses/by/4.0/deed.pt_BR", BrowserLaunchMode.SystemPreferred);
        }

        private void Icons8_Tapped(object sender, EventArgs e)
        {
            Browser.OpenAsync("https://icons8.com/", BrowserLaunchMode.SystemPreferred);
        }

        private void Email_Tapped(object sender, EventArgs e)
        {
            Browser.OpenAsync("mailto:rayan@ralile.com.br", BrowserLaunchMode.SystemPreferred);
        }
        /*
        private void Notificar_Tapped(object sender, EventArgs e)
        {
            CrossLocalNotifications.Current.Show("Agendado no passado", "Garrou", 101, DateTime.Now.AddSeconds(5));
        }

        private void SegundoClique_Tapped(object sender, EventArgs e)
        {
            CrossLocalNotifications.Current.Cancel(101);
        }
        */
    }
}