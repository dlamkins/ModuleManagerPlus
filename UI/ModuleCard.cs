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

namespace ModuleManagerPlus.UI {
    internal class ModuleCard : Control {

        private const int IMAGE_LENGTH = 315;

        private const int DESCRIPTION_HEIGHT = 100;
        private const int FOOTER_HEIGHT = 45;

        private const int PADDING = 15;

        public Module Model { get; set; }

        private Texture2D _backgroundMask;
        private Texture2D _avatarMask;

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

            _backgroundMask = textureLoader.LoadTextureFromRef("textures/blackcarousel-tile_default.png");
            _avatarMask = textureLoader.LoadTextureFromRef("textures/avatar_mask.png");

            _defaultBackground = textureLoader.LoadTextureFromRef("textures/darkcarousel-tile_default.png");
            _selectedBackground = textureLoader.LoadTextureFromRef("textures/darkcarousel-tile_default.png");

            if (model.HeroUrl != null) {
                _heroTexture = textureLoader.LoadTextureFromWeb(model.HeroUrl);
            }

            _authorTexture = textureLoader.LoadTextureFromWeb(model.AuthorAvatarUrl);

            this.Size = new Point(IMAGE_LENGTH, IMAGE_LENGTH + DESCRIPTION_HEIGHT + FOOTER_HEIGHT);
        }

        protected override void Paint(SpriteBatch spriteBatch, Rectangle bounds) {
            //spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, bounds, Color.FromNonPremultiplied(42, 42, 42, 255));
            spriteBatch.DrawOnCtrl(this, this.MouseOver ? _selectedBackground : _defaultBackground, bounds, Color.White);

            // Draw Name
            spriteBatch.DrawStringOnCtrl(this, this.Model.Name, GameService.Content.DefaultFont18, new Rectangle(PADDING, IMAGE_LENGTH + PADDING, IMAGE_LENGTH, 15), Color.White, false, true, 1, HorizontalAlignment.Left);

            // Draw Description
            //spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, new Rectangle(PADDING, IMAGE_LENGTH + PADDING + 25, IMAGE_LENGTH - (2 * PADDING), DESCRIPTION_HEIGHT - 25 - PADDING), Color.Magenta);
            spriteBatch.DrawStringOnCtrl(this, this.Model.Description, GameService.Content.DefaultFont16, new Rectangle(PADDING, IMAGE_LENGTH + PADDING + 25, IMAGE_LENGTH - (2 * PADDING), DESCRIPTION_HEIGHT - 30 - PADDING), Color.LightGray, true, false, 1, HorizontalAlignment.Left, VerticalAlignment.Top, new Rectangle(PADDING, IMAGE_LENGTH + PADDING + 25, IMAGE_LENGTH - (2 * PADDING), DESCRIPTION_HEIGHT - 30 - PADDING));

            // Draw Author Name
            spriteBatch.DrawStringOnCtrl(this, this.Model.AuthorName, GameService.Content.DefaultFont18, new Rectangle(FOOTER_HEIGHT + (PADDING / 2), IMAGE_LENGTH + DESCRIPTION_HEIGHT, Width, FOOTER_HEIGHT - PADDING), Color.LightGray, false, true, 1, HorizontalAlignment.Left, VerticalAlignment.Middle);

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.LinearWrap, null, _scissorOn, _maskEffect, GameService.Graphics.UIScaleTransform);

            // Draw Hero Image
            if (_heroTexture != null && _heroTexture.HasTexture) {
                _maskEffect.Parameters["Mask"].SetValue(_backgroundMask);
                spriteBatch.DrawOnCtrl(this, _heroTexture, new Rectangle(0, 0, IMAGE_LENGTH, IMAGE_LENGTH), Color.White);
            } else {
                LoadingSpinnerUtil.DrawLoadingSpinner(this, spriteBatch, new Rectangle(IMAGE_LENGTH / 2 - 32, IMAGE_LENGTH / 2 - 32, 64, 64));
            }

            if (_authorTexture != null && _authorTexture.HasTexture) {
                // Draw Author Avatar
                _maskEffect.Parameters["Mask"].SetValue(_avatarMask);
                spriteBatch.DrawOnCtrl(this, _authorTexture, new Rectangle(PADDING, IMAGE_LENGTH + DESCRIPTION_HEIGHT, FOOTER_HEIGHT - PADDING, FOOTER_HEIGHT - PADDING), Color.White);
            } else {
                LoadingSpinnerUtil.DrawLoadingSpinner(this, spriteBatch, new Rectangle(PADDING, IMAGE_LENGTH + DESCRIPTION_HEIGHT, FOOTER_HEIGHT - PADDING, FOOTER_HEIGHT - PADDING));
            }


            //spriteBatch.Begin();
            // Draw Description
        }
    }
}
