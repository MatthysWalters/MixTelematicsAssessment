internal class VehiclePosition
{

    public int VehicleId { get; set; }
    public string VehicleRegistration { get; set; }
    public Coordinate Coordinate { get; set; }
    public DateTime RecordedTimeUTC { get; set; }

    public VehiclePosition(int vehicleId, string vehicleRegistration, Coordinate coordinate, DateTime recordedTimeUTC)
    {
        VehicleId = vehicleId;
        VehicleRegistration = vehicleRegistration;
        Coordinate = coordinate;
        RecordedTimeUTC = recordedTimeUTC;
    }   
}

internal class CloseVehicle
{

    public Coordinate SearchCoordinate { get; set; }
    public VehiclePosition? VehicleInfo { get; set; }
    public double Distance { get; set; }

    public CloseVehicle(Coordinate searchCoordinate)
    {
        SearchCoordinate = searchCoordinate;
        VehicleInfo = null;
        Distance = double.MaxValue;
    }
}

internal class Coordinate
{
    private float _latitude;
    private float _longitude;
    public float Latitude 
    {
        get
        {
            return _latitude;
        }
        set
        {
            if (value > 90.0f || value < -90.0f)
            {
                if (value > 90.0f)
                {
                    Console.WriteLine("Lattude is {0}", value.ToString());
                    Console.WriteLine("Latitude is greater than 90.0f");
                }
                else
                {
                    Console.WriteLine("Latitude is less than -90.0f");
                }

                 throw new ArgumentOutOfRangeException("Latitude", "Argument must be in range of -90 to 90");
            }

            _latitude = value;
        }
    }
    public float Longitude
    {
        get
        {
            return _longitude;
        }
        set
        {
            if (value > 180.0f || value < -180.0f)
            {
                throw new ArgumentOutOfRangeException("Longitude", "Argument must be in range of -180 to 180");
            }

            _longitude = value;
        }
    }

    public Coordinate(float latitude, float longitude)
    {
        Latitude = latitude;
        Longitude = longitude;
    }

    public static double CalculateDistance(Coordinate point1, Coordinate point2)
    {
        /*
         * Selected formula since it matches the GeoCoordinatePortable package the closest.
         * Returns the distance between the 2 coordinates in km.
         */
        var d1 = point1.Latitude * (Math.PI / 180.0);
        var num1 = point1.Longitude * (Math.PI / 180.0);
        var d2 = point2.Latitude * (Math.PI / 180.0);
        var num2 = point2.Longitude * (Math.PI / 180.0) - num1;
        var d3 = Math.Pow(Math.Sin((d2 - d1) / 2.0), 2.0) +
                 Math.Cos(d1) * Math.Cos(d2) * Math.Pow(Math.Sin(num2 / 2.0), 2.0);
        return 6376500.0 * (2.0 * Math.Atan2(Math.Sqrt(d3), Math.Sqrt(1.0 - d3))) / 1000.0;
    }

    public override string ToString()
    {
        return string.Format("Latitude: {0}, Longitude: {1}", Latitude, Longitude);
    }
/*
    internal int CompareTo(Coordinate coordinate)
    {
        if ((this.Longitude < coordinate.Longitude) && (this.Latitude < coordinate.Latitude))
        {
            return -1;
        }
        else if ((this.Longitude == coordinate.Longitude) && (this.Latitude == coordinate.Latitude))
        {
            return 0;
        }
        else
        {
            return 1;
        }
    }
*/
}