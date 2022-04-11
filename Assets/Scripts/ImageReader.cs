using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections;
using System.Threading;
using System.Linq;
using UnityEngine;
using System.IO;
using System;
using SFB;

// Reads file data from given path and spawns object to display file name/image/date
public class ImageReader : MonoBehaviour
{
	[SerializeField] private List<ScrollListItem> spawnedItems = new List<ScrollListItem>();
	[SerializeField] private ScrollListItem prefab;
	[SerializeField] private Transform holder;
	private ConcurrentBag<PathArrayClass> textureLoaders = new ConcurrentBag<PathArrayClass>();
	private Stack<Thread> threads = new Stack<Thread>();
	private List<string> currentFiles = new List<string>();
	private string folderPath;
	private bool isLoading = false;

	public Action<int> onCounterIncrease;

	public bool IsPathCorrect => Directory.Exists(folderPath) || !string.IsNullOrEmpty(folderPath);
	private List<string> LoadedFiles => spawnedItems.Select(x => x.FilePath).ToList();
	public bool IsLoading => isLoading;

	private void Start()
	{
		StartCoroutine(ThreadsManagementCoroutine());
		StartCoroutine(LoadingTexturesCoroutine());
	}

	private IEnumerator LoadingTexturesCoroutine()
	{
		// Loading textures from concurent bag and spawning new list element
		while (true)
		{
			yield return null;
			if (textureLoaders.Count == 0)
            {
				isLoading = false;
				continue;
            }

            for (int i = 0; i < textureLoaders.Count; i++)
            {
				Texture2D texture = new Texture2D(2, 2);
				if(textureLoaders.TryTake(out PathArrayClass p))
                {
					isLoading = true;

					if (texture.LoadImage(p.array))
					{
						// Spawning new panel for scroll list
						var item = Instantiate(prefab, holder);
						item.PopulateItem(texture, p.path);
						spawnedItems.Add(item);

						onCounterIncrease?.Invoke(spawnedItems.Count);
					}
                }

				yield return null;
			}
		}	
	}

	private IEnumerator ThreadsManagementCoroutine()
	{
		// Joining finished threads to main thread from stack 
		while (true)
		{
			yield return null;

			if (threads.Count == 0 || threads.Peek().IsAlive) continue;

			threads?.Pop()?.Join();
		}
	}

	private void ClearPrevData()
	{
		// Clearing previously spawned items on browse
		if(spawnedItems.Count > 0)
		{
			foreach (var item in spawnedItems) Destroy(item.gameObject);

			// Clearing everything to ensure we wont do operations on old data
			threads.Clear();
			spawnedItems.Clear();
			textureLoaders = new ConcurrentBag<PathArrayClass>();
		}
	}

	private void GetFilesData()
	{
		currentFiles = new List<string>();

		// Removing files which doesnt exist in currently found files  
		foreach (var path in LoadedFiles)   
		{
			if (!currentFiles.Contains(path)) RemoveFile(path);
		}
		
		foreach (var path in Directory.EnumerateFiles(folderPath, "*.png"))
		{
			// Adding file only after we're sure we're checking it
			currentFiles.Add(path);
			// If loaded files doesnt contain found file -> we spawn it
			// If loaded files contains it -> data must have changed -> refresh file

			if (!LoadedFiles.Contains(path)) AddFile(path); 
			else RefreshFile(path);
		}
	}

	private void AddFile(string filePath)
	{
		// Creating new thread to load image data
		Thread t = new Thread(new ParameterizedThreadStart(LoadFileThread));
		t.Start(filePath);

		threads.Push(t);
	}

	private void RemoveFile(string filePath)
	{
		var item = spawnedItems.Find(x => x.FilePath == filePath);

		spawnedItems.Remove(item);
		Destroy(item.gameObject);
	}

	private void RefreshFile(string filePath)
	{
		var file = new FileInfo(filePath);
		var item = spawnedItems.Find(x => x.FilePath == filePath);

		if (file != item.FileInfo)
		{
			spawnedItems.Remove(item);
			Destroy(item.gameObject);
			AddFile(filePath);
		}
	}

	private void LoadFileThread(object filePath)
	{
		string path = (string)filePath;

		FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read);
		byte[] headerArray = new byte[4];

		fs.Read(headerArray, 0, 4);
		
		if (headerArray[0] == 0x89
			&& headerArray[1] == 0x50
			&& headerArray[2] == 0x4e
			&& headerArray[3] == 0x47)    // Checking first 4 bytes to make sure our image is .png type
		{
			fs.Position = 0;
			var fileArray = new byte[fs.Length];
			fs.Read(fileArray, 0, fileArray.Length);
			fs.Close();

			// Since Unity let us create images only in main thread
			// we're loading image byte data in newly created thread
			// and creating an object with readed data to
			// load image and spawn object from that data in main thread
			textureLoaders.Add(new PathArrayClass()
			{
				path = path,
				array = fileArray
			});
		}
		else Debug.Log($"File {fs.Name} is not proper .png file!");
	}

	public bool FindFilePath()
	{
		// Opening windows dialog box for folder selection
		var paths = StandaloneFileBrowser.OpenFolderPanel("", "", false);

		if (paths != null && paths.Length > 0) folderPath = paths.First();

		if (folderPath != null)
		{
			ClearPrevData();
			if(CheckFilePath()) return true;

			return false;
		}

		return false;
	}

	public bool CheckFilePath()
	{
		if (!IsPathCorrect)
		{
			Debug.Log("Path doesnt exist or is empty!");
			return false;
		}
		else
		{
			GetFilesData();
			return true;
		}
	}
}

public class PathArrayClass
{
	public string path;
	public byte[] array;
}