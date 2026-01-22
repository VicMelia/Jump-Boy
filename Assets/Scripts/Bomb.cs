using System.Collections;
using UnityEngine;

public class Bomb : MonoBehaviour
{
    [SerializeField] private float _explosionCd = 2f;
    [SerializeField] private ParticleSystem _particleSystem;

    [SerializeField] private CircleCollider2D _circleTrigger;
    SpriteRenderer _spriteRenderer;
    Rigidbody2D _rb;
    bool _exploding;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _rb = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void Update()
    {
        if(!_exploding) transform.Rotate(0, 0, 30 * Time.deltaTime);
    }

    private IEnumerator WaitForExplosion()
    {
        yield return new WaitForSeconds(_explosionCd);
        Explosion();
    }

    public void SetBombTimer(float t)
    {
        _explosionCd = t;
        StartCoroutine(WaitForExplosion());
    }
    private void Explosion()
    {
        _exploding = true;
        _rb.linearVelocity = Vector2.zero;
        _rb.gravityScale = 0;
        _rb.freezeRotation = true;
        _spriteRenderer.enabled = false;
        _circleTrigger.gameObject.SetActive(true);
        _particleSystem.Play();
        Destroy(gameObject, 1.2f);
    }
}
