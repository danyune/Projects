using System;

namespace Team7GUI.Information
{
    public class Users : BaseInfo
    {
        public string Userid { get; set; }
        public string Name { get; set; }
        public string Yelpingsince { get; set; }
        public int Reviewcount { get; set; }
        public int Fans { get; set; }
        public double Averagestars { get; set; }
        public int Funny { get; set; }
        public int Useful { get; set; }
        public int Cool { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }

        // Used as a separate attribute but gets result from other attributes, to display as the displaymemberpath in the listbox
        public string SearchResult => Name + "  " + Userid;

        public Users()
        {
        }
    }
}