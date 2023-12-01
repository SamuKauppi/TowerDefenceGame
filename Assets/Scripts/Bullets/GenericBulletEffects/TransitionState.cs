using UnityEngine;

[System.Serializable]
public class TransitionState
{
    public float Time;
    public Vector3 Scale;
    public float Rotation;
    public LeanTweenType easeType = LeanTweenType.linear;
    public Color Color;
}