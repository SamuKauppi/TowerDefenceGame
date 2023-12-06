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
    private readonly List<WaveUi> _currentWaves = new();        // Waves

    // Positioning waveUi elements
    private Vector2 _endOfNextWave;                             // Position to spawn next wave
    private RectTransform _lastSpawnedWave;                     // Last wave spawned
    private bool _isFirstSpawn = true;                          // Has a wave been spawned

    // Updating waveUi elements 
    private Vector3 _moveOffset;                                // The movement done every frame
    private float _containerXPos;                               // The position of the left most side of container in x-axis

    // Syncing waveUi elements
    private int _isAllowedToMove = -1;                          // When less 0, stop moving ui (to sync Ui and waves)
                                                                // Starts at -1 to not move at beginning
    private float _middleXPos;                                  // Middle point x pos
    private readonly HashSet<WaveUi> _wavesPastMiddle = new();  // When a new ui element passed the middlepoint, add it to set

    // From interface
    public GameObject Object => gameObject;

    /// <summary>
    /// Initilize variables
    /// </summary>
    private void Start()
    {
        _containerXPos = container.anchoredPosition.x - container.sizeDelta.x * 0.5f;
        _middleXPos = middlePoint.position.x;
        _endOfNextWave.x = middlePoint.localPosition.x;
        GameObjectUpdateManager.Instance.AddObject(this);
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
        if (!_isFirstSpawn)
        {
            _endOfNextWave = new Vector2(_lastSpawnedWave.rect.width + _lastSpawnedWave.localPosition.x, 0);
        }

        // Move the ui element to position
        waveUi.WaveTransform.localPosition = _endOfNextWave;

        // Update lastSpawned RectTransform
        _lastSpawnedWave = waveUi.WaveTransform;
        _isFirstSpawn = false;

        // Add it to be updated
        _currentWaves.Add(waveUi);
    }

    /// <summary>
    /// Update wave positions
    /// </summary>
    public void UpdateObject()
    {
        if (_isAllowedToMove < 0)
        {
            return;
        }

        // Set the movement _moveOffset
        _moveOffset.x = -uiScaleFactor * Time.deltaTime;

        for (int i = _currentWaves.Count - 1; i >= 0; i--)
        {
            // Move the wave ui element
            WaveUi waveUi = _currentWaves[i];
            waveUi.WaveTransform.localPosition += _moveOffset;

            // Get the position of this wave ui element
            float waveUiXPos = waveUi.WaveTransform.position.x + waveUi.WaveTransform.sizeDelta.x;

            // Check if this element has passed the middle point and it has not already been checked
            if (waveUiXPos < _middleXPos && !_wavesPastMiddle.Contains(waveUi))
            {
                // Reduce _isAllowedToMove and add it to list
                _isAllowedToMove--;
                _wavesPastMiddle.Add(waveUi);
            }

            // Check if the wave ui elements right most side has moved out of the containers left most side
            if (waveUiXPos < _containerXPos)
            {
                waveUi.gameObject.SetActive(false);
                _currentWaves.Remove(waveUi);
                _wavesPastMiddle.Remove(waveUi);
            }
        }
    }

    /// <summary>
    /// Allow moving again
    /// </summary>
    public void ContinueUiMovement()
    {
        _isAllowedToMove++;
    }
}
