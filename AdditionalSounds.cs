using MelonLoader;
using System;
using System.Media;
using System.Threading;
using UnityEngine;
using RUMBLE.Players.Subsystems;
using RUMBLE.Managers;
using RUMBLE.Players;

namespace RumbleSoundsOnSceneChange
{
    public class AdditionalSounds : MelonMod
    {
        //File Paths
        private string[] FilePaths = new string[13];
        //variables
        private string lastScene = "";
        private string currentScene = "";
        private static Thread[] threads = new Thread[4];
        private static bool[] threadActive = new bool[threads.Length];
        bool playedTooLowSound = false;
        bool tooLowSoundListen = false;
        bool playedTooHighSound = false;
        bool tooHighSoundListen = false;
        private float tooLowRespawnHeight = 0;
        private float tooLowHeight = 0;
        private float tooHighRespawnHeight = 0;
        private float tooHighHeight = 0;
        private GameObject player;
        private PlayerManager playerManager;
        private Player localPlayer;
        int localPlayerHealth = -1;
        bool finishedHealingAfterDeath = false;

        //initializes things
        public override void OnInitializeMelon()
        {
            base.OnInitializeMelon();
            //set File Paths
            FilePaths[0] = @"UserData\AdditionalSounds\GameLoadSound.wav";
            FilePaths[1] = @"UserData\AdditionalSounds\EnterGymFromLoaderSound.wav";
            FilePaths[2] = @"UserData\AdditionalSounds\EnterGymFromParkSound.wav";
            FilePaths[3] = @"UserData\AdditionalSounds\EnterGymFromMatchmakingSound.wav";
            FilePaths[4] = @"UserData\AdditionalSounds\EnterParkSound.wav";
            FilePaths[5] = @"UserData\AdditionalSounds\EnterMatchmakingFromGymSound.wav";
            FilePaths[6] = @"UserData\AdditionalSounds\EnterMatchmakingFromMatchmakingSound.wav";
            FilePaths[7] = @"UserData\AdditionalSounds\TooLowSound.wav";
            FilePaths[8] = @"UserData\AdditionalSounds\TooHighSound.wav";
            FilePaths[9] = @"UserData\AdditionalSounds\SelfDamagedSound.wav";
            FilePaths[10] = @"UserData\AdditionalSounds\EnemyDamagedSound.wav";
            FilePaths[11] = @"UserData\AdditionalSounds\SelfHealSound.wav";
            FilePaths[12] = @"UserData\AdditionalSounds\LowHealthSound.wav";
            threadActive[0] = false;
            threadActive[1] = false;
            MelonLogger.Msg("Initialized");
        }

        //run every update
        public override void OnUpdate()
        {
            //normal updates
            base.OnUpdate();
            if ((currentScene != "") && (currentScene != "Loader"))
            {
                //if player null
                if (player == null)
                {
                    try
                    {
                        //if in matchmaking
                        if ((currentScene == "Map0") || (currentScene == "Map1"))
                        {
                            //initializes remote player fields
                            PlayerHealth enemyHealth = GameObject.Find("Health/Remote").transform.parent.GetComponent<PlayerHealth>();
                            //create listener for enemy player
                            enemyHealth.onDamageTaken.AddListener(new System.Action<short>(component =>
                            {
                                PlaySoundIfFileExists(FilePaths[10], 2);
                            }));
                        }
                        GameObject playermanagerTemp = GameObject.Find("Game Instance/Initializable/PlayerManager");
                        playerManager = playermanagerTemp.gameObject.GetComponent<PlayerManager>();
                        localPlayer = playerManager.localPlayer;
                        //initializes local player fields
                        player = localPlayer.Controller.gameObject.transform.GetChild(2).GetChild(13).GetChild(0).gameObject;
                        //set the local health
                        localPlayerHealth = localPlayer.Data.HealthPoints;
                    }
                    catch {}
                }
                else
                {
                    MelonLogger.Msg("Y: " + player.transform.position.y);
                    //if listening to make the too low sound
                    if (tooLowSoundListen)
                    {
                        CheckForTooLow();
                    }
                    //if listening to make the too high sound
                    if (tooHighSoundListen)
                    {
                        CheckForTooHigh();
                    }
                    checkLocalHealth();
                }
            }
        }

        //called when a scene is loaded
        public override void OnSceneWasLoaded(int buildIndex, string sceneName)
        {
            base.OnSceneWasLoaded(buildIndex, sceneName);
            //update last and current scenes
            lastScene = currentScene;
            currentScene = sceneName;
            finishedHealingAfterDeath = false;
            localPlayerHealth = -1;
            //Do things dependent on currentScene and lastScene
            switch (currentScene)
            {
                case "Loader": //if currentScene is loader (t-pose start screen)
                    PlaySoundIfFileExists(FilePaths[0], 0);
                    break;
                case "Gym": //if currentScene is gym
                    //set height limits for sounds
                    tooLowRespawnHeight = 0;
                    tooLowHeight = -10;
                    tooHighRespawnHeight = 1;
                    tooHighHeight = 22;
                    switch (lastScene)
                    {
                        case "Loader": //if lastScene is the loader
                            PlaySoundIfFileExists(FilePaths[1], 0);
                            break;
                        case "Park": //if lastScene is the park
                            PlaySoundIfFileExists(FilePaths[2], 0);
                            break;
                        case "Map0": //if lastScene is the ring
                        case "Map1": //or the pit
                            PlaySoundIfFileExists(FilePaths[3], 0);
                            break;
                    }
                    break;
                case "Park": //if currentScene is park
                    //set height limits for sounds
                    tooLowRespawnHeight = -3;
                    tooLowHeight = -11;
                    tooHighRespawnHeight = 4f;
                    tooHighHeight = 26f;
                    PlaySoundIfFileExists(FilePaths[4], 0);
                    break;
                case "Map0": //if currentScene is the ring
                case "Map1": //or the pit
                    //if ring
                    if (currentScene == "Map0")
                    {
                        //set height limits for sounds
                        tooLowRespawnHeight = 0;
                        tooLowHeight = -6;
                        tooHighRespawnHeight = 1;
                        tooHighHeight = 12.5f;
                    }
                    //if pit
                    else if (currentScene == "Map1")
                    {
                        //set height limits for sounds
                        tooLowRespawnHeight = 0;
                        tooLowHeight = -6;
                        tooHighRespawnHeight = 1;
                        tooHighHeight = 12.75f;
                    }
                    switch (lastScene)
                    {
                        case "Gym": //if matching from the gym
                            PlaySoundIfFileExists(FilePaths[5], 0);
                            break;
                        case "Map0": //if rematching on the ring
                        case "Map1": //or the pit
                            PlaySoundIfFileExists(FilePaths[6], 0);
                            break;
                    }
                    break;
            }
            //activate height sound listeners
            tooLowSoundListen = true;
            tooHighSoundListen = true;
        }

        //checks local health for changes
        private void checkLocalHealth()
        {
            bool playHeartbeat = false;
            //if player is dead
            if (localPlayer.Data.HealthPoints == 0)
            {
                finishedHealingAfterDeath = false;
            }
            //if player full health
            else if (localPlayerHealth == 20)
            {
                finishedHealingAfterDeath = true;
            }
            //if not done healing
            else if ((!finishedHealingAfterDeath) || (localPlayerHealth == -1))
            {
                localPlayerHealth = localPlayer.Data.HealthPoints;
                return;
            }
            //if under 8 health
            else if (localPlayer.Data.HealthPoints <= 7)
            {
                playHeartbeat = true;
            }
            //if health changed
            if (localPlayer.Data.HealthPoints != localPlayerHealth)
            {
                if (finishedHealingAfterDeath)
                {
                    //if player hurt
                    if (localPlayer.Data.HealthPoints < localPlayerHealth)
                    {
                        PlaySoundIfFileExists(FilePaths[9], 0);
                    }
                    //if player healed
                    else if (localPlayer.Data.HealthPoints > localPlayerHealth)
                    {
                        PlaySoundIfFileExists(FilePaths[11], 0);
                    }
                }
                localPlayerHealth = localPlayer.Data.HealthPoints;
            }
            if (playHeartbeat)
            {
                PlaySoundIfFileExists(FilePaths[12], 3);
            }
        }

        //checks for fall 
        private void CheckForTooLow()
        {
            //if player has respawned
            if ((player.transform.position.y >= tooLowRespawnHeight) && (playedTooLowSound))
            {
                playedTooLowSound = false;
            }
            //if player fell
            else if ((player.transform.position.y <= tooLowHeight) && (!playedTooLowSound))
            {
                playedTooLowSound = true;
                PlaySoundIfFileExists(FilePaths[7], 1);
            }
        }

        //checks for fall 
        private void CheckForTooHigh()
        {
            //if player has respawned
            if ((player.transform.position.y <= tooHighRespawnHeight) && (playedTooHighSound))
            {
                playedTooHighSound = false;
            }
            //if player is too high
            else if ((player.transform.position.y >= tooHighHeight) && (!playedTooHighSound))
            {
                playedTooHighSound = true;
                PlaySoundIfFileExists(FilePaths[8], 1);
            }
        }

        //Plays the File Sound if it Exists
        private void PlaySoundIfFileExists(string soundFilePath, int threadToPlayOn)
        {
            //Check if the sound file exists
            if (System.IO.File.Exists(soundFilePath))
            {
                //Ensure that only one sound is playing at a time
                if (threadActive[threadToPlayOn])
                {
                    return;
                }
                try
                {
                    //Create a SoundPlayer instance with the specified sound file path
                    using (SoundPlayer player = new SoundPlayer(soundFilePath))
                    {
                        //Set flag to indicate that a sound is currently playing
                        threadActive[threadToPlayOn] = true;
                        //Create a new thread if no thread is active
                        if (threads[threadToPlayOn] == null || !threads[threadToPlayOn].IsAlive)
                        {
                            threads[threadToPlayOn] = new Thread(() =>
                            {
                                //Use PlaySync for synchronous playback
                                player.PlaySync();
                                //Reset flag to indicate that the sound has finished playing
                                threadActive[threadToPlayOn] = false;
                            });
                            //Start the thread
                            threads[threadToPlayOn].Start();
                        }
                    }
                }
                catch (Exception ex)
                {
                    MelonLogger.Msg($"Error playing sound: {ex.Message}");
                }
            }
            else
            {
                MelonLogger.Msg("Sound File Doesn't Exist: " + soundFilePath);
            }
        }
    }
}
