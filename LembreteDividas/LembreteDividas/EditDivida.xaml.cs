using LembreteDividas.Models;
using Plugin.LocalNotifications;
using SQLite;
using System;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace LembreteDividas
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class EditDivida : ContentPage
    {
        private int _id;
        private SQLiteAsyncConnection _conn;
        private Bill _originalBill;
        public EditDivida(Bill bill = null, int id = -1)
        {
            InitializeComponent();
            _conn = DependencyService.Get<ISQLiteDb>().GetConnection();
            _id = id;
            _originalBill = bill;
            if (id > -1)
            {
                lbTitulo.Text = bill.Titulo;
                taObservacao.Text = bill.Descricao;
                lbValor.Text = bill.Valor.ToString();
                dfData.Date = bill.DataVencimento;
                swIsMensal.On = bill.IsMensal;
                this.Title = bill.Titulo;
            }
            else
                this.Title = "Nova Conta";
        }

        private async void Salvar_Clicked(object sender, EventArgs e)
        {
            if (String.IsNullOrWhiteSpace(lbTitulo.Text) || String.IsNullOrWhiteSpace(lbValor.Text))
            {
                await DisplayAlert("Preencha os dados", "É necessário preencher o título e o valor da conta para salvar", "Ok");
                return;
            }
            if(dfData.Date.Subtract(DateTime.Now).Days < 0)
            {
                Bill bill = new Bill { Id = _id, Titulo = lbTitulo.Text, DataVencimento = dfData.Date, Descricao = taObservacao.Text, IsMensal = swIsMensal.On, Valor = Math.Round(double.Parse(lbValor.Text),2) };
                if(_id == -1)
                {
                    await _conn.InsertAsync(bill);
                    var temp1 = await _conn.Table<Bill>().OrderByDescending(x => x.Id).FirstAsync();
                    bill.Id = temp1.Id;
                    OverdueBills.InsertBill(bill);
                    CrossLocalNotifications.Current.Show("Você tem conta(s) atrasada(s)!", "Abra o App clicando aqui para ver mais detalhes", 0, DateTime.Today.AddDays(1).AddHours(12));
                }
                else
                {
                   // bill.Id = _id;
                    if(_originalBill.DataVencimento.Subtract(DateTime.Now).Days < 0)
                        OverdueBills.UpdateBill(bill);
                    else
                    {
                        Dividas.DeleteBill(bill);
                        OverdueBills.InsertBill(bill);
                        CrossLocalNotifications.Current.Cancel(_id);
                    }
                    CrossLocalNotifications.Current.Show("Você tem conta(s) atrasada(s)!", "Abra o App clicando aqui para ver mais detalhes", 0, DateTime.Today.AddDays(1).AddHours(12));
                    await _conn.UpdateAsync(bill);
                }
            }
            else
            {
                Bill bill = new Bill { Id = _id, Titulo = lbTitulo.Text, DataVencimento = dfData.Date, Descricao = taObservacao.Text, IsMensal = swIsMensal.On, Valor = Math.Round(double.Parse(lbValor.Text), 2) };
                if (_id == -1)
                {
                    await _conn.InsertAsync(bill);
                    var temp2 = await _conn.Table<Bill>().OrderByDescending(x => x.Id).FirstAsync();
                    bill.Id = temp2.Id;
                    Dividas.InsertBill(bill);
                    if (DateTime.Now.Subtract(bill.DataVencimento.AddHours(12)).Hours < 0)
                        CrossLocalNotifications.Current.Show($"A conta {bill.Titulo} vence hoje",
                        $"Valor: R${String.Format("{0:F2}", bill.Valor)} - Clique aqui para abrir o App",
                        bill.Id, bill.DataVencimento.AddHours(12));
                }
                else
                {
                //    bill.Id = _id;
                    if (_originalBill.DataVencimento.Subtract(DateTime.Now).Days >= 0)
                        Dividas.UpdateBill(bill);
                    else
                    {
                        OverdueBills.DeleteBill(bill);
                        Dividas.InsertBill(bill);
                    }
                    if (DateTime.Now.Subtract(bill.DataVencimento.AddHours(12)).Hours < 0)
                        CrossLocalNotifications.Current.Show($"A conta {bill.Titulo} vence hoje",
                        $"Valor: R${String.Format("{0:F2}", bill.Valor)} - Clique aqui para abrir o App",
                        bill.Id, bill.DataVencimento.AddHours(12));

                    await _conn.UpdateAsync(bill);
                }
            }
            await Navigation.PopAsync();
        }

        private void lbTitulo_TextChanged(object sender, TextChangedEventArgs e)
        {
            this.Title = lbTitulo.Text;
        }
    }
}