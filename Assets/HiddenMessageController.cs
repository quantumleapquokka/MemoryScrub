using UnityEngine;
using TMPro;

public class HiddenMessageController : MonoBehaviour
{
    public RectTransform canvasRect;
    public TextMeshProUGUI[] words; // assign your 5 word objects in the Inspector
    public float verticalSpacing = 10f; // adjust for line spacing

    void Start()
    {
        if (canvasRect == null)
            canvasRect = GetComponent<RectTransform>();
    }

    public void RevealWords()
    {
        // Center them vertically on screen
        float startY = (words.Length - 1) * 0.5f * verticalSpacing;

        for (int i = 0; i < words.Length; i++)
        {
            var rect = words[i].GetComponent<RectTransform>();
            rect.anchoredPosition = new Vector2(0, startY - i * verticalSpacing);

            words[i].alpha = 0;
            words[i].gameObject.SetActive(true);
            StartCoroutine(FadeInWord(words[i]));
        }
    }

    System.Collections.IEnumerator FadeInWord(TextMeshProUGUI word)
    {
        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime;
            word.alpha = Mathf.Lerp(0, 1, t);
            yield return null;
        }
    }
}
