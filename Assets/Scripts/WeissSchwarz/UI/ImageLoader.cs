using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

namespace TCG.Weiss.UI
{
    /// <summary>
    /// リモートURLのカード画像を読み込み、同じURLの画像はSpriteとして再利用するユーティリティ。
    /// CardUIの再利用時に古い非同期結果を直接反映しないよう、必要に応じてコールバックで受け取れる形にしています。
    /// </summary>
    public static class ImageLoader
    {
        private static readonly Dictionary<string, Sprite> SpriteCache = new Dictionary<string, Sprite>();

        /// <summary>
        /// 指定URLのSpriteがすでに読み込み済みなら取得します。
        /// 一覧更新やカードUI再生成時に、同じ画像を毎回ダウンロードしないための入口です。
        /// </summary>
        public static bool TryGetCachedSprite(string url, out Sprite sprite)
        {
            if (string.IsNullOrEmpty(url))
            {
                sprite = null;
                return false;
            }

            return SpriteCache.TryGetValue(url, out sprite) && sprite != null;
        }

        /// <summary>
        /// 指定URLから画像を非同期に取得し、Spriteへ変換してキャッシュします。
        /// targetImageが指定されている場合は直接反映し、onLoadedが指定されている場合は呼び出し元で反映可否を判断できます。
        /// </summary>
        public static IEnumerator LoadImage(string url, Image targetImage, Action<Sprite> onLoaded = null)
        {
            if (string.IsNullOrEmpty(url))
            {
                yield break;
            }

            if (TryGetCachedSprite(url, out var cachedSprite))
            {
                if (targetImage != null)
                {
                    targetImage.sprite = cachedSprite;
                }

                onLoaded?.Invoke(cachedSprite);
                yield break;
            }

            using (var request = UnityWebRequestTexture.GetTexture(url))
            {
                // 一部の画像配信元はUser-AgentなしのUnityWebRequestを拒否するため、一般的なブラウザUAを付与します。
                request.SetRequestHeader("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36");

                yield return request.SendWebRequest();

                if (request.result != UnityWebRequest.Result.Success)
                {
                    Debug.LogError($"ImageLoader: failed to load image ({url}): {request.error}");
                    yield break;
                }

                var texture = DownloadHandlerTexture.GetContent(request);
                if (texture == null)
                {
                    yield break;
                }

                var sprite = Sprite.Create(
                    texture,
                    new Rect(0, 0, texture.width, texture.height),
                    Vector2.one * 0.5f);
                sprite.name = $"RemoteCard:{url}";
                SpriteCache[url] = sprite;

                if (targetImage != null)
                {
                    targetImage.sprite = sprite;
                }

                onLoaded?.Invoke(sprite);
            }
        }
    }
}
