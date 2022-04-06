using UnityEngine.UI;
using System.Linq;
using UnityEngine;
using System.IO;

// Reads image data from given path
public class ImageReader : MonoBehaviour
{
    [SerializeField] private InputField pathInputField;
    [SerializeField] private Button refreshButton;
    [SerializeField] private ScrollListItem item;
    [SerializeField] private Transform holder;
    [SerializeField] private string folderPath;

    void Awake()
    {
        refreshButton?.onClick.AddListener(RefreshImageData);
    }

    private void RefreshImageData()
    {
        if (!Directory.Exists(folderPath) || string.IsNullOrEmpty(folderPath))
        {
            Debug.Log("ImageReader - Path doesnt exist or is empty!");
            return;
        }

        var files = new DirectoryInfo(folderPath).GetFiles().Where(x => x.Name.EndsWith(".png") || x.Name.EndsWith(".jpg") || x.Name.EndsWith(".jpeg")).ToList();

        foreach (var file in files)
        {
            Debug.Log($"{file.Name} | {file.CreationTime}");

            byte[] byteArray = File.ReadAllBytes(file.FullName);
            Texture2D sampleTexture = new Texture2D(2, 2);

            if (sampleTexture.LoadImage(byteArray))
            {
                var newItem = Instantiate(item, holder);
                newItem.OnRefresh(sampleTexture, file.Name, file.CreationTime.ToShortDateString());
            }
        }
    }
}
