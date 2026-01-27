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

namespace ModuleManagerPlus.UI {
    internal class ModuleCard : Control {

        private const int IMAGE_LENGTH = 300;

        private const int DESCRIPTION_HEIGHT = 100;
        private const int FOOTER_HEIGHT = 40;

        private const int PADDING = 10;

        public Module Model { get; set; }

        private Texture2D _backgroundMask;

        private Texture2D _defaultBackground;
        private Texture2D _selectedBackground;

        private Effect _maskEffect;

        private AsyncTexture2D _heroTexture;

        public ModuleCard(Module model, TextureLoader textureLoader) {
            Model = model;

            _maskEffect = ModuleManagerPlus.MaskEffect;

            _backgroundMask = textureLoader.LoadTextureFromRef("textures/blackcarousel-tile_default.png");

            _defaultBackground = textureLoader.LoadTextureFromRef("textures/darkcarousel-tile_default.png");
            _selectedBackground = textureLoader.LoadTextureFromRef("textures/darkcarousel-tile_default.png");

            if (model.HeroUrl != null) {
                _heroTexture = textureLoader.LoadTextureFromWeb(model.HeroUrl);
            }

            this.Size = new Point(IMAGE_LENGTH, IMAGE_LENGTH + DESCRIPTION_HEIGHT + FOOTER_HEIGHT);
        }

        protected override void Paint(SpriteBatch spriteBatch, Rectangle bounds) {
            //spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, bounds, Color.FromNonPremultiplied(42, 42, 42, 255));
            spriteBatch.DrawOnCtrl(this, this.MouseOver ? _selectedBackground : _defaultBackground, bounds, Color.White);

            // Draw Name
            spriteBatch.DrawStringOnCtrl(this, this.Model.Name, GameService.Content.DefaultFont18, new Rectangle(PADDING, IMAGE_LENGTH + PADDING, IMAGE_LENGTH, 15), Color.White, false, true, 1, HorizontalAlignment.Left);

            spriteBatch.End();

            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.LinearWrap, null, null, _maskEffect, GameService.Graphics.UIScaleTransform);
            _maskEffect.Parameters["Mask"].SetValue(_backgroundMask);

            //spriteBatch.GraphicsDevice.Textures[1] = _backgroundMask;
            //spriteBatch.GraphicsDevice.SamplerStates[1] = SamplerState.LinearWrap;

            // Draw Hero Image
            if (_heroTexture != null && _heroTexture.HasTexture) {
                spriteBatch.DrawOnCtrl(this, _heroTexture, new Rectangle(0, 0, IMAGE_LENGTH, IMAGE_LENGTH), Color.White);
            } else {
                LoadingSpinnerUtil.DrawLoadingSpinner(this, spriteBatch, new Rectangle(64, 64, IMAGE_LENGTH / 2, IMAGE_LENGTH / 2));
            }


            //spriteBatch.Begin();
            // Draw Description
        }
    }
}
