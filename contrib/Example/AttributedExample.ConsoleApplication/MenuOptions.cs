using System;
using System.Collections.Generic;

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

        public MenuOptions()
        {
            foreach (var value in Enum.GetNames(typeof(TEnum)))
            {
                var optionLetter = value.Substring(0, 1).ToUpper();
                var optionText = string.Format("[{0}]{1}", value.Substring(0, 1), value.Substring(1, value.Length - 1));

                _menuOptions[optionLetter] = new MenuItemInfo<TEnum>((TEnum)Enum.Parse(typeof(TEnum), value),
                                                                     optionText);
            }
        }

    }
}
