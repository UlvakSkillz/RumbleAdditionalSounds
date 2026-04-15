using HarmonyLib;
using Il2CppRUMBLE.Environment;
using Il2CppRUMBLE.Managers;
using Il2CppRUMBLE.Players;
using Il2CppRUMBLE.Players.Subsystems;
using Il2CppRUMBLE.Pools;
using Il2CppRUMBLE.Poses;
using MelonLoader;
using System.Collections;
using UnityEngine;
using static AdditionalSounds.Main;

namespace AdditionalSounds.Patches
{
    //used for boundary audio
    [HarmonyPatch(typeof(PlayerResetSystem), nameof(PlayerResetSystem.ResetPlayerController), new Type[] { })]
    public static class ResetPlayerController
    {
        private static void Postfix(ref PlayerResetSystem __instance)
        {
            //stop if Boundary Mod Setting is off or remote player
            if ((!Preferences.PrefPlayerBoundaryKillsToggle.Value) || (__instance.parentController.controllerType == ControllerType.Remote)) { return; }
            //get foot collider as feet are the trigger area
            Vector3 playerPos = PlayerManager.instance.localPlayer.Controller.PlayerVR.gameObject.transform.FindChild("Foot Collider").position;
            //lets go static PlayerSceneBoundary grab!
            Transform sceneBoundaryTransform = SceneBoundary.CurrentActivePlayerSceneBoundary.transform;
            //get top of boundary
            float boundaryUpperHeight = sceneBoundaryTransform.position.y + (sceneBoundaryTransform.localScale.y / 2f);
            //get bottom of boundary
            float boundaryLowerHeight = sceneBoundaryTransform.position.y - (sceneBoundaryTransform.localScale.y / 2f);
            if (playerPos.y < boundaryLowerHeight) //if below boundary
            { //play TooLow Audio
                RumbleModdingAPI.RMAPI.AudioManager.PlaySound(audioCalls[(int)SoundsOrder.BoundaryFileName][0], playerPos);
            }
            else if (playerPos.y > boundaryUpperHeight) //if above boundary
            { //play TooHigh Audio
                RumbleModdingAPI.RMAPI.AudioManager.PlaySound(audioCalls[(int)SoundsOrder.BoundaryFileName][1], playerPos);
            }
        }
    }

    //used for pose audio
    [HarmonyPatch(typeof(PlayerPoseSystem), nameof(PlayerPoseSystem.OnPoseSetCompleted), new Type[] { typeof(PoseSet) })]
    public static class PosePatch
    {
        private static void Postfix(ref PlayerPoseSystem __instance, PoseSet set)
        {
            //stop if pose Mod Setting is off
            if (!Preferences.PrefPosesToggle.Value) { return; }
            //where to spawn audio
            Vector3 playerPos = __instance.parentController.PlayerCamera.gameObject.transform.position;
            //switch to play pose name audio
            switch (set.name)
            {
                case "SprintingPoseSet":
                    RumbleModdingAPI.RMAPI.AudioManager.PlaySound(audioCalls[(int)SoundsOrder.MoveFileNames][(int)MovesOrder.Sprint], playerPos);
                    break;
                case "PoseSetFlick":
                    RumbleModdingAPI.RMAPI.AudioManager.PlaySound(audioCalls[(int)SoundsOrder.MoveFileNames][(int)MovesOrder.Flick], playerPos);
                    break;
                case "PoseSetExplode":
                    RumbleModdingAPI.RMAPI.AudioManager.PlaySound(audioCalls[(int)SoundsOrder.MoveFileNames][(int)MovesOrder.Explode], playerPos);
                    break;
                case "PoseSetHoldLeft":
                    RumbleModdingAPI.RMAPI.AudioManager.PlaySound(audioCalls[(int)SoundsOrder.MoveFileNames][(int)MovesOrder.Hold], playerPos);
                    break;
                case "PoseSetHoldRight":
                    RumbleModdingAPI.RMAPI.AudioManager.PlaySound(audioCalls[(int)SoundsOrder.MoveFileNames][(int)MovesOrder.Hold], playerPos);
                    break;
                case "PoseSetParry":
                    RumbleModdingAPI.RMAPI.AudioManager.PlaySound(audioCalls[(int)SoundsOrder.MoveFileNames][(int)MovesOrder.Parry], playerPos);
                    break;
                case "PoseSetDash":
                    RumbleModdingAPI.RMAPI.AudioManager.PlaySound(audioCalls[(int)SoundsOrder.MoveFileNames][(int)MovesOrder.Dash], playerPos);
                    break;
                case "PoseSetSpawnCube":
                    RumbleModdingAPI.RMAPI.AudioManager.PlaySound(audioCalls[(int)SoundsOrder.MoveFileNames][(int)MovesOrder.Cube], playerPos);
                    break;
                case "PoseSetUppercut":
                    RumbleModdingAPI.RMAPI.AudioManager.PlaySound(audioCalls[(int)SoundsOrder.MoveFileNames][(int)MovesOrder.Uppercut], playerPos);
                    break;
                case "PoseSetWall_Grounded":
                    RumbleModdingAPI.RMAPI.AudioManager.PlaySound(audioCalls[(int)SoundsOrder.MoveFileNames][(int)MovesOrder.Wall], playerPos);
                    break;
                case "PoseSetRockjump":
                    RumbleModdingAPI.RMAPI.AudioManager.PlaySound(audioCalls[(int)SoundsOrder.MoveFileNames][(int)MovesOrder.Jump], playerPos);
                    break;
                case "PoseSetStomp":
                    RumbleModdingAPI.RMAPI.AudioManager.PlaySound(audioCalls[(int)SoundsOrder.MoveFileNames][(int)MovesOrder.Stomp], playerPos);
                    break;
                case "PoseSetBall":
                    RumbleModdingAPI.RMAPI.AudioManager.PlaySound(audioCalls[(int)SoundsOrder.MoveFileNames][(int)MovesOrder.Ball], playerPos);
                    break;
                case "PoseSetKick":
                    RumbleModdingAPI.RMAPI.AudioManager.PlaySound(audioCalls[(int)SoundsOrder.MoveFileNames][(int)MovesOrder.Kick], playerPos);
                    break;
                case "PoseSetDisc":
                    RumbleModdingAPI.RMAPI.AudioManager.PlaySound(audioCalls[(int)SoundsOrder.MoveFileNames][(int)MovesOrder.Disc], playerPos);
                    break;
                case "PoseSetStraight":
                    RumbleModdingAPI.RMAPI.AudioManager.PlaySound(audioCalls[(int)SoundsOrder.MoveFileNames][(int)MovesOrder.Straight], playerPos);
                    break;
                case "PoseSetSpawnPillar":
                    RumbleModdingAPI.RMAPI.AudioManager.PlaySound(audioCalls[(int)SoundsOrder.MoveFileNames][(int)MovesOrder.Pillar], playerPos);
                    break;
                default:
                    //shouldn't ever get here until new moves are added
                    break;
            }
        }
    }

    //used for damages, healing, and low health audio
    [HarmonyPatch(typeof(PlayerHealth), nameof(PlayerHealth.SetHealth), new Type[] { typeof(short), typeof(short), typeof(bool) })]
    public static class SetHealth
    {
        private static void Postfix(ref PlayerHealth __instance, short newHealth, short previousHealth, bool useEffects)
        {
            //triggers when players spawn so I use this to stop that
            if (!useEffects) { return; }
            //if healing
            if (newHealth > previousHealth)
            {
                //local player healed
                if (__instance.ParentController.controllerType == ControllerType.Local)
                {
                    //if above Mod Setting amount
                    if (newHealth > Preferences.PrefLowHealthAmount.Value)
                    { //stop low health audio
                        StopLowHealthSoundEffect();
                    }
                    if (Preferences.PrefHealingToggle.Value && fileExists[(int)SoundsOrder.HealFileNames][0])
                    { //play local heal sound
                        RumbleModdingAPI.RMAPI.AudioManager.PlaySound(audioCalls[(int)SoundsOrder.HealFileNames][0], __instance.parentController.PlayerCamera.gameObject.transform.position);
                    }
                }
                //remote player healed
                else if (Preferences.PrefHealingToggle.Value && fileExists[(int)SoundsOrder.HealFileNames][1])
                { //play remote heal sound
                    RumbleModdingAPI.RMAPI.AudioManager.PlaySound(audioCalls[(int)SoundsOrder.HealFileNames][1], __instance.parentController.PlayerCamera.gameObject.transform.position);
                }
            }
            //if damage
            else
            {
                //local player damaged
                if (__instance.ParentController.controllerType == ControllerType.Local)
                {
                    if (Preferences.PrefYouTakingDamageToggle.Value && fileExists[(int)SoundsOrder.LocalDamageFileNames][Math.Min(previousHealth - newHealth, 8)])
                    { //play local damage sound
                        RumbleModdingAPI.RMAPI.AudioManager.PlaySound(audioCalls[(int)SoundsOrder.LocalDamageFileNames][Math.Min(previousHealth - newHealth, 8)], __instance.parentController.PlayerCamera.gameObject.transform.position);
                    }
                    if (Preferences.PrefLowHealthToggle.Value //if toggle is on
                        && fileExists[(int)SoundsOrder.LowHealthFileName][0] //if filefound
                        && ((currentScene == "Map0") || (currentScene == "Map1")) //if matchmaking
                        && (lowHealthSoundEffect == null) //if sound not playing
                        && (newHealth > 0) //if not final hit
                        && (newHealth <= Preferences.PrefLowHealthAmount.Value)) //if below mod setting's trigger amount
                    { //play low health sound effect
                        MelonCoroutines.Start(StartLowHealthSoundEffect());
                    }
                }
                //remote player damaged
                else if (Preferences.PrefOthersTakingDamageToggle.Value && fileExists[(int)SoundsOrder.RemoteDamageFileNames][Math.Min(previousHealth - newHealth, 8)] && (previousHealth != 0))
                { //play remote damage sound
                    RumbleModdingAPI.RMAPI.AudioManager.PlaySound(audioCalls[(int)SoundsOrder.RemoteDamageFileNames][Math.Min(previousHealth - newHealth, 8)], __instance.parentController.PlayerCamera.gameObject.transform.position);
                }
                if (newHealth == 0) //end of match
                {
                    StopLowHealthSoundEffect();
                }
            }
        }

        //variable to see if low health sound is currently playing
        private static PooledAudioSource lowHealthSoundEffect = null;
        private static IEnumerator StartLowHealthSoundEffect()
        {
            Melon<Main>.Logger.Msg("Starting Low Health Sound");
            //start playing sound
            lowHealthSoundEffect = RumbleModdingAPI.RMAPI.AudioManager.PlaySound(audioCalls[(int)SoundsOrder.LowHealthFileName][0], PlayerManager.instance.localPlayer.Controller.PlayerCamera.gameObject.transform.position);
            //wait the clip length
            yield return new WaitForSeconds(lowHealthSoundEffect.audioSource.clip.length);
            //while the PooledAudioSource isn't null and mod setting is on, loop (null is when mod tells it to stop)
            while ((lowHealthSoundEffect != null) && Preferences.PrefLowHealthToggle.Value)
            {
                //start playing sound
                lowHealthSoundEffect = RumbleModdingAPI.RMAPI.AudioManager.PlaySound(audioCalls[(int)SoundsOrder.LowHealthFileName][0], PlayerManager.instance.localPlayer.Controller.PlayerCamera.gameObject.transform.position);
                //wait the clip length
                yield return new WaitForSeconds(lowHealthSoundEffect.audioSource.clip.length);
            }
        }

        internal static void StopLowHealthSoundEffect()
        {
            //if PooledAudioSource is not null (on/playing)
            if (lowHealthSoundEffect != null)
            {
                Melon<Main>.Logger.Msg("Stopping Low Health Sound");
                //end audio
                lowHealthSoundEffect.ReturnToPool();
                //set to null to tell mod it's off
                lowHealthSoundEffect = null;
            }
        }
    }
}
