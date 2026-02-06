using System.Collections.Generic;

namespace ModuleManagerPlus.Data {
    internal class PkgRoot {

        public Dictionary<string, Author> Authors { get; set; }

        public List<Module> Modules { get; set; }

    }
}
