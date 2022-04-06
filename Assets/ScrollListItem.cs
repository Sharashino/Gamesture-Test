using UnityEngine.UI;
using UnityEngine;
using TMPro;

// Panel in scroll list, displays readed image, date and name
public class ScrollListItem : MonoBehaviour
{
    [SerializeField] private TMP_Text dateCreatedText = null;
    [SerializeField] private TMP_Text fileNameText = null;
    [SerializeField] private RawImage fileImage = null;  

    public void OnRefresh(Texture image, string name, string date)
    {
        dateCreatedText.text = date;
        fileNameText.text = name;
        fileImage.texture = image;
    }   
}
