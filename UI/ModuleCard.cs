using Blish_HUD;
using Blish_HUD.Content;
using Blish_HUD.Controls;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ModuleManagerPlus.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ModuleManagerPlus.Utility;
using Blish_HUD.Input;
using System.Diagnostics;

namespace ModuleManagerPlus.UI {
    internal class ModuleCard : Container {

        private const int IMAGE_LENGTH = 315;

        private const int DESCRIPTION_HEIGHT = 99;
        private const int FOOTER_HEIGHT = 45;
        private const int INTERACT_HEIGHT = 27;

        private const int PADDING = 15;

        public Module Model { get; set; }

        private Texture2D _backgroundMask;
        private Texture2D _avatarMask;

        private Texture2D _lightBackground;

        private Texture2D _defaultBackground;
        private Texture2D _selectedBackground;

        private Effect _maskEffect;

        private AsyncTexture2D _heroTexture;
        private AsyncTexture2D _authorTexture;

        private static readonly RasterizerState _scissorOn = new RasterizerState() {
            CullMode = CullMode.None,
            ScissorTestEnable = true
        };

        public ModuleCard(Module model, TextureLoader textureLoader) {
            Model = model;

            _maskEffect = ModuleManagerPlus.MaskEffect;

            _lightBackground = textureLoader.LoadTextureFromRef("textures/lightcarousel-tile_default.png");

            _backgroundMask = textureLoader.LoadTextureFromRef("textures/blackcarousel-tile_default.png");
            _avatarMask = textureLoader.LoadTextureFromRef("textures/avatar_mask.png");

            _defaultBackground = textureLoader.LoadTextureFromRef("textures/darkcarousel-tile_default.png");
            _selectedBackground = textureLoader.LoadTextureFromRef("textures/darkcarousel-tile_default.png");

            if (model.HeroUrl != null) {
                _heroTexture = textureLoader.LoadTextureFromWeb(model.HeroUrl);
            }

            _authorTexture = textureLoader.LoadTextureFromWeb(model.HeroUrl);

            this.Size = new Point(IMAGE_LENGTH, IMAGE_LENGTH + DESCRIPTION_HEIGHT + FOOTER_HEIGHT /*+ INTERACT_HEIGHT */);

            this.BasicTooltipText = model.Description;

            //var installBttn = new StandardButton {
            //    Text = "Install",
            //    Location = new Point(0, this.Height - INTERACT_HEIGHT),
            //    Parent = this
            //};

            //var versionDropdown = new Dropdown() {
            //    Location = new Point(installBttn.Right, this.Height - INTERACT_HEIGHT),
            //    Width = this.Width - installBttn.Width,
            //    Parent = this
            //};

            //versionDropdown.Items.Add("1.0.0");
            //versionDropdown.Items.Add("1.2.0");
            //versionDropdown.Items.Add("1.3.0");
            //versionDropdown.Items.Add("2.0.0");

            //versionDropdown.SelectedItem = "2.0.0";
        }

        protected override void OnClick(MouseEventArgs e) {
            Process.Start($"https://blishhud.com/modules/?module={this.Model.Namespace}");
        }

        public override void PaintBeforeChildren(SpriteBatch spriteBatch, Rectangle bounds) {
            int offset = this.MouseOver ? -2 : 0;
            //spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, bounds, Color.FromNonPremultiplied(42, 42, 42, 255));

            spriteBatch.DrawOnCtrl(this, this.MouseOver ? _selectedBackground : _defaultBackground, this.MouseOver ? bounds.OffsetBy(0, offset) : bounds, Color.White * (this.MouseOver ? 0.4f : 0.8f));

            // Draw Name
            spriteBatch.DrawStringOnCtrl(this, this.Model.Name, GameService.Content.DefaultFont18, new Rectangle(PADDING, IMAGE_LENGTH + PADDING + offset, IMAGE_LENGTH, 15), Color.White, false, true, 1, HorizontalAlignment.Left);

            // Draw Description
            //spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, new Rectangle(PADDING, IMAGE_LENGTH + PADDING + 25, IMAGE_LENGTH - (2 * PADDING), DESCRIPTION_HEIGHT - 25 - PADDING), Color.Magenta);
            spriteBatch.DrawStringOnCtrl(this, this.Model.Description, GameService.Content.DefaultFont14, new Rectangle(PADDING, IMAGE_LENGTH + PADDING + 25 + offset, IMAGE_LENGTH - (2 * PADDING), DESCRIPTION_HEIGHT - 30 - PADDING), Color.LightGray, true, false, 1, HorizontalAlignment.Left, VerticalAlignment.Top, new Rectangle(PADDING, IMAGE_LENGTH + PADDING + 25, IMAGE_LENGTH - (2 * PADDING), DESCRIPTION_HEIGHT - 30 - PADDING));

            // Draw Author Name
            spriteBatch.DrawStringOnCtrl(this, "Freesnow", GameService.Content.DefaultFont18, new Rectangle(FOOTER_HEIGHT + (PADDING / 2), IMAGE_LENGTH + DESCRIPTION_HEIGHT + offset, Width, FOOTER_HEIGHT - PADDING), Color.LightGray, false, true, 1, HorizontalAlignment.Left, VerticalAlignment.Middle);

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.LinearWrap, null, _scissorOn, _maskEffect, GameService.Graphics.UIScaleTransform);

            // Draw Hero Image
            if (_heroTexture != null && _heroTexture.HasTexture) {
                _maskEffect.Parameters["Mask"].SetValue(_backgroundMask);
                spriteBatch.DrawOnCtrl(this, _heroTexture, new Rectangle(0, +offset, IMAGE_LENGTH, IMAGE_LENGTH), Color.White);
            } else {
                LoadingSpinnerUtil.DrawLoadingSpinner(this, spriteBatch, new Rectangle(IMAGE_LENGTH / 2 - 32, IMAGE_LENGTH / 2 - 32 + offset, 64, 64));
            }

            if (_authorTexture != null && _authorTexture.HasTexture) {
                // Draw Author Avatar
                _maskEffect.Parameters["Mask"].SetValue(_avatarMask);
                spriteBatch.DrawOnCtrl(this, _authorTexture, new Rectangle(PADDING, IMAGE_LENGTH + DESCRIPTION_HEIGHT + offset, FOOTER_HEIGHT - PADDING, FOOTER_HEIGHT - PADDING), Color.White);
            } else {
                LoadingSpinnerUtil.DrawLoadingSpinner(this, spriteBatch, new Rectangle(PADDING, IMAGE_LENGTH + DESCRIPTION_HEIGHT + offset, FOOTER_HEIGHT - PADDING, FOOTER_HEIGHT - PADDING));
            }
        }

        public override void PaintAfterChildren(SpriteBatch spriteBatch, Rectangle bounds) {
            //spriteBatch.DrawOnCtrl(this, _lightBackground, bounds.OffsetBy(0, 1), Color.White);
            //spriteBatch.DrawOnCtrl(this, _lightBackground, bounds.OffsetBy(-1, 1), Color.White);
            //spriteBatch.DrawOnCtrl(this, _lightBackground, bounds.OffsetBy(1, 1), Color.White);
        }

    }
}
