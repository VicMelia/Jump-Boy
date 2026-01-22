using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public Transform lastCheckpoint;
    private void Awake()
    {
        Instance = this;
    }
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }


    public void SetCheckpoint(Transform checkpoint)
    {
        lastCheckpoint = checkpoint;
    }
    public void LoadFromLastCheckpoint()
    {
        PlayerMovement pm = GameObject.FindAnyObjectByType<PlayerMovement>();
        pm.transform.position = lastCheckpoint.position;
    }
}
