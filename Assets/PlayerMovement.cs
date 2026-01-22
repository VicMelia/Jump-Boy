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
    Rigidbody2D _groundRb;

    [Header("Jump Settings")]
    [SerializeField] float _minJumpForce = 3f;
    [SerializeField] float _maxJumpForce = 9f;
    [SerializeField] float _groundCheckDistance = 0.7f;
    [SerializeField] float _maxChargeTime = 1.5f;
    bool _isJumping;
    bool _isChargingJump;
    Vector2 _currentJumpDirection = Vector2.zero;

    [Header("Bounce Settings")]
    [SerializeField] private float _horizontalBounceMultiplier = 0.4f;
    [SerializeField] private float _verticalBounceMultiplier = 0.4f;

    [Header("Ice Settings")]
    [SerializeField] private float _groundAccel = 35f;
    [SerializeField] private float _groundDecel = 45f;

    [SerializeField] private float _iceAccel = 10f;
    [SerializeField] private float _iceDecel = 2f;
    bool _isOnIce;

    [Header("Teleport Settings")]
    [SerializeField] private float _teleportCooldown = 0.5f;
    bool _canTeleport = true;

    [Header("Reset Settings")]
    [SerializeField] private float _maxResetTimer = 2f;

    [Header("Bombs Settings")]
    [SerializeField] private float _bombForce = 10f;


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
        if (Input.GetKeyDown(KeyCode.R)) //Reset from last checkpoint
        {
            if(GameManager.Instance.lastCheckpoint != null) StartCoroutine(ResetFromCheckpoint());
        }

        bool isGrounded = IsGrounded();
        if(isGrounded && _isJumping && _rb.linearVelocity.y < 0) _isJumping = false; //Raycast reset when falling
        if (_isJumping) return; //Input blocked while jumping

        if (isGrounded)
        {
            Move();
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
        _speed = _isOnIce ? 15f : 5f;
        float targetX = _horizontalMovement * _speed;
        float accel;
        if (targetX != 0) //acceleration
        {
            accel = _isOnIce ? _iceAccel : _groundAccel;
        }
        else //deceleration
        {
            accel = _isOnIce ? _iceDecel : _groundDecel;
        }
        float platformX = (_groundRb != null) ? _groundRb.linearVelocity.x : 0f;
        float newX = Mathf.MoveTowards(_rb.linearVelocity.x, targetX + platformX, accel * Time.fixedDeltaTime);
        _rb.linearVelocity = new Vector2(newX, _rb.linearVelocity.y);
    }

    private void Move()
    {
        _horizontalMovement = Input.GetAxisRaw("Horizontal");
        if (_isChargingJump) _horizontalMovement = 0f;
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
        _isChargingJump = true;
        float elapsed = 0f;
        float jumpForce = 0f;
        if (_isOnIce) _maxJumpForce *= 1.5f; //On ice jumps higher
        while (Input.GetKey(KeyCode.Space))
        {
            if(!IsGrounded()) StopCoroutine(ChargeJump()); //TO DO: Check if this works
            if (elapsed < _maxChargeTime)
            {
                elapsed += Time.deltaTime;
                jumpForce = Mathf.Lerp(_minJumpForce, _maxJumpForce, elapsed);
            }
            else jumpForce = _maxJumpForce;
            yield return null;

        }

        _isChargingJump = false;
        Jump(jumpForce);
    }

    private IEnumerator ResetFromCheckpoint()
    {
        float elapsed = 0f;
        while (Input.GetKey(KeyCode.R))
        {
          
            if (elapsed < _maxResetTimer)
            {
                elapsed += Time.deltaTime;
               
            }
            if (elapsed >= _maxResetTimer)
            {
                GameManager.Instance.LoadFromLastCheckpoint();
                break;
            }
            yield return null;
  
        }

    }

    private bool IsGrounded()
    {
        if (Mathf.Abs(_rb.linearVelocity.y) < 0.01f) return true;
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

    private void OnCollisionStay2D(Collision2D col)
    {
        for (int i = 0; i < col.contactCount; i++) //Detects ground rb
        {
            if (col.GetContact(i).normal.y > 0.5f)
            {
                _groundRb = col.rigidbody;
                return;
            }
        }
    }

    private void OnCollisionExit2D(Collision2D col)
    {
        if (col.rigidbody == _groundRb) _groundRb = null;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Ice"))
        {
            _isOnIce = true;
        }
        if (collision.CompareTag("CameraTrigger"))
        {
            Camera cam = Camera.main;
            CameraManager.Instance.LerpCameraPosition(collision.transform.position);
        }
        if (_canTeleport && collision.CompareTag("Portal")) //tp to next portal
        {
            _canTeleport = false;
            transform.position = collision.GetComponent<PortalConnector>().nextPortal.position;
            StartCoroutine(ResetTeleportTimer());
        }

        if (collision.CompareTag("Checkpoint")) //sets checkpoint
        {
            GameManager.Instance.SetCheckpoint(collision.transform);
        }


        if (collision.CompareTag("Explosion")) //explosion colision
        {
            Vector3 launchDirection = transform.position - collision.transform.position;
            _rb.AddForce(launchDirection.normalized * _bombForce, ForceMode2D.Impulse);
        }
    }


    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Ice"))
        {
            _isOnIce = false;
        }
    }

    private IEnumerator ResetTeleportTimer()
    {
        yield return new WaitForSeconds(_teleportCooldown);
        _canTeleport = true;
    }
}
