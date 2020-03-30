using UnityEngine;
using UnityEngine.UI;

public class Alignment : MonoBehaviour
{
    [SerializeField] private RectTransform _scrollView;
    [SerializeField] private RectTransform _content;
    [SerializeField] private Button _buttonPrefab;
    
    private void ChooseAlignment()
    {
        if (_content.sizeDelta.x < _scrollView.sizeDelta.x)
            _content.pivot = new Vector2(0.5f, _content.pivot.y);
        else
            _content.pivot = new Vector2(0, _content.pivot.y);
    }

    public void AddButton()
    {
        Instantiate(_buttonPrefab, _content);
        ChooseAlignment();
    }

    public void RemoveButton()
    {
        Destroy(_content.GetChild(_content.childCount - 1).gameObject);
        ChooseAlignment();
    }
}
