using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;
using UnityEditor;

namespace Bestiary
{
    /// <summary>
    /// A lightweight csv to json converter that reads in a csv file (i.e. "voiceovers_animal.csv") into a list of dictionary of objects.
    /// The dictionary is re-organized into arrays of objects if needed, and saved to list of VoiceoverLine objects.
    /// This final list is serialized to a json file (i.e. "voiceovers_animal.json") with the wrapper class VoiceoverCollection.
    /// 
    /// The csv file is loaded from csvDir and the json file is saved to jsonDir; both are under the StreamingAssets directory.
    /// 
    /// Each csv file, as well as the converted json file, should contain voiceover/subtitle contents of all languages for a single animal/scene.
    /// </summary>
    public class CSVToJSON
    {

        // the special char used for split up the timestamps array
        private static char TIMESTAMP_SPLIT = ',';

        // the special char used for split up the lines array
        // since each line element may contain comma already, we use '^' as the seperater
        private static char LINE_SPLIT = '^';

        // The file path of the .csv files; not finding the specified file will throw an exception during file conversion
        public static string csvDir = Application.streamingAssetsPath+"/"+"VOResources/TextAssets/CSVFiles";

        // The folder path to store output json files
        public static string jsonDir =Application.streamingAssetsPath + "/" + "VOResources/TextAssets/JSONFiles";



        /// <summary>
        /// Convert the given csv file under csvDir to json and store in jsonDir folder.
        /// </summary>
        /// <param name="csvFileName"> csv file to convert, with naming format "voiceover_animal.csv" </param>
        public static bool ConvertCSVToJSON(string csvFileName)
        {
            string jsonFileNameToSave = csvFileName.Substring(0, (csvFileName.Length - 3))+"json";
            // create ouput directory
            try
            {
                if (!Directory.Exists(jsonDir))
                {
                    Directory.CreateDirectory(jsonDir);
                }
            }
            catch (IOException ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }

            var enumValues = Enum.GetValues(typeof(Language));

            // read in csv data
            List<Dictionary<string, object>> data = CSVReader.Read(csvDir+"/"+csvFileName);
            // build voJObjectList
            List<VoiceoverLine> voLineList = new List<VoiceoverLine>();
            
            for (int i = 0; i < data.Count; ++i)
            {
                string key = (string)data[i]["key"];
                string audioFileName = (string)data[i]["audiofilename"];
                LangObject[] langObjects = new LangObject[enumValues.Length];

                // populate langObject array
                foreach (var lang in enumValues)
                {
                    string timestampsKey = "timestamps" + "_" + lang.ToString().ToLower();
                    string linesKey = "lines" + "_" + lang.ToString().ToLower();

                    // obtain arrays from csv string and feed to LangObject's fields
                    LangObject langObject = new LangObject(ConvertToFloatArray(data[i][timestampsKey]),
                                                           ConvertToStringArray(data[i][linesKey]));
                    langObjects[(int)lang] = langObject;
                }

                voLineList.Add(new VoiceoverLine(key, audioFileName, langObjects));
            }

            VoiceoverLine[] voLines = voLineList.ToArray();
            VoiceoverCollection voCollection = new VoiceoverCollection() { voiceoverLines = voLines };

            // save to json file
            SaveToJson(voCollection, jsonFileNameToSave);
            return true;
        }


        /// <summary>
        ///  serialze the voCollection to a single json file; currently not assigning json files to the corresponding asset bundle.
        /// </summary>
        private static void SaveToJson(VoiceoverCollection voCollection, string jsonFileName)
        {
            // save to json file
            string jsonResult = JsonUtility.ToJson(voCollection);
            File.WriteAllText(jsonDir + "/" + jsonFileName, jsonResult);
            AssetDatabase.Refresh();

            //assign text asset to correct asset bundle
            //TextAsset jsonAsset = (TextAsset)Resources.Load(jsonDir + "/" + jsonFileName);
            //string assetPath = AssetDatabase.GetAssetPath(jsonAsset);
            //AssetImporter.GetAtPath(assetPath).SetAssetBundleNameAndVariant(bundleName, "");
            //AssetDatabase.Refresh();

        }


        private static string[] ConvertToStringArray(object input)
        {
            string inputString = input.ToString();
            string[] result = new string[0];
            if (inputString != "")
            {
                result = inputString.Split(LINE_SPLIT);
            }
            return result;
        }

        private static float[] ConvertToFloatArray(object input)
        {
            float[] result = new float[0]; // timestamps.Length == 0

            if (input is float) // timestamps.Length == 1
            {
                result = new float[] { (float)input };
            }
            else 
            {
                string inputString = input.ToString();
                if (inputString != "") // timestamps.Length > 1
                {
                    result = Array.ConvertAll(inputString.Split(TIMESTAMP_SPLIT), float.Parse);
                }
            }
            return result;
        }

    }
}
