using System.Collections;
using UnityEngine;

public class BulletBeam : BulletProperties
{
    [SerializeField] private Rigidbody2D m_rb;
    private Transform _towerBarrel;
    public override void OnBulletFixedUpdate()
    {
        m_rb.MoveRotation(_towerBarrel.rotation);
    }

    public void SetTowerBarrel(Transform barrel)
    {
        _towerBarrel = barrel;
    }
}
