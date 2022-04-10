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

	private void SetCounter(int amt) => elementsCountText.text = amt.ToString();

	void Awake()
	{
		selectFolderButton?.onClick.AddListener(OnFindFilePath);
		refreshButton?.onClick.AddListener(OnRefresh);

		ShowHideDetails(false);
		imageReader.onCounterIncrease = x => SetCounter(x);
	}

	private void OnRefresh()
	{
		// This prevents us from clicking refresh when the data is being loaded
		if (!imageReader.IsLoading) 
        {
			SetCounter(0);
			imageReader.CheckFilePath();
		}
	}

	private void OnFindFilePath()
	{
		SetCounter(0);
		ShowHideDetails(imageReader.FindFilePath());
	}
		
	private void ShowHideDetails(bool state)
	{
		if (state) detailsGroup.Enable();
		else detailsGroup.Disable();
	}    
}
