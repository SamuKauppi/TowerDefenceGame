using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathPoint : MonoBehaviour
{
    public bool IsPathable { get; private set; }
    public bool IsCloseToExit { get; private set; }
    public int DistanceToEnd { get; set; }
    [SerializeField] private float dist;
    [SerializeField] private int pathValue = 0;
    public Vector2 PathfindingId { get; set; }
    public PathPoint[] Neighbours { get; set; }

    [SerializeField] private SpriteRenderer m_rend;
    public void SetDistanceToEnd(int distanceToEnd)
    {
        this.DistanceToEnd = distanceToEnd;
        m_rend.color = new Color(0, 0, 0, distanceToEnd * 0.01f);
        dist = distanceToEnd;
        if (distanceToEnd < 5)
        {
            IsCloseToExit = true;
            m_rend.color = Color.green;
        }
    }
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
