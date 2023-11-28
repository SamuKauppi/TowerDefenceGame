[System.Serializable]
public class StatusElementClass
{
    public GameEntity bulletApplying;
    public StatusEffect statusEff;

    // Parameters
    public float duration;
    public float strength;

    // Timer for how long status is applied
    [UnityEngine.HideInInspector]
    public float timer;
}
