using System;

namespace Xamariners.RestClient.Test
{
    public class WeatherResponse
    {
        public Area_Metadata[] area_metadata { get; set; }
        public Item[] items { get; set; }
        public Api_Info api_info { get; set; }
    }
    public class Api_Info
    {
        public string status { get; set; }
    }

    public class Area_Metadata
    {
        public string name { get; set; }
        public Label_Location label_location { get; set; }
    }

    public class Label_Location
    {
        public float latitude { get; set; }
        public float longitude { get; set; }
    }

    public class Item
    {
        public DateTime update_timestamp { get; set; }
        public DateTime timestamp { get; set; }
        public Valid_Period valid_period { get; set; }
        public Forecast[] forecasts { get; set; }
    }

    public class Valid_Period
    {
        public DateTime start { get; set; }
        public DateTime end { get; set; }
    }

    public class Forecast
    {
        public string area { get; set; }
        public string forecast { get; set; }
    }
}