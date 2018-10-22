using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;


public class ExtractDataFromFile : MonoBehaviour
{
    public struct StarStats
    {
        public string ID;
        public double parallax;
        public double ascension;
        public double declination;
        public double luminosity;
        public double colour;

        //Movement variables 
        //Motion in Right Ascension
        public double pmra;

        //Motion in Declination 
        public double pmdec;

        //Radial velocity in km/s
        public double radialVelocity;
    }

    private int filesLoaded = 0;
    public int filesConverted = 0;

    public bool loadExistingMagic;
    public bool colour = false; 
    //string filePath = "Assets/Misc/GaiaSource-CSV.csv";
    public List<StarStats> stars = new List<StarStats>();
    string[] headings;
    ParticleSystem starSpawner;
    int counter = 0;


    // Use this for initialization
    void Start()
    {
        //LoadFromFile("Assets/Misc/GaiaSource-CSV.csv");
    }


    public IEnumerator LoadFromFile(string filePath)
    {
        string fileData = System.IO.File.ReadAllText(filePath);
        //Get headings
        headings = (fileData.Substring(0, fileData.IndexOf('\n') - 1)).Split(',');
        //Ignore first line of headings in main data set and then split by line
        string[] lines = (fileData.Substring(fileData.IndexOf('\n') + 1)).Split("\n"[0]);
        var count = stars.Count;
        for (int i = 0; i <= lines.GetUpperBound(0) - 1; i++)
        {
            string[] values = lines[i].Split(',');

            //Ignore data with missing values
            if (values[GetDataLocation(headings, "parallax")] == "" ||
                values[GetDataLocation(headings, "astrometric_pseudo_colour")] == "" ||
                values[GetDataLocation(headings, "astrometric_pseudo_colour")] == "\r" ||
                values[GetDataLocation(headings, "lum_val")] == "" ||
                values[GetDataLocation(headings, "ra")] == "" ||
                values[GetDataLocation(headings, "dec")] == "" ||
                values[GetDataLocation(headings, "pmra")] == "" ||
                values[GetDataLocation(headings, "pmdec")] == "" ||
                values[GetDataLocation(headings, "radial_velocity")] == "" ||
                values[GetDataLocation(headings, "duplicated_source")] == "TRUE")
                continue;

            StarStats temp = new StarStats();
            temp.ID = values[1];
            temp.parallax = double.Parse(values[GetDataLocation(headings, "parallax")]); //9
            temp.ascension = double.Parse(values[GetDataLocation(headings, "ra")]); //5
            temp.declination = double.Parse(values[GetDataLocation(headings, "dec")]); //7
            temp.luminosity = double.Parse(values[GetDataLocation(headings, "lum_val")]);
            temp.colour = double.Parse(values[GetDataLocation(headings, "astrometric_pseudo_colour")]); //37
            temp.pmra = double.Parse(values[GetDataLocation(headings, "pmra")]); //12
            temp.pmdec = double.Parse(values[GetDataLocation(headings, "pmdec")]); //14
            temp.radialVelocity = double.Parse(values[GetDataLocation(headings, "radial_velocity")]);

            stars.Add(temp);
        }

        if (!loadExistingMagic)
        {
            filesLoaded++;
            if (filesLoaded % 34 == 0)
            {
                StartCoroutine(ConvertFile());
            }
        }

        Debug.Log("Reading from: " + filePath + " complete, stars: " + (stars.Count - count));
        yield return null;
    }


    public IEnumerator ConvertFile()
    {
        var path = "Assets/Misc/MagicFiles/magic" + filesConverted + ".csv";
        Debug.Log(path);
        StreamWriter writer = new StreamWriter(path);
        string line =
            "id,source,ra,dec,parallax,pmra,pmdec,astrometric_pseudo_colour,lum_val,radial_velocity,duplicated_source";
        writer.Write(line);
        writer.WriteLine();
        for (int i = 0; i < stars.Count; i++)
        {
            StarStats item = (StarStats) stars[i];
            var line2 = i + ",";
            line2 += item.ID + ",";
            line2 += item.ascension + ",";
            line2 += item.declination + ",";
            line2 += item.parallax + ",";
            line2 += item.pmra + ",";
            line2 += item.pmdec + ",";
            line2 += item.colour + ",";
            line2 += item.luminosity + ",";
            line2 += item.radialVelocity + ",";
            line2 += "FALSE";
            writer.Write(line2);
            writer.WriteLine();
        }

        writer.Close();
        filesConverted++;
        stars = new List<StarStats>();
        yield return null;
    }


    //Finds and returns index of data heading
    private int GetDataLocation(string[] headings, string dataHeading)
    {
        for (int i = 0; i < headings.Length; i++)
            if (headings[i] == dataHeading)
                return i;

        Debug.Log("Error No Heading Found of that Name");
        return 0;
    }


    public void SetParticles()
    {
        var pParticles = new ParticleSystem.Particle[stars.Count];
        for (int i = 0; i < stars.Count; i++)
        {
            float distance = (float) (1 / stars[i].parallax);
            //Zero rotation 
            transform.rotation = Quaternion.identity;
            //Turn to direction of star
            transform.Rotate((float) stars[i].declination, (float) stars[i].ascension, 0);
            //Spawn star
            pParticles[i].position = transform.position + transform.forward * (distance * 10);
            pParticles[i].startSize3D = new Vector3((float) stars[i].luminosity / 1000,
                (float) stars[i].luminosity / 1000, (float) stars[i].luminosity / 1000);
            if (!colour)
             pParticles[i].startColor = new Color(1, 1, 1, 1);
            else
             pParticles[i].startColor = new Color(1, (float)stars[i].colour -1, 0, 1); 
        }

        starSpawner = gameObject.GetComponent<ParticleSystem>();
        starSpawner.SetParticles(pParticles, stars.Count);
        //starSpawner.Emit(stars.Count);
        //starSpawner.Pause(); 
    }


    private void SetGameObjects()
    {
        for (int i = 0; i < stars.Count; i++)
        {
            float distance = (float) (1 / stars[i].parallax);
            Debug.DrawLine(new Vector3(0, 0, 0), Vector3.forward * distance);
            //Zero rotation 
            transform.rotation = Quaternion.identity;
            //Turn to direction of star
            transform.Rotate((float) stars[i].declination, (float) stars[i].ascension, 0);
            //Spawn star
            GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            sphere.transform.position = transform.position + transform.forward * (distance * 10);
            sphere.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
            Debug.DrawLine(new Vector3(0, 0, 0), sphere.transform.position, Color.red, 0.1f);
            //Debug.Log(counter);
            //counter++;
        }
    }


    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyUp(KeyCode.Space))
        {
            SetGameObjects();
        }

        if (Input.GetKeyUp(KeyCode.Return))
        {
            SetParticles();
        }
    }
}


////Check for missing data
//bool isComplete = true;
//            for (int x = 0; x<values.Length; x++)
//                if (values[x] == "")
//                {
//                    isComplete = false;
//                    break; 
//                }
//            //If data is missing disgard
//            if (isComplete == false)
//                continue; 