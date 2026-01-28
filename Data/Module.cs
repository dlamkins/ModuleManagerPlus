using Blish_HUD.Content;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModuleManagerPlus.Data {
    internal class Module {

        [JsonProperty("Namespace")]
        public string @Namespace { get; set; }

        [JsonProperty("Name")]
        public string Name { get; set; }

        [JsonProperty("Summary")]
        public string Description { get; set; }

        public string HeroUrl => $"https://pkgs.blishhud.com/metadata/img/module/{this.Namespace}.png";

        [JsonProperty("Downloads")]
        public int TotalDownloads { get; set; }

        [JsonProperty("LastUpdate")]
        public DateTime LastRelease { get; set; }

        [JsonProperty("AuthorName")]
        public string AuthorName { get; set; }

        [JsonProperty("AuthorAvatar")]
        public string AuthorAvatarUrl { get; set; }

    }
}
