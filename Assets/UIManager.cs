using UnityEngine;
using System.Collections;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;
    //Intro elements
    public CanvasGroup jumpGirlCanvas;
    public CanvasGroup helpGirlCanvas;
    public CanvasGroup titleCanvas;

    //Canvas
    public CanvasGroup fadePanelCanvas;
    public CanvasGroup GameOverCanvas;
    public CanvasGroup WinCanvas;

    private void Awake()
    {
        Instance = this;
    }

    public IEnumerator FadeIn(CanvasGroup canvasGroup, float duration)
    {
        canvasGroup.alpha = 0f;
        canvasGroup.gameObject.SetActive(true);

        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(0f, 1f, t / duration);
            yield return null;
        }

        canvasGroup.alpha = 1f;
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;
    }
    public IEnumerator FadeOut(CanvasGroup canvasGroup, float duration)
    {
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;

        float startAlpha = canvasGroup.alpha;
        float t = 0f;

        while (t < duration)
        {
            t += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(startAlpha, 0f, t / duration);
            yield return null;
        }

        canvasGroup.alpha = 0f;
        canvasGroup.gameObject.SetActive(false);
    }
}
