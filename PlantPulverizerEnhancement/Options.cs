﻿using Newtonsoft.Json;
using PeterHan.PLib.Options;

namespace PlantPulverizerEnhancement {
    [RestartRequired]
    [ConfigFile("enhancement.json",true,true)]
    [JsonObject(MemberSerialization.OptIn)]
    public class Options {
        [Option("Strings.TITLE", "", null)]
        [JsonProperty]
        public bool AddRecipe { get; set; } = false;
    }
}
