namespace Team7GUI.Information
{
    internal class BusinessCheckins
    {
        public string Day { get; private set; }
        public double Count { get; private set; }

        public BusinessCheckins() : this(string.Empty, 0)
        {
        }

        public BusinessCheckins(string day, double count)
        {
            Day = day;
            Count = count;
        }
    }
}