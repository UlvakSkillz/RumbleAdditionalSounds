using MelonLoader;
using System;
using System.Threading;
using UnityEngine;
using Il2CppRUMBLE.Players.Subsystems;
using Il2CppRUMBLE.Managers;
using Il2CppRUMBLE.Players;
using System.Collections;
using MelonLoader.Utils;
using NAudio.Wave;
using HarmonyLib;
using Il2CppRUMBLE.Poses;

namespace RumbleSoundsOnSceneChange
{

    public class AdditionalSounds : MelonMod
    {

        [HarmonyPatch(typeof(PlayerPoseSystem), "OnPoseSetCompleted", new Type[] { typeof(PoseSet) })]
        private static class PosePatch
        {
            private static void Postfix(PoseSet set)
            {
                PlayMoveName(set.name);
            }
        }

        //File Paths
        private static string[] FilePaths = new string[27];
        public static string[] moveFileNames = new string[16];
        private static bool[] fileExists = new bool[43];
        //variables
        private string lastScene = "";
        private string currentScene = "Loader";
        private static Thread[] threads = new Thread[5];
        private static bool[] threadActive = new bool[threads.Length]; //sceneChange & self damage, too High/Low, enemyDamage, heartbeat
        bool playedTooLowSound = false;
        bool tooLowSoundListen = false;
        bool playedTooHighSound = false;
        bool tooHighSoundListen = false;
        private float tooLowRespawnHeight = 0;
        private float tooLowHeight = 0;
        private float tooHighRespawnHeight = 0;
        private float tooHighHeight = 0;
        private GameObject player;
        private Player localPlayer;
        int localPlayerHealth = -1;
        int remotePlayerHealth = -1;
        bool finishedHealingAfterDeath = false;
        bool checkHealth = false;
        bool endHealingCheck = false;

        public static void PlayMoveName(string poseName)
        {
            string fileName = "";
            int fileSpot = 0;
            switch (poseName)
            {
                case "SprintingPoseSet":
                    fileName = moveFileNames[0];
                    fileSpot = FilePaths.Length;
                    break;
                case "PoseSetFlick":
                    fileName = moveFileNames[1];
                    fileSpot = FilePaths.Length + 1;
                    break;
                case "PoseSetExplode":
                    fileName = moveFileNames[2];
                    fileSpot = FilePaths.Length + 2;
                    break;
                case "PoseSetHoldRight":
                    fileName = moveFileNames[3];
                    fileSpot = FilePaths.Length + 3;
                    break;
                case "PoseSetHoldLeft":
                    fileName = moveFileNames[3];
                    fileSpot = FilePaths.Length + 3;
                    break;
                case "PoseSetParry":
                    fileName = moveFileNames[4];
                    fileSpot = FilePaths.Length + 4;
                    break;
                case "PoseSetDash":
                    fileName = moveFileNames[5];
                    fileSpot = FilePaths.Length + 5;
                    break;
                case "PoseSetSpawnCube":
                    fileName = moveFileNames[6];
                    fileSpot = FilePaths.Length + 6;
                    break;
                case "PoseSetUppercut":
                    fileName = moveFileNames[7];
                    fileSpot = FilePaths.Length + 7;
                    break;
                case "PoseSetWall_Grounded":
                    fileName = moveFileNames[8];
                    fileSpot = FilePaths.Length + 8;
                    break;
                case "PoseSetRockjump":
                    fileName = moveFileNames[9];
                    fileSpot = FilePaths.Length + 9;
                    break;
                case "PoseSetStomp":
                    fileName = moveFileNames[10];
                    fileSpot = FilePaths.Length + 10;
                    break;
                case "PoseSetBall":
                    fileName = moveFileNames[11];
                    fileSpot = FilePaths.Length + 11;
                    break;
                case "PoseSetKick":
                    fileName = moveFileNames[12];
                    fileSpot = FilePaths.Length + 12;
                    break;
                case "PoseSetDisc":
                    fileName = moveFileNames[13];
                    fileSpot = FilePaths.Length + 13;
                    break;
                case "PoseSetStraight":
                    fileName = moveFileNames[14];
                    fileSpot = FilePaths.Length + 14;
                    break;
                case "PoseSetSpawnPillar":
                    fileName = moveFileNames[15];
                    fileSpot = FilePaths.Length + 15;
                    break;
                default:
                    fileSpot = -1;
                    break;
            }
            if ((fileSpot != -1) && fileExists[fileSpot])
            {
                if (fileSpot == FilePaths.Length)
                {
                    PlaySoundIfFileExists(fileName, 4);
                }
                else
                {
                    PlaySoundIfFileExists(fileName, 2);
                }
            }
        }

        //initializes things
        public override void OnInitializeMelon()
        {
            //set File Paths
            FilePaths[0] = @"\AdditionalSounds\GameLoadSound.mp3";
            FilePaths[1] = @"\AdditionalSounds\EnterGymFromLoaderSound.mp3";
            FilePaths[2] = @"\AdditionalSounds\EnterGymFromParkSound.mp3";
            FilePaths[3] = @"\AdditionalSounds\EnterGymFromMatchmakingSound.mp3";
            FilePaths[4] = @"\AdditionalSounds\EnterParkSound.mp3";
            FilePaths[5] = @"\AdditionalSounds\EnterMatchmakingFromGymSound.mp3";
            FilePaths[6] = @"\AdditionalSounds\EnterMatchmakingFromMatchmakingSound.mp3";
            FilePaths[7] = @"\AdditionalSounds\TooLowSound.mp3";
            FilePaths[8] = @"\AdditionalSounds\TooHighSound.mp3";
            FilePaths[9] = @"\AdditionalSounds\SelfDamagedSound1.mp3";
            FilePaths[10] = @"\AdditionalSounds\EnemyDamagedSound1.mp3";
            FilePaths[11] = @"\AdditionalSounds\SelfHealSound.mp3";
            FilePaths[12] = @"\AdditionalSounds\LowHealthSound.mp3";
            FilePaths[13] = @"\AdditionalSounds\SelfDamagedSound2.mp3";
            FilePaths[14] = @"\AdditionalSounds\EnemyDamagedSound2.mp3";
            FilePaths[15] = @"\AdditionalSounds\SelfDamagedSound3.mp3";
            FilePaths[16] = @"\AdditionalSounds\EnemyDamagedSound3.mp3";
            FilePaths[17] = @"\AdditionalSounds\SelfDamagedSound4.mp3";
            FilePaths[18] = @"\AdditionalSounds\EnemyDamagedSound4.mp3";
            FilePaths[19] = @"\AdditionalSounds\SelfDamagedSound5.mp3";
            FilePaths[20] = @"\AdditionalSounds\EnemyDamagedSound5.mp3";
            FilePaths[21] = @"\AdditionalSounds\SelfDamagedSound6.mp3";
            FilePaths[22] = @"\AdditionalSounds\EnemyDamagedSound6.mp3";
            FilePaths[23] = @"\AdditionalSounds\SelfDamagedSound7.mp3";
            FilePaths[24] = @"\AdditionalSounds\EnemyDamagedSound7.mp3";
            FilePaths[25] = @"\AdditionalSounds\SelfDamagedSound8.mp3";
            FilePaths[26] = @"\AdditionalSounds\EnemyDamagedSound8.mp3";
            moveFileNames[0] = @"\AdditionalSounds\Sprint.mp3";
            moveFileNames[1] = @"\AdditionalSounds\Flick.mp3";
            moveFileNames[2] = @"\AdditionalSounds\Explode.mp3";
            moveFileNames[3] = @"\AdditionalSounds\Hold.mp3";
            moveFileNames[4] = @"\AdditionalSounds\Parry.mp3";
            moveFileNames[5] = @"\AdditionalSounds\Dash.mp3";
            moveFileNames[6] = @"\AdditionalSounds\Cube.mp3";
            moveFileNames[7] = @"\AdditionalSounds\Uppercut.mp3";
            moveFileNames[8] = @"\AdditionalSounds\Wall.mp3";
            moveFileNames[9] = @"\AdditionalSounds\Jump.mp3";
            moveFileNames[10] = @"\AdditionalSounds\Stomp.mp3";
            moveFileNames[11] = @"\AdditionalSounds\Ball.mp3";
            moveFileNames[12] = @"\AdditionalSounds\Kick.mp3";
            moveFileNames[13] = @"\AdditionalSounds\Disc.mp3";
            moveFileNames[14] = @"\AdditionalSounds\Straight.mp3";
            moveFileNames[15] = @"\AdditionalSounds\Pillar.mp3";
            threadActive[0] = false;
            threadActive[1] = false;
            threadActive[2] = false;
            threadActive[3] = false;
            threadActive[4] = false;
            int i = 0;
            foreach(string path in FilePaths)
            {
                if (!System.IO.File.Exists(MelonEnvironment.UserDataDirectory + path))
                {
                    MelonLogger.Msg("(Optional) Sound File does not Exist at File Path: " + path);
                    fileExists[i] = false;
                }
                else
                {
                    fileExists[i] = true;
                }
                i++;
            }
            foreach (string path in moveFileNames)
            {
                if (!System.IO.File.Exists(MelonEnvironment.UserDataDirectory + path))
                {
                    MelonLogger.Msg("(Optional) Sound File does not Exist at File Path: " + path);
                    fileExists[i] = false;
                }
                else
                {
                    fileExists[i] = true;
                }
                i++;
            }
            MelonLogger.Msg("Initialized");
        }

        //run every update
        public override void OnFixedUpdate()
        {
            //normal updates
            if (currentScene != "Loader")
            {
                //if player null
                if (player == null)
                {
                    try
                    {
                        //if in matchmaking
                        if ((currentScene == "Map0") || (currentScene == "Map1"))
                        {
                            GameObject enemyGameObject = GameObject.Find("Health/Remote");
                            //initializes remote player fields
                            //create listener for enemy player
                            if (enemyGameObject != null)
                            {
                                remotePlayerHealth = PlayerManager.instance.AllPlayers[1].Data.HealthPoints;
                                PlayerHealth enemyHealth = enemyGameObject.transform.parent.GetComponent<PlayerHealth>();
                                enemyHealth.onDamageTaken.AddListener(new Action<short>(component =>
                                {
                                    if (!checkHealth)
                                    {
                                        return;
                                    }
                                    switch (remotePlayerHealth - PlayerManager.instance.AllPlayers[1].Data.HealthPoints)
                                    {
                                        case 0:
                                            break;
                                        case 1:
                                            if (fileExists[10])
                                            {
                                                PlaySoundIfFileExists(FilePaths[10], 0);
                                            }
                                            break;
                                        case 2:
                                            if (fileExists[14])
                                            {
                                                PlaySoundIfFileExists(FilePaths[14], 0);
                                            }
                                            break;
                                        case 3:
                                            if (fileExists[16])
                                            {
                                                PlaySoundIfFileExists(FilePaths[16], 0);
                                            }
                                            break;
                                        case 4:
                                            if (fileExists[18])
                                            {
                                                PlaySoundIfFileExists(FilePaths[18], 0);
                                            }
                                            break;
                                        case 5:
                                            if (fileExists[20])
                                            {
                                                PlaySoundIfFileExists(FilePaths[20], 0);
                                            }
                                            break;
                                        case 6:
                                            if (fileExists[22])
                                            {
                                                PlaySoundIfFileExists(FilePaths[22], 0);
                                            }
                                            break;
                                        case 7:
                                            if (fileExists[24])
                                            {
                                                PlaySoundIfFileExists(FilePaths[24], 0);
                                            }
                                            break;
                                        default:
                                            if (fileExists[26])
                                            {
                                                PlaySoundIfFileExists(FilePaths[26], 0);
                                            }
                                            break;
                                    }
                                    remotePlayerHealth = PlayerManager.instance.AllPlayers[1].Data.HealthPoints;
                                    if (remotePlayerHealth == 0)
                                    {
                                        MelonCoroutines.Start(WaitForHealingOnRoundChange(1));
                                    }
                                }));
                            }
                        }
                        localPlayer = PlayerManager.instance.localPlayer;
                        //initializes local player fields
                        player = localPlayer.Controller.gameObject.transform.GetChild(2).GetChild(13).GetChild(0).gameObject;
                        //set the local health
                        localPlayerHealth = localPlayer.Data.HealthPoints;
                    }
                    catch{ }
                }
                else
                {
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
                    if (checkHealth)
                    {
                        checkLocalHealth();
                    }
                    else
                    {
                        localPlayerHealth = localPlayer.Data.HealthPoints;
                    }
                }
            }
        }

        //called when a scene is loaded
        public override void OnSceneWasLoaded(int buildIndex, string sceneName)
        {
            //update last and current scenes
            lastScene = currentScene;
            currentScene = sceneName;
            finishedHealingAfterDeath = false;
            localPlayerHealth = -1;
            checkHealth = false;
            endHealingCheck = true;
            MelonCoroutines.Start(WaitForHealthCheckAfterSceneChange());
            //Do things dependent on currentScene and lastScene
            switch (currentScene)
            {
                case "Loader": //if currentScene is loader (t-pose start screen)

                    if (fileExists[0])
                    {
                        PlaySoundIfFileExists(FilePaths[0], 0);
                    }
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
                            if (fileExists[1])
                            {
                                PlaySoundIfFileExists(FilePaths[1], 0);
                            }
                            break;
                        case "Park": //if lastScene is the park
                            if (fileExists[2])
                            {
                                PlaySoundIfFileExists(FilePaths[2], 0);
                            }
                            break;
                        case "Map0": //if lastScene is the ring
                        case "Map1": //or the pit
                            if (fileExists[3])
                            {
                                PlaySoundIfFileExists(FilePaths[3], 0);
                            }
                            break;
                    }
                    break;
                case "Park": //if currentScene is park
                    //set height limits for sounds
                    tooLowRespawnHeight = -3;
                    tooLowHeight = -11;
                    tooHighRespawnHeight = 4f;
                    tooHighHeight = 26;
                    if (fileExists[4])
                    {
                        PlaySoundIfFileExists(FilePaths[4], 0);
                    }
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
                        tooHighHeight = 14.5f;
                    }
                    //if pit
                    else if (currentScene == "Map1")
                    {
                        //set height limits for sounds
                        tooLowRespawnHeight = 0;
                        tooLowHeight = -6;
                        tooHighRespawnHeight = 1;
                        tooHighHeight = 13.25f;
                    }
                    switch (lastScene)
                    {
                        case "Gym": //if matching from the gym
                            if (fileExists[5])
                            {
                                PlaySoundIfFileExists(FilePaths[5], 0);
                            }
                            break;
                        case "Map0": //if rematching on the ring
                        case "Map1": //or the pit
                            if (fileExists[6])
                            {
                                PlaySoundIfFileExists(FilePaths[6], 0);
                            }
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
                //if under 8 health
            if ((localPlayer.Data.HealthPoints <= 7))
            {
                if ((currentScene == "Map0") || (currentScene == "Map1"))
                {
                    playHeartbeat = true;
                    if (localPlayer.Data.HealthPoints == 0)
                    {
                        playHeartbeat = false;
                    }
                }
            }
            //if health changed
            if (localPlayer.Data.HealthPoints != localPlayerHealth)
            {
                //if player full health
                if (localPlayerHealth == 20)
                {
                    finishedHealingAfterDeath = true;
                }
                if ((!finishedHealingAfterDeath) || (localPlayerHealth == -1))
                {
                    localPlayerHealth = localPlayer.Data.HealthPoints;
                    return;
                }
                if (finishedHealingAfterDeath)
                {
                    //if player hurt
                    if (localPlayer.Data.HealthPoints < localPlayerHealth)
                    {
                        switch (localPlayerHealth - localPlayer.Data.HealthPoints)
                        {
                            case 1:
                                if (fileExists[9])
                                {
                                    PlaySoundIfFileExists(FilePaths[9], 0);
                                }
                                break;
                            case 2:
                                if (fileExists[13])
                                {
                                    PlaySoundIfFileExists(FilePaths[13], 0);
                                }
                                break;
                            case 3:
                                if (fileExists[15])
                                {
                                    PlaySoundIfFileExists(FilePaths[15], 0);
                                }
                                break;
                            case 4:
                                if (fileExists[17])
                                {
                                    PlaySoundIfFileExists(FilePaths[17], 0);
                                }
                                break;
                            case 5:
                                if (fileExists[19])
                                {
                                    PlaySoundIfFileExists(FilePaths[19], 0);
                                }
                                break;
                            case 6:
                                if (fileExists[21])
                                {
                                    PlaySoundIfFileExists(FilePaths[21], 0);
                                }
                                break;
                            case 7:
                                if (fileExists[23])
                                {
                                    PlaySoundIfFileExists(FilePaths[23], 0);
                                }
                                break;
                            default:
                                if (fileExists[25])
                                {
                                    PlaySoundIfFileExists(FilePaths[25], 0);
                                }
                                break;
                        }
                    }
                    //if player healed
                    else if (localPlayer.Data.HealthPoints > localPlayerHealth)
                    {
                        if (fileExists[11])
                        {
                            PlaySoundIfFileExists(FilePaths[11], 0);
                        }
                    }
                    //if player is dead
                    if (localPlayer.Data.HealthPoints == 0)
                    {
                        finishedHealingAfterDeath = false;
                        if ((PlayerManager.instance.AllPlayers.Count == 2) && ((currentScene == "Map0") || (currentScene == "Map1")))
                        {
                            MelonCoroutines.Start(WaitForHealingOnRoundChange(0));
                        }
                    }
                }
                localPlayerHealth = localPlayer.Data.HealthPoints;
            }
            if (playHeartbeat)
            {
                if (fileExists[12])
                {
                    PlaySoundIfFileExists(FilePaths[12], 3);
                }
            }
        }

        private IEnumerator WaitForHealthCheckAfterSceneChange()
        {
            yield return new WaitForSeconds(2f);
            checkHealth = true;
            endHealingCheck = true;
            yield break;
        }

        private IEnumerator WaitForHealingOnRoundChange(int player)
        {
            checkHealth = false;
            endHealingCheck = false;
            while (!endHealingCheck && (PlayerManager.instance.AllPlayers.Count - 1 >= player) && (PlayerManager.instance.AllPlayers[player].Data.HealthPoints != 20))
            {
                yield return new WaitForFixedUpdate();
            }
            if (!endHealingCheck)
            {
                checkHealth = true;
                localPlayerHealth = localPlayer.Data.HealthPoints;
                if (PlayerManager.instance.AllPlayers.Count == 2)
                {
                    remotePlayerHealth = PlayerManager.instance.AllPlayers[1].Data.HealthPoints;
                }
            }
            yield break;
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
                if (fileExists[7])
                {
                    PlaySoundIfFileExists(FilePaths[7], 1);
                }
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
                if (fileExists[8])
                {
                    PlaySoundIfFileExists(FilePaths[8], 1);
                }
            }
        }

        private static IEnumerator PlaySound(string FilePath, int threadToPlayOn)
        {
            threadActive[threadToPlayOn] = true;
            var reader = new Mp3FileReader(FilePath);
            var waveOut = new WaveOutEvent();
            waveOut.Init(reader);
            waveOut.Play();
            while (waveOut.PlaybackState == PlaybackState.Playing)
            {
                yield return new WaitForFixedUpdate();
            }
            threadActive[threadToPlayOn] = false;
            reader.Dispose();
            waveOut.Dispose();
            yield break;
        }

        public static void PlaySound(string FilePath)
        {
            MelonCoroutines.Start(PlaySoundCoroutine(FilePath));
        }

        private static IEnumerator PlaySoundCoroutine(string FilePath)
        {
            var reader = new Mp3FileReader(MelonEnvironment.UserDataDirectory + FilePath);
            var waveOut = new WaveOutEvent();
            waveOut.Init(reader);
            waveOut.Play();
            while (waveOut.PlaybackState == PlaybackState.Playing)
            {
                yield return new WaitForFixedUpdate();
            }
            reader.Dispose();
            waveOut.Dispose();
            yield break;
        }

        //Plays the File Sound if it Exists
        public static void PlaySoundIfFileExists(string soundFilePath, int threadToPlayOn)
        {
            //Ensure that only one sound is playing at a time
            if (((threadToPlayOn == 3) || (threadToPlayOn == 4)) && threadActive[threadToPlayOn])
            {
                return;
            }
            try
            {
                MelonCoroutines.Start(PlaySound(MelonEnvironment.UserDataDirectory + soundFilePath, threadToPlayOn));
            }
            catch (Exception ex)
            {
                MelonLogger.Error($"Error playing sound:{Environment.NewLine}{ex.Message}{Environment.NewLine}{ex.StackTrace}{Environment.NewLine}{ex.InnerException}");
            }
        }
    }
}
