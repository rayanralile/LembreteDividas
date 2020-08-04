using LembreteDividas.Interfaces;
using Plugin.LocalNotifications;
using System;
using System.Collections.Generic;
using System.Text;

namespace LembreteDividas.Services
{
    public class LocalNotification : ILocalNotification
    {
        public int Create(string title, string message, DateTime dateTime, int id = 0)
        {
            CrossLocalNotifications.Current.Show(title, message, id, dateTime);
            return id;
        }

        public void CreateNow(string title, string message, int id = 0)
        {
            CrossLocalNotifications.Current.Show(title, message, id);
        }

        public void Delete(int id)
        {
            CrossLocalNotifications.Current.Cancel(id);
        }

        public void Update(int id, string title, string message, DateTime dateTime)
        {
            CrossLocalNotifications.Current.Show(title, message, id, dateTime);
        }
    }
}
