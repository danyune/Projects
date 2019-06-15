using System.Collections.Generic;

namespace Team7GUI.QueryBuilder
{
    internal class OrFilter
    {
        internal List<string> Orfilter { get; private set; } = new List<string>();

        public OrFilter()
        {
        }

        public void Add(string filter)
        {
            Orfilter.Add(filter);
        }

        public void Clear()
        {
            Orfilter.Clear();
        }
    }
}