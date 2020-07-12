using System;
using System.Collections.Generic;

namespace LembreteDividas.Models
{
    public static class MockBills
    {
        public static List<Bill> GetAllBills()
        {
            return new List<Bill>
            {
                new Bill{Id=1, Titulo="Conta de Luz", Descricao="Light daqui de casa", DataVencimento = Convert.ToDateTime("7/8/2020"), Valor=150, IsMensal=true},
                new Bill{Id=2, Titulo="Conta de Água", Descricao="CEDAE daqui de casa", DataVencimento = Convert.ToDateTime("7/9/2020"), Valor=50, IsMensal=true},
                new Bill{Id=3, Titulo="Taxa da Milícia", Descricao="Do tamanho de um cometa", DataVencimento = Convert.ToDateTime("7/20/2020"), Valor=850, IsMensal=true},
                new Bill{Id=4, Titulo="Conta de Cartão", Descricao="Do tamanho de um cometa", DataVencimento = Convert.ToDateTime("7/21/2020"), Valor=150, IsMensal=true},
                new Bill{Id=5, Titulo="Conta do Agiota", Descricao="Do tamanho de um cometa", DataVencimento = Convert.ToDateTime("7/22/2020"), Valor=250, IsMensal=false},
                new Bill{Id=6, Titulo="Taxa dos Bombeiros", Descricao="Do tamanho de um cometa", DataVencimento = Convert.ToDateTime("8/23/2020"), Valor=350, IsMensal=false}
            };
        }
        public static List<PaidBill> GetAllPaidBills()
        {
            return new List<PaidBill>
            {
                new PaidBill{Id=1, Titulo="Conta de Gato", Descricao="Light daqui de casa", DataVencimento = Convert.ToDateTime("7/8/2020"), Valor=150},
                new PaidBill{Id=2, Titulo="Conta de Maica", Descricao="CEDAE daqui de casa", DataVencimento = Convert.ToDateTime("7/9/2020"), Valor=50},
                new PaidBill{Id=3, Titulo="Taxa da CV", Descricao="Do tamanho de um cometa", DataVencimento = Convert.ToDateTime("7/20/2020"), Valor=850},
                new PaidBill{Id=4, Titulo="Conta de roxxinho", Descricao="Do tamanho de um cometa", DataVencimento = Convert.ToDateTime("7/21/2020"), Valor=150},
                new PaidBill{Id=5, Titulo="Conta do Vigarista", Descricao="Do tamanho de um cometa", DataVencimento = Convert.ToDateTime("7/22/2020"), Valor=250},
                new PaidBill{Id=6, Titulo="Taxa dos Assaltos", Descricao="Do tamanho de um cometa", DataVencimento = Convert.ToDateTime("8/23/2020"), Valor=350}
            };
        }
    }
}
