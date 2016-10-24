using System;
using System.Collections.Generic;
using System.Linq;
using Autofac.Util;
using Xunit;

namespace Autofac.Test.Util
{
    public class FallbackDictionaryTests
    {
        [Fact]
        public void Add_LocalContainsKey()
        {
            var dict = new FallbackDictionary<string, object>();
            dict.Add("key", true);
            Assert.Throws<ArgumentException>(() => dict.Add("key", false));
        }

        [Fact]
        public void Add_ParentContainsKey()
        {
            var parent = new Dictionary<string, object>();
            parent["key"] = true;

            var dict = new FallbackDictionary<string, object>(parent);
            Assert.Throws<ArgumentException>(() => dict.Add("key", false));
        }

        [Fact]
        public void Contains_LocalAndParent()
        {
            var parent = new Dictionary<string, object>();
            parent.Add("first", 1);
            parent.Add("second", 2);

            var dict = new FallbackDictionary<string, object>(parent);
            dict.Add("third", 3);

            Assert.True(dict.Contains(new KeyValuePair<string, object>("first", 1)));
            Assert.True(dict.Contains(new KeyValuePair<string, object>("second", 2)));
            Assert.True(dict.Contains(new KeyValuePair<string, object>("third", 3)));
        }

        [Fact]
        public void Contains_LocalOnly()
        {
            var dict = new FallbackDictionary<string, object>();
            dict.Add("first", 1);
            dict.Add("second", 2);

            Assert.True(dict.Contains(new KeyValuePair<string, object>("first", 1)));
            Assert.True(dict.Contains(new KeyValuePair<string, object>("second", 2)));
        }

        [Fact]
        public void Contains_ParentOnly()
        {
            var parent = new Dictionary<string, object>();
            parent.Add("first", 1);
            parent.Add("second", 2);

            var dict = new FallbackDictionary<string, object>(parent);

            Assert.True(dict.Contains(new KeyValuePair<string, object>("first", 1)));
            Assert.True(dict.Contains(new KeyValuePair<string, object>("second", 2)));
        }

        [Fact]
        public void Contains_UsesOverrides()
        {
            var parent = new Dictionary<string, object>();
            parent["first"] = 1;
            parent["second"] = 2;

            var dict = new FallbackDictionary<string, object>(parent);
            dict["second"] = "two";
            dict["third"] = 3;

            Assert.True(dict.Contains(new KeyValuePair<string, object>("first", 1)));
            Assert.False(dict.Contains(new KeyValuePair<string, object>("second", 2)));
            Assert.True(dict.Contains(new KeyValuePair<string, object>("second", "two")));
            Assert.True(dict.Contains(new KeyValuePair<string, object>("third", 3)));
        }

        [Fact]
        public void Count_Empty()
        {
            var dict = new FallbackDictionary<string, object>();
            Assert.Equal(0, dict.Count);
        }

        [Fact]
        public void Count_IgnoresOverrides()
        {
            var parent = new Dictionary<string, object>();
            parent["first"] = 1;
            parent["second"] = 2;

            var dict = new FallbackDictionary<string, object>(parent);
            dict["second"] = "two";
            dict["third"] = 3;

            Assert.Equal(3, dict.Count);
        }

        [Fact]
        public void Count_LocalAndParent()
        {
            var parent = new Dictionary<string, object>();
            parent.Add("first", 1);
            parent.Add("second", 2);

            var dict = new FallbackDictionary<string, object>(parent);
            dict.Add("third", 3);

            Assert.Equal(3, dict.Count);
        }

        [Fact]
        public void Count_LocalOnly()
        {
            var dict = new FallbackDictionary<string, object>();
            dict.Add("first", 1);
            dict.Add("second", 2);

            Assert.Equal(2, dict.Count);
        }

        [Fact]
        public void Count_ParentOnly()
        {
            var parent = new Dictionary<string, object>();
            parent.Add("first", 1);
            parent.Add("second", 2);

            var dict = new FallbackDictionary<string, object>(parent);

            Assert.Equal(2, dict.Count);
        }

        [Fact]
        public void Ctor_Default()
        {
            var dict = new FallbackDictionary<string, object>();
            Assert.IsAssignableFrom<IDictionary<string, object>>(dict.Parent);
        }

        [Fact]
        public void Ctor_NullParent()
        {
            Assert.Throws<ArgumentNullException>(() => new FallbackDictionary<string, object>(null));
        }

        [Fact]
        public void GetEnumerator_UsesOverrides()
        {
            var parent = new Dictionary<string, object>();
            parent["first"] = 1;
            parent["second"] = 2;

            var dict = new FallbackDictionary<string, object>(parent);
            dict["second"] = "two";
            dict["third"] = 3;

            var list = new List<KeyValuePair<string, object>>();
            foreach (var kvp in dict)
            {
                list.Add(kvp);
            }

            Assert.Equal(3, list.Count);
            Assert.Contains(1, list.Select(k => k.Value));
            Assert.Contains("two", list.Select(k => k.Value));
            Assert.Contains(3, list.Select(k => k.Value));
        }

        [Fact]
        public void Keys_Empty()
        {
            var dict = new FallbackDictionary<string, object>();
            Assert.Equal(0, dict.Keys.Count);
        }

        [Fact]
        public void Keys_IgnoresDuplicatesInOverrides()
        {
            var parent = new Dictionary<string, object>();
            parent["first"] = 1;
            parent["second"] = 2;

            var dict = new FallbackDictionary<string, object>(parent);
            dict["second"] = "two";
            dict["third"] = 3;

            Assert.Equal(3, dict.Keys.Count);
            Assert.Contains("first", dict.Keys);
            Assert.Contains("second", dict.Keys);
            Assert.Contains("third", dict.Keys);
        }

        [Fact]
        public void Keys_LocalAndParent()
        {
            var parent = new Dictionary<string, object>();
            parent.Add("first", 1);
            parent.Add("second", 2);

            var dict = new FallbackDictionary<string, object>(parent);
            dict.Add("third", 3);

            Assert.Equal(3, dict.Keys.Count);
            Assert.Contains("first", dict.Keys);
            Assert.Contains("second", dict.Keys);
            Assert.Contains("third", dict.Keys);
        }

        [Fact]
        public void Keys_LocalOnly()
        {
            var dict = new FallbackDictionary<string, object>();
            dict.Add("first", 1);
            dict.Add("second", 2);

            Assert.Equal(2, dict.Keys.Count);
            Assert.Contains("first", dict.Keys);
            Assert.Contains("second", dict.Keys);
        }

        [Fact]
        public void Keys_ParentOnly()
        {
            var parent = new Dictionary<string, object>();
            parent.Add("first", 1);
            parent.Add("second", 2);

            var dict = new FallbackDictionary<string, object>(parent);

            Assert.Equal(2, dict.Keys.Count);
            Assert.Contains("first", dict.Keys);
            Assert.Contains("second", dict.Keys);
        }

        [Fact]
        public void Keys_Values_SameOrder()
        {
            var parent = new Dictionary<string, object>();
            parent["first"] = 1;
            parent["second"] = 2;

            var dict = new FallbackDictionary<string, object>(parent);
            dict["second"] = "two";
            dict["third"] = 3;

            var keys = dict.Keys.ToArray();
            var values = dict.Values.ToArray();

            Assert.Equal(3, keys.Length);
            Assert.Equal(3, values.Length);

            Assert.Equal(values[0], dict[keys[0]]);
            Assert.Equal(values[1], dict[keys[1]]);
            Assert.Equal(values[2], dict[keys[2]]);
        }

        [Fact]
        public void Values_LocalAndParent()
        {
            var parent = new Dictionary<string, object>();
            parent.Add("first", 1);
            parent.Add("second", 2);

            var dict = new FallbackDictionary<string, object>(parent);
            dict.Add("third", 3);

            Assert.Equal(3, dict.Values.Count);
            Assert.Contains(1, dict.Values);
            Assert.Contains(2, dict.Values);
            Assert.Contains(3, dict.Values);
        }

        [Fact]
        public void Values_LocalOnly()
        {
            var dict = new FallbackDictionary<string, object>();
            dict.Add("first", 1);
            dict.Add("second", 2);

            Assert.Equal(2, dict.Values.Count);
            Assert.Contains(1, dict.Values);
            Assert.Contains(2, dict.Values);
        }

        [Fact]
        public void Values_ParentOnly()
        {
            var parent = new Dictionary<string, object>();
            parent.Add("first", 1);
            parent.Add("second", 2);

            var dict = new FallbackDictionary<string, object>(parent);

            Assert.Equal(2, dict.Values.Count);
            Assert.Contains(1, dict.Values);
            Assert.Contains(2, dict.Values);
        }

        [Fact]
        public void Values_UsesOverrides()
        {
            var parent = new Dictionary<string, object>();
            parent["first"] = 1;
            parent["second"] = 2;

            var dict = new FallbackDictionary<string, object>(parent);
            dict["second"] = "two";
            dict["third"] = 3;

            Assert.Equal(3, dict.Values.Count);
            Assert.Contains(1, dict.Values);
            Assert.Contains("two", dict.Values);
            Assert.Contains(3, dict.Values);
        }
    }
}
