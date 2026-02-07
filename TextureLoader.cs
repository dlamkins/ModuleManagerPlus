using Blish_HUD;
using Blish_HUD.Content;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using Flurl.Http;
using Blish_HUD.Modules.Managers;

namespace ModuleManagerPlus {
    public class TextureLoader {

        private Dictionary<int, AsyncTexture2D> _textureCache = new Dictionary<int, AsyncTexture2D>();

        private readonly ContentsManager _contents;

        public TextureLoader(ContentsManager contents) {
            _contents = contents;
        }

        public Texture2D LoadTextureFromRef(string path) {
            int hash = path.GetHashCode();

            if (!_textureCache.ContainsKey(hash)) {
                _textureCache[hash] = new AsyncTexture2D(_contents.GetTexture(path));
            }

            return _textureCache[hash];
        }

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
            _textureCache.Clear();
        }

    }
}
