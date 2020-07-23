using LembreteDividas.Models;
using Plugin.LocalNotifications;
using SQLite;
using System;
using System.ComponentModel;
using System.Linq;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace LembreteDividas
{
    // Learn more about making custom code visible in the Xamarin.Forms previewer
    // by visiting https://aka.ms/xamarinforms-previewer
    [DesignTimeVisible(false)]
    public partial class MainPage : MasterDetailPage
    {
        private SQLiteAsyncConnection _conn;
        public MainPage()
        {
            InitializeComponent();
            _conn = DependencyService.Get<ISQLiteDb>().GetConnection();
            
        }
        protected override async void OnAppearing()
        {
            await _conn.CreateTableAsync<Bill>();
            await _conn.CreateTableAsync<PaidBill>();
            var contas = await _conn.Table<Bill>().ToListAsync();
            if (contas.FirstOrDefault(x => x.DataVencimento.Subtract(DateTime.Now).Days < 0) != null) 
            {
                CrossLocalNotifications.Current.Show("Você tem conta(s) atrasada(s)!", "Abra o App clicando aqui para ver mais detalhes", 0, DateTime.Today.AddDays(1).AddHours(12));
                Detail = new NavigationPage(new OverdueBills());
            }
            else 
            {
                CrossLocalNotifications.Current.Cancel(0);
                foreach (var item in contas)
                {
                    if (DateTime.Now.Subtract(item.DataVencimento.AddHours(12)).Hours < 0)
                        CrossLocalNotifications.Current.Show($"A conta {item.Titulo} vence hoje",
                        $"Valor: R${String.Format("{0:F2}", item.Valor)} - Clique aqui para abrir o App",
                        item.Id, item.DataVencimento.AddHours(12));
                }
                Detail = new NavigationPage(new Dividas());
            }
            base.OnAppearing();
        }

        private void DividasPagar_Tapped(object sender, EventArgs e)
        {
            //   (this.Detail as NavigationPage).PushAsync(new Dividas());
            Detail = new NavigationPage(new Dividas());
            IsPresented = false;
        }

        private void DividasJaPagas_Tapped(object sender, EventArgs e)
        {
            Detail = new NavigationPage(new PaidBillsView());
            IsPresented = false;
        }

        private void OverdueBills_Tapped(object sender, EventArgs e)
        {
            Detail = new NavigationPage(new OverdueBills());
            IsPresented = false;
        }

        private void Feedback_Tapped(object sender, EventArgs e)
        {
            var address = "rayan@ralile.com.br";
            Launcher.OpenAsync(new Uri($"mailto:{address}"));
        }

        private void Autor_Tapped(object sender, EventArgs e)
        {
            Detail = new NavigationPage(new Autor());
            IsPresented = false;
        }
    }
}
