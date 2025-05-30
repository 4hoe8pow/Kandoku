using UnityEngine;
using UnityEngine.UI;

public class CellBorder : MonoBehaviour
{
    public Image Top;
    public Image Bottom;
    public Image Left;
    public Image Right;

    public Color thinColor = new(0, 0, 0, 0.2f); // 薄い黒
    public Color thickColor = new(1.2f, 0.2f, 0.2f, 1f); // 濃い灰色
    public float thin = 1f;
    public float thick = 198f;

    public void SetBorder(bool thickTop, bool thickBottom, bool thickLeft, bool thickRight)
    {
        if (Top)
        {
            Top.rectTransform.sizeDelta = new Vector2(0, thickTop ? thick : thin);
            Top.color = thickTop ? thickColor : thinColor;
        }
        if (Bottom)
        {
            Bottom.rectTransform.sizeDelta = new Vector2(0, thickBottom ? thick : thin);
            Bottom.color = thickBottom ? thickColor : thinColor;
        }
        if (Left)
        {
            Left.rectTransform.sizeDelta = new Vector2(thickLeft ? thick : thin, 0);
            Left.color = thickLeft ? thickColor : thinColor;
        }
        if (Right)
        {
            Right.rectTransform.sizeDelta = new Vector2(thickRight ? thick : thin, 0);
            Right.color = thickRight ? thickColor : thinColor;
        }
    }
}