using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

[RequireComponent(typeof(CanvasGroup))]
public class RainbowWave : MonoBehaviour
{
    [Tooltip("虹色の回転速度（360度/秒）")]
    public float hueSpeed = 555f;
    [Tooltip("波の長さ (同時に色が切り替わるボタン数)")]
    public int waveLength = 10;
    [Tooltip("彩度")]
    [Range(0f, 1f)]
    public float saturation = 0.62f;
    [Tooltip("明度")]
    [Range(0f, 1f)]
    public float value = 0.38f;

    private List<Image> images;

    // 外部から呼び出してImageリストを再取得
    public void RefreshImages()
    {
        images = new List<Image>(GetComponentsInChildren<Image>(includeInactive: true));
    }

    // アニメーションを進める関数
    public void AnimateRainbow(float time)
    {
        if (images == null) return;

        float baseHue = time * hueSpeed * 0.00277778f % 1f;
        int count = images.Count;

        for (int i = 0; i < count; i++)
        {
            float phase = (float)i / waveLength;
            float hue = (baseHue + phase) % 1f;
            images[i].color = Color.HSVToRGB(hue, saturation, value);
        }
    }
}
