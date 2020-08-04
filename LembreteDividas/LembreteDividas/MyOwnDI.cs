using LembreteDividas.Services;
using System;
using System.Collections.Generic;
using System.Text;

namespace LembreteDividas
{
    public static class MyOwnDI
    {
        /// <summary>
        /// Implementa a dependência da interface ILocalNotification
        /// </summary>
        /// <returns>A interface implementada e pronta pra uso</returns>
        public static LocalNotification ILocalNotificationDI()
        {
            return new LocalNotification();
        }
    }
}
