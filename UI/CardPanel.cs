using Blish_HUD;
using Blish_HUD.Content;
using Blish_HUD.Controls;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SharpDX.Direct3D9;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

namespace ModuleManagerPlus.UI {
    internal class CardPanel : FlowPanel {

        private readonly AsyncTexture2D _backgroundTexture;

        public CardPanel(TextureLoader textureLoader) {
            this.Size = new Point(1050, 1050);

            this.OuterControlPadding = new Vector2(50, 50);
            this.ControlPadding = new Vector2(50, 50);
            this.CanScroll = true;
            this.HeightSizingMode = SizingMode.Standard;
            this.WidthSizingMode = SizingMode.Standard;
            this.ClipsBounds = true;

            _backgroundTexture = textureLoader.LoadTextureFromRef("textures/1909321.png");
        }

        public override void PaintBeforeChildren(SpriteBatch spriteBatch, Rectangle bounds) {
            if (_backgroundTexture.HasTexture) {
                spriteBatch.DrawOnCtrl(this, _backgroundTexture, bounds, Color.White);
            }
        }

    }
}
