using UnityEngine;
using UnityEngine.UI;
using TMPro;

[RequireComponent(typeof(Button))]
public class InputKeyButton : MonoBehaviour
{
    public TMP_Text label;
    private Button button;
    private Image buttonImage;
    private static readonly Color normalColor = Color.white;
    private static readonly Color disabledColor = new(0.7f, 0.7f, 0.7f, 1f); // グレー

    private void Awake()
    {
        label = GetComponentInChildren<TMP_Text>();
        button = GetComponent<Button>();
        buttonImage = GetComponent<Image>();
        button.onClick.AddListener(OnClick);
    }

    public void SetInteractable(bool interactable)
    {
        if (button != null) button.interactable = interactable;
        if (buttonImage != null) buttonImage.color = interactable ? normalColor : disabledColor;
        if (label != null) label.color = interactable ? Color.black : new Color(0.4f, 0.4f, 0.4f, 1f);
    }

    private void OnClick()
    {
        var selected = CellSelectionManager.Instance.selectedCell;
        if (selected != null)
        {
            if (selected.label.text == label.text)
            {
                selected.SetSymbol(string.Empty); // 文字をクリア
            }
            else
            {
                selected.SetSymbol(label.text);
                // 盤面一時保存用のJson更新
                CellSelectionManager.Instance.SendMessage("SaveGameStateToPlayerPrefs", SendMessageOptions.DontRequireReceiver);
                // 入力キーの活性・非活性を即時更新
                CellSelectionManager.Instance.UpdateInputKeyButtonsInteractable();
            }
        }
    }
}
