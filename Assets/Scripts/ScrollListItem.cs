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
    private Texture fileSprite;
    private string filePath;
    private string fileName;
    private string fileDate;

    public FileInfo FileInfo => fileInfo;
    public string FilePath => filePath;

    public void PopulateItem(Texture sprite, string path)
    {
        fileInfo = new FileInfo(path);
        filePath = path;
        
        fileDate = dateCreatedText.text = fileInfo.CreationTime.ToString();
        fileName = fileNameText.text = fileInfo.Name;
        fileSprite = fileImage.texture = sprite;
        
        OnNameChange(fileInfo);
        OnDateChange(fileInfo);
    }

    public void OnNameChange(FileInfo file)
    {
        if (file.Name == fileName) return;

        fileName = file.Name;
        fileNameText.text = fileName;
    }

    public void OnDateChange(FileInfo file)
    {
        if (file.CreationTime.ToString() == fileDate) return;

        fileDate = file.CreationTime.ToString();
        dateCreatedText.text = fileDate;
    }
}
