using LembreteDividas.Models;
using Plugin.LocalNotifications;
using SQLite;
using System;
using System.Collections.ObjectModel;
using System.Linq;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace LembreteDividas
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class OverdueBills : ContentPage
    {
        private static bool _firstRun = true;
        private static ObservableCollection<Bill> _bills;
        private SQLiteAsyncConnection _conn;
        private MyObservableItem _tituloValor = new MyObservableItem();
        public OverdueBills()
        {
            InitializeComponent();
            _conn = DependencyService.Get<ISQLiteDb>().GetConnection();
            _tituloValor.TituloValor = "Total a pagar: R$0,00";
            this.BindingContext = _tituloValor;
        }
        protected override async void OnAppearing()
        {
            _firstRun = false;
            var listaDividas = await _conn.Table<Bill>().ToListAsync();
            var listaDividasPronta = listaDividas.Where(x => x.DataVencimento.Subtract(DateTime.Now).Days < 0);
            listaDividasPronta = listaDividasPronta.OrderBy(x => x.DataVencimento);
            _bills = new ObservableCollection<Bill>(listaDividasPronta);
            listView.ItemsSource = _bills;
            CalculateSum();
            base.OnAppearing();
        }

        private void CalculateSum()
        {
            if (_bills != null)
            {
                double valorTotal = 0;
                foreach (var item in _bills)
                {
                    valorTotal += item.Valor;
                }
                _tituloValor.TituloValor = $"Total a pagar: R${String.Format("{0:F2}", valorTotal)}";
                if(valorTotal == 0)
                    CrossLocalNotifications.Current.Cancel(999999999);
            }
            else 
            { 
                _tituloValor.TituloValor = $"Total a pagar: R$0,00";
                CrossLocalNotifications.Current.Cancel(999999999);
            }
        }

        private void Button_Clicked(object sender, EventArgs e)
        {
            Navigation.PushAsync(new EditDivida());
        }

        private void listView_ItemSelected(object sender, SelectedItemChangedEventArgs e)
        {
            if (e.SelectedItem == null)
                return;
            var bill = e.SelectedItem as Bill;
            listView.SelectedItem = null;
            Navigation.PushAsync(new EditDivida(bill, bill.Id));
        }

        private async void Paga_Clicked(object sender, EventArgs e)
        {
            var bill = (sender as MenuItem).CommandParameter as Bill;
            // Etapa 1: Remover notificação e remover do BD de dívidas e da view
            CrossLocalNotifications.Current.Cancel(bill.Id);
            _bills.Remove(bill);
            var tbills = _bills.OrderBy(x => x.DataVencimento).ToList();
            _bills.Clear();
            foreach (var item in tbills)
            {
                _bills.Add(item);
            }
            await _conn.DeleteAsync(bill);
            // Etapa 2: Criar o objeto divida paga e inserir na view das pagas
            PaidBill billPaid = new PaidBill
            {
                Id = -1,
                Titulo = bill.Titulo,
                DataVencimento = bill.DataVencimento,
                Descricao = bill.Descricao,
                Valor = bill.Valor
            };
            await _conn.InsertAsync(billPaid);
            var temp1 = await _conn.Table<PaidBill>().OrderByDescending(x => x.Id).FirstAsync();
            billPaid.Id = temp1.Id;
            PaidBillsView.InsertBill(billPaid);
            // Etapa 3 - se a dívida é mensal, criar nova para daqui a um mês.
            if(bill.IsMensal)
            {
                Bill novaDivida = new Bill
                {
                    Titulo = bill.Titulo,
                    Descricao = bill.Descricao,
                    DataVencimento = bill.DataVencimento.AddMonths(1),
                    IsMensal = true,
                    Valor = bill.Valor
                };
                await _conn.InsertAsync(novaDivida);
                var temp2 = await _conn.Table<Bill>().OrderByDescending(x => x.Id).FirstAsync();
                novaDivida.Id = temp2.Id;
                // Se a pessoa pagou essa dívida com mais de 1 mês de atraso, sua próxima boleta já está atrasada
                // A linha abaixo irá ver isso e direcionar pra view certa
                if (novaDivida.DataVencimento.Subtract(DateTime.Now).Days < 0)
                    OverdueBills.InsertBill(novaDivida);
                else
                {
                    Dividas.InsertBill(novaDivida);
                    
                    CrossLocalNotifications.Current.Show($"A conta {novaDivida.Titulo} vence hoje", 
                        $"Valor: R${String.Format("{0:F2}", novaDivida.Valor)} - Clique aqui para abrir o App",
                        novaDivida.Id,novaDivida.DataVencimento.AddHours(12));
                }
                await DisplayAlert("Conta recorrente!", $"A conta {novaDivida.Titulo} foi criada com vencimento em {novaDivida.DataMensal}","Ok");
            }
            CalculateSum();
        }

        private async void Apagar_Clicked(object sender, EventArgs e)
        {
            var bill = (sender as MenuItem).CommandParameter as Bill;
            bool decision = await DisplayAlert($"Apagar conta de {String.Format("R${0:F2}", bill.Valor)}", "Tem certeza que " +
                "deseja apagar esta conta?", "Sim", "Não");
            if (decision)
            {
                _bills.Remove(bill);
                var tbills = _bills.OrderBy(x => x.DataVencimento).ToList();
                _bills.Clear();
                foreach (var item in tbills)
                {
                    _bills.Add(item);
                }
                CalculateSum();
                CrossLocalNotifications.Current.Cancel(bill.Id);
                await _conn.DeleteAsync(bill);
            }
        }
        public static void InsertBill(Bill conta)
        {
            if (!_firstRun)
            {
                _bills.Add(conta);
                var tbills = _bills.OrderBy(x => x.DataVencimento).ToList();
                _bills.Clear();
                foreach (var item in tbills)
                {
                    _bills.Add(item);
                }
            }
        }
        public static void DeleteBill(Bill conta)
        {
            if (!_firstRun)
            {
                _bills.Remove(conta);
                var tbills = _bills.OrderBy(x => x.DataVencimento).ToList();
                _bills.Clear();
                foreach (var item in tbills)
                {
                    _bills.Add(item);
                }
            }
        }
        public static void UpdateBill(Bill conta)
        {
            if (!_firstRun) 
            { 
                var oldBill = _bills.FirstOrDefault(x => x.Id == conta.Id);
                int idObs = _bills.IndexOf(oldBill);
                _bills[idObs] = conta;
                var tbills = _bills.OrderBy(x => x.DataVencimento).ToList();
                _bills.Clear();
                foreach (var item in tbills)
                {
                    _bills.Add(item);
                }
            }
        }
    }
}