using SQLite;
using System;
using System.ComponentModel;

namespace LembreteDividas.Models
{
    public class PaidBill : INotifyPropertyChanged
    {
        private int _id;
        [PrimaryKey, AutoIncrement]
        public int Id
        {
            get { return _id; }
            set
            {
                if (_id == value)
                    return;
                _id = value;
                OnPropertyChanged(nameof(Id));
            }
        }
        

        private string _titulo;
        [MaxLength(100)]
        public string Titulo
        {
            get
            { return _titulo; }
            set
            {
                if (_titulo == value)
                    return;
                _titulo = value;
                OnPropertyChanged(nameof(Titulo));
            }
        }

        private string _descricao;
        [MaxLength(255)]
        public string Descricao
        {
            get { return _descricao; }
            set
            {
                if (_descricao == value)
                    return;
                _descricao = value;
                OnPropertyChanged(nameof(Descricao));
            }
        }
        private double _valor;
        public double Valor
        {
            get { return _valor; }
            set
            {
                if (_valor == value)
                    return;
                _valor = value;
                OnPropertyChanged(nameof(Valor));
            }
        }
        private DateTime _dataVencimento;
        public DateTime DataVencimento
        {
            get { return _dataVencimento; }
            set
            {
                if (_dataVencimento == value)
                    return;
                _dataVencimento = value;
                OnPropertyChanged(nameof(DataVencimento));
            }
        }
        public string DataMensal { get { return String.Format("{0:D2}/{1:D2}", DataVencimento.Day, DataVencimento.Month); } }
        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
