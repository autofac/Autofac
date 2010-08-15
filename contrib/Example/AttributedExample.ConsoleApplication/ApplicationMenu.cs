using System;
using System.Collections.Generic;
using System.Linq;
using AttributedExample.ConsoleApplication.StateTypes;

namespace AttributedExample.ConsoleApplication
{
    public class ApplicationMenu
    {
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
