using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;
using System.Collections;

namespace Autofac.Configuration.Elements
{
    public class ConfigurationElementCollection<TElementType> : ConfigurationElementCollection, IEnumerable<TElementType>
        where TElementType : ConfigurationElement
    {
        private string _elementName;

        public ConfigurationElementCollection(string elementName)
        {
            _elementName = elementName;
        }

        protected override string ElementName
        {
            get
            {
                return _elementName;
            }
        }

        protected override bool IsElementName(string elementName)
        {
            return elementName != null && elementName == _elementName;
        }

        public new IEnumerator<TElementType> GetEnumerator()
        {
            foreach (TElementType element in (IEnumerable)this)
                yield return element;
        }

        protected override ConfigurationElement CreateNewElement()
        {
            return Activator.CreateInstance<TElementType>();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return Guid.NewGuid();
        }

        public override ConfigurationElementCollectionType CollectionType
        {
            get
            {
                return ConfigurationElementCollectionType.BasicMap;
            }
        }
    }
}
