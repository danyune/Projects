using System.Collections.Generic;

namespace Team7GUI.Information
{
    public static class MainData
    {
        private static Dictionary<string, string> _qualifiedAttributes = new Dictionary<string, string>() {
                { "BikeParking", "Bike Parking" },
                { "BusinessAcceptsCreditCards", "Business Accepts Credit Cards" },
                { "GoodForKids", "Good For Kids" },
                { "OutdoorSeating", "Outdoor Seating" },
                { "RestaurantsCounterService", "Restaurant Counter Service" },
                { "RestaurantsDelivery", "Restaurant Delivery" },
                { "RestaurantsGoodForGroups", "Restaurant Good For Groups" },
                { "RestaurantsReservations", "Restaurant Reservation" },
                { "RestaurantsTableService", "Restaurant Table Service" },
                { "RestaurantsTakeOut", "Restaurant Take Out" },
                { "WheelchairAccessible", "Wheelchair Accessible" },
                { "WiFi", "WiFi (Free or Paid)" }
            };

        public static Dictionary<string, string> GetQualifiedAttributes()
        {
            return _qualifiedAttributes;
        }
    }
}