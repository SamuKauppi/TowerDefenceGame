using System.Collections;
using UnityEngine;
public class EnemyRegen : Enemy, ISpecialAbility
{
    [SerializeField] private float healthRegen;

    /// <summary>
    /// Deal minus damage to enemy to gain health every frame
    /// </summary>
    /// <returns></returns>
    private IEnumerator StartHealthRegen()
    {
        while (true)
        {
            TakeDamage(-healthRegen * Time.deltaTime);
            yield return null;
        }
    }

    public void ActivateAbility()
    {
        StartCoroutine(StartHealthRegen());
    }

}
