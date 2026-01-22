using UnityEngine;

public class Arrow : MonoBehaviour
{
    [SerializeField] float _speed = 5f;
    [SerializeField] Vector2 _travelDirection;
    Rigidbody2D _rb;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        _rb.linearVelocity = _travelDirection * _speed;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if(!collision.collider.CompareTag("Player"))
        {
            Destroy(gameObject);
        }
    }
}
