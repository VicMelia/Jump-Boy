using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] float _speed = 5f;
    float _horizontalMovement;
    Rigidbody2D _rb;
    SpriteRenderer _spriteRenderer;
    bool _isRight = true;
    private Vector2 _lastVelocity;

    [Header("Jump Settings")]
    [SerializeField] float _minJumpForce = 3f;
    [SerializeField] float _maxJumpForce = 9f;
    [SerializeField] float _groundCheckDistance = 0.7f;
    [SerializeField] float _maxChargeTime = 1.5f;
    bool _isJumping;
    Vector2 _currentJumpDirection = Vector2.zero;

    [Header("Bounce Settings")]
    [SerializeField] private float _horizontalBounceMultiplier = 0.4f;
    [SerializeField] private float _verticalBounceMultiplier = 0.4f;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _spriteRenderer = GetComponentInChildren<SpriteRenderer>();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        bool isGrounded = IsGrounded();
        if(isGrounded && _isJumping && _rb.linearVelocity.y < 0) _isJumping = false; //Raycast reset when falling
        if (_isJumping) return; //Input blocked while jumping
        
        Move();
        if (isGrounded)
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                StartCoroutine(ChargeJump());
            }
        }

    }

    private void FixedUpdate()
    {
        _lastVelocity = _rb.linearVelocity;
        if (_isJumping) return;
        _rb.linearVelocity = new Vector2(_horizontalMovement * _speed, _rb.linearVelocity.y);
    }

    private void Move()
    {
        _horizontalMovement = Input.GetAxisRaw("Horizontal");
        if (_horizontalMovement > 0)
        {
            _spriteRenderer.transform.rotation = Quaternion.Euler(0, 0, 0);
            _isRight = true;
        }
        else if (_horizontalMovement < 0)
        {
            _spriteRenderer.transform.rotation = Quaternion.Euler(0, 180, 0);
            _isRight = false;
        }
    }

    private void Jump(float jumpForce)
    {
        _isJumping = true;
        _currentJumpDirection = new Vector2(jumpForce / 1.5f, jumpForce);
        if (!_isRight) _currentJumpDirection.x *= -1f; //left = inverted
        _rb.AddForce(_currentJumpDirection, ForceMode2D.Impulse);
    }

    private IEnumerator ChargeJump()
    {
        float elapsed = 0f;
        float jumpForce = 0f;
        while (Input.GetKey(KeyCode.Space))
        {
            if (elapsed < _maxChargeTime)
            {
                elapsed += Time.deltaTime;
                jumpForce = Mathf.Lerp(_minJumpForce, _maxJumpForce, elapsed);
            }
            else jumpForce = _maxJumpForce;
            yield return null;

        }

        Jump(jumpForce);
    }

    private bool IsGrounded()
    {
        return Physics2D.Raycast(transform.position, Vector2.down, _groundCheckDistance, LayerMask.GetMask("Ground"));
    }
    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawLine(
            transform.position,
            transform.position + Vector3.down * _groundCheckDistance
        );
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (IsGrounded()) return;
        Vector2 n = collision.GetContact(0).normal;
        if (n.y > 0.5f) return; //ground = nothing

        if (Mathf.Abs(n.x) > 0.5f) //wall = bounce
        {
            _rb.linearVelocity = new Vector2(
                -_lastVelocity.x * _horizontalBounceMultiplier,
                _lastVelocity.y * _verticalBounceMultiplier
            );
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("CameraTrigger"))
        {
            Camera cam = Camera.main;
            CameraManager.Instance.LerpCameraPosition(collision.transform.position);
        }
    }
}
