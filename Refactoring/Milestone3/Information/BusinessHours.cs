using System;

namespace Team7GUI.Information
{
    public class BusinessHours : BaseInfo
    {
        public string Day { get; set; }
        public string Businessid { get; set; }
        public string Closetime { get; set; }
        public string Opentime { get; set; }

        public BusinessHours()
        {
        }

        /// <summary>
        /// Override comparing attribute objects by name only
        /// </summary>
        /// <param name="obj">BusinessAttribute</param>
        /// <returns>True or False</returns>
        public override bool Equals(object obj)
        {
            if (!(obj is BusinessHours other))
            {
                return false;
            }

            return Day == other.Day;
        }

        /// <summary>
        /// Unsure if needed, but override HashCode
        /// </summary>
        /// <returns>HashCode</returns>
        public override int GetHashCode() => Day == null ? 0 : Day.GetHashCode();
    }
}