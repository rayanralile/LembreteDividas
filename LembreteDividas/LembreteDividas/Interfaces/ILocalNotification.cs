using System;
using System.Collections.Generic;
using System.Text;

namespace LembreteDividas.Interfaces
{
    public interface ILocalNotification
    {
        /// <summary>
        /// Cria uma notificação local, que será exibida no tempo determinado
        /// </summary>
        /// <param name="title">Título da notificação</param>
        /// <param name="message">Mensagem a ser exibida</param>
        /// <param name="dateTime">Data e hora de quando a notificação surgirá</param>
        /// <param name="id">OBSOLETO: Id que a antiga biblioteca deverá utilizar para registrar a operação</param>
        /// <returns>Retorna o Id que identifica a notificação criada. Ela será pedida se precisar atualizar ou apagar a notificação</returns>
        int Create(string title, string message, DateTime dateTime, int id = 0);
        /// <summary>
        /// Cria uma notificação local imediatamente
        /// </summary>
        /// <param name="title">Título da notificação</param>
        /// <param name="message">Texto a ser exibido na notificação</param>
        /// <param name="id">OBSOLETO: Id que a antiga biblioteca deverá utilizar para registrar a operação</param>
        void CreateNow(string title, string message, int id = 0);
        /// <summary>
        /// Atualiza a notificação agendada
        /// </summary>
        /// <param name="id">O id da notificação a ser atualizada</param>
        /// <param name="title">Título da notificação</param>
        /// <param name="message">A nova mensagem a ser inserida</param>
        /// <param name="dateTime">A nova data-hora a ser exibida a notificação</param>
        void Update(int id, string title, string message, DateTime dateTime);
        /// <summary>
        /// Deleta notificação que está no sistema agendado
        /// </summary>
        /// <param name="id">O id da notificação a ser apagado.</param>
        void Delete(int id);
    }
}
