using Blish_HUD;
using Blish_HUD.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Flurl.Http;
using System.IO;

namespace ModuleManagerPlus {
    public class WebTextureLoader {

        private Dictionary<int, AsyncTexture2D> _textureCache = new Dictionary<int, AsyncTexture2D>();

        public AsyncTexture2D LoadTextureFromWeb(string url) {
            int hash = url.GetHashCode();

            if (_textureCache.ContainsKey(hash)) {
                return _textureCache[hash];
            }

            var newTexture = new AsyncTexture2D(null);

            PopulateTexture(newTexture, url);

            _textureCache[hash] = newTexture;

            return newTexture;
        }

        private void PopulateTexture(AsyncTexture2D target, string url) {
            url.GetStreamAsync().ContinueWith(t => {
                if (t.IsFaulted) {
                    target.SwapTexture(ContentService.Textures.Error);
                    return;
                }

                var gc = GameService.Graphics.LendGraphicsDeviceContext();
                try {
                    var newTexture = TextureUtil.FromStreamPremultiplied(gc.GraphicsDevice, t.Result);
                    target.SwapTexture(newTexture);
                } catch {
                    target.SwapTexture(ContentService.Textures.Error);
                } finally {
                    gc.Dispose();
                }
            });
        }

        public void Unload() {
            foreach (var texture in _textureCache.Values) {
                texture.Dispose();
            }
        }

    }
}
