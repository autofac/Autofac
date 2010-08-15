using System;
using System.Collections.Generic;
using System.Linq;
using AttributedExample.ConsoleApplication.StateTypes;

namespace AttributedExample.ConsoleApplication
{
    /// <summary>
    /// Provides the menu loops for capturing user input
    /// </summary>
    public class ApplicationMenu
    {
        /// <summary>
        /// Provides the key portion of the menu loop where the user is asked for the document type
        /// </summary>
        /// <returns>selected document type</returns>
        public DocumentType? Loop()
        {
            var options = new MenuOptions<DocumentType>();

            Console.WriteLine("\n\n -------- Main Menu ------------");
            Console.WriteLine(" ------------------------------");
            foreach (var item in options.Options)
                Console.WriteLine("  {0}", item.Value.Text);
            Console.WriteLine("\nEnter value or any other value to exit");

            var input = Console.ReadLine().ToUpper();
            MenuItemInfo<DocumentType> menuItem;

            if (options.Options.TryGetValue(input, out menuItem))
                return menuItem.Value;

            return null;
        }

        /// <summary>
        /// performs the action loop according to trigger values available fromt the state engine
        /// </summary>
        /// <param name="stateEngine">state engine containinig workflow definitions</param>
        public void ActionLoop(StateEngine stateEngine)
        {
            IEnumerable<WorkflowTrigger> activeTriggers;

            while( (activeTriggers = stateEngine.Actions).Count() > 0)
            {
                var optionSet = new MenuOptions<WorkflowTrigger>(activeTriggers);

                foreach(var option in optionSet.Options)
                    Console.WriteLine("  {0}", option.Value.Text);

                var input = Console.ReadLine().ToUpper();
                MenuItemInfo<WorkflowTrigger> menuItem;
                if(! optionSet.Options.TryGetValue(input, out menuItem))
                {
                    Console.WriteLine(" ** Option {0} was not understood, try again", input);
                    continue;
                }

                stateEngine.Fire(menuItem.Value);
            }
        }
    }
}
