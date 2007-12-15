using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Calculator.Api;
using System.Windows.Forms;

namespace Calculator
{
    class MessageBoxNotifier : INotifier
    {
        #region INotifier Members

        public void Notify(string message)
        {
            if (message == null)
                throw new ArgumentNullException("message");

            MessageBox.Show(message);
        }

        #endregion
    }
}
