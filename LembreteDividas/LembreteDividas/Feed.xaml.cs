using Plugin.LocalNotifications;
using SQLite;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace LembreteDividas
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class Feed : ContentPage
    {
        private SQLiteAsyncConnection _conn;
        public Feed()
        {
            InitializeComponent();
            _conn = DependencyService.Get<ISQLiteDb>().GetConnection();
        }
        protected override async void OnAppearing()
        {
            CrossLocalNotifications.Current.Cancel(999999999);
            for (int i = 0; i < 1000; i++)
            {
                CrossLocalNotifications.Current.Cancel(i);
            }
            await DisplayAlert("Notificações apagadas", "Todas as notificações foram apagadas!", "Ok");
            /*
            var bills = await _conn.Table<Bill>().ToListAsync();
            var billsPagas = await _conn.Table<PaidBill>().ToListAsync();

            foreach (var item in bills)
            {
                await _conn.DeleteAsync(item);
            }
            foreach (var item in billsPagas)
            {
                await _conn.DeleteAsync(item);
            }
            await DisplayAlert("Banco apagado", "Tudo zerado!", "Ok");
            */

            /*
            await _conn.CreateTableAsync<Bill>();
            await _conn.CreateTableAsync<PaidBill>();
            var listBills = MockBills.GetAllBills();
            foreach (var item in listBills)
            {
                await _conn.InsertAsync(item);
            }

            var listPaidBills = MockBills.GetAllPaidBills();
            foreach (var item in listPaidBills)
            {
                await _conn.InsertAsync(item);
            }
            */
            /*  var listBills = await _conn.Table<Bill>().ToListAsync();
              genericBill.ItemsSource = listBills;
              var listPaidBills = await _conn.Table<PaidBill>().ToListAsync();
              genericPaidBill.ItemsSource = listPaidBills; 
            */
            base.OnAppearing();
        }
    }
}