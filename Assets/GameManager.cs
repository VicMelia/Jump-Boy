using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public Transform lastCheckpoint;

    public enum GameState { Intro, Playing, GameOver, Ending}
    public GameState state = GameState.Intro;
    bool intro = true;
    private void Awake()
    {
        Instance = this;
        StartCoroutine(IntroSequence());

    }
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

    }

    private IEnumerator IntroSequence()
    {
        state = GameState.Intro;
        StartCoroutine(UIManager.Instance.FadeIn(UIManager.Instance.jumpGirlCanvas, 1.5f));
        yield return new WaitForSeconds(1.5f);
        StartCoroutine(UIManager.Instance.FadeOut(UIManager.Instance.jumpGirlCanvas, 1.5f));
        yield return new WaitForSeconds(1.5f);
        StartCoroutine(UIManager.Instance.FadeIn(UIManager.Instance.helpGirlCanvas, 3f));
        yield return new WaitForSeconds(3f);
        StartCoroutine(UIManager.Instance.FadeOut(UIManager.Instance.helpGirlCanvas, 2f));
        yield return new WaitForSeconds(2f);
        StartCoroutine(UIManager.Instance.FadeIn(UIManager.Instance.titleCanvas, 2.5f));
        yield return new WaitForSeconds(2.5f);
        StartCoroutine(UIManager.Instance.FadeOut(UIManager.Instance.fadePanelCanvas, 3f));
        yield return new WaitForSeconds(2f);
        StartCoroutine(UIManager.Instance.FadeOut(UIManager.Instance.titleCanvas, 2f));
        yield return new WaitForSeconds(1f);
        state = GameState.Playing;


    }

    public void SetCheckpoint(Transform checkpoint)
    {
        lastCheckpoint = checkpoint;
    }
    public void LoadFromLastCheckpoint()
    {
        PlayerController pm = FindAnyObjectByType<PlayerController>();
        pm.GetComponentInChildren<SpriteRenderer>().enabled = true;
        pm.transform.position = lastCheckpoint.position;
    }

    public void GameOver()
    {
        state = GameState.GameOver;
        StartCoroutine(UIManager.Instance.FadeIn(UIManager.Instance.fadePanelCanvas, 1.5f));
        StartCoroutine(UIManager.Instance.FadeIn(UIManager.Instance.GameOverCanvas, 3.5f));
    }
    public void WinGame()
    {
        state = GameState.Ending;
        StartCoroutine(UIManager.Instance.FadeIn(UIManager.Instance.fadePanelCanvas, 1.5f));
        StartCoroutine(UIManager.Instance.FadeIn(UIManager.Instance.WinCanvas, 3.5f));
        StartCoroutine(ResetFromIntro());
    }

    private IEnumerator ResetFromIntro()
    {
        yield return new WaitForSeconds(3f);
        StartCoroutine(UIManager.Instance.FadeOut(UIManager.Instance.WinCanvas, 3.5f));
        SceneManager.LoadScene("Game");
    }

    public void RestartGame()
    {
        Debug.Log("Reinicio");
        state = GameState.Playing;
        StartCoroutine(UIManager.Instance.FadeOut(UIManager.Instance.GameOverCanvas, 1.5f));
        LoadFromLastCheckpoint();
        StartCoroutine(UIManager.Instance.FadeOut(UIManager.Instance.fadePanelCanvas, 3.5f));
    }
}
