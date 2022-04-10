using UnityEngine.UI;
using UnityEngine;
using TMPro;

// Controlls UI buttons and canvas groups
public class UIController : MonoBehaviour
{
    [SerializeField] private ImageReader imageReader;
    [SerializeField] private CanvasGroup detailsGroup;
    [SerializeField] private TMP_Text elementsCountText;
    [SerializeField] private Button selectFolderButton;
    [SerializeField] private Button refreshButton;

    public int ItemsAmount => imageReader.SpawnedItems.Count;

    void Awake()
    {
        selectFolderButton?.onClick.AddListener(OnFindFilePath);
        refreshButton?.onClick.AddListener(OnRefresh);

        ShowHideDetails(false);
    }

    private void OnRefresh()
    {
        imageReader.CheckFilePath();
        elementsCountText.text = ItemsAmount.ToString();
    }

    public void OnFindFilePath()
    {
        ShowHideDetails(true);
        imageReader.FindFilePath();
        elementsCountText.text = ItemsAmount.ToString();
    }

    private void ShowHideDetails(bool state)
    {
        if (state) detailsGroup.Enable();
        else detailsGroup.Disable();
    }    
}
