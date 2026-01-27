using Blish_HUD.Content;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModuleManagerPlus.Data {
    internal class Module {

        public string @Namespace { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public string HeroUrl { get; set; }

        public int TotalDownloads { get; set; }

        public DateTime LastRelease { get; set; }

    }
}
