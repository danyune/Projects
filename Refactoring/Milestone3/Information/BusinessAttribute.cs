using System.Collections.Generic;

namespace Team7GUI.Information
{
    internal class BusinessAttribute : BaseInfo
    {
        public string Attributename { get; set; }
        public double Total { get; set; }
        
        // Used so the listBox doesn't say the Attributename from the SQL database
        public string Realname
        {
            get
            {
                if (_realNames.TryGetValue(Attributename, out string value))
                {
                    return value;
                }
                return string.Empty;
            }
        }

        /// <summary>
        /// Edit the QualifiedAttributes static class to add or remove attributes to show and filter with
        /// </summary>
        private readonly Dictionary<string, string> _realNames = MainData.GetQualifiedAttributes();

        /// <summary>
        /// Replace default constructor even if empty
        /// </summary>
        public BusinessAttribute()
        {
        }

        /// <summary>
        /// Override ToString to show the real name and not the SQL name
        /// </summary>
        /// <returns>Realname</returns>
        public override string ToString()
        {
            return Realname;
        }

        /// <summary>
        /// Override comparing attribute objects by name only
        /// </summary>
        /// <param name="obj">BusinessAttribute</param>
        /// <returns>True or False</returns>
        public override bool Equals(object obj)
        {
            if (!(obj is BusinessAttribute other))
            {
                return false;
            }

            return Attributename == other.Attributename;
        }

        /// <summary>
        /// Unsure if needed, but override HashCode
        /// </summary>
        /// <returns>HashCode</returns>
        public override int GetHashCode() => Attributename == null ? 0 : Attributename.GetHashCode();
    }
}