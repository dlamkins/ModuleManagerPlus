using Blish_HUD.Controls;
using Blish_HUD.Graphics.UI;
using Flurl.Http;
using Microsoft.Xna.Framework;
using ModuleManagerPlus.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ModuleManagerPlus.UI {
    internal class ModuleRepoView : IView {

        public event EventHandler<EventArgs> Loaded;
        public event EventHandler<EventArgs> Built;
        public event EventHandler<EventArgs> Unloaded;

        public FlowPanel _panel;

        private readonly TextureLoader _textureLoader;

        private readonly TextBox _searchBox;

        public ModuleRepoView(TextureLoader textureLoader) {
            _textureLoader = textureLoader;

            _searchBox = new TextBox() {
                PlaceholderText = "Search Modules...",
            };

            _searchBox.TextChanged += _searchBox_TextChanged;

            _panel = new FlowPanel() {
                OuterControlPadding = new Vector2(0, 0),
                //ControlPadding = new Vector2(30, 30),
                ControlPadding = new Vector2(0, 30),
                HeightSizingMode = SizingMode.Standard,
                WidthSizingMode = SizingMode.Standard,
                //CanScroll = true
            };
        }

        private void _searchBox_TextChanged(object sender, EventArgs e) {
            string needle = _searchBox.Text.ToLowerInvariant();

            _panel.FilterChildren<HModuleCard>((mc) => {
                return mc.Module.Name.ToLowerInvariant().Contains(needle) || mc.Module.Description.Contains(needle);
            });
        }

        public void DoBuild(Container buildPanel) {
            _searchBox.Width = buildPanel.Width;
            _searchBox.Parent = buildPanel;

            _panel.Top = _searchBox.Bottom + 20;
            _panel.Parent = buildPanel;
            _panel.Size = new Point(buildPanel.Width, buildPanel.Height - _searchBox.Height - 20);
            //_panel.BackgroundColor = Color.Black * 0.25f;
            //_panel.ClipsBounds = true;

            _panel.CanScroll = true;

            this.Built?.Invoke(this, EventArgs.Empty);
        }

        public async Task<bool> DoLoad(IProgress<string> progress) {
            try {
                progress.Report("Fetching module data...");
                var pkgRoot = await "https://pkgs.blishhud.com/packagesv2.json".GetJsonAsync<PkgRoot>();

                progress.Report("Loading UI...");

                _panel.SuspendLayout();

                foreach (var module in pkgRoot.Modules.OrderByDescending(m => m.TotalDownloads)) {
                    var newModuleCard = new HModuleCard(module, pkgRoot.Authors[module.AuthorId], _textureLoader);
                    newModuleCard.Parent = _panel;
                }

                _panel.ResumeLayout(true);

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
            _searchBox?.TextChanged -= _searchBox_TextChanged;
        }

    }
}
