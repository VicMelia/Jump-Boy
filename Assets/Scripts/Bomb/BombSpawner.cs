using UnityEngine;
using System.Collections;

public class BombSpawner : Spawner
{
    [SerializeField] private float _launchForce = 5f;
    [SerializeField] private float _spawnCd = 4.5f;
    [SerializeField] private float _bombTimer = 2f;

    private void Awake()
    {
        StartCoroutine(LaunchBomb());
    }

    private IEnumerator LaunchBomb()
    {
        yield return new WaitForSeconds(_spawnCd);
        GameObject bomb = Instantiate(_spawnPrefab, _spawnTransform.position, Quaternion.identity);
        Rigidbody2D rb = bomb.GetComponent<Rigidbody2D>();
        rb.AddForce(_spawnTransform.right * _launchForce, ForceMode2D.Impulse);
        Bomb b = bomb.GetComponent<Bomb>();
        b.SetBombTimer(_bombTimer);
        StartCoroutine(LaunchBomb());
    }
}
