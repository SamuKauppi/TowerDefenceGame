using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class WaveUi : MonoBehaviour
{
    [SerializeField] private RectTransform endOfWaveMarker;
    [SerializeField] private RectTransform waveTransform;
    [SerializeField] private TMP_Text waveText;
    [SerializeField] private Image waveImage;
    public RectTransform WaveTransform { get { return waveTransform; } }
    public float GetEndWaveXPosition {  get { return endOfWaveMarker.position.x; } }

    public void DetermineWaveElement(float time, string waveName, float scale, Color waveColor)
    {
        waveText.text = waveName;
        waveImage.color = new Color(waveColor.r, waveColor.g, waveColor.b, 1f);

        waveTransform.sizeDelta = new Vector2(scale * time, waveTransform.sizeDelta.y);
    }
}
