using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Framerate : MonoBehaviour
{
    [SerializeField] private TMP_Text m_Text;
    int frameRate;
    float timer = 0f;
    void Update()
    {
        timer += Time.deltaTime;
        if (timer > 0.25f)
        {
            frameRate = Mathf.FloorToInt(1f / Time.deltaTime);
            m_Text.text = frameRate.ToString();
            timer = 0f;
        }

    }
}
