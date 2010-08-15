using System;
using System.Collections.Generic;
using System.Linq;

namespace AttributedExample.ConsoleApplication
{
    public class MenuItemInfo<TEnumType>
    {
        public string Text { get; private set; }
        public TEnumType Value { get; private set; }

        public MenuItemInfo(TEnumType value, string text)
        {
            Value = value;
            Text = text;
        }
    }

    public class MenuOptions<TEnum>
    {
        private IDictionary<string, MenuItemInfo<TEnum>> _menuOptions = new Dictionary<string, MenuItemInfo<TEnum>>();
        public IDictionary<string, MenuItemInfo<TEnum>> Options { get { return _menuOptions; } }

        public MenuOptions(IEnumerable<TEnum> enumeratedOptions)
        {
            var optionNames = from p in enumeratedOptions select Enum.GetName(typeof (TEnum), p);

            foreach(var optionName in optionNames)
            {
                
                var optionLetter = optionName.Substring(0, 1).ToUpper();
                var optionText = string.Format("[{0}]{1}", optionName.Substring(0, 1), optionName.Substring(1, optionName.Length - 1));

                _menuOptions[optionLetter] = new MenuItemInfo<TEnum>((TEnum)Enum.Parse(typeof(TEnum), optionName),
                                                                     optionText);
                
            }
        }
    
        public MenuOptions() : this(from p in Enum.GetNames(typeof(TEnum)) select (TEnum) Enum.Parse(typeof(TEnum), p))
        {}

    }
}
