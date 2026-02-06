using Blish_HUD.Controls;
using Blish_HUD.Graphics.UI;
using Flurl.Http;
using Microsoft.Xna.Framework;
using ModuleManagerPlus.Data;
using ModuleManagerPlus.Services;
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
        private readonly ModuleInstallService _installService;

        private readonly TextBox _searchBox;
        private readonly Checkbox _prereleaseCb;
        private readonly StandardButton _updateAllBttn;
        private readonly Dropdown _sortOrderDd;
        private readonly Label _moduleCountLbl;

        public ModuleRepoView(TextureLoader textureLoader) {
            _textureLoader = textureLoader;
            _installService = new ModuleInstallService();

            _searchBox = new TextBox() {
                PlaceholderText = "Search Modules...",
            };

            _sortOrderDd = new Dropdown() {
                SelectedItem = "Downloads"
            };

            _sortOrderDd.Items.Add("Downloads");
            _sortOrderDd.Items.Add("A to Z");
            _sortOrderDd.Items.Add("Z to A");
            _sortOrderDd.Items.Add("Last Updated");

            _moduleCountLbl = new Label() {
                Text = "Loading modules...",
                AutoSizeWidth = true
            };

            _prereleaseCb = new Checkbox() {
                Text = "Show Prereleases",
            };

            _updateAllBttn = new StandardButton() {
                Text = "Update All",
                Width = 96,
            };

            _searchBox.TextChanged += _searchBox_TextChanged;
            _prereleaseCb.CheckedChanged += _prereleaseCb_CheckedChanged;
            _updateAllBttn.Click += _updateAllBttn_Click;

            _panel = new FlowPanel() {
                OuterControlPadding = new Vector2(0, 0),
                //ControlPadding = new Vector2(30, 30),
                ControlPadding = new Vector2(0, 30),
                HeightSizingMode = SizingMode.Standard,
                WidthSizingMode = SizingMode.Standard,
                //CanScroll = true
            };
        }

        private void UpdateModuleFilter() {
            string searchNeedle = _searchBox.Text.ToLowerInvariant();

            _panel.FilterChildren<HModuleCard>((mc) => {
                return (string.IsNullOrWhiteSpace(searchNeedle) || mc.Module.Name.ToLowerInvariant().Contains(searchNeedle) || mc.Module.Description.Contains(searchNeedle))
                    && (_prereleaseCb.Checked || mc.Module.Releases.Any(r => !r.IsPrerelease));
            });

            _moduleCountLbl.Text = $"Showing {_panel.GetChildrenOfType<HModuleCard>().Where(card => card.Visible).Count()} Modules";
        }

        private void _searchBox_TextChanged(object sender, EventArgs e) {
            UpdateModuleFilter();
        }

        public void DoBuild(Container buildPanel) {
            _searchBox.Width = buildPanel.Width - _prereleaseCb.Width - _updateAllBttn.Width - 20;
            _searchBox.Parent = buildPanel;

            _prereleaseCb.Location = new Point(_searchBox.Right + 10, _searchBox.Top + _searchBox.Height / 2 - _prereleaseCb.Height / 2);
            _prereleaseCb.Parent = buildPanel;

            _updateAllBttn.Location = new Point(_prereleaseCb.Right + 5, _searchBox.Top + _searchBox.Height / 2 - _updateAllBttn.Height / 2);
            _updateAllBttn.Parent = buildPanel;

            _moduleCountLbl.Location = new Point(_searchBox.Left, _searchBox.Bottom + 10);
            _moduleCountLbl.Parent = buildPanel;

            _panel.Top = _moduleCountLbl.Bottom + 20;
            _panel.Parent = buildPanel;
            _panel.Size = new Point(buildPanel.Width, buildPanel.Height - _moduleCountLbl.Bottom - 20);
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
                    var newModuleCard = new HModuleCard(module, pkgRoot.Authors[module.AuthorId], _textureLoader, _installService);
                    newModuleCard.Parent = _panel;
                }

                _panel.ResumeLayout(true);

                UpdateModuleFilter();

                this.Built?.Invoke(this, EventArgs.Empty);

                return true;
            } catch (Exception ex) {
                progress.Report($"Loading repo failed: {ex.Message}");
                return false;
            } finally {
                progress.Report("");
            }
        }

        private void _prereleaseCb_CheckedChanged(object sender, CheckChangedEvent e) {
            bool showPrereleases = _prereleaseCb.Checked;

            foreach (var card in _panel.Children.OfType<HModuleCard>()) {
                card.PopulateVersionDropdown(showPrereleases);
            }

            UpdateModuleFilter();
        }

        private async void _updateAllBttn_Click(object sender, Blish_HUD.Input.MouseEventArgs e) {
            _updateAllBttn.Enabled = false;
            _updateAllBttn.Text = "Updating...";

            var cardsToUpdate = _panel.Children.OfType<HModuleCard>().Where(c => c.HasUpdate).ToList();
            int updated = 0;

            foreach (var card in cardsToUpdate) {
                if (await card.PerformUpdate()) {
                    updated++;
                }
            }

            _updateAllBttn.Text = "Update All";
            _updateAllBttn.Enabled = true;
        }

        public void DoUnload() {
            this.Unloaded?.Invoke(this, EventArgs.Empty);
            _searchBox.TextChanged -= _searchBox_TextChanged;
            _prereleaseCb.CheckedChanged -= _prereleaseCb_CheckedChanged;
            _updateAllBttn.Click -= _updateAllBttn_Click;
        }

    }
}
