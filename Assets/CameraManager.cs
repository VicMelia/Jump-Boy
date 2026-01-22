using UnityEngine;

public class CameraManager : MonoBehaviour
{
    public static CameraManager Instance;
    bool _lerping;

    //Lerp
    [SerializeField] float lerpTime = 2f;
    Vector2 _currentDestination;

    private void Awake()
    {
        Instance = this;

    }
    // Update is called once per frame
    void Update()
    {
        if (_lerping)
        {
            transform.position = Vector3.Lerp(transform.position, _currentDestination, lerpTime);
            transform.position = new Vector3(transform.position.x, transform.position.y, -10f);
            if(Vector2.Distance(transform.position, _currentDestination) < 0.01) _lerping = false;
        }
    }

    public void LerpCameraPosition(Vector3 destination)
    {
        _currentDestination = destination;
        _lerping = true;
    }


}
