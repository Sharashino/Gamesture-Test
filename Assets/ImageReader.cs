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
    [SerializeField] private Button refreshButton;
    [SerializeField] private Button selectFolderButton;
    [SerializeField] private ScrollListItem item;
    [SerializeField] private Transform holder;
    [SerializeField] private string folderPath;
    //[SerializeField] private List<string> loadedFiles = new List<string>();

    private List<Thread> threads = new List<Thread>();
    private Queue<Action> textureLoaders = new Queue<Action>();

    public bool IsPathCorrect => Directory.Exists(folderPath) || !string.IsNullOrEmpty(folderPath);
    private IEnumerable<string> loadedFiles => spawnedItems.Select(x => x.FilePath);

    void Awake()
    {
        selectFolderButton?.onClick.AddListener(OnFindFile);
        refreshButton?.onClick.AddListener(OnRefresh);

        refreshButton.gameObject.SetActive(false);
        holder.gameObject.SetActive(false);
    }

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

    private void OnFindFile()
    {
        folderPath = OpenDirDialog("");

        if (folderPath != null)
        {
            refreshButton.gameObject.SetActive(true);
            holder.gameObject.SetActive(true);
            OnRefresh();
        }
    }

    public string OpenDirDialog(string directory)
    {
        var fd = new VistaFolderBrowserDialog();
        fd.SelectedPath = directory;
        
        var res = fd.ShowDialog(new WindowWrapper(GetActiveWindow())); // Display dialog parented by current window (game window)
        var filenames = res == DialogResult.OK ? new[] { fd.SelectedPath } : new string[0];
        fd.Dispose();

        return filenames.Length > 0 ? filenames.First() : null;
    }

    private void OnRefresh()
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
        var currentFiles = Directory.EnumerateFiles(folderPath, "*.png").Select(x => x.Replace("\\", "/").Replace("\\", ""));

        foreach (var item in loadedFiles)
        {
            if (!currentFiles.Contains(item))
            {
                RemoveFile(item);
                // remove file
            }
        }

        foreach (var item in currentFiles)
        {
            if(!loadedFiles.Contains(item))
            {
                AddFile(item);
                // create new image
            }
        }
    }

    private void AddFile(string filePath)
    {
        if (!File.Exists(filePath)) return;

        var item = Instantiate(this.item, holder);
        spawnedItems.Add(item);

        Thread t = new Thread(new ParameterizedThreadStart(AddFileThread));
        t.Start(new object[]
        {
            item,
            filePath
        }); 
        threads.Add(t);
    }

    private void RemoveFile(string filePath)
    {
        if (!File.Exists(filePath)) return;

        var item = spawnedItems.Find(x => x.FilePath == filePath);

        if (item != null)
        {
            Destroy(item.gameObject);
            spawnedItems.Remove(item);
        }
    }

    private void AddFileThread(object args)
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

                item.OnRefresh(sprite, path);
            }
        };

        textureLoaders.Enqueue(act);    
    }
}

public class WindowWrapper : IWin32Window
{
    private IntPtr _hwnd;
    public WindowWrapper(IntPtr handle) { _hwnd = handle; }
    public IntPtr Handle { get { return _hwnd; } }
}