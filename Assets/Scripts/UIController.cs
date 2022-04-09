using UnityEngine.UI;
using UnityEngine;
using TMPro;

public class UIController : MonoBehaviour
{
    [SerializeField] private ImageReader imageReader;
    [SerializeField] private CanvasGroup detailsGroup;
    [SerializeField] private TMP_Text elementsCountText;
    [SerializeField] private Button selectFolderButton;
    [SerializeField] private Button refreshButton;

    public int ElementsAmount => imageReader.SpawnedItems.Count;

    void Awake()
    {
        selectFolderButton?.onClick.AddListener(OnFindFilePath);
        refreshButton?.onClick.AddListener(OnRefresh);

        ShowHideDetails(false);
    }

    private void OnRefresh()
    {
        imageReader.GetFiles();
        elementsCountText.text = ElementsAmount.ToString();
    }

    public void OnFindFilePath()
    {
        ShowHideDetails(true);
        imageReader.FindFilePath();
        elementsCountText.text = ElementsAmount.ToString();
    }

    private void ShowHideDetails(bool state)
    {
        if (state) detailsGroup.Enable();
        else detailsGroup.Disable();
    }    
}
