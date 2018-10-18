using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class ExtractDataFromFile : MonoBehaviour {

    public struct StarStats
    {
        public string ID;
        public double parallax;
        public double ascension;
        public double declination;
        public double colour;

        //Movement variables 
        //Motion in Right Ascension
        public double pmra;
        //Motion in Declination 
        public double pmdec;
        //Radial velocity in km/s
        public double radialVelocity;
    }

    string filePath = "Assets/Misc/GaiaSource-CSV.csv";
	public List<StarStats> stars = new List<StarStats>(); 
	// Use this for initialization
	void Start () {
		string fileData = System.IO.File.ReadAllText(filePath);
        fileData = fileData.Substring(fileData.IndexOf('\n') + 1); 
        string[] lines = fileData.Split("\n"[0]);

        for (int i = 0; i <= lines.GetUpperBound(0) - 1; i++)
		{
            string[] values = lines[i].Split(',');
            if (values[9] == "" || values[37] == "")
                continue; 
            StarStats temp = new StarStats();
            temp.ID             = values[1];
            temp.parallax       = double.Parse(values[9]);
            temp.ascension      = double.Parse(values[5]);
            temp.declination    = double.Parse(values[7]);
            temp.colour         = double.Parse(values[37]);
            temp.pmra           = double.Parse(values[12]);
            temp.pmdec          = double.Parse(values[14]);
            stars.Add(temp); 
            //temp.radialVelocity = double.Parse(values[66]); 
            //double[] value = System.Array.ConvertAll(line.Split(','), double.Parse); 
		}
        Debug.Log("complete"); 
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
