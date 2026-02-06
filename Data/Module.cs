using Newtonsoft.Json;
using System.Collections.Generic;

namespace ModuleManagerPlus.Data {
    internal class Module {

        [JsonProperty("Namespace")]
        public string @Namespace { get; set; }

        [JsonProperty("Name")]
        public string Name { get; set; }

        [JsonProperty("Description")]
        public string Description { get; set; }

        [JsonProperty("HeroUrl")]
        public string HeroUrl { get; set; }

        [JsonProperty("HasMoreInfo")]
        public bool HasMoreInfo { get; set; }

        [JsonProperty("TotalDownloads")]
        public int TotalDownloads { get; set; }

        [JsonProperty("Releases")]
        public List<Release> Releases { get; set; }

        [JsonProperty("AuthorId")]
        public string AuthorId { get; set; }

        private Author _author;
        public Author Author(PkgRoot root) => _author ??= root.Authors[this.AuthorId];

    }
}
