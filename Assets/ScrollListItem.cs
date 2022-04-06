using UnityEngine.UI;
using UnityEngine;
using TMPro;
using System.IO;

// Panel in scroll list, displays readed image, date and name
public class ScrollListItem : MonoBehaviour
{
    [SerializeField] private TMP_Text dateCreatedText = null;
    [SerializeField] private TMP_Text fileNameText = null;
    [SerializeField] private RawImage fileImage = null;
    private FileInfo fileInfo;

    public FileInfo FileInfo => fileInfo;

    public void OnRefreshAll(Texture image, FileInfo file)
    {
        fileInfo = file;
        
        dateCreatedText.text = file.CreationTime.ToString();
        fileNameText.text = file.Name;
        fileImage.texture = image;
    }
}
