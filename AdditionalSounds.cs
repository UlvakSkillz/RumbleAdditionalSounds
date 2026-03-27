using Il2CppRUMBLE.Audio;
using Il2CppRUMBLE.Managers;
using MelonLoader;
using RumbleModUI;
using System;
using System.Collections;
using UnityEngine;
using static Il2CppRUMBLE.Audio.AudioCall;

namespace AdditionalSounds
{
    //Mod Setting Groupings
    public enum SoundsOrder
    {
        MoveFileNames = 0,
        SceneLoadFileNames = 1,
        LocalDamageFileNames = 2,
        RemoteDamageFileNames = 3,
        BoundaryFileName = 4,
        HealFileNames = 5,
        LowHealthFileName = 6
    }

    //MoveFileNames file order as file names
    public enum MovesOrder
    {
        Sprint = 0,
        Flick = 1,
        Explode = 2,
        Hold = 3,
        Parry = 4,
        Dash = 5,
        Cube = 6,
        Uppercut = 7,
        Wall = 8,
        Jump = 9,
        Stomp = 10,
        Ball = 11,
        Kick = 12,
        Disc = 13,
        Straight = 14,
        Pillar = 15
    };

    //SceneLoadOrder file order as file names
    public enum SceneLoadOrder
    {
        EnterGymFromLoaderSound = 0,
        EnterGymFromParkSound = 1,
        EnterGymFromMatchmakingSound = 2,
        EnterParkSound = 3,
        EnterMatchmakingFromGymSound = 4,
        EnterMatchmakingFromMatchmakingSound = 5
    }

    //used to reference in multiple areas but 1 spot edits all
    public static class ModBuildInfo
    {
        public const string Name = "Rumble Additional Sounds";
        public const string Version = "3.0.3";
    }
    public class AdditionalSounds : MelonMod
    {
        //mods Logging Method for easy calling
        internal static void Log(string msg)
        {
            Melon<AdditionalSounds>.Logger.Msg(msg);
        }

        //File Path object[][]s
        private static string[][] fileNames = new string[][] {
            new string[] { //poses
                @"UserData\AdditionalSounds\Sprint.wav",
                @"UserData\AdditionalSounds\Flick.wav",
                @"UserData\AdditionalSounds\Explode.wav",
                @"UserData\AdditionalSounds\Hold.wav",
                @"UserData\AdditionalSounds\Parry.wav",
                @"UserData\AdditionalSounds\Dash.wav",
                @"UserData\AdditionalSounds\Cube.wav",
                @"UserData\AdditionalSounds\Uppercut.wav",
                @"UserData\AdditionalSounds\Wall.wav",
                @"UserData\AdditionalSounds\Jump.wav",
                @"UserData\AdditionalSounds\Stomp.wav",
                @"UserData\AdditionalSounds\Ball.wav",
                @"UserData\AdditionalSounds\Kick.wav",
                @"UserData\AdditionalSounds\Disc.wav",
                @"UserData\AdditionalSounds\Straight.wav",
                @"UserData\AdditionalSounds\Pillar.wav" },
            new string[] { //scene changes
                @"UserData\AdditionalSounds\EnterGymFromLoaderSound.wav",
                @"UserData\AdditionalSounds\EnterGymFromParkSound.wav",
                @"UserData\AdditionalSounds\EnterGymFromMatchmakingSound.wav",
                @"UserData\AdditionalSounds\EnterParkSound.wav",
                @"UserData\AdditionalSounds\EnterMatchmakingFromGymSound.wav",
                @"UserData\AdditionalSounds\EnterMatchmakingFromMatchmakingSound.wav" },
            new string[] { //local damages
                @"UserData\AdditionalSounds\LocalDamageSound0.wav",
                @"UserData\AdditionalSounds\LocalDamageSound1.wav",
                @"UserData\AdditionalSounds\LocalDamageSound2.wav",
                @"UserData\AdditionalSounds\LocalDamageSound3.wav",
                @"UserData\AdditionalSounds\LocalDamageSound4.wav",
                @"UserData\AdditionalSounds\LocalDamageSound5.wav",
                @"UserData\AdditionalSounds\LocalDamageSound6.wav",
                @"UserData\AdditionalSounds\LocalDamageSound7.wav",
                @"UserData\AdditionalSounds\LocalDamageSound8.wav" },
            new string[] { //remote damages
                @"UserData\AdditionalSounds\RemoteDamageSound0.wav",
                @"UserData\AdditionalSounds\RemoteDamageSound1.wav",
                @"UserData\AdditionalSounds\RemoteDamageSound2.wav",
                @"UserData\AdditionalSounds\RemoteDamageSound3.wav",
                @"UserData\AdditionalSounds\RemoteDamageSound4.wav",
                @"UserData\AdditionalSounds\RemoteDamageSound5.wav",
                @"UserData\AdditionalSounds\RemoteDamageSound6.wav",
                @"UserData\AdditionalSounds\RemoteDamageSound7.wav",
                @"UserData\AdditionalSounds\RemoteDamageSound8.wav" },
            new string[] { //scene boundaries
                @"UserData\AdditionalSounds\PlayerTooLow.wav",
                @"UserData\AdditionalSounds\PlayerTooHigh.wav" },
            new string[] { //heals
                @"UserData\AdditionalSounds\LocalHealSound.wav",
                @"UserData\AdditionalSounds\RemoteHealSound.wav" },
            new string[] { //low health
                @"UserData\AdditionalSounds\LowHealthSound.wav" } };
        internal static bool[][] fileExists = new bool[TOGGLESCOUNT][];
        internal static AudioCall[][] audioCalls = new AudioCall[TOGGLESCOUNT][];
        internal static float[][] volumes = new float[TOGGLESCOUNT][];

        //ModUI Mods
        public Mod RumbleAdditionalSounds = new Mod();
        public Mod RumbleAdditionalSoundsVolumes = new Mod();

        //variables
        internal static string lastScene = "";
        internal static string currentScene = "Loader";
        internal static bool[] togglesIsEnabled = new bool[TOGGLESCOUNT];
        internal static int lowHealthAmount = 7;
        internal static bool volumesAdded = false;

        //variable for # of Setting Toggles (audio groupings)
        private const int TOGGLESCOUNT = 7;

        //initializes things
        public override void OnLateInitializeMelon()
        {
            //sets ModUI Mod Name
            RumbleAdditionalSounds.ModName = "Additional Sounds";
            //sets mods Version ModUI sees and checks with Thunderstore (should match Version elsewhere)
            RumbleAdditionalSounds.ModVersion = ModBuildInfo.Version;
            //sets thee UserData folder Name for storing Settings.txt in
            RumbleAdditionalSounds.SetFolder("AdditionalSounds");
            //audio groupings
            RumbleAdditionalSounds.AddToList("Poses", true, 0, "Toggles Pose Sounds from Playing.", new Tags { });
            RumbleAdditionalSounds.AddToList("Scene Change", true, 0, "Toggles Scene Change Sounds from Playing.", new Tags { });
            RumbleAdditionalSounds.AddToList("You Taking Damage", true, 0, "Toggles You Taking Damage Sounds from Playing.", new Tags { });
            RumbleAdditionalSounds.AddToList("Others Taking Damage", true, 0, "Toggles Others Taking Damage Sounds from Playing.", new Tags { });
            RumbleAdditionalSounds.AddToList("Player Boundary Kills", true, 0, "Toggles Kill Boundary Sounds from Playing.", new Tags { });
            RumbleAdditionalSounds.AddToList("Healing", true, 0, "Toggles Healing Sounds from Playing.", new Tags { });
            RumbleAdditionalSounds.AddToList("Low Health", true, 0, "Toggles Low Health Sound from Playing. In Matchmaking Only.", new Tags { });
            //amount to start playing the low health audio (audio stops at end of rounds or above trigger amount)
            RumbleAdditionalSounds.AddToList("Low Health Amount", 7, "Sets the Amount of Health to Start Triggering the Low Health Sound", new Tags { });
            //bool to toggle on/off Volume ModUI Mod
            RumbleAdditionalSounds.AddToList("Edit Volumes", false, 0, "Toggles Volumes Mod in ModUI to Edit individual Sound Clip Volumes. Look near the Bottom of the Mod List. YOU MUST CLOSE AND REOPEN MODUI TO SEE IT.", new Tags { DoNotSave = true });
            //loads saved settings
            RumbleAdditionalSounds.GetFromFile();
            //runs Save when the save button is pressed (while this mod is selected)
            RumbleAdditionalSounds.ModSaved += Save;
            //setsup adding the mod to ModUI. will run when ModUI is ready
            UI.instance.UI_Initialized += delegate { UI.instance.AddMod(RumbleAdditionalSounds); };
            //initializes togglesIsEnabled list and lowHealthAmount variable (will skip adding/removing Volumes Mod as volumesAdded doesn't save and always starts off)
            Save();
            //creates but doesn't add the Volumes Mod for ModUI
            CreateVolumesMod();
            //initializes the volumes lists with ModUI settings (will skip setting audio in method as AudioCalls list is null)
            SaveVolumes();
            //initializes the audioCalls lists and fileExists lists with data as files are found/loaded
            LoadSounds();
        }

        private void CreateVolumesMod()
        {
            //Sets Volumes Mod Name
            RumbleAdditionalSoundsVolumes.ModName = "Additional Sounds Volumes";
            //Sets Volumes Mod Version (never changes so old settings don't reset when updating)
            RumbleAdditionalSoundsVolumes.ModVersion = "1.0.0";
            //sets save file to it's own folder
            RumbleAdditionalSoundsVolumes.SetFolder("AdditionalSoundsVolumes");
            //for each fileNames grouping
            for (int i = 0; i < TOGGLESCOUNT; i++)
            {
                //for each file name in the grouping
                for (int x = 0; x < fileNames[i].Length; x++)
                {
                    //get just the name of the file, no folders or .wav
                    string[] fileNamesSplit = fileNames[i][x].Replace(".wav", "").Split('\\');
                    string fileName = fileNamesSplit[fileNamesSplit.Length - 1];
                    //create volume setting for the file
                    RumbleAdditionalSoundsVolumes.AddToList($"{fileName} Volume", 50f, $"Edits {fileName}'s Volume in Game. 0 - 100.", new Tags { });
                }
            }
            //load stored settings
            RumbleAdditionalSoundsVolumes.GetFromFile();
            //run SaveVolumes when ModUI's Save button is pressed (while this mod is selected)
            RumbleAdditionalSoundsVolumes.ModSaved += SaveVolumes;
        }

        private void AddVolumesMod()
        { //adds the volumes mod to ModUI's Selector (user must turn ModUI off/on)
            UI.instance.AddMod(RumbleAdditionalSoundsVolumes);
        }

        private void RemoveVolumesMod()
        { //adds the volumes mod to ModUI's Selector (user must turn ModUI off/on)
            UI.instance.RemoveMod(RumbleAdditionalSoundsVolumes);
        }

        private void SaveVolumes()
        {
            //variable to keep track of what setting in ModUI it is at
            int settingSpot = 0;
            //variable to keep track of how many files (didn't hardcode so it's modular)
            int filesCount = 0;
            //for each loop to get the file names
            foreach (string[] names in fileNames)
            {
                filesCount += names.Length;
            }
            //for each audio grouping
            for (int i = 0; i < TOGGLESCOUNT; i++)
            {
                //create a new list of volumes
                volumes[i] = new float[fileNames[i].Length];
                //for each file in the audio grouping
                for (int x = 0; x < fileNames[i].Length; x++)
                { //clamp and store in the volumes list
                    //clamps to 0-100
                    float savedValueClamped = Mathf.Clamp((float)RumbleAdditionalSoundsVolumes.Settings[settingSpot].SavedValue, 0f, 100f);
                    //if value was clamped
                    if ((float)RumbleAdditionalSoundsVolumes.Settings[settingSpot].SavedValue != savedValueClamped)
                    { //set shown saved values in ModUI (UI.instance.ForceRefresh() would show new values, but errors if no description) (User nust close/reopen ModUI to see updated values)
                        RumbleAdditionalSoundsVolumes.Settings[settingSpot].Value = savedValueClamped;
                        RumbleAdditionalSoundsVolumes.Settings[settingSpot].SavedValue = savedValueClamped;
                    }
                    //store clamped value in volumes list
                    volumes[i][x] = savedValueClamped;
                    //increase settings spot
                    settingSpot++;
                }
            }
            //sets the audioCalls list's volumes to volumes list volumes
            SetVolumes();
        }

        private void SetVolumes()
        {
            //for each audio group
            for (int i = 0; i < TOGGLESCOUNT; i++)
            {
                //stop if list is null (error prevention)
                if (audioCalls[i] == null) { continue; }
                //for each number in volumes list
                for (int x = 0; x < volumes[i].Length; x++)
                {
                    //stop if AudioCall is null (error prevention)
                    if (audioCalls[i][x] == null) { continue; }
                    //create new GeneralAudioSettings variable (editing the existing one wasn't changing values I found)
                    GeneralAudioSettings generalSettings = new GeneralAudioSettings();
                    //set volume (defaults to 0 if not done)
                    generalSettings.SetVolume(volumes[i][x] / 100f);
                    //set pitch (defaults to 0 if not done)
                    generalSettings.Pitch = 1;
                    //set as the stored AudioCall's generalSettings
                    audioCalls[i][x].generalSettings = generalSettings;
                }
            }
        }

        public void Save()
        {
            //for each audio group
            for (int i = 0; i < TOGGLESCOUNT; i++)
            { //set if it's on or off
                togglesIsEnabled[i] = (bool)RumbleAdditionalSounds.Settings[i].SavedValue;
            }
            //set the low health value
            lowHealthAmount = (int)RumbleAdditionalSounds.Settings[TOGGLESCOUNT].SavedValue;
            //checks if Edit Volumes setting is toggled on (starts off every time so player had to turn it on)
            bool editVolumes = (bool)RumbleAdditionalSounds.Settings[TOGGLESCOUNT + 1].SavedValue;
            //if toggled on and ModUI Volumes Mod hasn't been added
            if (editVolumes && !volumesAdded)
            {
                //edit variable to show it's been added
                volumesAdded = true;
                //add volumes mod to ModUI
                AddVolumesMod();
            }
            //if toggled off and ModUI Volumes Mod has been added
            else if (!editVolumes && volumesAdded)
            {
                //remove volumes mod to ModUI
                RemoveVolumesMod();
                //edit variable to show it's been added
                volumesAdded = false;
            }
        }

        private void LoadSounds()
        {
            //clear any stored messages
            string msgToSend = "";
            //for each audio group
            for (int x = 0; x < TOGGLESCOUNT; x++)
            {
                //initialize the lists for if the file exists and AudioCalls
                fileExists[x] = new bool[fileNames[x].Length];
                audioCalls[x] = new AudioCall[fileNames[x].Length];
                //for each file in the audio group
                for (int i = 0; i < fileNames[x].Length; i++)
                {
                    //create the AudioCall with the file
                    audioCalls[x][i] = RumbleModdingAPI.RMAPI.AudioManager.CreateAudioCall(fileNames[x][i], volumes[x][i] / 100f);
                    //if something went wrong and the AudioCall is null
                    if (audioCalls[x][i] == null)
                    { //issue catch
                        //set fileExists to false as soemthing went wrong
                        fileExists[x][i] = false;
                        //grab file name
                        string[] filePath = fileNames[x][i].Split('\\');
                        string name = filePath[filePath.Length - 1];
                        //store message
                        msgToSend += Environment.NewLine + name;
                    }
                    else
                    {
                        if ((x == (int)SoundsOrder.SceneLoadFileNames) //scene change
                            || (x == (int)SoundsOrder.RemoteDamageFileNames) //remote damage
                            || ((x == (int)SoundsOrder.HealFileNames) && (i == 1)) //remote heal
                            || (x == (int)SoundsOrder.LowHealthFileName)) //low health
                        {
                            //set audio falloff to linear for better hearing
                            SetAudioRollOffToLinear(x, i);
                        }
                        //file loaded file, so file exists
                        fileExists[x][i] = true;
                    }
                }
            }
            //send log if there is one
            if (msgToSend != "") { Log("Optional Files Not Found:" + msgToSend); }
            Log("Initialized");
        }

        private void SetAudioRollOffToLinear(int soundGrouping, int spot)
        {
            //grab existing spacial settings
            SpatialAudioSettings spatialAudioSettings = audioCalls[soundGrouping][spot].spatialSettings;
            //set audio roll off more to linear
            spatialAudioSettings.AudioRollOff = AudioRolloffMode.Linear;
            //set as clip's settings
            audioCalls[soundGrouping][spot].spatialSettings = spatialAudioSettings;
        }

        //called when a scene is loaded
        public override void OnSceneWasLoaded(int buildIndex, string sceneName)
        {
            //set's last/current scene variables
            lastScene = currentScene;
            currentScene = sceneName;
            //stop the Low Health Sound if it's playing
            Patches.SetHealth.StopLowHealthSoundEffect();
            //stop if scene change toggle is off
            if (!togglesIsEnabled[(int)SoundsOrder.SceneLoadFileNames]) { return; }
            //play scene load sound
            MelonCoroutines.Start(PlaySceneLoadSound());
        }

        private IEnumerator PlaySceneLoadSound()
        {
            //wait 1 second to ensure the entire local player spawns and initializes
            yield return new WaitForSeconds(1f);
            Vector3 playerPos;
            try
            {
                playerPos = PlayerManager.instance.localPlayer.Controller.PlayerCamera.gameObject.transform.position;
            }
            catch
            {
                switch (currentScene)
                {
                    case "Gym":
                        playerPos = new Vector3(2.9169f, 1.7373f, -2.1799f);
                        break;
                    case "Park":
                        playerPos = new Vector3(-22.259f, -1.0704f, -11.6696f);
                        break;
                    case "Map0":
                    case "Map1":
                        playerPos = new Vector3(0f, 1.449f, 0f);
                        break;
                    default:
                        break;
                }
                playerPos = Vector3.zero;
            }
            // basically (if)/(else if) (currentScene == (list of scene names)
            switch (currentScene)
            {
                case "Gym":
                    // basically (if)/(else if) (lastScene == (list of scene names)
                    switch (lastScene)
                    {
                        case "Loader":
                            //wait's 3 seconds to ensure at least 4 seconds total have passed to allow audio to finish loading (no sound plays otherwise)
                            yield return new WaitForSeconds(3f);
                            if (fileExists[(int)SoundsOrder.SceneLoadFileNames][(int)SceneLoadOrder.EnterGymFromLoaderSound])
                            { //if file exists, play Loader->Gym Audio
                                RumbleModdingAPI.RMAPI.AudioManager.PlaySound(audioCalls[(int)SoundsOrder.SceneLoadFileNames][(int)SceneLoadOrder.EnterGymFromLoaderSound], playerPos);
                            }
                            break;
                        case "Park":
                            if (fileExists[(int)SoundsOrder.SceneLoadFileNames][(int)SceneLoadOrder.EnterGymFromParkSound])
                            { //if file exists, play Park->Gym Audio
                                RumbleModdingAPI.RMAPI.AudioManager.PlaySound(audioCalls[(int)SoundsOrder.SceneLoadFileNames][(int)SceneLoadOrder.EnterGymFromParkSound], playerPos);
                            }
                            break;
                        case "Map0":
                        case "Map1":
                            if (fileExists[(int)SoundsOrder.SceneLoadFileNames][(int)SceneLoadOrder.EnterGymFromMatchmakingSound])
                            { //if file exists, play Matchmaking->Gym Audio
                                RumbleModdingAPI.RMAPI.AudioManager.PlaySound(audioCalls[(int)SoundsOrder.SceneLoadFileNames][(int)SceneLoadOrder.EnterGymFromMatchmakingSound], playerPos);
                            }
                            break;
                        default:
                            break;
                    }
                    break;
                case "Park":
                    if (fileExists[(int)SoundsOrder.SceneLoadFileNames][(int)SceneLoadOrder.EnterParkSound])
                    { //if file exists, play Anywhere->Park Audio
                        RumbleModdingAPI.RMAPI.AudioManager.PlaySound(audioCalls[(int)SoundsOrder.SceneLoadFileNames][(int)SceneLoadOrder.EnterParkSound], playerPos);
                    }
                    break;
                case "Map0":
                case "Map1":
                    // basically (if)/(else if) (lastScene == (list of scene names)
                    switch (lastScene)
                    {
                        case "Gym":
                            if (fileExists[(int)SoundsOrder.SceneLoadFileNames][(int)SceneLoadOrder.EnterMatchmakingFromGymSound])
                            { //if file exists, play Gym->Matchmaking Audio
                                RumbleModdingAPI.RMAPI.AudioManager.PlaySound(audioCalls[(int)SoundsOrder.SceneLoadFileNames][(int)SceneLoadOrder.EnterMatchmakingFromGymSound], playerPos);
                            }
                            break;
                        case "Map0":
                        case "Map1":
                            if (fileExists[(int)SoundsOrder.SceneLoadFileNames][(int)SceneLoadOrder.EnterMatchmakingFromMatchmakingSound])
                            { //if file exists, play Matchmaking->Matchmaking Audio
                                RumbleModdingAPI.RMAPI.AudioManager.PlaySound(audioCalls[(int)SoundsOrder.SceneLoadFileNames][(int)SceneLoadOrder.EnterMatchmakingFromMatchmakingSound], playerPos);
                            }
                            break;
                        default:
                            break;
                    }
                    break;
                default:
                    break;
            }
            yield break;
        }
    }
}
