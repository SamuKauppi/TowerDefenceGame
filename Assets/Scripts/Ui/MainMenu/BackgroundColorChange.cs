using System.Collections;
using UnityEngine;

public class BackgroundColorChange : MonoBehaviour
{
    [SerializeField] private SpriteRenderer sr;
    [SerializeField] private float MaxTimeForColor;
    [SerializeField] private float minTimeForColor;
    [SerializeField] private float transitionTime;
    private void Start()
    {
        sr.color = Color.white;
        StartCoroutine(CreateEffect());
    }

    private IEnumerator CreateEffect()
    {
        while (true)
        {
            StaticFunctions.Instance.StartTransition(sr, Random.ColorHSV(), transitionTime, 150);
            float randomTime = Random.Range(minTimeForColor, MaxTimeForColor);
            yield return new WaitForSeconds(randomTime);
        }
    }
}
