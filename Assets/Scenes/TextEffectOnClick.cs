using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class TextEffectOnClick : MonoBehaviour
{
    public float duration = 0.5f;
    public float scaleMultiplier = 1.5f;

    private TextMeshProUGUI targetText;
    private Vector3 originalScale;
    private Color originalColor;

    void Start()
    {
        // 子コンポーネントから自動で取得
        targetText = GetComponentInChildren<TextMeshProUGUI>();
        if (targetText == null)
        {
            Debug.LogError("TextMeshProUGUI が見つかりません。Button の子に Text (TMP) を配置してください。");
            return;
        }

        originalScale = targetText.rectTransform.localScale;
        originalColor = targetText.color;

        // Button イベントに登録
        if (TryGetComponent<Button>(out var button))
        {
            button.onClick.AddListener(PlayEffect);
        }
        else
        {
            Debug.LogError("Button コンポーネントがこのオブジェクトに存在しません。");
        }
    }

    public void PlayEffect()
    {
        if (targetText == null) return;

        StopAllCoroutines();
        StartCoroutine(AnimateText());
    }

    private System.Collections.IEnumerator AnimateText()
    {
        float time = 0f;
        while (time < duration)
        {
            float t = time / duration;

            float scale = Mathf.Lerp(1f, scaleMultiplier, t);
            targetText.rectTransform.localScale = originalScale * scale;

            Color c = originalColor;
            c.a = Mathf.Lerp(1f, 0f, t);
            targetText.color = c;

            time += Time.deltaTime;
            yield return null;
        }

        targetText.rectTransform.localScale = originalScale * scaleMultiplier;
        Color finalColor = originalColor;
        finalColor.a = 0f;
        targetText.color = finalColor;
    }
}
