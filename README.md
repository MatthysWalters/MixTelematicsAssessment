# MixTelematicsAssessment

The console application accepts one parameter which determines which operation will be performed.
Output can then be compared to evaluate search times for the brute force method versus the optimised method.
The 10 reference coordinates are loaded into a list.

1. Test
   I needed to verify that my distance calculation was correct calling the application with this parameter runs set coordinate calculations.
   It evaluates the different distance calculations.
   Baseline was the function using the GeoCoordinatePortable package.
   This package was used to validate the coordinates retrieved from the binary file to ensure they were processed correctly.
   It is not used in the final solution.
   The console output is redirected to a file "ConsoleOutput.txt"
2. Brute
   The binary file data is read a list structure.
   The list is used to baseline the brute force method of finding the closest vehicle to a given coordinate.
   The duration of the processing is captured for comparison.
   The console output is redirected to a file "BruteOutput.txt"
3. Optimised
   The binary file data is read a KD-tree structure.
   The tree is used to optimise the search for the closest vehicle to a given coordinate.
   It searches a sorted tree of vehicle positions and calculates the distance between the reference coordinate and the vehicle position.
   It tracks the shortest distance and vehicle information.
   The duration of the processing is captured for comparison.
   The console output is redirected to a file "OptimisedOutput.txt"

NOTE: There are still balancing issues with the tree which required some additional tests.
There needs to be an improvement in the plane selection when creating the tree to ensure a more balanced tree.
Currently there are a couple of reference points where the search is returning incorrect vehicles. The search must either do additional branch checks or the tree must be balanced better.
