using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using Random = UnityEngine.Random;

public class Pathfinding : MonoBehaviour
{
    public static Pathfinding Instance { get; private set; }
    [SerializeField] private PathPoint point_prefab;

    [SerializeField] PathPoint[] pathPoints;
    [SerializeField] private PathPoint endPoint;
    [SerializeField] private PathPoint startingPoint;

    // Create pathpoints and determine singleton
    private void Awake()
    {
        Instance = this;
        endPoint.SetPathable(true);
        startingPoint = endPoint; // give a starting point a starting value
        GeneratePaths();
    }
    //Determine position of pathpoints
    private void GeneratePaths()
    {
        // Initialize pathpoint list with endpoint at start
        List<PathPoint> points = new()
        {
            endPoint
        };

        // Determine positions of pathpoints
        float xPos = 6f;
        float yPos = 0f;
        int yPoint = 0;
        int xPoint = 0;
        while (yPos <= 4.6f)
        {
            while (xPos >= -9)
            {
                xPoint++;
                points.Add(CreatePathPoint(new Vector3(xPos, yPos), new Vector2(xPoint, yPoint)));
                xPos -= 0.2f;
            }
            xPos = 6f;
            xPoint = 0;
            if (yPos > 0)
            {
                yPos *= -1f;
                yPoint *= -1;
            }
            else
            {
                yPos = Math.Abs(yPos) + 0.2f;
                yPoint = Math.Abs(yPoint) + 1;
            }
        }
        pathPoints = points.ToArray();
        // Determine neighbours for each pathpoint
        for (int i = 0; i < pathPoints.Length; i++)
        {
            pathPoints[i].Neighbours = DetermineNeighbours(pathPoints[i]);
        }

        // Calculate distances from end to start
        RecalculateDistances();
    }
    // Create pathpoint at position and determine variables
    private PathPoint CreatePathPoint(Vector3 pos, Vector2 pathId)
    {
        // Create pathpoint
        PathPoint pathpoint = Instantiate(point_prefab, pos, Quaternion.identity, transform);
        // Set position relative to other pathpoints (used to determine neighbours later)
        pathpoint.PathfindingId = pathId;
        // Set it pathable as default
        pathpoint.SetPathable(true);
        // Save the last point on x-axis as starting point (used to check if path was blocked when building a tower)
        if (pathId.y == 0 && pathId.x > startingPoint.PathfindingId.x)
        {
            startingPoint = pathpoint;
        }
        return pathpoint;

    }
    // Initialize neighbours for a pathpoint
    private PathPoint[] DetermineNeighbours(PathPoint currentPoint)
    {
        // Make list to save neighbours
        List<PathPoint> neighbours = new();
        for (int i = 0; i < pathPoints.Length; i++)
        {
            // If pathpoint is directly LEFT, RIGHT, UP or DOWN 
            // Add it as a neighbour
            if ((pathPoints[i].PathfindingId.x == currentPoint.PathfindingId.x + 1
                || pathPoints[i].PathfindingId.x == currentPoint.PathfindingId.x - 1) &&
                pathPoints[i].PathfindingId.y == currentPoint.PathfindingId.y)
            {
                neighbours.Add(pathPoints[i]);
            }
            if ((pathPoints[i].PathfindingId.y == currentPoint.PathfindingId.y + 1
                || pathPoints[i].PathfindingId.y == currentPoint.PathfindingId.y - 1) &&
                pathPoints[i].PathfindingId.x == currentPoint.PathfindingId.x)
            {
                neighbours.Add(pathPoints[i]);
            }
        }
        return neighbours.ToArray();
    }
    // Calculate pathfinding
    private void RecalculateDistances()
    {
        // Create queue and dictionary with the first point as first value
        // Dictionary value is int (number of steps from start)
        Queue<PathPoint> frontier = new();
        frontier.Enqueue(pathPoints[0]);
        Dictionary<PathPoint, int> distances = new()
        {
            { pathPoints[0], 0 }
        };

        // As long as there is a pathpoint to check, continue on checking
        while (frontier.Count > 0)
        {
            // Get the next pathpoint in queue
            PathPoint current = frontier.Dequeue();
            // Get it's neighbours and go through them
            PathPoint[] currentNeighbours = current.Neighbours;
            for (int i = 0; i < currentNeighbours.Length; i++)
            {
                // If this neighbouring pathpoint has not been already been checked and is pathable
                if (!distances.ContainsKey(currentNeighbours[i]) && currentNeighbours[i].IsPathable)
                {
                    // Enqueue to check this pathpoints to check it 
                    frontier.Enqueue(currentNeighbours[i]);
                    // Add it to dictionary and save it's own distance with current pathpoint distance + 1
                    distances.Add(currentNeighbours[i], 1 + current.DistanceToEnd);
                    currentNeighbours[i].SetDistanceToEnd(1 + current.DistanceToEnd);
                }
            }
        }
    }
    // Get the next pathpoint with shortest distance
    public PathPoint GetNextPathPoint(PathPoint currentPoint)
    {
        PathPoint shortestPoint = null;
        for (int i = 0; i < currentPoint.Neighbours.Length; i++)
        {
            // Get a neighbour that is pathable and has shortest distance (same distances get coin flipped)
            if (currentPoint.Neighbours[i].IsPathable)
            {
                if (!shortestPoint)
                {
                    shortestPoint = currentPoint.Neighbours[i];
                }
                else
                {
                    // Compare distance and prefer the shorter
                    if (currentPoint.Neighbours[i].DistanceToEnd < shortestPoint.DistanceToEnd)
                    {
                        shortestPoint = currentPoint.Neighbours[i];
                    }
                    // If distances equal, 50/50 chance to pick either one
                    else if (currentPoint.Neighbours[i].DistanceToEnd == shortestPoint.DistanceToEnd && Random.Range(0, 4) != 0)
                    {
                        shortestPoint = currentPoint.Neighbours[i];
                    }
                }
            }
        }

        // As a failsafe, find the closest pathpoint
        if (!shortestPoint)
        {
            shortestPoint = GetClosestPathPoint(currentPoint.transform.position);
        }

        return shortestPoint;
    }

    //Get closests pathpoint from current position 
    public PathPoint GetClosestPathPoint(Vector3 currentPos)
    {
        PathPoint shortestDist = pathPoints[0];
        for (int i = 0; i < pathPoints.Length; i++)
        {
            if (Vector3.Distance(currentPos, pathPoints[i].transform.position)
                < Vector3.Distance(currentPos, shortestDist.transform.position) &&
                pathPoints[i].IsPathable)
            {
                shortestDist = pathPoints[i];
            }
        }
        return shortestDist;
    }

    //Check if path would be valid if a tower is built in position
    public bool CheckIfPathIsValid(Vector3 position, Vector2 size)
    {
        // Get relevant pathpoints
        PathPoint[] points = GetPathpointsInArea(position, size);

        // Check if any of them are too close to exit
        // Return false if yes
        for (int i = 0; i < points.Length; i++)
        {
            if (points[i].IsCloseToExit)
            {
                // Too close to exit
                Debug.Log("Too close to exit");
                return false;
            }
        }

        // Disable relevant pathpoints and recalculate pathing
        SetPathPointsActive(points, false);

        // Make a pathfinding simulation from the start until it's close to exit
        PathPoint testPath = startingPoint;
        List<PathPoint> lastPathpoints = new();
        while (!testPath.IsCloseToExit)
        {
            testPath = GetNextPathPoint(testPath);
            if (lastPathpoints.Contains(testPath))
            {
                // Blocking path
                SetPathPointsActive(points, true);
                Debug.Log("Will block the path at: " + testPath.PathfindingId + ", " + testPath.DistanceToEnd);
                return false;
            }
            lastPathpoints.Add(testPath);
        }

        // Is a valid path
        Debug.Log("Valid path");
        return true;
    }
    // Get pathpoints inside an area
    // (NOTE: size is increased to avoid enemies getting stuck on towers)
    private PathPoint[] GetPathpointsInArea(Vector2 position, Vector2 size)
    {
        Rect check = MakeACheckRect(position, size * 1.42f);
        List<PathPoint> points = new();
        for (int i = 0; i < pathPoints.Length; i++)
        {
            if (check.Contains(pathPoints[i].transform.position))
            {
                points.Add(pathPoints[i]);
            }
        }
        return points.ToArray();
    }

    // Create a rectangle at position with a size
    private Rect MakeACheckRect(Vector3 position, Vector2 size)
    {
        return new()
        {
            x = position.x - (size.x * 0.5f),
            y = position.y - (size.y * 0.5f),
            width = size.x,
            height = size.y
        };
    }

    //Set selected pathpoint pathable
    private void SetPathPointsActive(PathPoint[] points, bool active)
    {
        for (int i = 0; i < points.Length; i++)
        {
            points[i].SetPathable(active);
        }
        RecalculateDistances();
    }

    //Get endpoint
    public Vector3 GetEndPoint()
    {
        return endPoint.transform.position;
    }
}
