using UnityEngine;

public class BackgroundManager : MonoBehaviour
{
    [SerializeField] private GameObject _bg1;
    [SerializeField] private GameObject _bg2;
    [SerializeField] private GameObject _bg3;
    [SerializeField] private Transform _playerTransform;

    private void Awake()
    {
        _playerTransform = GameObject.Find("Player").transform;
    }
    void Update()
    {
        if (_playerTransform.position.y < 23f)
        {
            _bg1.SetActive(true);
            _bg2.SetActive(false);
            _bg3.SetActive(false);
        }
        else if (_playerTransform.position.y >= 22f && _playerTransform.position.y < 50.3f)
        {
            _bg1.SetActive(false);
            _bg2.SetActive(true);
            _bg3.SetActive(false);
        }
        else
        {
            _bg1.SetActive(false);
            _bg2.SetActive(false);
            _bg3.SetActive(true);
        }
    }
}
