using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Threading.Tasks;
using Blish_HUD;
using Blish_HUD.Content;
using Blish_HUD.Modules;
using Blish_HUD.Modules.Managers;
using Blish_HUD.Settings;
using Flurl.Http;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ModuleManagerPlus.UI;
using MonoGame.Extended;
using static System.Net.WebRequestMethods;

namespace ModuleManagerPlus
{
    [Export(typeof(Module))]
    public class ModuleManagerPlus : Module {
        private static readonly Logger Logger = Logger.GetLogger<ModuleManagerPlus>();

        internal SettingsManager SettingsManager => this.ModuleParameters.SettingsManager;
        internal ContentsManager ContentsManager => this.ModuleParameters.ContentsManager;
        internal DirectoriesManager DirectoriesManager => this.ModuleParameters.DirectoriesManager;
        internal Gw2ApiManager Gw2ApiManager => this.ModuleParameters.Gw2ApiManager;

        public TextureLoader TextureLoader { get; private set; }

        public static Effect MaskEffect { get; private set; }

        private List<Data.Module> _modules = new List<Data.Module>();


        [ImportingConstructor]
        public ModuleManagerPlus([Import("ModuleParameters")] ModuleParameters moduleParameters) : base(moduleParameters) {
            
        }

        protected override void DefineSettings(SettingCollection settings) {
        }

        protected override async Task LoadAsync() {
            this.TextureLoader = new TextureLoader(this.ContentsManager);

            MaskEffect = this.ContentsManager.GetEffect("effects/alphashader.mgfx");

            
        }

        protected override void OnModuleLoaded(EventArgs e) {
            GameService.Overlay.SettingsTab.RegisterSettingMenu(new Blish_HUD.Controls.MenuItem("Module Repo v2", AsyncTexture2D.FromAssetId(156764)), (m) => new UI.ModuleRepoView(this.TextureLoader));
        }

        protected override void Update(GameTime gameTime) {
            
        }

        protected override void Unload() {
            TextureLoader.Unload();
        }

    }
}
