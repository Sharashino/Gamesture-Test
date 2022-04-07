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
    private FileInfo fileInfo;
    private Sprite fileSprite;
    private string filePath;
    private string fileName;
    private string fileDate;

    public FileInfo FileInfo => fileInfo;
    public Sprite FileSprite => fileSprite;
    public string FilePath => filePath;
    public string FileName => fileName;
    public string FileDate => fileDate;

    public void PopulateItem(Sprite sprite, string path)
    {
        fileInfo = new FileInfo(path);
        filePath = path;
        
        fileDate = dateCreatedText.text = fileInfo.CreationTime.ToString();
        fileName = fileNameText.text = fileInfo.Name;
        fileSprite = fileImage.sprite = sprite;
        
        OnNameChange(fileInfo);
        OnDateChange(fileInfo);
    }

    public void RefreshSprite(Sprite sprite, string path)
    {
        OnSpriteChange(sprite);
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

    public void OnSpriteChange(Sprite newSprite)
    {
        if (newSprite == fileSprite) return;

        fileSprite = newSprite;
        fileImage.sprite = fileSprite;
    }
}
