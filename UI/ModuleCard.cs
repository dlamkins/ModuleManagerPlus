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

        private const int IMAGE_LENGTH = 256;

        private const int DESCRIPTION_HEIGHT = 100;
        private const int FOOTER_HEIGHT = 40;

        private const int PADDING = 10;

        public Module Model { get; set; }

        private AsyncTexture2D _heroTexture;

        public ModuleCard(Module model, WebTextureLoader textureLoader) {
            Model = model;

            if (model.HeroUrl != null) {
                _heroTexture = textureLoader.LoadTextureFromWeb(model.HeroUrl);
            }

            this.Size = new Point(IMAGE_LENGTH, IMAGE_LENGTH + DESCRIPTION_HEIGHT + FOOTER_HEIGHT);
        }

        protected override void Paint(SpriteBatch spriteBatch, Rectangle bounds) {
            spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, bounds, Color.FromNonPremultiplied(42, 42, 42, 255));

            // Draw Hero Image
            if (_heroTexture != null && _heroTexture.HasTexture) {
                spriteBatch.DrawOnCtrl(this, _heroTexture, new Rectangle(0, 0, IMAGE_LENGTH, IMAGE_LENGTH), Color.White);
            } else {
                LoadingSpinnerUtil.DrawLoadingSpinner(this, spriteBatch, new Rectangle(64, 64, IMAGE_LENGTH / 2, IMAGE_LENGTH / 2));
            }

            // Draw Name
            spriteBatch.DrawStringOnCtrl(this, this.Model.Name, GameService.Content.DefaultFont18, new Rectangle(PADDING, IMAGE_LENGTH + PADDING, IMAGE_LENGTH, 15), Color.White, false, true, 1, HorizontalAlignment.Left);

            // Draw Description
        }
    }
}
