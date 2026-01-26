using System;
using System.ComponentModel.Composition;
using System.Linq;
using System.Threading.Tasks;
using Blish_HUD;
using Blish_HUD.Modules;
using Blish_HUD.Modules.Managers;
using Blish_HUD.Settings;
using Microsoft.Xna.Framework;
using MonoGame.Extended;

namespace ModuleManagerPlus
{
    [Export(typeof(Module))]
    public class ModuleManagerPlus : Module
    {
        private static readonly Logger Logger = Logger.GetLogger<ModuleManagerPlus>();

        internal SettingsManager SettingsManager => this.ModuleParameters.SettingsManager;
        internal ContentsManager ContentsManager => this.ModuleParameters.ContentsManager;
        internal DirectoriesManager DirectoriesManager => this.ModuleParameters.DirectoriesManager;
        internal Gw2ApiManager Gw2ApiManager => this.ModuleParameters.Gw2ApiManager;

        [ImportingConstructor]
        public ModuleManagerPlus([Import("ModuleParameters")] ModuleParameters moduleParameters) : base(moduleParameters) {
            
        }

        private bool _firstLoad = true;

        protected override void DefineSettings(SettingCollection settings) {
        }

        protected override async Task LoadAsync() {
            
        }

        protected override void OnModuleLoaded(EventArgs e) {

        }

        protected override void Update(GameTime gameTime) {
            //if (_firstLoad) {
            //    _firstLoad = false;

            //    Logger.Info("============================= ModuleManager+: Checking for Updates =============================");
            //    foreach (var pendingUpdate in GameService.Module.ModulePkgRepoHandler.PendingUpdates) {
            //        Logger.Info($"{pendingUpdate.Name} has an update pending (new release: {pendingUpdate.Version}).");
            //    }

            //    Logger.Info("============================= ModuleManager+: Performing Updates =============================");

            //    foreach (var pendingUpdate in GameService.Module.ModulePkgRepoHandler.PendingUpdates) {
            //        GameService.Overlay.QueueMainThreadUpdate((gt) => {
            //            Logger.Info($"Attempting an update for {pendingUpdate.Name} to v{pendingUpdate.Version}!");
            //            var replaceModule = Task.Run(() => GameService.Module.ModulePkgRepoHandler.ReplacePackage(pendingUpdate, GameService.Module.Modules.First(m => m.Manifest.Namespace == pendingUpdate.Namespace))).GetAwaiter().GetResult();

            //            if (replaceModule.Success) {
            //                Logger.Info($"Successfully updated {pendingUpdate.Name} to v{pendingUpdate.Version}!");
            //            } else {
            //                Logger.Warn($"Failed to update {pendingUpdate.Name} to v{pendingUpdate.Version}: {replaceModule.Error}");
            //            }
            //        });
            //    }
            //}
        }

        protected override void Unload() {
            
        }

    }
}
