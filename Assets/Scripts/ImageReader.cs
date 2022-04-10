using System.Collections.Generic;
using System.Threading;
using System.Linq;
using UnityEngine;
using System.IO;
using System;

// Reads file data from given path and spawns object to display file name/image/date
public class ImageReader : MonoBehaviour
{
    [SerializeField] private List<ScrollListItem> spawnedItems = new List<ScrollListItem>();
    [SerializeField] private ScrollListItem prefab;
    [SerializeField] private Transform holder;

    private List<Thread> threads = new List<Thread>();
    private Queue<Action> textureLoaders = new Queue<Action>();
    private List<string> currentFiles = new List<string>();
    private string folderPath;

    public bool IsPathCorrect => Directory.Exists(folderPath) || !string.IsNullOrEmpty(folderPath);
    private List<string> LoadedFiles => spawnedItems.Select(x => x.FilePath).ToList();
    public List<ScrollListItem> SpawnedItems => spawnedItems;

    private void Update()
    {
        if (threads.Count > 0)
        {
            // Removing threads that are finished
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

        if (textureLoaders.Count > 0)
        {
            do
            {
                // Invoking actions with image load
                textureLoaders?.Dequeue()?.Invoke();
            } while (textureLoaders.Count > 0);
        }
    }

    public void FindFilePath()
    {
        // Opening windows dialog box for folder selection
        folderPath = OpenDirDialog.Dialog.Open("Select folder to load files from...");

        if (folderPath != null)
        {
            ClearPrevData();
            CheckFilePath();
        }
    }

    private void ClearPrevData()
    {
        // Clearing previously spawned items on browse
        if(spawnedItems.Count > 0)
        {
            foreach (var item in spawnedItems)
            {
                Destroy(item.gameObject);
            }

            // Clearing all lists to ensure we wont do operations on old data
            threads.Clear();
            spawnedItems.Clear();
            textureLoaders.Clear();
        }
    }

    public void CheckFilePath()
    {
        if (!IsPathCorrect)
        {
            Debug.Log("Path doesnt exist or is empty!");
            return;
        }
        else GetFilesData();
    }

    private void GetFilesData()
    {
        currentFiles = Directory.EnumerateFiles(folderPath, "*.png").ToList();

        // Removing all files which doesn't exist in currently found files  
        foreach (var path in LoadedFiles)   
        {
            if (!currentFiles.Contains(path)) RemoveFile(path);
        }
        
        foreach (var path in currentFiles)
        {
            // If loaded files doesnt contain found file -> we spawn it
            // If loaded files contains it -> data must have changed -> refresh file
            if (!LoadedFiles.Contains(path)) AddFile(path); 
            else RefreshFile(path);
        }
    }

    private void AddFile(string filePath)
    {
        if (!File.Exists(filePath)) return;

        // Spawning new panel for scroll list
        var item = Instantiate(prefab, holder);
        spawnedItems.Add(item);

        // Creating new thread to load image data 
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
        if (!File.Exists(filePath)) return;
        
        var item = spawnedItems.Find(x => x.FilePath == filePath);

        spawnedItems.Remove(item);
        Destroy(item.gameObject);
    }

    private void RefreshFile(string filePath)
    {
        if (!File.Exists(filePath)) return;

        var file = new FileInfo(filePath);
        var item = spawnedItems.Find(x => x.FilePath == filePath);

        if (file != item.FileInfo)
        {
            spawnedItems.Remove(item);
            Destroy(item.gameObject);
            AddFile(filePath);
        }
    }

    // Thread loading byte data from file
    private void LoadFileThread(object args)
    {
        var item = (ScrollListItem)((object[])args)[0];
        string path = (string)((object[])args)[1];

        if (!File.Exists(path)) return;
        
        byte[] byteArray = File.ReadAllBytes(path);

        // Since Unity only let us create images in main thread
        // we're loading image byte data in newly created thread
        // and creating action to load image from that data in main thread
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
