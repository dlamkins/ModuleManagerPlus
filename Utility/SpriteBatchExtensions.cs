using Blish_HUD;
using Blish_HUD.Controls;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.BitmapFonts;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModuleManagerPlus.Utility {
    internal static class SpriteBatchExtensions {

        public static void DrawStringOnCtrl(this SpriteBatch spriteBatch,
                                            Control ctrl,
                                            string text,
                                            BitmapFont font,
                                            Rectangle destinationRectangle,
                                            Color color,
                                            bool wrap,
                                            bool stroke,
                                            int strokeDistance,
                                            HorizontalAlignment horizontalAlignment,
                                            VerticalAlignment verticalAlignment,
                                            Rectangle? clippingRectangle) {

            if (string.IsNullOrEmpty(text)) return;

            text = wrap ? DrawUtil.WrapText(font, text, destinationRectangle.Width) : text;

            // TODO: This does not account for vertical alignment
            if (horizontalAlignment != HorizontalAlignment.Left && (wrap || text.Contains("\n"))) {
                using (StringReader reader = new StringReader(text)) {
                    string line;

                    int lineHeightDiff = 0;

                    while (destinationRectangle.Height - lineHeightDiff > 0 && (line = reader.ReadLine()) != null) {
                        DrawStringOnCtrl(spriteBatch, ctrl, line, font, destinationRectangle.Add(0, lineHeightDiff, 0, -0), color, wrap, stroke, strokeDistance, horizontalAlignment, verticalAlignment, clippingRectangle);

                        lineHeightDiff += font.LineHeight;
                    }
                }

                return;
            }

            Vector2 textSize = font.MeasureString(text);

            clippingRectangle = clippingRectangle?.ToBounds(ctrl.AbsoluteBounds);

            destinationRectangle = destinationRectangle.ToBounds(ctrl.AbsoluteBounds);

            int xPos = destinationRectangle.X;
            int yPos = destinationRectangle.Y;

            switch (horizontalAlignment) {
                case HorizontalAlignment.Center:
                    xPos += destinationRectangle.Width / 2 - (int)textSize.X / 2;
                    break;
                case HorizontalAlignment.Right:
                    xPos += destinationRectangle.Width - (int)textSize.X;
                    break;
            }

            switch (verticalAlignment) {
                case VerticalAlignment.Middle:
                    yPos += destinationRectangle.Height / 2 - (int)textSize.Y / 2;
                    break;
                case VerticalAlignment.Bottom:
                    yPos += destinationRectangle.Height - (int)textSize.Y;
                    break;
            }

            var textPos = new Vector2(xPos, yPos);

            float absoluteOpacity = ctrl.AbsoluteOpacity();

            if (stroke) {
                var strokePreMultiplied = Color.Black * absoluteOpacity;

                spriteBatch.DrawString(font, text, textPos.OffsetBy(0, -strokeDistance), strokePreMultiplied, clippingRectangle);
                spriteBatch.DrawString(font, text, textPos.OffsetBy(strokeDistance, -strokeDistance), strokePreMultiplied, clippingRectangle);
                spriteBatch.DrawString(font, text, textPos.OffsetBy(strokeDistance, 0), strokePreMultiplied, clippingRectangle);
                spriteBatch.DrawString(font, text, textPos.OffsetBy(strokeDistance, strokeDistance), strokePreMultiplied, clippingRectangle);
                spriteBatch.DrawString(font, text, textPos.OffsetBy(0, strokeDistance), strokePreMultiplied, clippingRectangle);
                spriteBatch.DrawString(font, text, textPos.OffsetBy(-strokeDistance, strokeDistance), strokePreMultiplied, clippingRectangle);
                spriteBatch.DrawString(font, text, textPos.OffsetBy(-strokeDistance, 0), strokePreMultiplied, clippingRectangle);
                spriteBatch.DrawString(font, text, textPos.OffsetBy(-strokeDistance, -strokeDistance), strokePreMultiplied, clippingRectangle);
            }

            spriteBatch.DrawString(font, text, textPos, color * absoluteOpacity, clippingRectangle);
        }

    }
}
