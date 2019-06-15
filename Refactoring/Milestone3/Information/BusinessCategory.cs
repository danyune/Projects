namespace Team7GUI.Information
{
    public class BusinessCategory : BaseInfo
    {
        public string Categoryname { get; set; }
        public double Total { get; set; }

        /// <summary>
        /// Override so we display just the Categoryname
        /// </summary>
        /// <returns>Categoryname</returns>
        public override string ToString()
        {
            return Categoryname;
        }

        /// <summary>
        /// Override to only check Categoryname and ignore the Total count, total only needed for ordering categories
        /// </summary>
        /// <param name="obj">Comparing object</param>
        /// <returns>True if equal</returns>
        public override bool Equals(object obj)
        {
            if (!(obj is BusinessCategory other))
            {
                return false;
            }

            return Categoryname == other.Categoryname;
        }

        /// <summary>
        /// Not sure if I needed to also override this, but intellisense was mad that I didn't initially
        /// </summary>
        /// <returns>Categoryname Hashcode if not null, or 0</returns>
        public override int GetHashCode() => Categoryname == null ? 0 : Categoryname.GetHashCode();
    }
}
