using UnityEngine;
using TMPro;

public class BackgroundScatterSymbols : MonoBehaviour
{
    [SerializeField] private int symbolCount = 100;
    [SerializeField] private TMP_Text symbolPrefab;

    private static readonly string[] Symbols = new string[]
    {
        "臨", "兵", "闘", "者", "皆", "陣", "烈", "在", "前"
    };

    [System.Obsolete]
    void Start()
    {
        // 1. Canvas取得
        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            Debug.LogError("Canvas がシーン内に存在しません。");
            return;
        }

        // 2. ScatterArea を動的生成
        GameObject scatterAreaGO = new("ScatterArea", typeof(RectTransform));
        scatterAreaGO.transform.SetParent(canvas.transform, false);
        // Canvas直下の一番上に配置
        scatterAreaGO.transform.SetSiblingIndex(0);

        RectTransform scatterArea = scatterAreaGO.GetComponent<RectTransform>();
        scatterArea.anchorMin = Vector2.zero;
        scatterArea.anchorMax = Vector2.one;
        scatterArea.offsetMin = Vector2.zero;
        scatterArea.offsetMax = Vector2.zero;
        scatterArea.pivot = new Vector2(0.5f, 0.5f);

        // 3. Symbol を散布
        for (int i = 0; i < symbolCount; i++)
        {
            SpawnRandomSymbol(scatterArea);
        }
    }

    private void SpawnRandomSymbol(RectTransform scatterArea)
    {
        var instance = Instantiate(symbolPrefab, scatterArea);
        var rect = instance.GetComponent<RectTransform>();

        instance.text = Symbols[Random.Range(0, Symbols.Length)];

        // ランダム位置（中心からの相対）
        float width = scatterArea.rect.width;
        float height = scatterArea.rect.height;
        rect.anchorMin = rect.anchorMax = new Vector2(0, 0);
        rect.pivot = new Vector2(0.5f, 0.5f);

        Vector2 randPos = new(
            Random.Range(0f, width),
            Random.Range(0f, height)
        );
        rect.anchoredPosition = randPos;

        // ランダム回転
        float angle = Random.Range(0f, 360f);
        rect.localRotation = Quaternion.Euler(0, 0, angle);

        // ランダムサイズ
        float scale = Random.Range(0.5f, 1.5f);
        rect.localScale = new Vector3(scale, scale, 1f);

        // ランダム色（透明度あり）
        Color color = new Color(
            Random.Range(0.6f, 1f),
            Random.Range(0.6f, 1f),
            Random.Range(0.6f, 1f),
            Random.Range(0.2f, 0.6f)
        );
        instance.color = color;
    }
}
