using System.Collections;
using UnityEngine;

public class BulletBeam : BulletProperties
{
    [SerializeField] private Rigidbody2D m_rb;
    private Transform _towerBarrel;
    private Quaternion _rotation;
    private float _angleOffset;
    public override void OnBulletFixedUpdate()
    {
        _rotation = _towerBarrel.rotation * Quaternion.Euler(0f, 0f, _angleOffset);
        m_rb.MoveRotation(_rotation);
    }

    public void SetTowerBarrel(Transform barrel, float angleOffset)
    {
        _towerBarrel = barrel;
        _angleOffset = angleOffset;
    }
}
