﻿global using static vtolLogMod.Logger;
using System.IO;
using System.Reflection;
using ModLoader.Framework;
using ModLoader.Framework.Attributes;

namespace vtolLogMod;

[ItemId("Brillcrafter.vtolLogMod")] // Harmony ID for your mod, make sure this is unique
public class Main : VtolMod
{
    public string ModFolder;

    private void Awake()
    {
        ModFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        Log($"Awake at {ModFolder}");
    }

    public override void UnLoad()
    {
        // Destroy any objects
    }
}