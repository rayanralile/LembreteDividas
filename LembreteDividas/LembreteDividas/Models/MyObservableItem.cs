﻿using System.ComponentModel;

namespace LembreteDividas.Models
{
    public class MyObservableItem : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private string _tituloValor;
        public string TituloValor { get { return _tituloValor;}
            set 
            {
                if (_tituloValor == value)
                    return;
                _tituloValor = value;
                OnPropertyChanged(nameof(TituloValor));
            } }
        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
