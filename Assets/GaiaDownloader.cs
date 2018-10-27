using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using UnityEngine;
using UnityEngine.UI;

public class GaiaDownloader : MonoBehaviour
{
    private string filePath = "Assets/Misc/gaiaLinks.txt";
    [Range(1, 1801)] [SerializeField] private int filesCount;
    [SerializeField] private bool deleteFiles;
    [SerializeField] private ExtractDataFromFile extract;

    [Range(1, 300)] [SerializeField] private int magicCount;
    [SerializeField] private bool loadExistingMagic;
    private List<string> linkList;
    private List<string> filesToDelete;

    private DateTime start;

    private DateTime finish;


    // Use this for initialization
    void Start()
    {
        filesCount *= 34;
        start = DateTime.UtcNow;
        filesToDelete = new List<string>();
        StreamReader reader = new StreamReader(filePath);
        string x = reader.ReadToEnd();
        string[] links = x.Split('\n');
        linkList = new List<string>(links);
        linkList.RemoveRange(filesCount, links.Length - filesCount);
        if (!loadExistingMagic)
            StartCoroutine(LoadLinks(linkList));
        else
        {
            StartCoroutine(LoadExistingMagic());
        }
    }


    private IEnumerator LoadExistingMagic()
    {
        int i = 0;
        var startingindex = 0;
        extract.loadExistingMagic = true;
        for (i = 0; i < magicCount; i++)
        {
            if (File.Exists("Assets/Misc/MagicFiles/magic" + i + ".csv"))
            {
                yield return extract.LoadFromFile("Assets/Misc/MagicFiles/magic" + i + ".csv");
            }
            else
            {
                break;
            }
        }

        extract.stars = extract.starsBag.ToList<ExtractDataFromFile.StarStats>();
        extract.SetParticles();
        finish = DateTime.UtcNow;
        TimeSpan ts = new TimeSpan(finish.Ticks - start.Ticks);
        Debug.Log(i - 1 + " magic files loaded in " + ts.TotalSeconds + " seconds\n" + (i - 1) * 34 +
                  " datasets included");
        yield return null;
    }


    static byte[] Decompress(byte[] gzip)
    {
        // Create a GZIP stream with decompression mode.
        // ... Then create a buffer and write into while reading from the GZIP stream.
        using (GZipStream stream = new GZipStream(new MemoryStream(gzip),
            CompressionMode.Decompress))
        {
            const int size = 4096;
            byte[] buffer = new byte[size];
            using (MemoryStream memory = new MemoryStream())
            {
                int count = 0;
                do
                {
                    count = stream.Read(buffer, 0, size);
                    if (count > 0)
                    {
                        memory.Write(buffer, 0, count);
                    }
                } while (count > 0);

                return memory.ToArray();
            }
        }
    }


    public bool ByteArrayToFile(string fileName, byte[] byteArray)
    {
        try
        {
            using (var fs = new FileStream(fileName, FileMode.Create, FileAccess.Write))
            {
                fs.Write(byteArray, 0, byteArray.Length);
                return true;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Exception caught in process: {0}", ex);
            return false;
        }
    }


    IEnumerator LoadLinks(List<string> linksList)
    {
        var startingindex = 0;
        for (int i = 0; i < 1801; i++)
        {
            if (!File.Exists("Assets/Misc/MagicFiles/magic" + i + ".csv"))
            {
                startingindex = i * 34;
                extract.filesConverted = i;
                Debug.Log(i + "not found");
                break;
            }
        }

        for (var index = startingindex; index < linksList.Count; index++)
        {
            var link = linksList[index];
            yield return StartCoroutine(Download(link, index));
        }

        finish = DateTime.UtcNow;
        TimeSpan ts = new TimeSpan(finish.Ticks - start.Ticks);
        Debug.Log(ts.TotalSeconds);
        //else
        //{
        //    start = DateTime.UtcNow;
        //    yield return StartCoroutine(extract.LoadFromFile("Temp/magic.csv"));

        //    finish = DateTime.UtcNow;
        //    TimeSpan ts2 = new TimeSpan(finish.Ticks - start.Ticks);
        //    Debug.Log(ts2.TotalSeconds);
        //}

        if (deleteFiles)
        {
            StartCoroutine(DeleteFiles());
        }

        //extract.SetParticles();
        yield return null;
    }


    IEnumerator Download(string url, int number)
    {
        if (!File.Exists("Temp/gaiaTest" + number + ".csv"))
        {
            Debug.Log("Downloading: " + "gaiaTest" + number + ".gz");
            WebClient client = new WebClient();
            client.DownloadFile(url, "Temp/gaiaTest" + number + ".gz");
            Stream data = client.OpenRead(url);
            StreamReader reader = new StreamReader(data);
            data.Close();
            reader.Close();


            byte[] file = File.ReadAllBytes("Temp/gaiaTest" + number + ".gz");
            byte[] decompressed = Decompress(file);
            ByteArrayToFile("Temp/gaiaTest" + number + ".csv", decompressed);
        }

        yield return extract.LoadFromFile("Temp/gaiaTest" + number + ".csv");

        filesToDelete.Add("Temp/gaiaTest" + number + ".csv");
        filesToDelete.Add("Temp/gaiaTest" + number + ".gz");

        //var www = new WWW(url);
        //yield return www;
        //if (!string.IsNullOrEmpty(www.error))
        //{
        //    Debug.Log("Download from: " + url + " successful.");
        //}
        //else
        //{
        //    Debug.LogWarning("Could not download file! Error:" + www.error);
        //}
    }


    private IEnumerator DeleteFiles()
    {
        foreach (var file in filesToDelete)
        {
            if (File.Exists(file))
            {
                File.Delete(file);
            }

            yield return null;
        }

        yield return null;
    }


    private IEnumerator DeleteFile(string _p0, float delay)
    {
        yield return new WaitForSeconds(delay);

        if (File.Exists(_p0))
        {
            File.Delete(_p0);
        }
    }


    // Update is called once per frame
    void Update()
    {
    }
}