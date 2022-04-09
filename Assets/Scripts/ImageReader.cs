using System.Collections.Generic;
using System.Threading;
using System.Linq;
using UnityEngine;
using System.IO;
using System;
using TMPro;

// Reads image data from given path
public class ImageReader : MonoBehaviour
{
    [SerializeField] private List<ScrollListItem> spawnedItems = new List<ScrollListItem>();
    [SerializeField] private ScrollListItem prefab;
    [SerializeField] private Transform holder;
    [SerializeField] private string folderPath;
    [SerializeField] private TMP_Text bugText;

    private List<Thread> threads = new List<Thread>();
    private Queue<Action> textureLoaders = new Queue<Action>();
    private List<string> currentFiles = new List<string>();
    public bool IsPathCorrect => Directory.Exists(folderPath) || !string.IsNullOrEmpty(folderPath);
    private List<string> LoadedFiles => spawnedItems.Select(x => x.FilePath).ToList();
    public List<ScrollListItem> SpawnedItems => spawnedItems;

    private void Update()
    {
        if (textureLoaders.Count > 0)
        {
            do
            {
                textureLoaders?.Dequeue()?.Invoke();

            } while (textureLoaders.Count > 0);
        }

        if (threads.Count > 0)
        {
            var toRemove = new List<Thread>();

            for (int i = 0; i < threads.Count; ++i)
            {
                if (!threads[i].IsAlive)
                {
                    toRemove.Add(threads[i]);
                }
            }

            foreach (Thread th in toRemove)
            {
                th.Join();  // Joins to main thread
                threads.Remove(th);
            }
        }
    }

    public void FindFilePath()
    {
        folderPath = OpenDirDialog.Dialog.Open("Text");

        if (folderPath != null)
        {
            ClearPrevData();
            GetFiles();
        }
    }

    private void ClearPrevData()
    {
        if(spawnedItems.Count > 0)
        {
            foreach (var item in spawnedItems)
            {
                Destroy(item.gameObject);
            }

            spawnedItems.Clear();
            threads.Clear();
            textureLoaders.Clear();
        }
    }

    public void GetFiles()
    {
        if (!IsPathCorrect)
        {
            Debug.Log("ImageReader - Path doesnt exist or is empty!");
            return;
        }
        else RefreshImageData();
    }

    private void RefreshImageData()
    {
        currentFiles = Directory.EnumerateFiles(folderPath, "*.png").Select(x => x.Replace("\\", "/").Replace("\\", @"\")).ToList();
        bugText.text += $"found files {currentFiles.Count} ";

        if (LoadedFiles != null)
        {
            foreach (var path in LoadedFiles)
            {
                if (!currentFiles.Contains(path)) RemoveFile(path);
            }
        }
        
        foreach (var path in currentFiles)
        {
            if (!LoadedFiles.Contains(path)) AddFile(path);
            else RemoveFile(path);
        }
    }

    private void AddFile(string filePath)
    {
        if (!File.Exists(filePath.Replace("/", "\\").Replace(@"\", "\\"))) return;

        var item = Instantiate(prefab, holder);
        spawnedItems.Add(item);

        Thread t = new Thread(new ParameterizedThreadStart(LoadFileThread));
        t.Start(new object[]
        {
            item,
            filePath
        });

        threads.Add(t);
    }

    private void RemoveFile(string filePath)
    {
        if (!File.Exists(filePath.Replace("/", "\\").Replace(@"\", "\\")))
        {
            return; 
        }
        
        var file = new FileInfo(filePath);
        var item = spawnedItems.Find(x => x.FilePath == filePath);

        if(file != item.FileInfo)
        {
            spawnedItems.Remove(item);
            Destroy(item.gameObject);
            AddFile(filePath);
        }
        else
        {
            spawnedItems.Remove(item);
            Destroy(item.gameObject);
        }
    }

    private void LoadFileThread(object args)
    {
        var item = (ScrollListItem)((object[])args)[0];
        string path = (string)((object[])args)[1];

        if (!File.Exists(path.Replace("/", "\\").Replace(@"\", "\\"))) return;
        
        byte[] byteArray = File.ReadAllBytes(path);
        
        Action act = () =>
        {
            Texture2D texture = new Texture2D(2, 2);

            if (texture.LoadImage(byteArray))
            {
                item.PopulateItem(texture, path);
            }
        };

        textureLoaders.Enqueue(act);    
    }
}
