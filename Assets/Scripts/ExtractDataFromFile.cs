using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System.Linq;

public class ExtractDataFromFile : MonoBehaviour
{
    protected ComputeBuffer compute_buffer;
    public Material material;

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

    public bool GPUParticles = false;
    public int starsLimit;
    private int filesLoaded = 0;
    public int filesConverted = 0;
    public bool DistanceTrim;
    public float DistanceTrimValue = 7.5f;
    private Point[] cloud;
    public bool loadExistingMagic;
    public bool colour = false;
    public float distanceMultiplier = 0;
    Vector3 positionFuckThreading;
    float lastdist;
    private Action<int> UpdateParticleAction;

    private Action<int> CreateParticleAction;

    //string filePath = "Assets/Misc/GaiaSource-CSV.csv";
    public ConcurrentBag<StarStats> starsBag = new ConcurrentBag<StarStats>();

    public List<StarStats> stars = new List<StarStats>();
    string[] headings;
    ParticleSystem starSpawner;
    int counter = 0;

    string[] lines;

    //public SetParticlesJobSystem job { get; private set; }
    public ParticleSystem.Particle[] pParticlesGl { get; private set; }


    // Use this for initialization
    void Start()
    {
        lastdist = distanceMultiplier;
        //LoadFromFile("Assets/Misc/GaiaSource-CSV.csv");
    }


    public IEnumerator LoadFromFile(string filePath)
    {
        //Convert.ToDouble(values[GetDataLocation(headings, "parallax")]) <= 15f ||
        string fileData = System.IO.File.ReadAllText(filePath);
        //Get headings
        headings = (fileData.Substring(0, fileData.IndexOf('\n') - 1)).Split(',');
        //Ignore first line of headings in main data set and then split by line
        lines = (fileData.Substring(fileData.IndexOf('\n') + 1)).Split("\n"[0]);
        //var count = stars.Count;

        CreateParticleAction = CreateParticle;
        if (lines.GetUpperBound(0) != 0)
            Parallel.For(0, lines.GetUpperBound(0) - 1, CreateParticleAction);
        else
            Debug.Log(filePath);


        if (!loadExistingMagic)
        {
            filesLoaded++;
            if (filesLoaded % 34 == 0)
            {
                StartCoroutine(ConvertFile());
            }
        }

        //Debug.Log("Reading from: " + filePath + " complete, stars: " + (stars.Count - count));
        yield return null;
    }


    public IEnumerator ConvertFile()
    {
        stars = starsBag.ToList<ExtractDataFromFile.StarStats>();
        var path = "Assets/Misc/MagicFiles/magic" + filesConverted + ".csv";
        Debug.Log(path);
        StreamWriter writer = new StreamWriter(path);
        string line =
            "id,source,ra,dec,parallax,pmra,pmdec,astrometric_pseudo_colour,lum_val,radial_velocity,duplicated_source";
        writer.Write(line);
        writer.WriteLine();
        for (int i = 0; i < stars.Count; i++)
        {
            StarStats item = (StarStats)stars[i];
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
        starsBag = new ConcurrentBag<StarStats>();
        GC.Collect();
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


    struct Point
    {
        public Vector3 position;
        public Color colour;
    }


    public void SetParticles()
    {
        compute_buffer = new ComputeBuffer(stars.Count, sizeof(float) * 7, ComputeBufferType.Default);
        var pParticles = new ParticleSystem.Particle[stars.Count];
        Transform tcopy = transform;
        cloud = new Point[stars.Count];
        for (int i = 0; i < stars.Count; i++)
        {
            float distance = (float)(1 / stars[i].parallax);
            //Zero rotation 
            tcopy.rotation = Quaternion.identity;
            //Turn to direction of star
            tcopy.Rotate((float)stars[i].declination, (float)stars[i].ascension, 0);
            //Spawn star
            pParticles[i].rotation3D = tcopy.forward;
            pParticles[i].position = tcopy.position + tcopy.forward * (distance * lastdist);

            cloud[i].position.x = pParticles[i].position.x;
            cloud[i].position.y = pParticles[i].position.y;
            cloud[i].position.z = pParticles[i].position.z;
            pParticles[i].startSize3D = new Vector3((float)stars[i].luminosity / 1000,
                (float)stars[i].luminosity / 1000, (float)stars[i].luminosity / 1000);
            if (!colour)
            {
                cloud[i].colour = new Color(1, 1, 1, 1);
                pParticles[i].startColor = new Color(1, 1, 1, 1);
            }

            else
            {
                pParticles[i].startColor = PseudoToRGB(stars[i].colour);
                cloud[i].colour = PseudoToRGB(stars[i].colour);
            }
        }

        if (GPUParticles)
            compute_buffer.SetData(cloud);
        else
        {
            starSpawner = gameObject.GetComponent<ParticleSystem>();
            starSpawner.SetParticles(pParticles, stars.Count);
        }

        //starSpawner.Stop();
        //starSpawner.Emit(stars.Count);
        //
    }


    public void UpdateParticles()
    {
        pParticlesGl = new ParticleSystem.Particle[stars.Count];
        gameObject.GetComponent<ParticleSystem>().GetParticles(pParticlesGl);
        UpdateParticleAction = UpdateParticle;
        positionFuckThreading = transform.position;
        Parallel.For(0, stars.Count, UpdateParticleAction);

        starSpawner = gameObject.GetComponent<ParticleSystem>();
        starSpawner.SetParticles(pParticlesGl, stars.Count);
        starSpawner.Stop();
    }


    private void CreateParticle(int index)
    {
        string[] values = lines[index].Split(',');

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
            return;

        if (loadExistingMagic && DistanceTrim)
        {
            if (loadExistingMagic &&
                Convert.ToDouble(values[GetDataLocation(headings, "parallax")]) <= DistanceTrimValue)
            {
                return;
            }
        }

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

        starsBag.Add(temp);
    }


    private void UpdateParticle(int index)
    {
        float distance = (float)(1 / stars[index].parallax);
        pParticlesGl[index].position = positionFuckThreading + pParticlesGl[index].rotation3D * (distance * lastdist);
        cloud[index].position = positionFuckThreading + pParticlesGl[index].rotation3D * (distance * lastdist);
    }


    Color PseudoToRGB(double wavenumber)
    {
        //Keeping precision with double over float
        var nanometers = wavenumber / 1000.0f;
        var wavelengthdouble = 1.0f / nanometers;
        float wavelength = (float)wavelengthdouble;
        float Gamma = 0.80f;
        int IntensityMax = 255;
        float factor, red, green, blue;

        if ((wavelength >= 380) && (wavelength < 440))
        {
            red = -(wavelength - 440) / (440 - 380);
            green = 0.0f;
            blue = 1.0f;
        }
        else if ((wavelength >= 440) && (wavelength < 490))
        {
            red = 0.0f;
            green = (wavelength - 440) / (490 - 440);
            blue = 1.0f;
        }
        else if ((wavelength >= 490) && (wavelength < 510))
        {
            red = 0.0f;
            green = 1.0f;
            blue = -(wavelength - 510) / (510 - 490);
        }
        else if ((wavelength >= 510) && (wavelength < 580))
        {
            red = (wavelength - 510) / (580 - 510);
            green = 1.0f;
            blue = 0.0f;
        }
        else if ((wavelength >= 580) && (wavelength < 645))
        {
            red = 1.0f;
            green = -(wavelength - 645) / (645 - 580);
            blue = 0.0f;
        }
        else if ((wavelength >= 645) && (wavelength < 781))
        {
            red = 1.0f;
            green = 0.0f;
            blue = 0.0f;
        }
        else
        {
            red = 0.0f;
            green = 0.0f;
            blue = 0.0f;
        }

        // Let the intensity fall off near the vision limits
        if ((wavelength >= 380) && (wavelength < 420))
            factor = 0.3f + 0.7f * (wavelength - 380) / (420 - 380);

        else if ((wavelength >= 420) && (wavelength < 701))
            factor = 1.0f;

        else if ((wavelength >= 701) && (wavelength < 781))
            factor = 0.3f + 0.7f * (780 - wavelength) / (780 - 700);

        else
            factor = 0.0f;

        if (red != 0)
            red = (float)Math.Round(IntensityMax * Math.Pow(red * factor, Gamma));

        if (green != 0)
            green = (float)Math.Round(IntensityMax * Math.Pow(green * factor, Gamma));

        if (blue != 0)
            blue = (float)Math.Round(IntensityMax * Math.Pow(blue * factor, Gamma));

        return new Color(red, green, blue);
    }


    private void SetGameObjects()
    {
        for (int i = 0; i < stars.Count; i++)
        {
            float distance = (float)(1 / stars[i].parallax);
            Debug.DrawLine(new Vector3(0, 0, 0), Vector3.forward * distance);
            //Zero rotation 
            transform.rotation = Quaternion.identity;
            //Turn to direction of star
            transform.Rotate((float)stars[i].declination, (float)stars[i].ascension, 0);
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
       // if (Input.GetKeyUp(KeyCode.Space))
            if (lastdist != distanceMultiplier)
            {
                //StartCoroutine(Explode());
                UpdateParticles(); 
                lastdist = distanceMultiplier;
            }
        if (Input.GetKeyUp(KeyCode.Return))
        {
            distanceMultiplier = 100;
            StartCoroutine(Explode());
        }

        if (Input.GetKeyUp(KeyCode.R))
        {
            lastdist = 0;
            SetParticles();
        }
    }


    public IEnumerator Explode()
    {
        var destDistM = distanceMultiplier;
        var timer = 0.0f;
        while (lastdist < distanceMultiplier - 1)
        {
            //timer += Time.deltaTime / 10.0f;
            lastdist += (distanceMultiplier - lastdist) * 0.1f;
            //transform.localScale = new Vector3(lastdist, lastdist, lastdist);
            UpdateParticles();
            yield return new WaitForEndOfFrame();
        }
    }


    void OnPostRender()
    {
        if (compute_buffer != null)
        {
            material.SetPass(0);
            material.SetBuffer("cloud", compute_buffer);
            Graphics.DrawProcedural(MeshTopology.Points, stars.Count, 1);
        }
    }


    void OnDestroy()
    {
        if (compute_buffer != null)
        {
            compute_buffer.Release();
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