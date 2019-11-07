using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;

namespace Microsoft.Localization.LocSolutions.Cerberus.Core
{
    /// <summary>
    /// This class exposes some of the Configuration settings of Cerberus to external components. The main scenario is OSLEBot rules that may want to recycle
    /// some of the Cerberus settings like list of known languages, anguage groups, etc.
    /// Cerberus.Executor is responsible for calling the Initialize method to populate the object with data before it is accessed by the components.
    /// </summary>
    public static class ConfigurationHelper
    {
        static ConfigurationHelper()
        {
            // set all public properties to empty
            // PROBLEM: i am using HashSet here in order to make often string lookups efficient. however the collections returned should be
            // read-only in order to prevent user code from modifying Configuration settings - in .NET framework there is no easy way of
            // wrapping HashSet into ReadOnlyCollection
            AllLanguages = new ReadonlyHashSet<string>(StringComparer.OrdinalIgnoreCase);
            EALanguages = new ReadonlyHashSet<string>(StringComparer.OrdinalIgnoreCase);
            BiDiLanguages = new ReadonlyHashSet<string>(StringComparer.OrdinalIgnoreCase);
            CSLanguages = new ReadonlyHashSet<string>(StringComparer.OrdinalIgnoreCase);
        }
        /// <summary>
        /// Cerberus.Executor needs to call this in order to initialize configuration settings based on the config it reads in.
        /// This code needs to be maintained in order to be in sync with any changes to the configuration.
        /// </summary>
        /// <param name="config">Cerberus configuration object</param>
        static internal void Initialize(CheckConfiguration config)
        {
            Func<string, IEnumerable<string>> GetLanguageGroup =
                groupName => 
                    {
                        var group = config.GetTagLanguages(groupName);
                        if (!group.Any())
                        {
                            throw new InvalidOperationException(String.Format("Invalid Cerberus configuration: unknown language group: {0}", groupName));
                        }
                        return group;
                    };
            
            EALanguages = new ReadonlyHashSet<string>(GetLanguageGroup("East Asian"), StringComparer.OrdinalIgnoreCase);
            BiDiLanguages = new ReadonlyHashSet<string>(GetLanguageGroup("Bidirectional"), StringComparer.OrdinalIgnoreCase);

            AllLanguages = new ReadonlyHashSet<string>(config.GetAllLanguages(), StringComparer.OrdinalIgnoreCase);



        }

        /// <summary>
        /// A collection of all Languages known to Cerberus as defined in Cerberus config.
        /// Strings represent language name in &lt;languagecode2&gt;-&lt;country/regioncode2&gt; format.
        /// </summary>
        static public IReadOnlyCollection<string> AllLanguages { get; private set; }
        /// <summary>
        /// A collection of all East Asia languages known to Cerberus as defined in Cerberus config.
        /// Strings represent language name in &lt;languagecode2&gt;-&lt;country/regioncode2&gt; format.
        /// </summary>
        static public IReadOnlyCollection<string> EALanguages { get; private set; }
        /// <summary>
        /// A collection of all Bi-Di languages known to Cerberus as defined in Cerberus config.
        /// Strings represent language name in &lt;languagecode2&gt;-&lt;country/regioncode2&gt; format.
        /// </summary>
        static public IReadOnlyCollection<string> BiDiLanguages { get; private set; }
        /// <summary>
        /// A collection of all Complex Script languages known to Cerberus as defined in Cerberus config.
        /// Strings represent language name in &lt;languagecode2&gt;-&lt;country/regioncode2&gt; format.
        /// </summary>
        static public IReadOnlyCollection<string> CSLanguages { get; private set; }



        /// <summary>
        /// Reduced HashSet functionality to be used as perf efficient IReadOnlyCollection.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        private class ReadonlyHashSet<T> : IReadOnlyCollection<T>
        {
            HashSet<T> _set;
            public ReadonlyHashSet()
            {
                _set = new HashSet<T>();
            }
            public ReadonlyHashSet(IEnumerable<T> collection, IEqualityComparer<T> comparer)
            {
                _set = new HashSet<T>(collection, comparer);
            }
            public ReadonlyHashSet(IEqualityComparer<T> comparer)
            {
                _set = new HashSet<T>(comparer);
            }
            #region IReadOnlyCollection<T> Members

            public int Count
            {
                get { return _set.Count; }
            }

            public bool Contains(T item)
            {
                return _set.Contains(item);
            }

            #endregion

            #region IEnumerable<T> Members

            public IEnumerator<T> GetEnumerator()
            {
                return _set.GetEnumerator();
            }

            #endregion

            #region IEnumerable Members

            System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
            {
                return this.GetEnumerator();
            }

            #endregion
        }
        /// <summary>
        /// Workaround the fact that .NET framework has no read only collection interface.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public interface IReadOnlyCollection<T> : IEnumerable<T>
        {
            /// <summary>
            /// Number of items in the readonly collection.
            /// </summary>
            int Count { get; }
            /// <summary>
            /// Checks if readonly collection contains the item.
            /// </summary>
            /// <param name="item"></param>
            /// <returns></returns>
            bool Contains(T item);
        }
    }

}
