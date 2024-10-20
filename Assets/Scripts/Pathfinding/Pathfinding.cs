using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Pathfinding : MonoBehaviour
{
    // Singleton
    public static Pathfinding Instance { get; private set; }

    // Endpoint rendering
    [SerializeField] private Transform endObj;                  // Visual object of parented to endPoint
    private Vector2 highestYpos;                                // Position where endrend top left corner will be                                
    private Vector2 lowestYpos;                                 // Position where endrend bot left corner will be
    private bool isEndObjScaled;                                // Ensures that endObj is scaled for first recalculation

    // Pathpoints
    [SerializeField] private PathPoint point_prefab;            // Pathpoint prefab
    [SerializeField] private PathPoint endPoint;                // Pathpoint which enemies try to get to
    private PathPoint[] pathPoints =  new PathPoint[0];         // Pathpoints that have been created
    private PathPoint startingPoint;                            // Pathpoint which enemies start (used for checking if path is valid)

    // Pathfinding area variables
    [SerializeField] private float exitPointSize = 5f;          // How big is the area where towers can't be built
    [SerializeField] private float positionStep = 0.2f;         // Distance between pathpoints
    [SerializeField] private float mapwidth = 9;                // How wide is the pathfinding area
    [SerializeField] private float mapheight = 4.6f;            // How high is the pathfinding area
    [SerializeField] private float startingXPosition = 6f;      // From where the pathpoints start being spawned in x axis
    [SerializeField] private float topYPosOffset;               // Offset from top where no more pathpoints are spawned
    [SerializeField] private float botYPosOffset;               // Offset from bot where no more pathpoints are spawned
    private const float sizeMultiplier = 1.1f;                  // To increase the size of area disabled when placing tower
                                                                // This improves the pathfinding

    /// <summary>
    /// Create Singleton and call GeneratePaths
    /// </summary>
    private void Awake()
    {
        Instance = this;
        endPoint.SetPathable(true);
        endPoint.ID = new(0, 0);
        startingPoint = endPoint; // give a starting point a starting value
        GeneratePaths();
    }

    /// <summary>
    /// Generate pathpoints 
    /// </summary>
    private void GeneratePaths()
    {
        // Initialize pathpoint list with endpoint at start
        List<PathPoint> points = new()
        {
            endPoint
        };
        // Determine positions of pathpoints
        float xPos = startingXPosition;
        float yPos = 0f;
        // Determine x and y indexes
        int yIndex = 0;
        int xIndex = 0;
        // Continue loop until yPos is over both top and bot mapHeight
        while (MathF.Abs(yPos) <= mapheight - topYPosOffset || MathF.Abs(yPos) <= mapheight - botYPosOffset)
        {
            if ((yPos >= 0 && yPos <= mapheight - topYPosOffset) ||
                (yPos < 0 && yPos >= -(mapheight - botYPosOffset)))
            {
                while (xPos >= -mapwidth)
                {
                    xIndex++;
                    points.Add(CreatePathpoint(new Vector3(xPos, yPos), new PathfindingID(xIndex, yIndex)));
                    xPos -= positionStep;
                }
            }

            xPos = startingXPosition;
            xIndex = 0;
            if (yPos > 0)
            {
                yPos *= -1f;
                yIndex *= -1;
            }
            else
            {
                yPos = Math.Abs(yPos) + positionStep;
                yIndex = Math.Abs(yIndex) + 1;
            }
        }
        pathPoints = points.ToArray();
        // Determine neighbours for each pathpoint
        for (int i = 0; i < pathPoints.Length; i++)
        {
            pathPoints[i].Neighbours = DetermineNeighbours(pathPoints[i]);
        }

        // Calculate pathPointChecked from end to start
        RecalculateDistancesAStar();
    }

    /// <summary>
    /// Create a pathpoint
    /// </summary>
    /// <param name="pos"></param>
    /// <param name="pathId"></param>
    /// <returns></returns>
    private PathPoint CreatePathpoint(Vector3 pos, PathfindingID pathId)
    {
        // Create pathpoint
        PathPoint pathpoint = Instantiate(point_prefab, pos, Quaternion.identity, transform);
        // Set position relative to other pathpoints (used to determine neighbours later)
        pathpoint.ID = pathId;
        // Set it pathable as default
        pathpoint.SetPathable(true);
        // Save the last point on x-axis as starting point (used to check if path was blocked when building a tower)
        if (pathId.y == 0 && pathId.x > startingPoint.ID.x)
        {
            startingPoint = pathpoint;
        }
        return pathpoint;

    }

    /// <summary>
    /// Determine neighbours for pathpoints
    /// </summary>
    /// <param name="currentPoint"></param>
    /// <returns></returns>
    private PathPoint[] DetermineNeighbours(PathPoint currentPoint)
    {
        // Make list to save neighbours
        List<PathPoint> neighbours = new();
        for (int i = 0; i < pathPoints.Length; i++)
        {
            int xDifference = Math.Abs(pathPoints[i].ID.x - currentPoint.ID.x);
            int yDifference = Math.Abs(pathPoints[i].ID.y - currentPoint.ID.y);

            // If pathpoint is directly LEFT, RIGHT, UP, DOWN, or DIAGONAL
            // Add it as a neighbour
            if ((xDifference == 1 && yDifference == 0) || // Horizontal
                (xDifference == 0 && yDifference == 1) || // Vertical
                (xDifference == 1 && yDifference == 1))   // Diagonal
            {
                neighbours.Add(pathPoints[i]);
            }
        }

        return neighbours.OrderBy(p => Mathf.Abs(p.ID.x) + Mathf.Abs(p.ID.y)).ToArray();
    }

    /// <summary>
    /// Calculate pathfinding
    /// </summary>
    private void RecalculateDistancesAStar()
    {
        // Create queue and dictionary with the first point as first value
        // Dictionary value is int (number of steps from start)
        Queue<PathPoint> frontier = new();
        frontier.Enqueue(pathPoints[0]);
        Dictionary<PathPoint, int> pathPointChecked = new()
        {
            { pathPoints[0], 0 }
        };

        // As long as there is a pathpoint to check, continue on checking
        while (frontier.Count > 0)
        {
            // Get the next pathpoint from queue
            PathPoint current = frontier.Dequeue();
            // Get it's neighbours and go through them
            PathPoint[] currentNeighbours = current.Neighbours;
            foreach (PathPoint pathPoint in currentNeighbours)
            {
                // If this neighbouring pathpoint has not been already been checked and is pathable
                if (!pathPointChecked.ContainsKey(pathPoint) && pathPoint.IsPathable)
                {
                    // Enqueue to check this pathpoints to check it 
                    frontier.Enqueue(pathPoint);
                    // Add it to dictionary and save it's own distance with current pathpoint distance + 1
                    pathPointChecked.Add(pathPoint, 1 + current.StepsFromEnd);
                    pathPoint.SetDistanceToEnd(1 + current.StepsFromEnd, exitPointSize);

                    if (!isEndObjScaled && pathPoint.IsCloseToExit)
                    {
                        DetermineEndPointDimensions(pathPoint.transform.position);
                    }
                }
            }
        }

        isEndObjScaled = true;
    }

    /// <summary>
    /// Get pathpoints inside an area
    /// </summary>
    /// <param name="position"></param>
    /// <param name="size"></param>
    /// <returns></returns>
    private PathPoint[] GetPathpointsInArea(Vector2 position, Vector2 size)
    {
        Rect check = MakeACheckRect(position, size * sizeMultiplier);
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

    /// <summary>
    /// Create a rectangle at position with a size
    /// </summary>
    /// <param name="position"></param>
    /// <param name="size"></param>
    /// <returns></returns>
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

    /// <summary>
    /// Set selected pathpoint pathable
    /// </summary>
    /// <param name="points"></param>
    /// <param name="active"></param>
    private void SetPathPointsActive(PathPoint[] points, bool active)
    {
        for (int i = 0; i < points.Length; i++)
        {
            points[i].SetPathable(active);
        }
        RecalculateDistancesAStar();
    }

    /// <summary>
    /// Scales the endpoint visual object to size
    /// </summary>
    /// <param name="newPos"></param>
    private void DetermineEndPointDimensions(Vector2 newPos)
    {
        if (newPos.y > highestYpos.y)
        {
            highestYpos = newPos;
        }
        if (newPos.y < lowestYpos.y)
        {
            lowestYpos = newPos;
        }

        // Calculate scale factor
        float scale = Mathf.Abs(highestYpos.y - lowestYpos.y);

        endObj.localScale = new Vector3(scale, scale);
    }

    /// <summary>
    /// Get the closest pathpoint neighbour
    /// </summary>
    /// <param name="currentPoint"></param>
    /// <returns></returns>
    public PathPoint GetNextPathpoint(PathPoint currentPoint)
    {
        PathPoint shortestPoint = null;
        for (int i = 0; i < currentPoint.Neighbours.Length; i++)
        {
            // Get a neighbour that is pathable and has shortest distance (same pathPointChecked get coin flipped)
            if (currentPoint.Neighbours[i].IsPathable)
            {
                if (!shortestPoint)
                {
                    shortestPoint = currentPoint.Neighbours[i];
                }
                else
                {
                    // Compare which is closer to end
                    if (currentPoint.Neighbours[i].StepsFromEnd < shortestPoint.StepsFromEnd)
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

    /// <summary>
    /// Get the closest pathpoint from position
    /// </summary>
    /// <param name="currentPos"></param>
    /// <returns></returns>
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

    /// <summary>
    /// Check if a path is valid when building tower
    /// 1. Check if tower is too close to exit
    /// 2. Do a simulation if path will be valid
    /// </summary>
    /// <param name="position"></param>
    /// <param name="size"></param>
    /// <returns>Is the path valid</returns>
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
            testPath = GetNextPathpoint(testPath);
            if (lastPathpoints.Contains(testPath))
            {
                // Blocking path
                SetPathPointsActive(points, true);
                return false;
            }
            lastPathpoints.Add(testPath);

        }

        // Is a valid path
        return true;
    }

    /// <summary>
    /// Return endpoint
    /// </summary>
    /// <returns></returns>
    public Vector3 GetEndPoint()
    {
        return endPoint.transform.position;
    }

    /// <summary>
    /// Disables pathpoins in area
    /// </summary>
    /// <param name="pos"></param>
    /// <param name="size"></param>
    public void DisablePathpointsInArea(Vector2 pos, Vector2 size)
    {
        PathPoint[] points = GetPathpointsInArea(pos, size);
        SetPathPointsActive(points, false);
    }
}
