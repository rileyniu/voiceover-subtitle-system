using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Bestiary
{
    /// <summary>
    /// The language enum declares all languages in the subtitle system.
    /// The csv file should have two columns {"timestamps_language", "lines_language"} to construct a langObject for each declared Language enum.
    /// These columns for each language should be layed out in the same order as they are declared in the Language enum.
    /// </summary>
    public enum Language { EN, JP };

    public class VoiceoverManager : MonoBehaviour
    {
        // type-safe wrapper class for voiceover line object identification
        public class VoiceoverID : CRCBasedID
        {
            public VoiceoverID (string name) : base(name)
            { }
        }

        // default scene language is English
        public Language current_language = Language.EN;

        // scene audio source for voiceovers; if not assigned, a correspondign AudioSource component will be created for this VoiceoverManager
        public static AudioSource audioSource;

        private static VoiceoverManager instance = null;
        private VoiceoverManager() { }
        public static VoiceoverManager Instance
        {
            get
            {
                if (instance == null)
                {
                    GameObject obj = new GameObject();
                    instance = obj.AddComponent<VoiceoverManager>();
                }
                return instance;
            }
        }
        
        private static string jsonDir = Application.streamingAssetsPath + "/" + "VOResources/TextAssets/JSONFiles"; // the streaming assets directory for all json files
        
        private static string sceneJsonFile; //scene JSON file for vocieovers/subtitles, with format "voiceovers_scenename.json"

        private Dictionary<VoiceoverID, VoiceoverLine> dict = new Dictionary<VoiceoverID, VoiceoverLine>(); // a scene dictionary that stores all voiceover lines

        private bool subtitleInDisplay = false; // whether there is currently a subtitle in display and has not finished its duration

        private bool canvasEnabled; // whether the canvas gameobject is enabled

        private bool missingFileErr = false; // missingFileErr is set to true if current scene json file is missing

        private string defaultErrMsg = "Missing scene json file: <>. Check the file name and directory again."; // error message to be displayed on canvas

        private void Awake()
        {
            // Obtain current scene name and locate the correct json file; set default error message
            string currentSceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name.ToLower();

            sceneJsonFile = "voiceovers_" + currentSceneName + ".json";
            //sceneJsonFile = "voiceovers_bluewhale.json";
            defaultErrMsg = defaultErrMsg.Insert(defaultErrMsg.IndexOf(':') + 3, sceneJsonFile);

            instance = this;

            // assign audio source for voiceovers
            if (!audioSource)
            {
                audioSource = GetComponent<AudioSource>();
                if (!audioSource)
                {
                    audioSource = gameObject.AddComponent<AudioSource>();
                }
            }

            if (sceneJsonFile != null)
            {
                InitializeVoiceoverManager();
            }
        }

        private void Update()
        {
            if (canvasEnabled)
            {
                if (OVRInput.GetDown(OVRInput.Button.One)) // Press button A to hide or show the subtitle 
                {
                    DisableSubtitleCanvas();
                }
            }
            else
            {
                if (OVRInput.GetDown(OVRInput.Button.One))
                {
                    EnableSubtitleCanvas();
                }
            }
        }


        /// <summary>
        /// Load scene voiceover jsons file.
        /// TODO: if we want to load json text assets from scene asset bundle, more work need to be done 
        /// in order to wire up the scene manager and the vocieover manager.
        /// </summary>
        private string ReadSceneJSONFile()
        {
            //AssetBundle bundle = GetSceneBundle();
            //TextAsset sceneJsonFile = bundle.LoadAssetsAsync<TextAsset>("");

            if (!File.Exists(jsonDir+ "/" +sceneJsonFile))
            {
                Debug.Log("Could not find the csv file at specified path " + jsonDir + "/" + sceneJsonFile);
                return null;
            }

            return File.ReadAllText(jsonDir + "/" + sceneJsonFile);
        }

        /// <summary>
        /// Populate the dictionary for voiceovers in this scene
        /// The key is audio clip file name and the value is corresponding VoiceoverLine object
        /// </summary>
        private void InitializeVoiceoverManager()
        {
            string data = ReadSceneJSONFile();
            if (data == null)
            {
                missingFileErr = true;
            }
            else
            {
                VoiceoverCollection collection = VoiceoverCollection.CreateFromJSON(data);
                VoiceoverLine[] voLines = collection.voiceoverLines;
                for (int i = 0; i < voLines.Length; ++i)
                {
                    VoiceoverID crc = new VoiceoverID(voLines[i].key);
                    dict.Add(crc, voLines[i]);
                }
            }
            
        }

        /// <summary>
        /// Retrive the voiceover value paired to the key from the scene dictionary; play the voiceover and set the subtitle text.
        /// Overloaded as the timeline signal receiver function; will always interrupt and stop the current vo/subtitle immediately and play the new vo.
        /// </summary>
        /// <param name="key">the string value key passed by the custom timeline signal. </param>
        public bool OnVoiceoverContentChanged(string key)
        {
             return OnVoiceoverContentChanged(new VoiceoverID(key), true);
        }


        /// <summary>
        /// Retrive the voiceover value paired to the crc ID from the scene dictionary and try to play the voiceover and set the subtitle text.
        /// </summary>
        /// <returns> returns true if the voiceover is set to be played successfully, false otherwise.
        /// <param name="key"> the voiceover id as encoded CRC Int. </param>
        /// <param name="stopCurrentIfNecessary"> - If OnVoiceoverContentChanged() is called when there is already an active subtitle content in display:
        ///                                           - setting this param to true will interrupt and stop the current vo/subtitle immediately and play the new vo; 
        ///                                           - setting this param to false will not interrupt the current vocieover; the new vo will not get played and
        ///                                             the function returns false.
        /// </param>
        public bool OnVoiceoverContentChanged(VoiceoverID key, bool stopCurrentIfNecessary)
        {
            // missing json file
            if (missingFileErr)
            {
                DisplayErrorMsg();
                return false;
            }

            // active subtitle content in display and we don't want to interrupt; fail to play the new voiceover
            if (subtitleInDisplay && !stopCurrentIfNecessary)
            {
                return false;
            }

            // stop the current vo and play the new one
            if (subtitleInDisplay && stopCurrentIfNecessary)
            {
                audioSource.Stop();
                StopAllCoroutines();
            }

            VoiceoverLine voiceoverLine;
            if (dict.TryGetValue(key, out voiceoverLine))
            {
                float[] timestamps = voiceoverLine.langObjects[(int)current_language].timestamps;
                string[] lines = voiceoverLine.langObjects[(int)current_language].lines;

                subtitleInDisplay = true;
                // TODO: the load method need to be updated if audio files are stored in streaming assets folder
                audioSource.clip = (AudioClip)Resources.Load(voiceoverLine.audiofilename, typeof(AudioClip));

                // play voiceover
                audioSource.Play();

                // display subtitle for entire audio duration
                if (timestamps.Length < 1)
                {
                    DisplaySubtitle(lines[0], audioSource.clip.length);
                }
                else // display subtitle according to durations in between timestamps
                {
                    float[] durations = new float[timestamps.Length + 1];
                    durations[0] = timestamps[0];
                    durations[timestamps.Length] = audioSource.clip.length - timestamps[timestamps.Length - 1];
                    for (int i = 1; i < timestamps.Length; i++)
                    {
                        durations[i] = timestamps[i] - timestamps[i - 1];
                    }
                    StartCoroutine(DisplaySubtitles(lines, durations));
                }
                return true;
            }
            return false;

        }


        /// <summary>
        /// DisplaySubtitle() displays the text in a single subtitle box; erase after given duration.
        /// </summary>
        private void DisplaySubtitle(string line, float duration)
        {
            SubtitleGUIManager.Instance.SetText(line);
            StartCoroutine(ClearSubtitle(duration));
        }


        /// <summary>
        /// DisplaySubtitles() is called when there multiple lines to display for a single voiceover.
        /// Each element in lines[] is displayed for its given duration. Text are erased after the last line finishes displaying.
        /// </summary>
        private IEnumerator DisplaySubtitles(string[] lines, float[] durations)
        {
            Debug.Assert(lines.Length == durations.Length, "number of line durations does not equal to number of lines.");
            for (int i = 0; i < lines.Length; ++i)
            {
                SubtitleGUIManager.Instance.SetText(lines[i]);
                yield return new WaitForSeconds(durations[i]);
                //SubtitleGUIManager.Instance.SetText(string.Empty);
            }
            // clear the subtitle after displaying the last line for this voiceover
            DisplaySubtitle(lines[lines.Length - 1], durations[durations.Length - 1]);


        }

        /// <summary>
        /// hold the current subtitle for given time and clear the text of the subtitle box 
        /// </summary>
        private IEnumerator ClearSubtitle(float time)
        {
            yield return new WaitForSeconds(time);
            SubtitleGUIManager.Instance.SetText(string.Empty);
            subtitleInDisplay = false;
        }

        private void DisplayErrorMsg()
        {
            SubtitleGUIManager.Instance.SetText(defaultErrMsg);
        }

        public void DisableSubtitleCanvas()
        {
            this.canvasEnabled = false;
            SubtitleGUIManager.Instance.DisableSubtitleCanvas();
        }

        public void EnableSubtitleCanvas()
        {
            this.canvasEnabled = true;
            SubtitleGUIManager.Instance.EnableSubtitleCanvas();
        }
        
        
        private void OnDestroy()
        {
            instance = null;
        }

    }

    // voiceover wrapper class for json serialization
    [Serializable]
    public class VoiceoverCollection
    {
        public VoiceoverLine[] voiceoverLines;

        public static VoiceoverCollection CreateFromJSON(string jsonString)
        {
            return JsonUtility.FromJson<VoiceoverCollection>(jsonString);
        }
    }

    // voiceover json object class 
    [Serializable]
    public class VoiceoverLine
    {
        public string key;
        public string audiofilename;
        public LangObject[] langObjects;

        public VoiceoverLine(string key, string audiofilename, LangObject[] langJObjects)
        {
            this.key = key;
            this.audiofilename = audiofilename;
            this.langObjects = langJObjects;
        }
    }

    [Serializable]
    public class LangObject
    {
        public float[] timestamps;
        public string[] lines;

        public LangObject(float[] timestamps, string[] lines)
        {
            this.timestamps = timestamps;
            this.lines = lines;
        }
    }
}