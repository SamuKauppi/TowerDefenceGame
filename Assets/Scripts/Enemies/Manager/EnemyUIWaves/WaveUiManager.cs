using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class WaveUiManager : MonoBehaviour, IUpdate
{
    // References
    [SerializeField] private RectTransform container;           // Container for the wave ui elements
    [SerializeField] private RectTransform middlePoint;         // middlepoint in the container
    [SerializeField] private float uiScaleFactor = 10f;         // How big and fast are the ui elements

    private const GameEntity waveIdent = GameEntity.WaveUi;     // Ident for searching ui elements
    private readonly List<WaveUi> currentWaves = new();         // Waves

    // Positioning waveUi elements
    private Vector2 endOfNextWave;              // Position to spawn next wave
    private RectTransform lastSpawnedWave;      // Last wave spawned
    private bool isFirstSpawn = true;           // Has a wave been spawned

    // Updating waveUi elements
    private Vector3 moveOffset;                 // The movement done every frame
    private float containerXPos;                // The position of the left most side of container in x-axis
    private bool isAllowedToMove = true;        // Are the waveUi elements allowed to be moved (to sync visuals and spawning)

    // From interface
    public GameObject Object => gameObject;

    /// <summary>
    /// Initilize variables
    /// </summary>
    private void Start()
    {
        containerXPos = container.anchoredPosition.x - container.sizeDelta.x * 0.5f;

        endOfNextWave.x = middlePoint.localPosition.x;
        GameObjectUpdateManager.Instance.AddObject(this);
    }

    /// <summary>
    /// Stop moving wave ui elements for a time
    /// </summary>
    /// <param name="time"></param>
    /// <returns></returns>
    private IEnumerator StopMovingFor(float time)
    {
        isAllowedToMove = false;
        yield return new WaitForSeconds(time);
        isAllowedToMove = true;
    }

    /// <summary>
    /// Add a new wave
    /// </summary>
    /// <param name="time"></param>
    /// <param name="waveName"></param>
    /// <param name="waveColor"></param>
    public void AddWave(float time, string waveName, Color waveColor)
    {
        // Get a new wave
        WaveUi waveUi = ObjectPooler.Instance.GetPooledObject(waveIdent).GetComponent<WaveUi>();
        // Determine wave
        waveUi.DetermineWaveElement(time, waveName, uiScaleFactor, waveColor);

        // If this is not the first spawn, find the right most position of the ui element
        if (!isFirstSpawn)
        {
            endOfNextWave = new Vector2(lastSpawnedWave.rect.width + lastSpawnedWave.localPosition.x, 0);
        }

        // Move the ui element to position
        waveUi.WaveTransform.localPosition = endOfNextWave;

        // Update lastSpawned RectTransform
        lastSpawnedWave = waveUi.WaveTransform;
        isFirstSpawn = false;

        // Add it to be updated
        currentWaves.Add(waveUi);
    }

    /// <summary>
    /// Update wave positions
    /// </summary>
    public void UpdateObject()
    {
        if (!isAllowedToMove)
        {
            return;
        }

        // Set the movement moveOffset
        moveOffset.x = -uiScaleFactor * Time.deltaTime;

        for (int i = currentWaves.Count - 1; i >= 0; i--)
        {
            // Move the wave ui element
            WaveUi waveUi = currentWaves[i];
            waveUi.WaveTransform.localPosition += moveOffset;

            // Check if the wave ui elements right most side has moved out of the containers left most side
            float waveUiXPos = waveUi.WaveTransform.position.x + waveUi.WaveTransform.sizeDelta.x;
            if (waveUiXPos < containerXPos)
            {
                waveUi.gameObject.SetActive(false);
                currentWaves.Remove(waveUi);
            }
        }
    }

    /// <summary>
    /// Allow moving again
    /// </summary>
    public void FreezeUiElementsFor(float time)
    {
        StartCoroutine(StopMovingFor(time));
    }
}
