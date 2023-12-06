using UnityEngine;

public class PathPoint : MonoBehaviour
{
    public bool IsPathable { get; private set; }        // Is this pathpoint pathable
    public bool IsCloseToExit { get; private set; }     // Is this pathpoint close to exit. If is, towers can't be place here
    public int StepsFromEnd { get; set; }               // Steps to reach the end
    public Vector2 PathfindingId { get; set; }          // Position in pathpoint grid
    public PathPoint[] Neighbours { get; set; }         // Neighbours to this pathpoint

    private int pathValue = 0;                          // Check how many towers are over this pathpoint

    /// <summary>
    /// Set pathpoints steps to end
    /// </summary>
    /// <param name="distanceToEnd"></param>
    public void SetDistanceToEnd(int distanceToEnd, float shortestDist = 5)
    {
        StepsFromEnd = distanceToEnd;
        if (distanceToEnd < shortestDist)
        {
            IsCloseToExit = true;
        }
    }

    /// <summary>
    /// Set this pathpoint to pathable
    /// </summary>
    /// <param name="value"></param>
    public void SetPathable(bool value)
    {
        if (!value)
        {
            pathValue++;
            IsPathable = false;
        }
        else
        {
            pathValue--;
            if (pathValue > 0)
            {
                return;
            }

            IsPathable = true;
            pathValue = 0;
        }
    }
}
