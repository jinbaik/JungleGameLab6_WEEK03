using TMPro;
using UnityEngine;

public class ResourceManager : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _resourceText;

    private int _currentResourceData = 0;

    public void UpdateResourceData()
    {
        _currentResourceData++;
        _resourceText.text = $"{_currentResourceData}";
    }
}
