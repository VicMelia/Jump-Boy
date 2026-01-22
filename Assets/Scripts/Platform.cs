using UnityEngine;

public class Platform : MonoBehaviour
{
    [SerializeField] float _speed = 2f;
    [SerializeField] Vector2 _travelDirection = Vector2.right;
    [SerializeField] float _maxDistance = 3f;

    Rigidbody2D _rb;
    Vector2 _startPosition;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _startPosition = _rb.position;
    }

    private void FixedUpdate()
    {
        _rb.linearVelocity = _travelDirection * _speed;

        float distance = Vector2.Distance(_rb.position, _startPosition);

        if (distance >= _maxDistance)
        {
            _travelDirection *= -1f;
            _startPosition = _rb.position;
        }
    }
}
