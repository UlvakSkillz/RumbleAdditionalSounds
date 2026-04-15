using MelonLoader;
using MelonLoader.Preferences;

namespace AdditionalSounds
{
	public class Preferences
	{
		private const string CONFIG_FILE = "config.cfg";
		private const string USER_DATA = "UserData/AdditionalSounds/";
        internal static Dictionary<MelonPreferences_Entry, object> LastSavedValues = new();

        internal static MelonPreferences_Category TogglesCategory;
        internal static List<MelonPreferences_Category> VolumeCategories;
        internal static MelonPreferences_Category PosesCategory;
        internal static MelonPreferences_Category SceneChangeCategory;
        internal static MelonPreferences_Category YouTakingDamageCategory;
        internal static MelonPreferences_Category OthersTakingDamageCategory;
        internal static MelonPreferences_Category PlayerBoundaryKillsCategory;
        internal static MelonPreferences_Category HealingCategory;
        internal static MelonPreferences_Category LowHealthCategory;

		internal static MelonPreferences_Entry<bool> PrefPosesToggle;
		internal static MelonPreferences_Entry<bool> PrefSceneChangeToggle;
		internal static MelonPreferences_Entry<bool> PrefYouTakingDamageToggle;
		internal static MelonPreferences_Entry<bool> PrefOthersTakingDamageToggle;
		internal static MelonPreferences_Entry<bool> PrefPlayerBoundaryKillsToggle;
		internal static MelonPreferences_Entry<bool> PrefHealingToggle;
		internal static MelonPreferences_Entry<bool> PrefLowHealthToggle;
		internal static MelonPreferences_Entry<int> PrefLowHealthAmount;
		internal static MelonPreferences_Entry<float>[][] PrefVolumes;

        internal static void InitPrefs()
		{
			if (!Directory.Exists(USER_DATA)) { Directory.CreateDirectory(USER_DATA); }

            TogglesCategory = MelonPreferences.CreateCategory("Toggles", "Toggles");
            TogglesCategory.SetFilePath(Path.Combine(USER_DATA, CONFIG_FILE));

            PrefPosesToggle = TogglesCategory.CreateEntry("Poses", true, "Poses Toggle", "Toggles Pose Sounds from Playing");
			PrefSceneChangeToggle = TogglesCategory.CreateEntry("SceneChange", true, "Scene Change Toggle", "Toggles Scene Change Sounds from Playing");
			PrefYouTakingDamageToggle = TogglesCategory.CreateEntry("YouTakingDamage", true, "You Taking Damage Toggle", "Toggles You Taking Damage Sounds from Playing");
			PrefOthersTakingDamageToggle = TogglesCategory.CreateEntry("OthersTakingDamage", true, "Others Taking Damage Toggle", "Toggles Others Taking Damage Sounds from Playing");
			PrefPlayerBoundaryKillsToggle = TogglesCategory.CreateEntry("PlayerBoundaryKills", true, "Player Boundary Kills Toggle", "Toggles Kill Boundary Sounds from Playing");
			PrefHealingToggle = TogglesCategory.CreateEntry("Healing", true, "Healing Toggle", "Toggles Healing Sounds from Playing");
			PrefLowHealthToggle = TogglesCategory.CreateEntry("LowHealth", true, "Low Health Toggle", "Toggles Low Health Sound from Playing. In Matchmaking Only.");
			PrefLowHealthAmount = TogglesCategory.CreateEntry("LowHealthAmount", 7, "Low Health Amount", "Sets the Amount of Health to Start Triggering the Low Health Sound", validator: new ValueRange<int>(0, 20));

            PosesCategory = MelonPreferences.CreateCategory("PosesVolumes", "Poses");
            PosesCategory.SetFilePath(Path.Combine(USER_DATA, CONFIG_FILE));

            SceneChangeCategory = MelonPreferences.CreateCategory("SceneChangeVolumes", "SceneChange");
            SceneChangeCategory.SetFilePath(Path.Combine(USER_DATA, CONFIG_FILE));

            YouTakingDamageCategory = MelonPreferences.CreateCategory("YouTakingDamageVolumes", "You Taking Damage");
            YouTakingDamageCategory.SetFilePath(Path.Combine(USER_DATA, CONFIG_FILE));

            OthersTakingDamageCategory = MelonPreferences.CreateCategory("OthersTakingDamageVolumes", "Others Taking Damage");
            OthersTakingDamageCategory.SetFilePath(Path.Combine(USER_DATA, CONFIG_FILE));

            PlayerBoundaryKillsCategory = MelonPreferences.CreateCategory("PlayerBoundaryKillsVolumes", "Player Boundary Kills");
            PlayerBoundaryKillsCategory.SetFilePath(Path.Combine(USER_DATA, CONFIG_FILE));

            HealingCategory = MelonPreferences.CreateCategory("HealingVolumes", "Healing");
            HealingCategory.SetFilePath(Path.Combine(USER_DATA, CONFIG_FILE));

            LowHealthCategory = MelonPreferences.CreateCategory("LowHealthVolumes", "Low Health");
            LowHealthCategory.SetFilePath(Path.Combine(USER_DATA, CONFIG_FILE));

            VolumeCategories = new List<MelonPreferences_Category> { PosesCategory, SceneChangeCategory, YouTakingDamageCategory, OthersTakingDamageCategory, PlayerBoundaryKillsCategory, HealingCategory, LowHealthCategory };

            PrefVolumes = new MelonPreferences_Entry<float>[Main.TOGGLESCOUNT][];
            for (int i = 0; i < Main.TOGGLESCOUNT; i++)
            {
				PrefVolumes[i] = new MelonPreferences_Entry<float>[Main.fileNames[i].Length];
                //for each file name in the grouping
                for (int x = 0; x < Main.fileNames[i].Length; x++)
                {
                    //get just the name of the file, no folders or .wav
                    string[] fileNamesSplit = Main.fileNames[i][x].Replace(".wav", "").Split('\\');
                    string fileName = fileNamesSplit[fileNamesSplit.Length - 1];
                    //create volume setting for the file
                    
                    PrefVolumes[i][x] = VolumeCategories[i].CreateEntry($"{fileName} Volume", 50f, $"{fileName} Volume", $"Edits {fileName}'s Volume in Game. 0 - 100.", validator: new ValueRange<float>(0, 100));
                }
            }
            StoreLastSavedPrefs();
		}

		internal static void StoreLastSavedPrefs()
		{
			List<MelonPreferences_Entry> prefs = new();
			prefs.AddRange(TogglesCategory.Entries);
			foreach (MelonPreferences_Entry entry in  prefs) { LastSavedValues[entry] = entry.BoxedValue; }
		}

        public static bool IsPrefChanged(MelonPreferences_Entry entry)
		{
			if (LastSavedValues.TryGetValue(entry, out object? lastValue)) { return !entry.BoxedValue.Equals(lastValue); }
			return false;
		}

		public static bool IsVolumePrefChanged()
        {
            for (int i = 0; i < PrefVolumes.Length; i++)
            {
                for (int x = 0; x < PrefVolumes[i].Length; x++)
                {
                    if (IsPrefChanged(PrefVolumes[i][x])) { return true; }
                }
            }
			return false;
        }
    }
}