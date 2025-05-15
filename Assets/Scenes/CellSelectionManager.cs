using UnityEngine;

public class CellSelectionManager : MonoBehaviour
{
    public static CellSelectionManager Instance { get; private set; }

    public AnswerCell selectedCell;

    private void Awake()
    {
        if (Instance != null) Destroy(gameObject);
        else Instance = this;
    }

    /// <summary>
    /// セルを選択状態にする。前のセルはハイライト解除。
    /// </summary>
    public void SelectCell(AnswerCell cell)
    {
        if (selectedCell == cell) return;

        if (selectedCell != null)
            selectedCell.SetSelected(false);

        selectedCell = cell;

        if (selectedCell != null)
            selectedCell.SetSelected(true);
    }

    /// <summary>
    /// 現在選択中のセルがあれば、その文字をセットする
    /// </summary>
    public void InputSymbol(string symbol)
    {
        if (selectedCell == null) return;
        selectedCell.SetSymbol(symbol);
    }
}
