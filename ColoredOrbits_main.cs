using System;

using ModLoader;
using UITools;
using HarmonyLib;
using SFS.IO;
using System.Collections.Generic;

namespace ColoredOrbits
{
    public class Main : Mod, IUpdatable
    {
        const string C_STR_MOD_ID = "COLORED_ORBITS";
        const string C_STR_MOD_NAME = "Colored Orbits";
        const string C_STR_AUTHOR = "Altaïr";
        const string C_STR_MODLOADER_VERSION = "1.5.10.2";
        const string C_STR_MOD_VERSION = "v1.0.0";
        const string C_STR_MOD_DESCRIPTION = "This mod colors the planets orbits so that they are easier to identify";

        public override string ModNameID => C_STR_MOD_ID;

        public override string DisplayName => C_STR_MOD_NAME;

        public override string Author => C_STR_AUTHOR;

        public override string MinimumGameVersionNecessary => C_STR_MODLOADER_VERSION;

        public override string ModVersion => C_STR_MOD_VERSION;

        public override string Description => C_STR_MOD_DESCRIPTION;

        public override string IconLink => "https://i.imgur.com/C9kKaR6.png"; // link to the logo

        // Set the dependencies
        public override Dictionary<string, string> Dependencies { get; } = new Dictionary<string, string> { { "UITools", "1.0" } };

        public Dictionary<string, FilePath> UpdatableFiles => new Dictionary<string, FilePath> { { "https://github.com/Kaskouy/Colored-Orbits/releases/latest/download/ColoredOrbits.dll", new FolderPath(ModFolder).ExtendToFile("ColoredOrbits.dll") } };
        
        public Main() : base()
        {
        }

        // This initializes the patcher. This is required if you use any Harmony patches
        public static Harmony patcher;

        public override void Load()
        {
            // Tells the loader what to run when your mod is loaded
            ModLoader.Helpers.SceneHelper.OnWorldSceneLoaded += new Action(MapManager_DrawTrajectories_Patch.InstantiateObjects);
            ModLoader.Helpers.SceneHelper.OnWorldSceneUnloaded += new Action(MapManager_DrawTrajectories_Patch.DesallocateResources);
        }

        public override void Early_Load()
        {
            // This method runs before anything from the game is loaded. This is where you should apply your patches, as shown below.

            // The patcher uses an ID formatted like a web domain
            Main.patcher = new Harmony($"{C_STR_MOD_ID}.{C_STR_MOD_NAME}.{C_STR_AUTHOR}");

            // This pulls your Harmony patches from everywhere in the namespace and applies them.
            Main.patcher.PatchAll();
        }
    }
}

