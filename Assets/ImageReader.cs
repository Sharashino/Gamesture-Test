using UnityEngine.UI;
using System.Linq;
using UnityEngine;
using System.IO;
using TMPro;
using System.Collections.Generic;

// Reads image data from given path
public class ImageReader : MonoBehaviour
{
    [SerializeField] private TMP_InputField pathInputField;
    [SerializeField] private Button refreshButton;
    [SerializeField] private ScrollListItem item;
    [SerializeField] private Transform holder;
    [SerializeField] private string folderPath;

    [SerializeField] private List<ScrollListItem> spawnedItems = new List<ScrollListItem>();

    public bool IsPathCorrect => Directory.Exists(folderPath) || !string.IsNullOrEmpty(folderPath);

    void Awake()
    {
        refreshButton?.onClick.AddListener(RefreshImageData);
    }

    private void RefreshImageData()
    {
        if (!IsPathCorrect)
        {
            Debug.Log("ImageReader - Path doesnt exist or is empty!");
            return;
        }

        var files = new DirectoryInfo(folderPath).GetFiles().Where(x => x.Name.EndsWith(".png") || x.Name.EndsWith(".jpg") || x.Name.EndsWith(".jpeg")).ToList();

        foreach (var file in files)
        {
            if (spawnedItems.Where(x => x.FileInfo.Length == file.Length && x.FileInfo.Name == file.Name).Any())
            {
                Debug.Log("Found matching file -> skipping!");
                continue;
            }

            var newItem = Instantiate(item, holder);
            newItem.OnRefreshAll(LoadImage(file), file);
            spawnedItems.Add(newItem);
        }
    }

    private Texture LoadImage(FileInfo file)
    {
        byte[] byteArray = File.ReadAllBytes(file.FullName);
        Texture2D sampleTexture = new Texture2D(2, 2);

        if (sampleTexture.LoadImage(byteArray))
        {
            return sampleTexture;
        }

        return null;
    }
}


