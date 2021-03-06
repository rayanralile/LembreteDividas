﻿using LembreteDividas.Interfaces;
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
        private ILocalNotification _localNotify = MyOwnDI.ILocalNotificationDI();
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
                    CrossLocalNotifications.Current.Cancel(0);
            }
            else 
            { 
                _tituloValor.TituloValor = $"Total a pagar: R$0,00";
                CrossLocalNotifications.Current.Cancel(0);
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
            double value = 0;
            var bill = (sender as MenuItem).CommandParameter as Bill;
            // Etapa 0: se a conta for recorrente, pegar novo valor.
            // Nessa etapa , o usuário pode cancelar a inserção
            // Abaixo trataremos sobre esse cenário
            if (bill.IsMensal)
            {
                string dados = await DisplayPromptAsync("Conta Recorrente", "Digite o novo valor da conta para o próximo mês:",
                    "Ok", "Cancelar", "Ex: 120", -1, Keyboard.Numeric, bill.Valor.ToString());
                if (String.IsNullOrWhiteSpace(dados))
                    return;
                try
                {
                    value = Convert.ToDouble(dados);
                }
                catch (FormatException)
                {
                    return;
                }
                catch (OverflowException)
                {
                    return;
                }
                if (value == 0)
                    return;
            }
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
                    Valor = value
                };
                await _conn.InsertAsync(novaDivida);
                var temp2 = await _conn.Table<Bill>().OrderByDescending(x => x.Id).FirstAsync();
                novaDivida.Id = temp2.Id;
                // Se a pessoa pagou essa dívida com mais de 1 mês de atraso, sua próxima boleta já está atrasada
                // A linha abaixo irá ver isso e direcionar pra view certa
                if (novaDivida.DataVencimento.Subtract(DateTime.Now).Days < 0) 
                { 
                    OverdueBills.InsertBill(novaDivida);
                    CrossLocalNotifications.Current.Show("Você tem conta(s) atrasada(s)!", "Abra o App clicando aqui para ver mais detalhes", 0, DateTime.Today.AddDays(1).AddHours(12));
                }
                else
                {
                    Dividas.InsertBill(novaDivida);
                    if (DateTime.Now.Subtract(novaDivida.DataVencimento.AddHours(12)).Hours < 0)
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
        private async void ApagarTodos_Clicked(object sender, EventArgs e)
        {
            if (await DisplayAlert("Apagar todas as contas", "Tem certeza que deseja apagar todas as contas?", "Sim", "Cancelar"))
            {
                foreach (var item in _bills)
                {
                    _localNotify.Delete(item.Id);
                    await _conn.DeleteAsync(item);
                }
                _bills.Clear();
                CalculateSum();
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