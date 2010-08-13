using System;
using AttributedExample.ConsoleApplication.StateTypes;

namespace AttributedExample.ConsoleApplication
{
    public class MainMenu
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
    }
}
