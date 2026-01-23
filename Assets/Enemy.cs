using UnityEngine;

public class Enemy : MonoBehaviour
{
    [SerializeField] float _speed = 2f;
    [SerializeField] Vector2 _moveDirection = Vector2.right;
    [SerializeField] float _maxDistance = 3f;

    Animator _anim;
    Rigidbody2D _rb;
    SpriteRenderer _spriteRenderer;
    Vector2 _startPosition;
    bool _isDead;

    private void Awake()
    {
        _anim = GetComponent<Animator>();
        _rb = GetComponent<Rigidbody2D>();
        _startPosition = _rb.position;
        _spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void FixedUpdate()
    {
        if (_isDead) {
            _rb.linearVelocity = Vector2.zero;
            return;

        }
        _rb.linearVelocity = _moveDirection * _speed;

        float distance = Vector2.Distance(_rb.position, _startPosition);

        if (distance >= _maxDistance)
        {
            _moveDirection *= -1f;
            if(_moveDirection.x < 0f) _spriteRenderer.flipX = true;
            else _spriteRenderer.flipX = false;

            _startPosition = _rb.position;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (_isDead || GameManager.Instance.state == GameManager.GameState.GameOver) return;
        if (collision.gameObject.CompareTag("Player"))
        {
            PlayerController pc = collision.gameObject.GetComponent<PlayerController>();
            pc.Die();
        }
    }

    public void Die()
    {
        _isDead = true;
        _anim.SetBool("Dead", true);
        Destroy(gameObject, 1f);
       
    }
}
