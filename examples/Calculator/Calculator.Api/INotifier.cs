using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Calculator.Api
{
    /// <summary>
    /// Alerts the user to a problem.
    /// </summary>
    public interface INotifier
    {
        /// <summary>
        /// Display the message to the user.
        /// </summary>
        /// <param name="message">Message to display.</param>
        void Notify(string message);
    }
}
