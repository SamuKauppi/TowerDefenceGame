using UnityEngine;

public class PathPoint : MonoBehaviour
{
    // Is this pathpoint pathable
    public bool IsPathable { get; private set; }
    // Is this pathpoint close to exit. If is, towers can't be place here
    public bool IsCloseToExit { get; private set; }
    // Steps to reach the end
    public int DistanceToEnd { get; set; }
    // Position in pathpoint grid
    public Vector2 PathfindingId { get; set; }
    // Neighbours to this pathpoint
    public PathPoint[] Neighbours { get; set; }

    // Renderer
    [SerializeField] private SpriteRenderer m_rend;
    // To display steps to end in inspector
    [SerializeField] private float dist;
    // Check how many towers are over this pathpoint
    [SerializeField] private int pathValue = 0;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="distanceToEnd"></param>
    public void SetDistanceToEnd(int distanceToEnd, float shortestDist = 5)
    {
        DistanceToEnd = distanceToEnd;
        m_rend.color = new Color(0, 0, 0, distanceToEnd * 0.01f);
        dist = distanceToEnd;
        if (distanceToEnd < shortestDist)
        {
            m_rend.enabled = true;
            IsCloseToExit = true;
            m_rend.color = Color.green;
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
            m_rend.color = Color.red;
        }
        else
        {
            pathValue--;
            if (pathValue <= 0)
            {
                IsPathable = true;
                pathValue = 0;
            }
        }
    }
}
