using Newtonsoft.Json;
using SemVer;

namespace ModuleManagerPlus.Data {
    internal class Release {

        [JsonProperty("Version")]
        public string Version { get; set; }

        public bool IsPrerelease { get; set; }

        private Version _typedVersion;
        public Version TypedVersion => _typedVersion ??= new Version(this.Version);

        public string DownloadUrl { get; set; }

    }
}
