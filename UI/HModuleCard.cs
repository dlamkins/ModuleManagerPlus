using Blish_HUD;
using Blish_HUD.Content;
using Blish_HUD.Controls;
using Blish_HUD.Input;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ModuleManagerPlus.Data;
using ModuleManagerPlus.Services;
using ModuleManagerPlus.Utility;
using MonoGame.Extended.BitmapFonts;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace ModuleManagerPlus.UI {
    internal class HModuleCard : Container {

        //private const int CONTROL_WIDTH = 650 /*458*/;

        private int CONTROL_WIDTH => 693;
        
        //private const int IMAGE_LENGTH = 160 /*133*/;

        private const int FOOTER_HEIGHT = 50;

        private const int TITLE_HEIGHT = 25;

        private const int BUFFER = 10;

        //private const int MINHEIGHT = IMAGE_LENGTH + BUFFER * 2 + FOOTER_HEIGHT /*153*/;

        public Module Module { get; set; }
        public Author Author { get; set; }

        private readonly ModuleInstallService _installService;
        private ModuleInstallState _installState;

        private int _heroSize = 160;
        public int HeroSize {
            get => _heroSize;
            set => SetProperty(ref _heroSize, value, true);
        }

        private int MeasuredHeight => _heroSize + BUFFER * 2 + FOOTER_HEIGHT;

        private Texture2D _backgroundMask;
        private Texture2D _avatarMask;

        private Effect _maskEffect;

        private AsyncTexture2D _heroTexture;
        private AsyncTexture2D _authorTexture;

        private Texture2D _downloadsTexture;
        private Texture2D _cornerTexture;
        private Texture2D _cornerGradientTexture;

        bool _newModule = false;
        bool _pendingUpdate = false;

        private static Color _newModuleColor = Color.FromNonPremultiplied(52, 179, 255, 255);
        private static Color _newUpdateColor = Color.FromNonPremultiplied(245, 177, 73, 255);

        private static readonly RasterizerState _scissorOn = new RasterizerState() {
            CullMode = CullMode.None,
            ScissorTestEnable = true
        };

        private Rectangle _heroRegion = Rectangle.Empty;
        private Rectangle _footerRegion = Rectangle.Empty;

        private Rectangle _nameRegion = Rectangle.Empty;
        private Rectangle _descriptionRegion = Rectangle.Empty;

        private Rectangle _authorAvatarRegion = Rectangle.Empty;
        private Rectangle _authorNameRegion = Rectangle.Empty;

        private Rectangle _downloadsIconRegion = Rectangle.Empty;
        private Rectangle _downloadsValueRegion = Rectangle.Empty;

        private Rectangle _releaseNotesCornerRegion = Rectangle.Empty;

        private readonly BlueButton _ctrlMoreInfoBttn;
        private readonly BlueButton _ctrlReleaseNotesBttn;
        private readonly StandardButton _ctrlActionBttn;
        private readonly Dropdown _ctrlVersionDd;

        private string _downloadCount = "0";

        private static readonly BitmapFont _fontModuleName = GameService.Content.GetFont(ContentService.FontFace.Menomonia, ContentService.FontSize.Size24, ContentService.FontStyle.Regular);
        private static readonly BitmapFont _fontModuleDesc = GameService.Content.DefaultFont18;



        public HModuleCard(Module module, Author author, TextureLoader textureLoader, ModuleInstallService installService) {
            this.Module = module;
            this.Author = author;
            _installService = installService;

            _installState = _installService.GetInstallState(module);
            _pendingUpdate = _installState == ModuleInstallState.UpdateAvailable;

            _maskEffect = ModuleManagerPlus.MaskEffect;

            _backgroundMask = textureLoader.LoadTextureFromRef("textures/blackcarousel-tile_default.png");
            _avatarMask = textureLoader.LoadTextureFromRef("textures/avatar_mask.png");

            _downloadsTexture = textureLoader.LoadTextureFromRef("textures/downloads.png");

            _cornerTexture = textureLoader.LoadTextureFromRef("textures/corner-grid.png");

            _cornerGradientTexture = textureLoader.LoadTextureFromRef("textures/bg.png");

            if (module.HeroUrl != null) {
                _heroTexture = textureLoader.LoadTextureFromWeb(module.HeroUrl);
            }

            _authorTexture = textureLoader.LoadTextureFromWeb(author.AvatarUrl);

            _downloadCount = FormatDownloads(module.TotalDownloads);

            _ctrlMoreInfoBttn = new BlueButton() {
                Text = "More Info",
                Width = 96,
                Parent = this
            };

            if (!this.Module.HasMoreInfo) {
                _ctrlMoreInfoBttn.Enabled = false;
                _ctrlMoreInfoBttn.BasicTooltipText = "No additional info is available for this module.";
            }

            _ctrlReleaseNotesBttn = new BlueButton() {
                Text = "Release Notes",
                Width = 128,
                Parent = this
            };

            _ctrlActionBttn = new StandardButton() {
                Location = new Point(0, 0),
                Width = 96,
                Parent = this
            };

            _ctrlVersionDd = new Dropdown() {
                Location = new Point(0, 0),
                Width = 96,
                BasicTooltipText = "Select Version",
                Parent = this
            };

            PopulateVersionDropdown(showPrereleases: false);

            RefreshActionButton();

            _ctrlActionBttn.Click += _ctrlActionBttn_Click;
            _ctrlVersionDd.ValueChanged += _ctrlVersionDd_ValueChanged;

            _ctrlMoreInfoBttn.Click += _ctrlMoreInfoBttn_Click;
            _ctrlReleaseNotesBttn.Click += _ctrlReleaseNotesBttn_Click;

            Invalidate();
        }

        private static string FormatDownloads(int? num) {
            if (!num.HasValue) return "0";

            double value = num.Value;

            if (value >= 1_000_000) {
                return (value / 1_000_000).ToString("0.#") + "m";
            }

            if (value >= 1_000) {
                return (value / 1_000).ToString("0.#") + "k";
            }

            return value.ToString();
        }

        private SemVer.Version GetInstalledVersion() {
            return _installService.FindInstalledModule(this.Module)?.Manifest.Version;
        }

        private Release GetSelectedRelease() {
            return this.Module.Releases.SingleOrDefault(r => $"v{r.Version}" == _ctrlVersionDd.SelectedItem);
        }

        private void RefreshActionButton() {
            var installedVersion = GetInstalledVersion();
            var selectedRelease = GetSelectedRelease();

            if (installedVersion == null) {
                // Not installed — any version can be installed.
                _ctrlActionBttn.Text = "Install";
                _ctrlActionBttn.Enabled = true;
                return;
            }

            if (selectedRelease == null) {
                _ctrlActionBttn.Text = "Installed";
                _ctrlActionBttn.Enabled = false;
                return;
            }

            var selectedVersion = selectedRelease.TypedVersion;

            if (selectedVersion > installedVersion) {
                _ctrlActionBttn.Text = "Update";
                _ctrlActionBttn.Enabled = true;
            } else if (selectedVersion < installedVersion) {
                _ctrlActionBttn.Text = "Downgrade";
                _ctrlActionBttn.Enabled = true;
            } else {
                // Selected version matches installed version.
                _ctrlActionBttn.Text = "Installed";
                _ctrlActionBttn.Enabled = false;
            }
        }

        private void _ctrlVersionDd_ValueChanged(object sender, ValueChangedEventArgs e) {
            RefreshActionButton();
        }

        /// <summary>
        /// Populates the version dropdown, optionally including prerelease versions.
        /// Prereleases the user currently has installed are always shown.
        /// </summary>
        public void PopulateVersionDropdown(bool showPrereleases) {
            var previousSelection = _ctrlVersionDd.SelectedItem;
            _ctrlVersionDd.Items.Clear();

            var installedVersion = GetInstalledVersion();

            foreach (var release in this.Module.Releases.OrderByDescending(r => r.TypedVersion)) {
                if (!showPrereleases && release.IsPrerelease) {
                    // Always include the installed prerelease version so the user can see what they're on.
                    if (installedVersion == null || release.TypedVersion != installedVersion) {
                        continue;
                    }
                }

                _ctrlVersionDd.Items.Add($"v{release.Version}");
            }

            // Try to restore the previous selection, otherwise default to the first item.
            if (previousSelection != null && _ctrlVersionDd.Items.Contains(previousSelection)) {
                _ctrlVersionDd.SelectedItem = previousSelection;
            } else if (_ctrlVersionDd.Items.Count > 0) {
                _ctrlVersionDd.SelectedItem = _ctrlVersionDd.Items.First();
            }

            RefreshActionButton();
        }

        /// <summary>
        /// Returns true if this card's module has an update available (latest stable > installed).
        /// </summary>
        public bool HasUpdate => _installService.GetInstallState(this.Module) == ModuleInstallState.UpdateAvailable;

        /// <summary>
        /// Triggers an update to the latest stable release. Used by "Update All".
        /// </summary>
        public async Task<bool> PerformUpdate(IProgress<string> progress = null) {
            var latestStable = this.Module.Releases
                .Where(r => !r.IsPrerelease)
                .OrderByDescending(r => r.TypedVersion)
                .FirstOrDefault();

            if (latestStable == null) return false;

            _ctrlActionBttn.Enabled = false;
            _ctrlActionBttn.Text = "Updating...";

            var (success, error) = await _installService.UpdateModule(this.Module, latestStable, progress);

            _installState = _installService.GetInstallState(this.Module);
            _pendingUpdate = _installState == ModuleInstallState.UpdateAvailable;
            RefreshActionButton();

            if (!success) {
                _ctrlActionBttn.BasicTooltipText = error;
            }

            return success;
        }

        private async void _ctrlActionBttn_Click(object sender, MouseEventArgs e) {
            var selectedRelease = GetSelectedRelease();
            if (selectedRelease == null) return;

            var progress = new Progress<string>(msg => {
                _ctrlActionBttn.BasicTooltipText = msg;
            });

            string previousText = _ctrlActionBttn.Text;
            _ctrlActionBttn.Enabled = false;

            var installedVersion = GetInstalledVersion();

            bool success;
            string error;

            if (installedVersion == null) {
                // Fresh install
                _ctrlActionBttn.Text = "Installing...";
                (success, error) = await _installService.InstallModule(this.Module, selectedRelease, progress);
            } else {
                // Update or downgrade — both use the same replace flow.
                _ctrlActionBttn.Text = previousText == "Downgrade" ? "Downgrading..." : "Updating...";
                (success, error) = await _installService.UpdateModule(this.Module, selectedRelease, progress);
            }

            _installState = _installService.GetInstallState(this.Module);
            _pendingUpdate = _installState == ModuleInstallState.UpdateAvailable;
            RefreshActionButton();

            if (!success) {
                _ctrlActionBttn.Text = previousText;
                _ctrlActionBttn.Enabled = true;
                _ctrlActionBttn.BasicTooltipText = error;
            }
        }

        private void _ctrlMoreInfoBttn_Click(object sender, MouseEventArgs e) {
            try {
                Process.Start($"https://blishhud.com/modules/?module={this.Module.Namespace}");
            } catch (Exception ex) {
                
            }
        }

        private void _ctrlReleaseNotesBttn_Click(object sender, MouseEventArgs e) {
            try {
                Process.Start($"https://blishhud.com/modules/?module={this.Module.Namespace}#releases");
            } catch (Exception ex) {

            }
        }

        public override void RecalculateLayout() {
            if (_ctrlActionBttn == null || _ctrlVersionDd == null || _ctrlMoreInfoBttn == null) {
                // We exit early because this recalculate call is going to end up redundant, anyways.
                return;
            }

            this.Size = new Point(CONTROL_WIDTH, this.MeasuredHeight);

            // Hero image and footer
            _heroRegion = new Rectangle(BUFFER, BUFFER, _heroSize, _heroSize);
            _footerRegion = new Rectangle(0, this.Height - FOOTER_HEIGHT, CONTROL_WIDTH, FOOTER_HEIGHT);

            // Module name and description
            _nameRegion = new Rectangle(_heroRegion.Right + BUFFER, BUFFER, CONTROL_WIDTH - _heroSize - (BUFFER * 2), TITLE_HEIGHT);
            _descriptionRegion = new Rectangle(_heroRegion.Right + BUFFER, _nameRegion.Bottom + BUFFER, CONTROL_WIDTH - _heroSize - (int)(BUFFER * 2.5), MeasuredHeight - TITLE_HEIGHT);

            // Author info
            _authorAvatarRegion = new Rectangle(BUFFER, _footerRegion.Top + _footerRegion.Height / 2 - 32 / 2, 32, 32);
            _authorNameRegion = new Rectangle(_authorAvatarRegion.Right + BUFFER, _footerRegion.Top, _footerRegion.Width - _authorAvatarRegion.Right - BUFFER, _footerRegion.Height);

            // Align the controls
            _ctrlActionBttn.Location = new Point(this.Width - _ctrlActionBttn.Width - BUFFER, _footerRegion.Top + _footerRegion.Height / 2 - _ctrlActionBttn.Height / 2);
            _ctrlVersionDd.Location = new Point(_ctrlActionBttn.Left - _ctrlVersionDd.Width - BUFFER / 2, _footerRegion.Top + _footerRegion.Height / 2 - _ctrlVersionDd.Height / 2);
                
            _ctrlMoreInfoBttn.Location = new Point(_heroRegion.Right + BUFFER, _ctrlActionBttn.Top);
            _ctrlReleaseNotesBttn.Location = new Point(_ctrlMoreInfoBttn.Right + BUFFER / 2, _ctrlActionBttn.Top);

            // Download stats
            _downloadsIconRegion = new Rectangle(_ctrlReleaseNotesBttn.Right + BUFFER / 2, _footerRegion.Top + _footerRegion.Height / 2 - 12, 24, 24);
            _downloadsValueRegion = new Rectangle(_downloadsIconRegion.Right, _footerRegion.Top, 40, _footerRegion.Height);

            // Release notes
            _releaseNotesCornerRegion = _cornerGradientTexture.Bounds.OffsetBy(this.Width - _cornerGradientTexture.Width, 0);

            base.RecalculateLayout();
        }

        protected override void OnMouseMoved(MouseEventArgs e) {
            var localMousePos = this.RelativeMousePosition;

            if (_downloadsValueRegion.Contains(localMousePos) || _downloadsIconRegion.Contains(localMousePos)) {
                this.BasicTooltipText = $"Total Downloads: {this.Module.TotalDownloads:N0}";
            } else {
                this.BasicTooltipText = string.Empty;
            }

            base.OnMouseMoved(e);
        }

        protected override void OnClick(MouseEventArgs e) {
            //Process.Start($"https://blishhud.com/modules/?module={this.Model.Namespace}");
        }

        private Stopwatch _hoverStart = Stopwatch.StartNew();

        protected override void OnMouseEntered(MouseEventArgs e) {
            _hoverStart = Stopwatch.StartNew();

            base.OnMouseEntered(e);
        }

        protected override void OnMouseLeft(MouseEventArgs e) {
            _hoverStart = Stopwatch.StartNew();

            base.OnMouseLeft(e);
        }

        public override void PaintBeforeChildren(SpriteBatch spriteBatch, Rectangle bounds) {
            if (this.MouseOver) {
                //spriteBatch.DrawOnCtrl(this, _testCornerCover, new Rectangle(0, 0, this.HeroSize + BUFFER * 2, this.HeroSize + BUFFER * 2), Color.White * (MathHelper.Clamp(_hoverStart.ElapsedMilliseconds / 333f, 0f, 0.5f)));
            } else {
                //spriteBatch.DrawOnCtrl(this, _testCornerCover, new Rectangle(0, 0, this.HeroSize + BUFFER * 2, this.HeroSize + BUFFER * 2), Color.White * (0.5f - MathHelper.Clamp(_hoverStart.ElapsedMilliseconds / 333f, 0f, 0.5f)));
            }

            // Background Base
            if (this.MouseOver) {
                spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, bounds, Color.Black * (0.15f + MathHelper.Clamp(_hoverStart.ElapsedMilliseconds / 2000f, 0f, 0.1f)));
                spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, _footerRegion, Color.Black * (0.2f + MathHelper.Clamp(_hoverStart.ElapsedMilliseconds / 2000f, 0f, 0.1f)));
            } else {
                spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, bounds, Color.Black * 0.15f);
                spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, _footerRegion, Color.Black * 0.2f);
                //spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, bounds, Color.Black * (0.15f + (0.5f - MathHelper.Clamp(_hoverStart.ElapsedMilliseconds / 333f, 0f, 0.2f))));
                //spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, _footerRegion, Color.Black * 0.2f);
            }

            // Draw Name
            spriteBatch.DrawStringOnCtrl(this, this.Module.Name, _fontModuleName, _nameRegion, Color.White, false, true, 1, HorizontalAlignment.Left);

            // Draw Description
            spriteBatch.DrawStringOnCtrl(this, this.Module.Description, GameService.Content.DefaultFont18, _descriptionRegion, Color.LightGray, true, false, 1, HorizontalAlignment.Left, VerticalAlignment.Top, _descriptionRegion);

            // Draw Author Name
            spriteBatch.DrawStringOnCtrl(this, this.Author.Name, GameService.Content.DefaultFont18, _authorNameRegion, Color.LightGray, false, true, 1, HorizontalAlignment.Left, VerticalAlignment.Middle);

            // Draw Corner Status
            if (_pendingUpdate) {
                var gemBlue = Color.FromNonPremultiplied(113, 163, 216, 255);
                var fineBlue = Color.FromNonPremultiplied(79, 157, 254, 255);
                var cornerYellow = Color.FromNonPremultiplied(245, 177, 73, 255);

                spriteBatch.DrawOnCtrl(this, _cornerGradientTexture, _releaseNotesCornerRegion);
                spriteBatch.DrawStringOnCtrl(this, "  New Update!", GameService.Content.DefaultFont14, _releaseNotesCornerRegion, cornerYellow, false, HorizontalAlignment.Center);
            } else if (_newModule) {
                var gemBlue = Color.FromNonPremultiplied(113, 163, 216, 255);
                var fineBlue = Color.FromNonPremultiplied(79, 157, 254, 255);
                var cornerBlue = Color.FromNonPremultiplied(52, 179, 255, 255);

                spriteBatch.DrawOnCtrl(this, _cornerGradientTexture, _releaseNotesCornerRegion);
                spriteBatch.DrawStringOnCtrl(this, "  New Module!", GameService.Content.DefaultFont14, _releaseNotesCornerRegion, cornerBlue, false, HorizontalAlignment.Center);
            }

            // Module's Total Downloads
            var statsClr = Color.FromNonPremultiplied(200, 193, 175, 255);
            spriteBatch.DrawOnCtrl(this, _downloadsTexture, _downloadsIconRegion, statsClr);
            spriteBatch.DrawStringOnCtrl(this, _downloadCount, GameService.Content.DefaultFont14, _downloadsValueRegion, statsClr, false, HorizontalAlignment.Left);

            // Enable mask shader
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.LinearWrap, null, _scissorOn, _maskEffect, GameService.Graphics.UIScaleTransform);

            // Draw Hero Image
            if (_heroTexture != null && _heroTexture.HasTexture) {
                _maskEffect.Parameters["Mask"].SetValue(_backgroundMask);
                spriteBatch.DrawOnCtrl(this, _heroTexture, _heroRegion, Color.White);
            } else {
                LoadingSpinnerUtil.DrawLoadingSpinner(this, spriteBatch, _heroRegion);
            }

            if (_authorTexture != null && _authorTexture.HasTexture) {
                // Draw Author Avatar
                _maskEffect.Parameters["Mask"].SetValue(_avatarMask);
                spriteBatch.DrawOnCtrl(this, _authorTexture, _authorAvatarRegion, Color.White);
            } else {
                LoadingSpinnerUtil.DrawLoadingSpinner(this, spriteBatch, _authorAvatarRegion);
            }
        }

        public override void PaintAfterChildren(SpriteBatch spriteBatch, Rectangle bounds) {
            if (_pendingUpdate) {
                spriteBatch.DrawOnCtrl(this, _cornerTexture, _cornerTexture.Bounds, _newUpdateColor);
            } else if (_newModule) {
                spriteBatch.DrawOnCtrl(this, _cornerTexture, _cornerTexture.Bounds, _newModuleColor);
            }
        }

    }
}
