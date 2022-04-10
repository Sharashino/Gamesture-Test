using UnityEngine.UI;
using UnityEngine;
using System.IO;
using TMPro;

// Panel in scroll list, displays readed image, date and name
public class ScrollListItem : MonoBehaviour
{
	[SerializeField] private TMP_Text dateCreatedText = null;
	[SerializeField] private TMP_Text fileNameText = null;
	[SerializeField] private RawImage fileImage = null;
	private FileInfo fileInfo;

	public FileInfo FileInfo => fileInfo;
	public string FilePath => fileInfo?.FullName;

	// Filling item with data from file
	public void PopulateItem(Texture texture, string path)
	{
		fileInfo = new FileInfo(path);

		dateCreatedText.text = fileInfo.CreationTime.ToString();
		fileNameText.text = fileInfo.Name;
		fileImage.texture = texture;
	}
}