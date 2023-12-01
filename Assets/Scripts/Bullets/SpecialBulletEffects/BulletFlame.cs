using System.Collections;
using UnityEngine;

public class BulletFlame : BulletBeam
{
    [SerializeField] private Animator _animator;        // Animator for flame effect
    [SerializeField] private float _animationLength;    // Lifetime of the animation
    private float _animationTimer;                      // Timer for life spawnTimer
    private bool _isPlaying = false;

    private void Start()
    {
        // Reduce the animation spawnTimer for 0.5 seconds (length of the end animation) 
        _animationLength -= 0.5f;
    }
    public override void OnBulletSpawn()
    {
        _animator.SetBool("IsBurning", true);
        _animationTimer = 0f;
        _isPlaying = true;
    }

    public override void OnBulletUpdate()
    {
        if (!_isPlaying)
            return;

        _animationTimer += Time.deltaTime;
        if (_animationTimer > _animationLength)
        {
            _animator.SetBool("IsBurning", false);
            _isPlaying = false;
        }
    }
}
