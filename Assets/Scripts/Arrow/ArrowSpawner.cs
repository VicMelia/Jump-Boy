using UnityEngine;
using System.Collections;

public class ArrowSpawner : Spawner
{
    [SerializeField] private float _spawnCd = 4.5f;

    private void Awake()
    {
        StartCoroutine(LaunchBomb());
    }

    private IEnumerator LaunchBomb()
    {
        yield return new WaitForSeconds(_spawnCd);
        GameObject arrow = Instantiate(_spawnPrefab, _spawnTransform.position, Quaternion.identity);
        StartCoroutine(LaunchBomb());
    }
}
