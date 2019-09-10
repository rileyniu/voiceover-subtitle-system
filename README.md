
# Bestiary Voiceover/Subtitle System
An integrated voiceover and subtitle system in Unity that includes:
- A lightweight csv-to-json converter
- Custom timeline event system
- Voiceover Manager
- Subtitle GUI Manager

## Workflow
### File Conversion
  1.  Declare all available language options as capitalized two letter abbreviations in the `Language` enum inside `VoiceOverManager` class:

		```
		public enum Language { EN, JP };
		```

		 The default langauges are EN -- English and JP -- Japanese.
  2. Prepare all voiceover audio files and save as .wav file. Each audio file should be named as "**audiofilename_languageAbbrev.wav**", with languageAbbrev from  `Language` enum  indicating the language of this audio.
	  See [todo: audio file directory](#To-do-List)
  4.  Create the CSV files for each animal scene. Ensure each CSV file is correctly named as "**voiceovers_animal.csv**". Save the files to the correct directory in the project (usually somewhere in the StreamingAssets folder);  the default directory right now is "**Project\Assets\StreamingAssets\VOResources\TextAssets\CSVFiles**".
  This directory can be altered in `CSVToJSON` class in the Editor folder:
  
		```
		public static string csvDir = Application.streamingAssetsPath+"/"+"VOResources/TextAssets/CSVFiles";
		```

	  See [CSV File Format](#CSV-File-Format) for detailed descriptions to edit the CSV file manually.

4.  Calling `BuildPlayer.Build()` will convert all “**voiceovers_animal.csv**” files under “**Project\Assets\StreamingAssets\VOResources\TextAssets\CSVFiles**”. 
to corresponding “**voiceovers_animal.json**” files under “**Project\Assets\StreamingAssets\VOResources\TextAssets\JSONFiles**”.
These JSON files will be loaded at runtime to play voiceovers and display subtitle.

### Unity Scene Set-up
6. Drag in VoiceoverManager  prefab and SubtitleGUIManager prefab to the scene, assign the scene timeline asset to the Playable Director component of VoiceoverManager.
7. To use Unity's Timeline system & fire signals in the timeline  to play voiceover:
	- In Timeline window, right click to add -> Bestiary -> Voiceover Track. 
	- Right click on the track -> add Voiceover Marker;  
	- In the inspector window for this marker, fill in the Key parameter -- as specified in the CSV file -- to indicate which voiceover to play.
	- Drag and place the marker to change the emit time for this signal.
	
	To use scripting to control voiceover content, see [VoiceoverManager class](#Voiceover-Manager-Class) in scripting documentation.


## CSV File Format
All voiceover data are stored in a master spreadsheet that has multiple sheets. Each sheet contains all voiceover/subtitle data of all languages for a single animal scene, and will eventually be exported as a seperate CSV file. 
The first line in spreadsheet is column headers, which contain: 
1. key, 
2. audio file name,  
3. lines and timestamps for each language. 

Each of the following line will be a single voiceover object.

 ### Headers
 - **key**: a unique string value used to retrieve this voiceover object
 - **audiofilename**: the name of the audio file (without "_languageAbbrev.wav")
 - **lines_language**: the text to be displayed in the subtitle box for this voiceover audio. All text will be displayed at once, otherwise it need to be split up manually if it's too long to fit in the subtitle box.
	 - To split up text into multiple lines, place a special character '^' in between words.
	 - To determine the duration for each split line, specify in the timestamps column for each inserted '^'.
		 i.e. for an audio file that last 5 seconds:
        
			 timestamps: 0.8, 2.4
			 lines: To be, ^ or not to be,^ that is the question.
             
		
		will display "To be" for 0.8 second, "or not to be," for 1.6 second,  " that is the question." for 5-2.4=2.6 seconds.
 - **timestamps_language**: a sequence of floating numbers indicating when to break up the lines. The numbers should be in the range (0, audioclip.Length).
		 - Note that the number of timestamps should always equal to the number of line seperater character '^'.
 - ...
### Template
See [this google sheet](https://docs.google.com/spreadsheets/d/1l1Cw5til2qUbpMl9ykIZ1Qg7grrDiX_VSg_nM3KkcV0/edit?usp=sharing) for reference.




## Scripting Ducumentation

### CSVToJSON Class


#### Description
A lightweight csv to json converter that reads in a csv file (i.e. "voiceovers_animal.csv") into a list of dictionary of objects. The dictionary is re-organized into arrays of objects if needed, and saved to list of VoiceoverLine objects. This final list is serialized to a json file (i.e. "voiceovers_animal.json") with the wrapper class VoiceoverCollection. The csv file is loaded from csvDir and the json file is saved to jsonDir; both are under the StreamingAssets directory.

#### Properties
```c#
// The directory of csv files to be converted; 
public static string csvDir = Application.streamingAssetsPath + "/" + "VOResources/TextAssets/CSVFiles";

// The folder path to store output json files
public static string jsonDir =Application.streamingAssetsPath + "/" + "VOResources/TextAssets/JSONFiles";
```

#### Public Methods
```c#
public static bool ConvertCSVToJSON(string csvFileName)
```

 - Convert the given CSV file (with naming format "voiceover_animal.csv")  under csvDir to json and store in jsonDir folder. Return true if conversion succeeds, false otherwise. 
 - Throws `IOException` if the directory `csvDir` does not exist or CSV file with name `csvFileName` does not exist.
        
### Voiceover Manager Class

 #### Description
 Defines a singleton scene gameobject that loads the scene json file ("voiceovers_scenename.json") and maintains a dictionary of generated CRC voiceover IDs and VoiceoverLine objects. It is responsible for playing the voiceover audios and displaying the corresponding subtitles.

#### Properties
```c#
// the current scene language for voiceover audio/subtitle, can be set/changed at runtime
public Language current_language = Language.EN;

// scene audio source component for voiceovers; 
//if not assigned, a correspondign AudioSource component will be created for this VoiceoverManager
public static AudioSource audioSource;
```

#### Public Methods
```c#
public bool OnVoiceoverContentChanged(VoiceoverID key, bool stopCurrentIfNecessary)
```

 - Retrive the voiceover value paired to the crc ID from the scene dictionary and try to play the voiceover and set the subtitle text. Returns true if the voiceover is set to be played successfully, false otherwise.
 - `key`: the voiceover id as encoded CRC Int. 
 - `stopCurrentIfNecessary:`a boolean value indicating whether we want to stop the current voiceover to play the new one. Makes no difference if there is no current voiceover/subtitle content.
	  - If `OnVoiceoverContentChanged()` is called when there is already an active subtitle content in display:
	    - setting this param to true will interrupt and stop the current vo/subtitle immediately and play the new vo; 
	    - setting this param to false will not interrupt the current vocieover; the new vo will not get played and the function returns false.
	   


 ```c#
 public void DisableSubtitleCanvas()
```

- Disable the subtitle box attached to the right controller.
 ```c#
 public void EnableSubtitleCanvas()
```

- Enable the subtitle box attached to the right controller.




### Subtitle GUI Manager Class
#### Description
 Defines a singleton scene gameobject that manages the appearance of the subtitle box. Properties of the text and the background can be modified under Canvas component.
 The subtitle box is attached to the dominant controller.
#### Properties
- Forward offset: adjusts the forward distance of the subtitle box from the dominant controller.
- Vertical offset: adjust the upward vertical distance of the subtitle box from the dominant controller.



## To-do List
Search "TODO" in the scripts to view the following to-do tasks.

 - [ ] Get the dominant controller for `SubtitleGUIManager` class. Right now it's set to the right controller by default.
 - [ ] Determine the controller button to disable/enable the subtitle box (or comment out this logic if no longer needed). This is in the `Update()` method in `VoiceoverManager` Class.
 - [ ] Once the subtitle canvas design is finalized, it will be nice to declare the maximum character/word count for a subtitle line to fit in. This gives a reference for determining whether or not to break up a line using timestamps in the CSV files.
 - [ ] Update the Load method for audio files if the directory of all .wav files is changed or we are loading from the asset bundles.
		 Right now audio files are loaded by 
 ```c#
	(AudioClip)Resources.Load(voiceoverLine.audiofilename, typeof(AudioClip)); 
```
 - [ ] If we were to pack JSON files into scene asset bundles and load them from there, the following methods need to be modified:
  
 - CSVToJSON class:
	```c#
	 private static void SaveToJson(VoiceoverCollection voCollection, string jsonFileName)
	```
	Assign the asset bundle name to JSON here.

 - VoiceoverManager class:
	```c#
	 private string ReadSceneJSONFile()
	```
	 Load JSON text assets from scene asset bundles here.

