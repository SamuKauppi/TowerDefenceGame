using UnityEngine;

public class BulletBoomerang : BulletProperties
{
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private float initialRotation = -15f;
    [SerializeField] private float rotationSpeed = 90f;
    [SerializeField] private float rotaionIncrease = 720f;
    [SerializeField] private float clampRotation = 450f;

    private float stratingRotationSpeed;
    private void Start()
    {
        stratingRotationSpeed = rotationSpeed;
    }
    public override void OnBulletSpawn()
    {
        base.OnBulletSpawn();
        transform.Rotate(0, 0, initialRotation);
    }
    public override void OnBulletFixedUpdate()
    {
        IncreaseRotationSpeed();
        rb.MoveRotation(rb.rotation + Time.fixedDeltaTime * rotationSpeed);
    }

    private void IncreaseRotationSpeed()
    {
        rotationSpeed += rotaionIncrease * Time.fixedDeltaTime;
        rotationSpeed = Mathf.Clamp(rotationSpeed, 0, clampRotation);
    }

    public override void OnBulletDespawn()
    {
        rotationSpeed = stratingRotationSpeed;
    }
}
