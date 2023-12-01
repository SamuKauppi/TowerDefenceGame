using System.Collections;
using UnityEngine;

public class KillEffectTimer : MonoBehaviour
{
    [SerializeField] private GameObject ObjectToKill;
    private IEnumerator KillEffect(float time)
    {
        yield return new WaitForSeconds(time);
        if (ObjectToKill != null)
        {
            ObjectToKill.SetActive(false);
        }
        else
        {
            gameObject.SetActive(false);
        }
    }

    public void StartKillCountdown(float time)
    {
        StartCoroutine(KillEffect(time));
    }
}
