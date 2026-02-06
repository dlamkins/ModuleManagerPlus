using Blish_HUD;
using Blish_HUD.Content;
using Blish_HUD.Controls;
using Blish_HUD.Input;
using Humanizer;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ModuleManagerPlus.Data;
using ModuleManagerPlus.Utility;
using MonoGame.Extended.BitmapFonts;
using System;
using System.Diagnostics;
using System.Linq;

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
        private Texture2D _lastUpdatedTexture;

        private Texture2D _blueCornerTexture;
        private Texture2D _yellowCornerTexture;
        private Texture2D _cornerTexture;

        private Texture2D _hoverTexture;
        private Texture2D _cornerGradientTexture;

        private Texture2D _testCornerCover;

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
        private Rectangle _lastUpdatedIconRegion = Rectangle.Empty;
        private Rectangle _lastUpdatedValueRegion = Rectangle.Empty;

        private Rectangle _releaseNotesCornerRegion = Rectangle.Empty;

        private readonly BlueButton _ctrlMoreInfoBttn;
        private readonly BlueButton _ctrlReleaseNotesBttn;
        private readonly StandardButton _ctrlActionBttn;
        private readonly Dropdown _ctrlVersionDd;

        private string _downloadCount = "0";
        private string _lastUpdated = "N/A";

        private static readonly BitmapFont _fontModuleName = GameService.Content.GetFont(ContentService.FontFace.Menomonia, ContentService.FontSize.Size24, ContentService.FontStyle.Regular);
        private static readonly BitmapFont _fontModuleDesc = GameService.Content.DefaultFont18;

        public static string FormatDownloads(int? num) {
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

        public HModuleCard(Module module, Author author, TextureLoader textureLoader) {
            this.Module = module;
            this.Author = author;

            //_newModule = RandomUtil.GetRandom(0, 20) == 0;
            //_pendingUpdate = RandomUtil.GetRandom(0, 10) == 0;

            _maskEffect = ModuleManagerPlus.MaskEffect;

            _backgroundMask = textureLoader.LoadTextureFromRef("textures/blackcarousel-tile_default.png");
            _avatarMask = textureLoader.LoadTextureFromRef("textures/avatar_mask.png");

            _downloadsTexture = textureLoader.LoadTextureFromRef("textures/downloads.png");
            _lastUpdatedTexture = textureLoader.LoadTextureFromRef("textures/lastupdated.png");

            _blueCornerTexture = textureLoader.LoadTextureFromRef("textures/corner-blue-grid.png");
            _yellowCornerTexture = textureLoader.LoadTextureFromRef("textures/corner-yellow-grid.png");
            _cornerTexture = textureLoader.LoadTextureFromRef("textures/corner-grid.png");

            _hoverTexture = textureLoader.LoadTextureFromRef("textures/mask-mustHave.png");
            _cornerGradientTexture = textureLoader.LoadTextureFromRef("textures/bg.png");

            _testCornerCover = textureLoader.LoadTextureFromRef("textures/carousel-tile_selected.png");

            //_lightBackground = textureLoader.LoadTextureFromRef("textures/lightcarousel-tile_default.png");

            //_defaultBackground = textureLoader.LoadTextureFromRef("textures/darkcarousel-tile_default.png");
            //_selectedBackground = textureLoader.LoadTextureFromRef("textures/darkcarousel-tile_default.png");

            if (module.HeroUrl != null) {
                _heroTexture = textureLoader.LoadTextureFromWeb(module.HeroUrl);
            }

            _authorTexture = textureLoader.LoadTextureFromWeb(author.AvatarUrl);

            _downloadCount = FormatDownloads(module.TotalDownloads);
            _lastUpdated = DateTime.Now.ToString(); // model.LastRelease.Humanize();

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
                Text = "Install",
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

            _ctrlActionBttn.Click += _ctrlActionBttn_Click;

            foreach (var release in module.Releases.OrderByDescending(r => r.TypedVersion)) {
                _ctrlVersionDd.Items.Add($"v{release.Version}");
            }

            _ctrlMoreInfoBttn.Click += _ctrlMoreInfoBttn_Click;
            _ctrlReleaseNotesBttn.Click += _ctrlReleaseNotesBttn_Click;

            Invalidate();
        }

        private void _ctrlActionBttn_Click(object sender, MouseEventArgs e) {
            var selectedRelease = this.Module.Releases.Single(r => $"v{r.Version}" == _ctrlVersionDd.SelectedItem);

            GameService.Module.ModulePkgRepoHandler.InstallPackage(new PkgH)
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

            int heroBuffer = BUFFER /*MINHEIGHT / 2 - IMAGE_LENGTH / 2*/;
            _heroRegion = new Rectangle(heroBuffer, heroBuffer, _heroSize, _heroSize);
            _footerRegion = new Rectangle(0, this.Height - FOOTER_HEIGHT, CONTROL_WIDTH, FOOTER_HEIGHT);

            _nameRegion = new Rectangle(_heroRegion.Right + heroBuffer, heroBuffer, CONTROL_WIDTH - _heroSize - (heroBuffer * 2), TITLE_HEIGHT);
            _descriptionRegion = new Rectangle(_heroRegion.Right + heroBuffer, _nameRegion.Bottom + heroBuffer, CONTROL_WIDTH - _heroSize - (int)(heroBuffer * 2.5), MeasuredHeight - TITLE_HEIGHT);

            //_authorAvatarRegion = new Rectangle(_heroRegion.Right + heroBuffer, _footerRegion.Top + _footerRegion.Height / 2 - 32 / 2, 32, 32);
            _authorAvatarRegion = new Rectangle(heroBuffer, _footerRegion.Top + _footerRegion.Height / 2 - 32 / 2, 32, 32);
            _authorNameRegion = new Rectangle(_authorAvatarRegion.Right + heroBuffer, _footerRegion.Top, _footerRegion.Width - _authorAvatarRegion.Right - heroBuffer, _footerRegion.Height);

            if (_ctrlActionBttn != null && _ctrlVersionDd != null && _ctrlMoreInfoBttn != null) {
                _ctrlActionBttn.Location = new Point(this.Width - _ctrlActionBttn.Width - heroBuffer, _footerRegion.Top + _footerRegion.Height / 2 - _ctrlActionBttn.Height / 2);
                _ctrlVersionDd.Location = new Point(_ctrlActionBttn.Left - _ctrlVersionDd.Width - heroBuffer / 2, _footerRegion.Top + _footerRegion.Height / 2 - _ctrlVersionDd.Height / 2);
                //_ctrlMoreInfoBttn.Location = new Point(_ctrlVersionDd.Left - _ctrlMoreInfoBttn.Width - heroBuffer * 2, _ctrlActionBttn.Top);
                _ctrlMoreInfoBttn.Location = new Point(_heroRegion.Right + heroBuffer, _ctrlActionBttn.Top);
                _ctrlReleaseNotesBttn.Location = new Point(_ctrlMoreInfoBttn.Right + heroBuffer / 2, _ctrlActionBttn.Top);
            }

            // Module stats
            //_downloadsIconRegion = new Rectangle(_heroRegion.Right + heroBuffer * 3, _footerRegion.Top + _footerRegion.Height / 2 - 12, 24, 24);
            //_downloadsValueRegion = new Rectangle(_downloadsIconRegion.Right + heroBuffer / 2, _footerRegion.Top, 40, _footerRegion.Height);
            _downloadsIconRegion = new Rectangle(_ctrlReleaseNotesBttn.Right + heroBuffer / 2, _footerRegion.Top + _footerRegion.Height / 2 - 12, 24, 24);
            _downloadsValueRegion = new Rectangle(_downloadsIconRegion.Right, _footerRegion.Top, 40, _footerRegion.Height);

            _lastUpdatedIconRegion = new Rectangle(_downloadsValueRegion.Right + heroBuffer, _downloadsIconRegion.Top, 24, 24);
            _lastUpdatedValueRegion = new Rectangle(_lastUpdatedIconRegion.Right + heroBuffer / 2, _footerRegion.Top, 100, _footerRegion.Height);

            // Release notes
            _releaseNotesCornerRegion = _cornerGradientTexture.Bounds.OffsetBy(this.Width - _cornerGradientTexture.Width, 0);




            // Test: Icon > Download Count
            //_downloadsIconRegion = new Rectangle(_releaseNotesCornerRegion.Left + heroBuffer * 2, _releaseNotesCornerRegion.Top + _releaseNotesCornerRegion.Height / 2 - 12, 24, 24);
            //_downloadsValueRegion = new Rectangle(_downloadsIconRegion.Right + heroBuffer / 2, _releaseNotesCornerRegion.Top, 40, _releaseNotesCornerRegion.Height);

            // Test: Download Count > Icon
            //_downloadsIconRegion = new Rectangle(this.Right - heroBuffer - 24, _releaseNotesCornerRegion.Top + _releaseNotesCornerRegion.Height / 2 - 12, 24, 24);
            //_downloadsValueRegion = new Rectangle(this.Right - heroBuffer - 40, _releaseNotesCornerRegion.Top, 40, _releaseNotesCornerRegion.Height);

            base.RecalculateLayout();
        }

        protected override void OnMouseMoved(MouseEventArgs e) {
            var localMousePos = this.RelativeMousePosition;

            if (_downloadsValueRegion.Contains(localMousePos) || _downloadsIconRegion.Contains(localMousePos)) {
                this.BasicTooltipText = $"Total Downloads: {this.Module.TotalDownloads:N0}";
            //} else if (_lastUpdatedValueRegion.Contains(localMousePos)) {
            //    this.BasicTooltipText = $"Last Updated: {Model.LastRelease}";
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

            // Module's last Update
            //spriteBatch.DrawOnCtrl(this, _lastUpdatedTexture, _lastUpdatedIconRegion, statsClr);
            //spriteBatch.DrawStringOnCtrl(this, _lastUpdated, GameService.Content.DefaultFont14, _lastUpdatedValueRegion, statsClr);

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


            /*
            if (this.MouseOver) {
                spriteBatch.DrawOnCtrl(this, _cornerGradientTexture, _releaseNotesCornerRegion);
                spriteBatch.DrawStringOnCtrl(this, "Release Notes", GameService.Content.DefaultFont14, _releaseNotesCornerRegion.OffsetBy(BUFFER, 0), !_releaseNotesCornerRegion.Contains(this.RelativeMousePosition) ? Color.FromNonPremultiplied(200, 193, 175, 255) : Color.White, false, HorizontalAlignment.Center);
            }
            */

            //spriteBatch.DrawOnCtrl(this, _lightBackground, bounds.OffsetBy(0, 1), Color.White);
            //spriteBatch.DrawOnCtrl(this, _lightBackground, bounds.OffsetBy(-1, 1), Color.White);
            //spriteBatch.DrawOnCtrl(this, _lightBackground, bounds.OffsetBy(1, 1), Color.White);
        }

    }
}
