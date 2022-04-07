using UnityEngine.UI;
using UnityEngine;
using TMPro;
using System.IO;

// Panel in scroll list, displays readed image, date and name
public class ScrollListItem : MonoBehaviour
{
    [SerializeField] private TMP_Text dateCreatedText = null;
    [SerializeField] private TMP_Text fileNameText = null;
    [SerializeField] private Image fileImage = null;
    private string filePath;

    public string FilePath => filePath;

    public void OnRefresh(Sprite image, string path)
    {
        var file = new FileInfo(path);
        filePath = path;

        dateCreatedText.text = file.CreationTime.ToString();
        fileNameText.text = file.Name;
        fileImage.sprite = image;
    }
}
