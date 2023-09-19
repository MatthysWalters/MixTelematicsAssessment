using GeoCoordinatePortable;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Text;

internal class Program
{
    private static void Main(string[] args)
    {
        const string fileName = @"VehiclePositions.dat";
        List<Coordinate> findCoordinatesList = new List<Coordinate>();
        List<VehiclePosition> vehiclePositions = new List<VehiclePosition>();

        KDTree kDTree = new KDTree(2);
        Node? root = null;

        var watchFileReadTotal = System.Diagnostics.Stopwatch.StartNew();
        var watch = System.Diagnostics.Stopwatch.StartNew();

        /* Latitude and longitude are a pair of numbers (coordinates) used to describe a position on the plane of a geographic coordinate system. 
         * The numbers are in decimal degrees format and range from -90 to 90 for latitude and -180 to 180 for longitude.
         */

        // Reference coordinates to find closest vehicle to
        findCoordinatesList.Add(new Coordinate(34.544909f, -102.100843f));
        findCoordinatesList.Add(new Coordinate(32.345544f, -99.123124f));
        findCoordinatesList.Add(new Coordinate(33.234235f, -100.214124f));
        findCoordinatesList.Add(new Coordinate(35.195739f, -95.348899f));
        findCoordinatesList.Add(new Coordinate(31.895839f, -97.789573f));
        findCoordinatesList.Add(new Coordinate(32.895839f, -101.789573f));
        findCoordinatesList.Add(new Coordinate(34.115839f, -100.225732f));
        findCoordinatesList.Add(new Coordinate(32.335839f, -99.992232f));
        findCoordinatesList.Add(new Coordinate(33.535339f, -94.792232f));
        findCoordinatesList.Add(new Coordinate(32.234235f, -100.222222f));

        // Redirect output to a file named ConsoleOutput.txt.
        StreamWriter sw = new StreamWriter(@"ConsoleOutput.txt");
        sw.AutoFlush = true;
        Console.SetOut(sw);

        if (args.Length > 0)
        {
            if (args[0] == "Test")
            {
                //option used to test the distance calculations
                TestCalculations();
            }
            else if (args[0] == "Brute")
            {
                StreamWriter bsw = new StreamWriter(@"BruteOutput.txt");
                bsw.AutoFlush = true;
                Console.SetOut(bsw);
                var watchTotal = System.Diagnostics.Stopwatch.StartNew();
                // Read in vehicle positions from file into a list for brute force calculation
                ReadPosistionsFromFile(true);
                // Benchmark the brute force method of finding the closest vehicle to a given coordinate.
                BenchmarkBrute();
                watchTotal.Stop();
                Console.WriteLine($"Total Execution time : {watchTotal.ElapsedMilliseconds} ms");
            }
            else if (args[0] == "Optimised")    
            {
                StreamWriter osw = new StreamWriter(@"OptimisedOutput.txt");
                osw.AutoFlush = true;
                Console.SetOut(osw);

                var watchTotal = System.Diagnostics.Stopwatch.StartNew();
                // Read in vehicle positions from file into a tree for optimised calculation
                ReadPosistionsFromFile(false);
                // Benchmark the optimised method of finding the closest vehicle to a given coordinate.
                BenchmarkOptimised();
                watchTotal.Stop();
                Console.WriteLine($"Total Execution time : {watchTotal.ElapsedMilliseconds} ms");
            } 
            /*
             * This option was there to attempt to debug how to correct the KD Tree balancing issue.
             * 
            else if (args[0] == "Check")
            {
                StreamWriter csw = new StreamWriter(@"CheckID.txt");
                csw.AutoFlush = true;
                Console.SetOut(csw);
                
                var watchTotal = System.Diagnostics.Stopwatch.StartNew();
                // Read in vehicle positions from file into a tree for optimised calculation
                ReadPosistionsFromFile(false);

                //test finding the closest vehicle to a given coordinate.

                var coord = new Coordinate(32.335839f, -99.992232f);
                var targetVehicleId = 1065091;
                var result = kDTree.searchShortestDistance(root, coord, targetVehicleId);
                if (result.HasValue)
                {
                    var tuple = result.Value;
                    Node node = tuple.node;
                    int depth = tuple.depth;
                    Console.WriteLine($"Closest vehicle to {coord} is {node.vehiclePosition.VehicleRegistration} at depth {depth}");
                    // Use the node and depth properties as needed
                }
                watchTotal.Stop();
                Console.WriteLine($"Total Execution time : {watchTotal.ElapsedMilliseconds} ms");
            }*/
            else
            {
                Console.WriteLine("Invalid argument passed");
            }   
        }
        else
        {
            Console.WriteLine("No arguments passed");
        }

        void TestCalculations()
        {
            /*
             * Funtion to evaluate the different distance calculations.
             * Baseline was the function using the GeoCoordinatePortable package.
             * This package was used to validate the coordinates retrieved from the binary file to ensure they were processed correctly.
             * It is not used in the final solution.
             */
            Console.WriteLine("Geo Distance: {0}", getGeoDistance(2.6884679f, 3.6354017f, 2.6884679f, 3.0f));
            Console.WriteLine("Distance Calc1 Distance: {0}", distance(2.6884679f, 3.6354017f, 2.6884679f, 3.0f));
            Console.WriteLine("Distance Calc2 Distance: {0}", distance2(2.6884679f, 3.6354017f, 2.6884679f, 3.0f));
            Console.WriteLine("Distance GetCalc Distance: {0}", CalculateDistance(new Coordinate(2.6884679f, 3.6354017f), new Coordinate(2.6884679f, 3.0f)));

            Console.WriteLine("Geo Distance: {0}", getGeoDistance(2.6884679f, 3.6354017f, 2.6884679f, -3.0f));
            Console.WriteLine("Distance Calc1 Distance: {0}", distance(2.6884679f, 3.6354017f, 2.6884679f, -3.0f));
            Console.WriteLine("Distance Calc2 Distance: {0}", distance2(2.6884679f, 3.6354017f, 2.6884679f, -3.0f));
            Console.WriteLine("Distance GetCalc Distance: {0}", CalculateDistance(new Coordinate(2.6884679f, 3.6354017f), new Coordinate(2.6884679f, -3.0f)));

        }

        void ReadPosistionsFromFile(bool bToList)
        {
            /*
             * Method to read the binary file data into either a list or a tree.
             * The list is used to baseline the brute force method of finding the closest vehicle to a given coordinate.
             * The tree is used to optimise the search for the closest vehicle to a given coordinate and compare it to the search times for the brute force method.
             */

            int vehicleId;
            string vehicleRegistration;
            float latitude;
            float longitude;
            ulong recordedTimeUTC;
            int count = 0;

            watchFileReadTotal.Restart();

            if (File.Exists(fileName))
            {
                using (var stream = File.Open(fileName, FileMode.Open))
                {
                    using (var reader = new BinaryReader(stream, Encoding.UTF8, false))
                    {
                        while (reader.BaseStream.Position != reader.BaseStream.Length)
                        {
                            try
                            {
                                var bFirstRecord = (reader.BaseStream.Position == 0);   
                                // Read values from binary file

                                vehicleId = reader.ReadInt32();
                                vehicleRegistration = ReadStringZ(reader);
                                latitude = ToFloat(reader.ReadBytes(4));
                                longitude = ToFloat(reader.ReadBytes(4));
                                recordedTimeUTC = reader.ReadUInt64();

                                DateTimeOffset dateTimeOffset = DateTimeOffset.FromUnixTimeSeconds((long)recordedTimeUTC);


                                var coord = new Coordinate(latitude, longitude);

                                if (bToList)
                                {
                                    // Populate list of vehicle positions
                                    VehiclePosition vehiclePosition = new VehiclePosition(vehicleId, vehicleRegistration, coord, dateTimeOffset.DateTime);
                                    vehiclePositions.Add(vehiclePosition);
                                    count++;
                                }
                                else
                                {
                                    // Populate Tree of vehicle positions
                                        root = kDTree.insert(root, new VehiclePosition(vehicleId, vehicleRegistration, coord, dateTimeOffset.DateTime));
                                    count++;
                                }
                            }
                            catch (ArgumentOutOfRangeException aorEx)
                            {
                                Console.WriteLine("Exception: {0}", aorEx.Message);
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine("Exception: {0}", ex.Message);
                            }
                        }
                    }
                }
                /*
                        Console.WriteLine("Vehicle Registration: {0}", vehicleRegistration);    
                        Console.WriteLine("Vehicle Id: {0}", vehicleId);    
                        Console.WriteLine("Latitude: {0}", latitude);   
                        Console.WriteLine("Longitude: {0}", longitude); 
                        Console.WriteLine("Recorded Time UTC: {0}", recordedTimeUTC);   
                */
            }
            watchFileReadTotal.Stop();
            Console.WriteLine($"Data file read execution time : {watchFileReadTotal.ElapsedMilliseconds} ms");

            if (bToList)
            {
                Console.WriteLine("Number of positions found: {0}", vehiclePositions.Count);
            }
            else
            {
                Console.WriteLine("Number of positions found: {0}",count);
            }
        }
        void BenchmarkBrute()
        {
            /*
             * This function is used to benchmark the brute force method of finding the closest vehicle to a given coordinate.
             * It iterates through a list of vehicle positions and calculates the distance between the reference coordinate and each vehicle position.
             * It then compares the calculated distance to the current shortest distance and if it is shorter it replaces the current shortest distance 
             * and tracks the vehicle id and registration number.
             * Output is capture to a file named BruteOutput.txt
             * */
            var watchTotalSearch = System.Diagnostics.Stopwatch.StartNew();
            var shortestDistanceID = 0;
            var currentShortestDistance = 0.0;
            var shortestDistanceRegNo = "";

            foreach (Coordinate coord in findCoordinatesList)
            {
                watch.Restart();
                Console.WriteLine("------------------------------------------------------------------");
                Console.WriteLine("Reference Coordinate: {0}, {1}", coord.Latitude, coord.Longitude);

                // Search list of vehicle positions for closest vehicle to reference position
                foreach (VehiclePosition vehiclePosition in vehiclePositions)
                {
                    var calulatedDistance = Coordinate.CalculateDistance(vehiclePosition.Coordinate, coord);

                    if (currentShortestDistance == 0.0)
                    {
                        currentShortestDistance = calulatedDistance;
                        shortestDistanceID = vehiclePosition.VehicleId;
                        shortestDistanceRegNo = vehiclePosition.VehicleRegistration;
                    }
                    else
                    {
                        if (calulatedDistance < currentShortestDistance)
                        {
                            currentShortestDistance = calulatedDistance;
                            shortestDistanceID = vehiclePosition.VehicleId;
                            shortestDistanceRegNo = vehiclePosition.VehicleRegistration;
                            /*
                            Console.WriteLine("**PROCESSING ... Vehicle Id: [{0}] Registration No: {1} Distance: {2}", shortestDistanceID, shortestDistanceRegNo, currentShortestDistance);
                            Console.WriteLine("PROCESSING ... Vehicle Coordinates: {0}", vehiclePosition.Coordinate.ToString());
                            */
                        }
                    }
                }

                Console.WriteLine("Shortest Distance from coordinate to vehicle: {0}", currentShortestDistance);
                Console.WriteLine("Shortest Distance Vehicle Id: [{0}] Registration No: {1}", shortestDistanceID, shortestDistanceRegNo);

                shortestDistanceID = 0;
                currentShortestDistance = 0.0;
                shortestDistanceRegNo = ""; 
                watch.Stop();
                Console.WriteLine($"Closest position calculation execution time : {watch.ElapsedMilliseconds} ms");

            }
            watchTotalSearch.Stop();
            Console.WriteLine($"Total Search time : {watchTotalSearch.ElapsedMilliseconds} ms");
        }

        void BenchmarkOptimised()
        {
            /*
             * This function is used to benchmark the optimised method of finding the closest vehicle to a given coordinate.
             * It searches a sorted tree of vehicle positions and calculates the distance between the reference coordinate and the vehicle position.
             * It tracks the shortest distance and vehicle information.
             * Output is capture to a file named OptimisedOutput.txt
             */
            var watchTotalSearch = System.Diagnostics.Stopwatch.StartNew();
            if (root == null)
            {
                Console.WriteLine("No vehicle positions found");
                return;
            }   

            foreach (Coordinate coord in findCoordinatesList)
            {
                watch.Restart();
                Console.WriteLine("------------------------------------------------------------------");
                Console.WriteLine("Reference Coordinate: {0}, {1}", coord.Latitude, coord.Longitude);

                // Search tree of vehicle positions for closest vehicle to reference position

                var closestVehicle = kDTree.searchShortestDistance(root, coord);
                if (closestVehicle != null)
                {
                    if (closestVehicle.VehicleInfo != null)
                    {
                        Console.WriteLine("Shortest Distance from coordinate to vehicle: {0}", closestVehicle.Distance);
                        Console.WriteLine("Shortest Distance Vehicle Id: [{0}] Registration No: {1}", closestVehicle.VehicleInfo.VehicleId, closestVehicle.VehicleInfo.VehicleRegistration);
                    }
                    else { Console.WriteLine("No vehicle found");}
                }
                else { Console.WriteLine("No vehicle found"); }
                    
                watch.Stop();
                Console.WriteLine($"Closest position calculation execution time : {watch.ElapsedMilliseconds} ms");

            }
            watchTotalSearch.Stop();
            Console.WriteLine($"Total Search time : {watchTotalSearch.ElapsedMilliseconds} ms");
        }

        static float ToFloat(byte[] input)
        {
            //Build the float from the bytes
            byte[] newArray = new[] { input[0], input[1], input[2], input[3] };
            return BitConverter.ToSingle(newArray, 0);
        }

        static string ReadStringZ(BinaryReader reader)
        {
            StringBuilder sb = new StringBuilder();
            char c;
            for (int i = 0; i < reader.BaseStream.Length; i++)
            {
                if ((c = (char)reader.ReadByte()) == 0)
                {
                    break;
                }
                sb.Append(c);
            }
            return sb.ToString();
        }

        double toRadians(double degrees)
        {
            return degrees * Math.PI / 180;
        }
        double toDegrees(double radians)
        {
            return radians / Math.PI * 180.0;
        }

        double distance(double lat1, double lon1, double lat2, double lon2)
        {
            /*
             * Haverine formula
            * Returns the distance between the 2 coordinates in km.
            */
            double theta = lon1 - lon2;
            double dist = Math.Sin(toRadians(lat1)) * Math.Sin(toRadians(lat2)) + Math.Cos(toRadians(lat1)) * Math.Cos(toRadians(lat2)) * Math.Cos(toRadians(theta));
            dist = Math.Acos(dist);
            dist = toDegrees(dist);
            dist = dist * 60 * 1.1515;
            dist = dist * 1.609344;
            return dist;
        }

        double distance2(double lat1, double lon1, double lat2, double lon2)
        {
            /*
             *  Alternative formula
            * Returns the distance between the 2 coordinates in km.
            */
            lon1 = toRadians(lon1);
            lon2 = toRadians(lon2);
            lat1 = toRadians(lat1);
            lat2 = toRadians(lat2);

            // Haversine formula
            double dlon = lon2 - lon1;
            double dlat = lat2 - lat1;
            double a = Math.Pow(Math.Sin(dlat / 2.0), 2.0) +
                       Math.Cos(lat1) * Math.Cos(lat2) *
                       Math.Pow(Math.Sin(dlon / 2.0), 2.0);

            double c = 2.0 * Math.Asin(Math.Sqrt(a));

            // Radius of earth in kilometers at equator.
            double r = 6371.0;
            return c * r;

        }

        double CalculateDistance(Coordinate point1, Coordinate point2)
        {
            /*
             * Selected formula since it matches the GeoCoordinatePortable package the closest.
             * Returns the distance between the 2 coordinates in km.
             * Added to the Coordinate Class as a static function for use in the List iteration and KDTree class.
             */
            var d1 = point1.Latitude * (Math.PI / 180.0);
            var num1 = point1.Longitude * (Math.PI / 180.0);
            var d2 = point2.Latitude * (Math.PI / 180.0);
            var num2 = point2.Longitude * (Math.PI / 180.0) - num1;
            var d3 = Math.Pow(Math.Sin((d2 - d1) / 2.0), 2.0) +
                     Math.Cos(d1) * Math.Cos(d2) * Math.Pow(Math.Sin(num2 / 2.0), 2.0);
            return 6376500.0 * (2.0 * Math.Atan2(Math.Sqrt(d3), Math.Sqrt(1.0 - d3))) / 1000.0;
        }

        double getGeoDistance(double lat1, double lon1, double lat2, double lon2)
        {
            /*
             * Used nuget package GeoCoordinatePortable to verify my own calculation in testing.
             * Returns the distance between the 2 coordinates in km.
             * validated the coordinates retrieved from the binary file to ensure they were processed correctly.
             */
            if (lat1 > 90.0f || lat1 < -90.0f)
            {
                throw new ArgumentOutOfRangeException("Latitude", "Argument must be in range of -90 to 90");
            }

            if (lat2 > 90.0 || lat2 < -90.0)
            {
                throw new ArgumentOutOfRangeException("Latitude", "Argument must be in range of -90 to 90");
            }

            if (lon1 > 180.0 || lon1 < -180.0)
            {
                throw new ArgumentOutOfRangeException("Longitude", "Argument must be in range of -180 to 180");
            }

            if (lon2 > 180.0 || lon2 < -180.0)
            {
                throw new ArgumentOutOfRangeException("Longitude", "Argument must be in range of -180 to 180");
            }

            var sCoord = new GeoCoordinate(lat1, lon1);
            var eCoord = new GeoCoordinate(lat2, lon2);

            return sCoord.GetDistanceTo(eCoord) / 1000.00;
        }
    }
}