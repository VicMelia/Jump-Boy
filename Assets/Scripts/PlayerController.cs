using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] float _speed = 5f;
    float _horizontalMovement;
    Rigidbody2D _rb;
    SpriteRenderer _spriteRenderer;
    bool _isRight = true;
    private Vector2 _lastVelocity;
    Rigidbody2D _groundRb;
    [SerializeField] private ParticleSystem _deathParticleSystem;
    Animator _anim;
    [Header("Camera Settings")]
    [SerializeField] private float _cameraCooldownTime = 0.1f;
    bool _cameraOnCooldown;
    private readonly List<Collider2D> _overlappedCameraTriggers = new List<Collider2D>();
    private Collider2D _activeCameraTrigger = null;

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

    [Header("Attack Settings")]
    [SerializeField] private Transform _attackTransform;
    [SerializeField] private float _attackRadius = 1f;
    [SerializeField] private LayerMask _enemyMask;
    [SerializeField] private float _cdTimer = 0f;
    [SerializeField] private float _cdTime = 0.5f;

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

    [Header("Sound Clip Settings")]
    [SerializeField] private AudioClip _jumpClip;
    [SerializeField] private AudioClip _bounceClip;
    [SerializeField] private AudioClip _dieClip;


    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        _anim = GetComponent<Animator>();
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
        if (GameManager.Instance.state != GameManager.GameState.Playing) return;

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
        //Attack 
        if (_cdTimer <= 0)
        {
            if (isGrounded && Input.GetKey(KeyCode.F))
            {
                Attack();
            }
        }
        else
        {
            _cdTimer -= Time.deltaTime;
        }

    }

    private void FixedUpdate()
    {
        if (GameManager.Instance.state != GameManager.GameState.Playing) 
        {
            _rb.linearVelocity = Vector2.zero;
            return;
        } 
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

    private void LateUpdate()
    {
        _anim.SetBool("Grounded", IsGrounded());
        if(GameManager.Instance.state == GameManager.GameState.Playing) _anim.SetFloat("Speed", Mathf.Abs(Input.GetAxisRaw("Horizontal")));
        _anim.SetFloat("YVelocity", _rb.linearVelocity.y);

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
        SFXManager.Instance.PlaySFX(_jumpClip, transform.position, 0.3f);
        _isJumping = true;
        _anim.SetTrigger("Jump");
        _currentJumpDirection = new Vector2(jumpForce / 1.5f, jumpForce);
        if (!_isRight) _currentJumpDirection.x *= -1f; //left = inverted
        _rb.AddForce(_currentJumpDirection, ForceMode2D.Impulse);
    }

    private IEnumerator ChargeJump()
    {
        _isChargingJump = true;
        _anim.SetTrigger("Charge");
        float elapsed = 0f;
        float jumpForce = 0f;
        if (_isOnIce) _maxJumpForce = 20f; //On ice jumps higher
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

    private void Attack()
    {
        _anim.SetTrigger("Attack");

        Collider2D[] enemiesInRange = Physics2D.OverlapCircleAll(_attackTransform.position, _attackRadius, _enemyMask);
        foreach (var enemy in enemiesInRange)
        {
            enemy.GetComponent<Enemy>().Die();
        }

        _cdTimer = _cdTime;
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
                Debug.Log("Hago algo");
                _isJumping = false;
                _isChargingJump = false;
                if (GameManager.Instance.state == GameManager.GameState.GameOver) GameManager.Instance.RestartGame();
                else GameManager.Instance.LoadFromLastCheckpoint();
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

        Gizmos.DrawWireSphere(_attackTransform.position, _attackRadius);
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
            SFXManager.Instance.PlaySFX(_bounceClip, transform.position, 0.3f);
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
            if (!_overlappedCameraTriggers.Contains(collision))
                _overlappedCameraTriggers.Add(collision);

            TryUpdateActiveCameraTrigger(); 
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

        if (GameManager.Instance.state != GameManager.GameState.GameOver && collision.CompareTag("Death"))
        {
            Die();
        }

        if (collision.CompareTag("Win"))
        {
            GameManager.Instance.WinGame();
        }
    }


    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Ice"))
        {
            _isOnIce = false;
        }

        if (collision.CompareTag("CameraTrigger"))
        {
            _overlappedCameraTriggers.Remove(collision);

            if (_activeCameraTrigger == collision) //new camera
            {
                _activeCameraTrigger = null;
                TryUpdateActiveCameraTrigger();
            }
        }
    }

    private IEnumerator ResetTeleportTimer()
    {
        yield return new WaitForSeconds(_teleportCooldown);
        _canTeleport = true;
    }

    public void Die()
    {
        SFXManager.Instance.PlaySFX(_dieClip, transform.position, 0.2f);
        MusicManager.Instance.StopMusic();
        _deathParticleSystem.Play();
        _spriteRenderer.enabled = false;
        GameManager.Instance.GameOver();
    }

    private void TryUpdateActiveCameraTrigger()
    {
        if (_cameraOnCooldown) return;
        _overlappedCameraTriggers.RemoveAll(t => t == null || !t.gameObject.activeInHierarchy);
        if (_overlappedCameraTriggers.Count == 0) return;

        Collider2D best = null;
        float bestDist = float.MaxValue;
        Vector2 p = transform.position;

        foreach (var t in _overlappedCameraTriggers) //distance to collider
        {
            Vector2 closest = t.ClosestPoint(p);
            float d = (closest - p).sqrMagnitude;
            if (d < bestDist)
            {
                bestDist = d;
                best = t;
            }
        }

        if (best != null && best != _activeCameraTrigger)
        {
            _activeCameraTrigger = best;
            CameraManager.Instance.LerpCameraPosition(best.transform.position);
            StartCoroutine(CameraTriggerCooldown());
        }
    }

    private IEnumerator CameraTriggerCooldown()
    {
        _cameraOnCooldown = true;
        yield return new WaitForSeconds(_cameraCooldownTime);
        _cameraOnCooldown = false;
        TryUpdateActiveCameraTrigger();
    }


}
