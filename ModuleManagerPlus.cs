using System;
using System.ComponentModel.Composition;
using System.Linq;
using System.Threading.Tasks;
using Blish_HUD;
using Blish_HUD.Modules;
using Blish_HUD.Modules.Managers;
using Blish_HUD.Settings;
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
            var panel = new CardPanel(this.TextureLoader);
            panel.Location = new Point(250, 250);

            var exampleModule1 = new Data.Module() {
                HeroUrl = "https://pkgs.blishhud.com/metadata/img/module/bh.community.pathing.png",
                Name = "Pathing",
                Description = "Renders community made markers & trails to guide you through map completion, difficult story content, and tedious achievements.",
                @Namespace = "bh.community.pathing",
                LastRelease = DateTime.Now.AddDays(-10),
                TotalDownloads = 779848
            };

            var exampleModule2 = new Data.Module() {
                HeroUrl = "https://pkgs.blishhud.com/metadata/img/module/Manlaan.Mounts.png",
                Name = "Mounts & More",
                Description = "Adds mounts, mastery skills and novelty icons in the form of radial, icon rows and corner icons.",
                @Namespace = "Manlaan.Mounts",
                LastRelease = DateTime.Now.AddDays(-10),
                TotalDownloads = 296203
            };

            var newModuleCard1 = new ModuleCard(exampleModule1, TextureLoader);
            var newModuleCard2 = new ModuleCard(exampleModule2, TextureLoader);

            newModuleCard1.Location = new Point(100, 150);
            newModuleCard1.Parent = panel;

            newModuleCard2.Location = new Point(450, 150);
            newModuleCard2.Parent = panel;

            panel.Parent = GameService.Graphics.SpriteScreen;
        }

        protected override void Update(GameTime gameTime) {
            
        }

        protected override void Unload() {
            TextureLoader.Unload();
        }

    }
}
