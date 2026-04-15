using Il2CppRUMBLE.Audio;
using Il2CppRUMBLE.Managers;
using MelonLoader;
using System.Collections;
using UnityEngine;
using UIFramework;
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
    public static class BuildInfo
    {
        public const string Name = "Rumble Additional Sounds";
        public const string Version = "3.1.0";
    }

    public class Main : MelonMod
    {
        //mods Logging Method for easy calling
        internal static void Log(string msg)
        {
            Melon<Main>.Logger.Msg(msg);
        }

        //File Path object[][]s
        internal static string[][] fileNames = new string[][] {
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

        //variables
        internal static string lastScene = "";
        internal static string currentScene = "Loader";

        //variable for # of Setting Toggles (audio groupings)
        internal const int TOGGLESCOUNT = 7;

        public override void OnInitializeMelon()
        {
            Preferences.InitPrefs();
            UI.Register(this, Preferences.TogglesCategory, Preferences.PosesCategory, Preferences.SceneChangeCategory, Preferences.YouTakingDamageCategory, Preferences.OthersTakingDamageCategory, Preferences.PlayerBoundaryKillsCategory, Preferences.HealingCategory, Preferences.LowHealthCategory).OnModSaved += Save;
            LoadSounds();
        }

        public void Save()
        {
            if (Preferences.IsVolumePrefChanged()) { SetVolumes(); }
            Preferences.StoreLastSavedPrefs();
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
                    audioCalls[x][i] = RumbleModdingAPI.RMAPI.AudioManager.CreateAudioCall(fileNames[x][i], Preferences.PrefVolumes[x][i].Value / 100f);
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

        private void SetVolumes()
        {
            //for each audio group
            for (int i = 0; i < TOGGLESCOUNT; i++)
            {
                //stop if list is null (error prevention)
                if (audioCalls[i] == null) { continue; }
                //for each number in volumes list
                for (int x = 0; x < audioCalls[i].Length; x++)
                {
                    //stop if AudioCall is null (error prevention)
                    if (audioCalls[i][x] == null) { continue; }
                    //create new GeneralAudioSettings variable (editing the existing one wasn't changing values I found)
                    GeneralAudioSettings generalSettings = new GeneralAudioSettings();
                    //set volume (defaults to 0 if not done)
                    generalSettings.SetVolume(Preferences.PrefVolumes[i][x].Value / 100f);
                    //set pitch (defaults to 0 if not done)
                    generalSettings.Pitch = 1;
                    //set as the stored AudioCall's generalSettings
                    audioCalls[i][x].generalSettings = generalSettings;
                }
            }
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
            //play scene load sound
            if (Preferences.PrefSceneChangeToggle.Value) { MelonCoroutines.Start(PlaySceneLoadSound()); }
            
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
