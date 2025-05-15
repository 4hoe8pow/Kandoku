using UnityEngine;
using UnityEngine.UI;
using TMPro;

[RequireComponent(typeof(Button))]
public class InputKeyButton : MonoBehaviour
{
    private TMP_Text label;

    private void Awake()
    {
        label = GetComponentInChildren<TMP_Text>();
        GetComponent<Button>().onClick.AddListener(OnClick);
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
            }
        }
    }
}
