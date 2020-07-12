using LembreteDividas.Models;
using Plugin.LocalNotifications;
using SQLite;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace LembreteDividas
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class PaidBillsView : ContentPage
    {
        private static bool _firstRun = true;
        private List<PaidBill> _paidBills;
        private static ObservableCollection<PaidBill> _paidBillsObs;
        private SQLiteAsyncConnection _conn;
        private MyObservableItem _tituloValor = new MyObservableItem();
        public PaidBillsView()
        {
            InitializeComponent();
            _conn = DependencyService.Get<ISQLiteDb>().GetConnection();
            _tituloValor.TituloValor = "Valor total de contas pagas: R$0,00";
            this.BindingContext = _tituloValor;
        }
        protected override async void OnAppearing()
        {
            _firstRun = false;
            _paidBills = await _conn.Table<PaidBill>().ToListAsync();
            var _paidBillsOrd = _paidBills.OrderBy(x => x.DataVencimento).ToList();
            _paidBillsObs = new ObservableCollection<PaidBill>(_paidBillsOrd);
            listView.ItemsSource = _paidBillsObs;
            CalculateSum(true);
            
            base.OnAppearing();
        }

        private void CalculateSum(bool isFirst=false)
        {
            if (_paidBillsObs != null)
            {
                double valorTotal = 0;
                foreach (var item in _paidBillsObs)
                {
                    valorTotal += item.Valor;
                }
                _tituloValor.TituloValor = $"Valor total de contas pagas: R${String.Format("{0:F2}", valorTotal)}";
                
            }
            else
                _tituloValor.TituloValor = $"Valor total de contas pagas: R$0,00";
        }

        private async void Apagar_Clicked(object sender, EventArgs e)
        {
            var bill = (sender as Button).CommandParameter as PaidBill;
            bool decision = await DisplayAlert($"Apagar conta de {String.Format("R${0:F2}", bill.Valor)}", "Tem certeza que " +
                "deseja apagar esta conta?", "Sim", "Não");
            if (decision)
            {
                _paidBillsObs.Remove(bill);
                var tbills = _paidBillsObs.OrderBy(x => x.DataVencimento).ToList();
                _paidBillsObs.Clear();
                foreach (var item in tbills)
                {
                    _paidBillsObs.Add(item);
                }
                CalculateSum();
                await _conn.DeleteAsync(bill);
            }
        }

        private async void ApagarTodas_Clicked(object sender, EventArgs e)
        {
            bool decision = await DisplayAlert("Apagar todas as contas pagas", "Tem certeza que " +
                "deseja apagar todas as contas pagas?", "Sim", "Não");
            if (decision)
            {
                foreach (var item in _paidBillsObs)
                {
                    await _conn.DeleteAsync(item);
                }
                _paidBillsObs.Clear();
                _tituloValor.TituloValor = $"Valor total de contas pagas: R$0,00";
            }
        }
        public static void InsertBill(PaidBill conta)
        {
            if (!_firstRun) 
            { 
                _paidBillsObs.Add(conta);
                var tbills = _paidBillsObs.OrderBy(x => x.DataVencimento).ToList();
                _paidBillsObs.Clear();
                foreach (var item in tbills)
                {
                    _paidBillsObs.Add(item);
                }
            }
        }

        private async void NotPaid_Clicked(object sender, EventArgs e)
        {
            var bill = (sender as MenuItem).CommandParameter as PaidBill;
            await _conn.DeleteAsync(bill);
            _paidBillsObs.Remove(bill);
            var tbills = _paidBillsObs.OrderBy(x => x.DataVencimento).ToList();
            _paidBillsObs.Clear();
            foreach (var item in tbills)
            {
                _paidBillsObs.Add(item);
            }
            Bill novaConta = new Bill
            {
                Id = -1,
                Titulo = bill.Titulo,
                Descricao = bill.Descricao,
                DataVencimento = bill.DataVencimento,
                Valor = bill.Valor,
                IsMensal = false
            };
            await _conn.InsertAsync(novaConta);
            var temp1 = await _conn.Table<Bill>().OrderByDescending(x => x.Id).FirstAsync();
            novaConta.Id = temp1.Id;

            if (novaConta.DataVencimento.Subtract(DateTime.Now).Days >= 0)
            {
                CrossLocalNotifications.Current.Show($"A conta {novaConta.Titulo} vence hoje",
                        $"Valor: R${String.Format("{0:F2}", novaConta.Valor)} - Clique aqui para abrir o App",
                        novaConta.Id, novaConta.DataVencimento.AddHours(12));
                Dividas.InsertBill(novaConta);
            }
            else 
            {
                CrossLocalNotifications.Current.Show("Você tem conta(s) atrasada(s)!", "Abra o App clicando aqui para ver mais detalhes", 999999999, DateTime.Today.AddDays(1).AddHours(12));
                OverdueBills.InsertBill(novaConta);
            }
        }
    }
}