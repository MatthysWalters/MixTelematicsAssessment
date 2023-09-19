//Defining the Node class with an VehiclePosition to hold the vehicle information and references to left and right nodes
using System.ComponentModel;
using System.Security.Cryptography;
using System;

class Node
{
    public VehiclePosition vehiclePosition;
    public Node? left;
    public Node? right;
    public Node? parent;

    // Defining a constructor to initialize the Node object
    public Node(VehiclePosition vehiclePosition)
    {
        this.vehiclePosition = vehiclePosition;
        left = null;
        right = null;
        parent = null;
    }

}



// Defining the KdTree class to represent a K-d tree with a specified value of k
class KDTree
{
    private int k;

    // Defining a constructor to initialize the KdTree object
    public KDTree(int k)
    {
        this.k = k;
    }

    // Defining a method to create a new Node object
    public Node newNode(VehiclePosition vehiclePosition)
    {
        return new Node(vehiclePosition);
    }

    // Defining a recursive method to insert a new Node into the K-d tree
    private Node insertRec(Node root, VehiclePosition vehiclePosition, int depth)
    {
        if (root == null)
        {
            return newNode(vehiclePosition);
        }

        root.parent = root;

        int cd = depth % k;

        if (cd == 0)
        {
            if (vehiclePosition.Coordinate.Longitude > root.vehiclePosition.Coordinate.Longitude)
            {
                root.left = insertRec(root.left, vehiclePosition, depth + 1);
            }
            else
            {
                root.right = insertRec(root.right, vehiclePosition, depth + 1);
            }
        }
        else
        {
            if (vehiclePosition.Coordinate.Latitude > root.vehiclePosition.Coordinate.Latitude)
            {
                root.left = insertRec(root.left, vehiclePosition, depth + 1);
            }
            else
            {
                root.right = insertRec(root.right, vehiclePosition, depth + 1);
            }
        }

        return root;
    }

    // Defining a method to insert a point into the K-d tree
    public Node insert(Node root, VehiclePosition vehiclePosition)
    {
        return insertRec(root, vehiclePosition, 0);
    }

    // Defining a recursive method to search in the K-d tree for a vehicle with the shortest distance to the coordinate supplied.
    private CloseVehicle? searchRec(Node root, Coordinate coord, int depth, CloseVehicle closestVehicle)
    {
        if (root == null)
        {
            return closestVehicle;
        }

        int cd = depth % k;
        var distance = Coordinate.CalculateDistance(coord, root.vehiclePosition.Coordinate);

        if (distance < closestVehicle.Distance)
        {
            closestVehicle.VehicleInfo = root.vehiclePosition;
            closestVehicle.Distance = distance;
        }

        if (cd == 0)
        {
            if (coord.Longitude > root.vehiclePosition.Coordinate.Longitude)
            {
                if (root.left != null)
                {
                    return searchRec(root.left, coord, depth + 1, closestVehicle);
                }
                else
                {
                    if (root.right != null)
                    {
                        return searchRec(root.right, coord, depth + 1, closestVehicle);
                    }
                    else
                    {
                        //Reached a leaf node. No more nodes to search. Return the closest vehicle found so far
                        //double check if there are other nodes with shorter distances to search in the tree by backtracking and trying the other branch (depth same as this call)
                        //parent cannot be null. If it is null, then the root node is the only node in the tree
                        var checkBranchNode = searchRec(root.parent, coord, depth - 1, closestVehicle);
                        if (checkBranchNode.VehicleInfo.VehicleId == closestVehicle.VehicleInfo.VehicleId)
                        {
                            //Branch did not contain a closer vehicle. Return the closest vehicle found so far
                            Console.WriteLine("Branch did not contain a closer vehicle.Return the closest vehicle found so far: " + closestVehicle.VehicleInfo.VehicleId);
                            return closestVehicle;
                        }
                        else
                        {
                            Console.WriteLine("Found shorter distance return: " + checkBranchNode.VehicleInfo.VehicleId);
                            return checkBranchNode;
                        }
                    }
                }
            }
            else
            {
                if (root.right != null)
                {
                    return searchRec(root.right, coord, depth + 1, closestVehicle);
                }
                else
                {
                    if (root.left != null)
                    {
                        return searchRec(root.left, coord, depth + 1, closestVehicle);
                    }
                    else
                    {
                        //Reached a leaf node. No more nodes to search. Return the closest vehicle found so far
                        //double check if there are other nodes with shorter distances to search in the tree by backtracking and trying the other branch (depth same as this call)
                        var checkBranchNode = searchRec(root.parent, coord, depth - 1, closestVehicle);
                        if (checkBranchNode.VehicleInfo.VehicleId == closestVehicle.VehicleInfo.VehicleId)
                        {
                            //Branch did not contain a closer vehicle. Return the closest vehicle found so far
                            Console.WriteLine("Branch did not contain a closer vehicle.Return the closest vehicle found so far: " + closestVehicle.VehicleInfo.VehicleId);
                            return closestVehicle;
                        }
                        else
                        {
                            Console.WriteLine("Found shorter distance return: " + checkBranchNode.VehicleInfo.VehicleId);
                            return checkBranchNode;
                        }
                    }
                }   
            }
        }
        else
        {
            if (coord.Latitude > root.vehiclePosition.Coordinate.Latitude)
            {
                if (root.left != null)
                {
                    return searchRec(root.left, coord, depth + 1, closestVehicle);
                }
                else
                {
                    if (root.right != null)
                    {
                        return searchRec(root.right, coord, depth + 1, closestVehicle);
                    }
                    else
                    {
                        //Reached a leaf node. No more nodes to search. Return the closest vehicle found so far
                        //double check if there are other nodes with shorter distances to search in the tree by backtracking and trying the other branch (depth same as this call)
                        //parent cannot be null. If it is null, then the root node is the only node in the tree
                        var checkBranchNode = searchRec(root.parent, coord, depth -1, closestVehicle);
                        if (checkBranchNode.VehicleInfo.VehicleId == closestVehicle.VehicleInfo.VehicleId)
                        {
                            //Branch did not contain a closer vehicle. Return the closest vehicle found so far
                            Console.WriteLine("Branch did not contain a closer vehicle.Return the closest vehicle found so far: " + closestVehicle.VehicleInfo.VehicleId);
                            return closestVehicle;
                        }
                        else
                        {
                            Console.WriteLine("Found shorter distance return: " + checkBranchNode.VehicleInfo.VehicleId);
                            return checkBranchNode;
                        }
                    }
                }   
            }
            else
            {
                if (root.right != null)
                {
                    return searchRec(root.right, coord, depth + 1, closestVehicle);
                }
                else
                {
                    if (root.left != null)
                    {
                        return searchRec(root.left, coord, depth + 1, closestVehicle);
                    }
                    else
                    {
                        //Reached a leaf node. No more nodes to search. Return the closest vehicle found so far
                        //double check if there are other nodes with shorter distances to search in the tree by backtracking and trying the other branch (depth same as this call)
                        var checkBranchNode = searchRec(root.parent, coord, depth - 1, closestVehicle);
                        if (checkBranchNode.VehicleInfo.VehicleId == closestVehicle.VehicleInfo.VehicleId)
                        {
                            //Branch did not contain a closer vehicle. Return the closest vehicle found so far
                            Console.WriteLine("Branch did not contain a closer vehicle.Return the closest vehicle found so far: " + closestVehicle.VehicleInfo.VehicleId);
                            return closestVehicle;
                        }
                        else
                        {
                            Console.WriteLine("Found shorter distance return: " + checkBranchNode.VehicleInfo.VehicleId);
                            return checkBranchNode;
                        }
                    }
                }
            }
        }
    }

    // Method to search for a shortest distance vehicle in the K-d tree
    public CloseVehicle searchShortestDistance(Node root, Coordinate coord)
    {
        CloseVehicle closestVehicle = new CloseVehicle(coord);   
        var result = searchRec(root, coord, 0, closestVehicle);
        if (result != null)
        {
            return result;
        }
        else
        {
            return closestVehicle;
        }   
    }

    private bool isLeaf(Node node)
    {
        return node.left == null && node.right == null;
    }

/*
    private (Node node, int depth)? searchVehicle(Node root, Coordinate coord, int depth, CloseVehicle closestVehicle)
    {
        if (root == null)
        {
            return (root, depth);
        }

        var distance = Coordinate.CalculateDistance(coord, root.vehiclePosition.Coordinate);
        if (isLeaf(root) && distance < closestVehicle.Distance)
        {
            closestVehicle.VehicleInfo = root.vehiclePosition;
            closestVehicle.Distance = distance;
            return (root, depth);
        }
        else if (isLeaf(root))
        {
            return (root, depth);
        }
        else
        {  
            // similar to the depth modulus calculation in the searchRec method above
            (Node nextBranch, Node otherBranch) = coord.CompareTo(root.vehiclePosition.Coordinate) < 0 ? (root.left, root.right) : (root.right, root.left);

            var nextResult = searchVehicle(nextBranch, coord, depth + 1, closestVehicle);

            if (otherBranch != null && (nextResult == null || Coordinate.CalculateDistance(coord, otherBranch.vehiclePosition.Coordinate) < Coordinate.CalculateDistance(coord, nextResult.Value.node.vehiclePosition.Coordinate)))
            {
                var otherResult = searchVehicle(otherBranch, coord, depth + 1, closestVehicle);
                return otherResult ?? nextResult;
            }

            return nextResult;
        }
    }

    private (Node node, int depth)? searchRec(Node root, Coordinate coord, int depth, int targetVehicleId)
    {
        if (root == null)
        {
            return null;
        }

        var distance = Coordinate.CalculateDistance(coord, root.vehiclePosition.Coordinate);

        if (root.vehiclePosition.VehicleId == targetVehicleId)
        {
            // Return the node and its depth when the target vehicle is found
            return (root, depth);
        }
        // similar to the depth modulus calculation in the searchRec method above
        (Node nextBranch, Node otherBranch) = coord.CompareTo(root.vehiclePosition.Coordinate) < 0 ? (root.left, root.right) : (root.right, root.left);

        var nextResult = searchRec(nextBranch, coord, depth + 1, targetVehicleId);

        if (otherBranch != null && (nextResult == null || Coordinate.CalculateDistance(coord, otherBranch.vehiclePosition.Coordinate) < Coordinate.CalculateDistance(coord, nextResult.Value.node.vehiclePosition.Coordinate)))
        {
            var otherResult = searchRec(otherBranch, coord, depth + 1, targetVehicleId);
            return otherResult ?? nextResult;
        }

        return nextResult;
    }


    public (Node node, int depth)? searchShortestDistance(Node root, Coordinate coord, int targetVehicleId)
    {
        CloseVehicle closestVehicle = new CloseVehicle(coord);
        return searchVehicle(root,coord, 0, closestVehicle);

        //return searchRec(root, coord, 0, targetVehicleId);
    }
*/

}
