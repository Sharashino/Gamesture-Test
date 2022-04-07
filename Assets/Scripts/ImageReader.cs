using System.Runtime.InteropServices;
using Button = UnityEngine.UI.Button;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Threading;
using Ookii.Dialogs;
using System.Linq;
using UnityEngine;
using System.IO;
using System;

// Reads image data from given path
public class ImageReader : MonoBehaviour
{
    [DllImport("user32.dll")]
    private static extern IntPtr GetActiveWindow();

    [SerializeField] private List<ScrollListItem> spawnedItems = new List<ScrollListItem>();
    [SerializeField] private ScrollListItem prefab;
    [SerializeField] private Transform holder;
    [SerializeField] private string folderPath;

    private List<Thread> threads = new List<Thread>();
    private Queue<Action> textureLoaders = new Queue<Action>();

    public bool IsPathCorrect => Directory.Exists(folderPath) || !string.IsNullOrEmpty(folderPath);
    private List<string> LoadedFiles => spawnedItems.Select(x => x.FilePath).ToList();

    public List<ScrollListItem> SpawnedItems => spawnedItems;

    private void Update()
    {
        if(threads.Count > 0)
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
                threads.Remove(th);
                th.Join();  // Joins to main thread
            }
        }

        if(textureLoaders.Count > 0)
        {
            do
            {
                textureLoaders.Dequeue().Invoke();

            } while (textureLoaders.Count > 0);
        }
    }

    public void FindFilePath()
    {
        folderPath = OpenDirDialog();

        if (folderPath != null)
        {
            GetFiles();
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
        

    private string OpenDirDialog()
    {
        var fd = new VistaFolderBrowserDialog();
        fd.SelectedPath = "";

        var res = fd.ShowDialog(new WindowWrapper(GetActiveWindow()));
        var filenames = res == DialogResult.OK ? new[] { fd.SelectedPath } : new string[0];
        fd.Dispose();

        return filenames.Length > 0 ? filenames.First() : null;
    }

    private void RefreshImageData()
    {
        var currentFiles = Directory.EnumerateFiles(folderPath, "*.png").Select(x => x.Replace("\\", "/").Replace("\\", @"\")).ToList();

        foreach (var path in LoadedFiles)
        {
            if(!currentFiles.Contains(path)) RemoveFile(path);
        }
        
        foreach (var path in currentFiles)
        {
            if (!LoadedFiles.Contains(path)) AddFile(path);
            else RemoveFile(path);
        }
    }

    private void AddFile(string filePath)
    {
        if (!File.Exists(filePath)) return;

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
        //if (!File.Exists(filePath)) return;
        var file = new FileInfo(filePath);
        var item = spawnedItems.Find(x => x.FilePath == filePath);

        if(item == null) return;

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
        
        byte[] byteArray = File.ReadAllBytes(path);
        Action act = () =>
        {
            Texture2D texture = new Texture2D(2, 2);

            if (texture.LoadImage(byteArray))
            {
                var sprite = Sprite.Create
                (
                    texture,
                    new Rect(0, 0, texture.width, texture.height),
                    Vector2.zero,
                    100
                );

                item.PopulateItem(sprite, path);
            }
        };

        textureLoaders.Enqueue(act);    
    }
}
