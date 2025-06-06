using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

[RequireComponent(typeof(Button))]
public class AnswerCell : MonoBehaviour, IPointerClickHandler
{
    public TMP_Text label;
    private Image buttonImage;

    public int row;
    public int col;

    private static readonly Color normalColor = Color.white;
    private static readonly Color selectedColor = new(1f, 1f, 0.5f, 1f); // 薄めの黄色系

    private void Awake()
    {
        // Button内の Text を取得
        label = GetComponentInChildren<TMP_Text>();
        buttonImage = GetComponent<Image>(); // Button自体のImage
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        // すでに選択されているセルを再度クリックした場合は選択解除
        if (CellSelectionManager.Instance.selectedCell == this)
        {
            CellSelectionManager.Instance.SelectCell(null);
        }
        else
        {
            CellSelectionManager.Instance.SelectCell(this);
        }
    }

    public void SetSelected(bool isSelected)
    {
        if (buttonImage != null)
            buttonImage.color = isSelected ? selectedColor : normalColor;
        // セル選択時に入力キーの活性・非活性を更新
        CellSelectionManager.Instance?.UpdateInputKeyButtonsInteractable();
    }

    public void SetSymbol(string symbol)
    {
        if (label != null)
            label.text = symbol;

        var manager = CellSelectionManager.Instance;
        if (manager != null && manager.currentBoard != null)
        {
            // row/colプロパティを利用
            manager.currentBoard[row, col] = symbol;

            // UIProblemMapper参照をCellSelectionManager経由で取得
            var mapper = manager.ProblemMapper;
            if (mapper != null)
                mapper.CheckSolved();
        }
    }
}
