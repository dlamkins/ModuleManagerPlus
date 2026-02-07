using Blish_HUD.Controls;
using Blish_HUD.Graphics.UI;
using Blish_HUD.Modules.Managers;
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
        private readonly Label _sortLbl;
        private readonly Dropdown _sortOrderDd;
        private readonly Checkbox _prereleaseCb;
        private readonly StandardButton _updateAllBttn;
        private readonly Label _moduleCountLbl;

        public ModuleRepoView(ContentsManager contentsManager) {
            _textureLoader = new TextureLoader(contentsManager);
            _installService = new ModuleInstallService();

            _searchBox = new TextBox() {
                PlaceholderText = "Search Modules...",
            };

            _sortLbl = new Label() {
                Text = "Sort",
                AutoSizeWidth = true
            };

            _sortOrderDd = new Dropdown() {
                Width = 150,
                SelectedItem = "Downloads"
            };

            _sortOrderDd.Items.Add("Downloads");
            _sortOrderDd.Items.Add("A to Z");
            _sortOrderDd.Items.Add("Z to A");
            //_sortOrderDd.Items.Add("Last Updated");

            _prereleaseCb = new Checkbox() {
                Text = "Show Prereleases",
            };

            _updateAllBttn = new StandardButton() {
                Text = "Update All",
                Width = 96,
            };

            _moduleCountLbl = new Label() {
                Text = "Loading modules...",
                AutoSizeWidth = true
            };

            _searchBox.TextChanged += _searchBox_TextChanged;
            _sortOrderDd.ValueChanged += _sortOrderDd_ValueChanged;
            _prereleaseCb.CheckedChanged += _prereleaseCb_CheckedChanged;
            _updateAllBttn.Click += _updateAllBttn_Click;

            _panel = new FlowPanel() {
                OuterControlPadding = new Vector2(0, 0),
                ControlPadding = new Vector2(0, 30),
                HeightSizingMode = SizingMode.Standard,
                WidthSizingMode = SizingMode.Standard,
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

        private void UpdateModuleSortOrder() {
            switch (_sortOrderDd.SelectedItem) {
                case "Downloads":
                    _panel.SortChildren<HModuleCard>((a, b) => b.Module.TotalDownloads.CompareTo(a.Module.TotalDownloads));
                    break;
                case "A to Z":
                    _panel.SortChildren<HModuleCard>((a, b) => string.Compare(a.Module.Name, b.Module.Name, StringComparison.OrdinalIgnoreCase));
                    break;
                case "Z to A":
                    _panel.SortChildren<HModuleCard>((a, b) => string.Compare(b.Module.Name, a.Module.Name, StringComparison.OrdinalIgnoreCase));
                    break;
                case "Last Updated":
                    // TODO: Implement once Release includes a release date field.
                    break;
            }
        }

        private void _sortOrderDd_ValueChanged(object sender, ValueChangedEventArgs e) {
            UpdateModuleSortOrder();
        }

        private void _searchBox_TextChanged(object sender, EventArgs e) {
            UpdateModuleFilter();
        }

        public void DoBuild(Container buildPanel) {
            _sortLbl.Parent = buildPanel;
            _sortOrderDd.Parent = buildPanel;

            int sortAreaWidth = _sortLbl.Width + 5 + _sortOrderDd.Width;

            // Search box | "Sort" | Dropdown
            _searchBox.Width = buildPanel.Width - sortAreaWidth - 10;
            _searchBox.Parent = buildPanel;

            _sortLbl.Location = new Point(_searchBox.Right + 10, _searchBox.Top + _searchBox.Height / 2 - _sortLbl.Height / 2);
            _sortOrderDd.Location = new Point(_sortLbl.Right + 5, _searchBox.Top + _searchBox.Height / 2 - _sortOrderDd.Height / 2);

            // Module count label
            _moduleCountLbl.Location = new Point(_searchBox.Left + 5, _searchBox.Bottom + 10);
            _moduleCountLbl.Parent = buildPanel;

            // Prerelease checkbox | Update All button
            _updateAllBttn.Location = new Point(_sortOrderDd.Right - _updateAllBttn.Width, _searchBox.Bottom + 10);
            _updateAllBttn.Parent = buildPanel;

            _prereleaseCb.Location = new Point(_updateAllBttn.Left - _prereleaseCb.Width - 5, _updateAllBttn.Bottom - _updateAllBttn.Height / 2 - _prereleaseCb.Height / 2);
            _prereleaseCb.Parent = buildPanel;

            _panel.Top = _moduleCountLbl.Bottom + 20;
            _panel.Parent = buildPanel;
            _panel.Size = new Point(buildPanel.Width, buildPanel.Height - _moduleCountLbl.Bottom - 20);

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
                    if (module.Namespace == "fs.modulemanagerplus") { 
                        // We're not going to support updating ourself - at least not in this version.
                        continue; 
                    }

                    var newModuleCard = new HModuleCard(module, pkgRoot.Authors[module.AuthorId], _textureLoader, _installService);
                    newModuleCard.Parent = _panel;
                }

                _panel.ResumeLayout(true);

                UpdateModuleSortOrder();
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
            _searchBox?.TextChanged -= _searchBox_TextChanged;
            _sortOrderDd?.ValueChanged -= _sortOrderDd_ValueChanged;
            _prereleaseCb?.CheckedChanged -= _prereleaseCb_CheckedChanged;
            _updateAllBttn?.Click -= _updateAllBttn_Click;

            _textureLoader?.Unload();
        }

    }
}
