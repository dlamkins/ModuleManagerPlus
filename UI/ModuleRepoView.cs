using Blish_HUD.Controls;
using Blish_HUD.Graphics.UI;
using Flurl.Http;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ModuleManagerPlus.UI {
    internal class ModuleRepoView : IView {

        public event EventHandler<EventArgs> Loaded;
        public event EventHandler<EventArgs> Built;
        public event EventHandler<EventArgs> Unloaded;

        public FlowPanel _panel;

        private readonly TextureLoader _textureLoader;

        public ModuleRepoView(TextureLoader textureLoader) {
            _textureLoader = textureLoader;

            _panel = new FlowPanel() {
                OuterControlPadding = new Vector2(30, 30),
                ControlPadding = new Vector2(30, 30),
                CanScroll = true,
                HeightSizingMode = SizingMode.Standard,
                WidthSizingMode = SizingMode.Standard
            };
        }

        public void DoBuild(Container buildPanel) {
            _panel.Size = new Point(buildPanel.Width, buildPanel.Height);
            _panel.Parent = buildPanel;
            _panel.BackgroundColor = Color.Black * 0.25f;
            _panel.ClipsBounds = true;

            this.Built?.Invoke(this, EventArgs.Empty);
        }

        public async Task<bool> DoLoad(IProgress<string> progress) {
            try {
                progress.Report("Fetching module data...");
                var modules = await "https://pkgs.blishhud.com/metadata/all.json".GetJsonAsync<List<Data.Module>>();

                progress.Report("Loading UI...");

                _panel.SuspendLayout();

                foreach (var module in modules) {
                    var newModuleCard = new ModuleCard(module, _textureLoader);
                    newModuleCard.Parent = _panel;
                }

                _panel.ResumeLayout();

                this.Built?.Invoke(this, EventArgs.Empty);

                return true;
            } catch (Exception ex) {
                progress.Report($"Loading repo failed: {ex.Message}");
                return false;
            } finally {
                progress.Report("");
            }
        }

        public void DoUnload() {
            this.Unloaded?.Invoke(this, EventArgs.Empty);
        }

    }
}
