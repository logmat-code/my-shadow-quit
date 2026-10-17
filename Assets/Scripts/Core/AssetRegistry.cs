using System.Collections.Generic;
using UnityEngine;

namespace MyShadowQuit.Core
{
    /// <summary>
    /// Loads and caches original pixel-art sprites and procedural audio clips from Resources.
    /// Ensures crisp nearest-neighbor filtering and consistent PPU across all runtime objects.
    /// </summary>
    public static class AssetRegistry
    {
        private static readonly Dictionary<string, Sprite> SpriteCache = new Dictionary<string, Sprite>();
        private static readonly Dictionary<string, AudioClip> AudioCache = new Dictionary<string, AudioClip>();

        public static Sprite GetSprite(string relativePath, float ppu = 32f, FilterMode filterMode = FilterMode.Point)
        {
            if (SpriteCache.TryGetValue(relativePath, out Sprite cached) && cached != null)
            {
                return cached;
            }

            string fullPath = "Sprites/" + relativePath;
            Texture2D tex = Resources.Load<Texture2D>(fullPath);
            if (tex != null)
            {
                tex.filterMode = filterMode;
                tex.wrapMode = TextureWrapMode.Clamp;
                Sprite created = Sprite.Create(
                    tex,
                    new Rect(0f, 0f, tex.width, tex.height),
                    new Vector2(0.5f, 0.5f),
                    ppu
                );
                created.name = relativePath;
                SpriteCache[relativePath] = created;
                return created;
            }

            Sprite loadedSprite = Resources.Load<Sprite>(fullPath);
            if (loadedSprite != null)
            {
                loadedSprite.texture.filterMode = filterMode;
                SpriteCache[relativePath] = loadedSprite;
                return loadedSprite;
            }

#if UNITY_EDITOR
            string diskPath = "Assets/Resources/" + fullPath + ".png";
            if (System.IO.File.Exists(diskPath))
            {
                byte[] bytes = System.IO.File.ReadAllBytes(diskPath);
                Texture2D diskTex = new Texture2D(2, 2, TextureFormat.RGBA32, false);
                if (ImageConversion.LoadImage(diskTex, bytes))
                {
                    diskTex.filterMode = filterMode;
                    diskTex.wrapMode = TextureWrapMode.Clamp;
                    Sprite diskSprite = Sprite.Create(
                        diskTex,
                        new Rect(0f, 0f, diskTex.width, diskTex.height),
                        new Vector2(0.5f, 0.5f),
                        ppu
                    );
                    diskSprite.name = relativePath;
                    SpriteCache[relativePath] = diskSprite;
                    return diskSprite;
                }
            }
#endif

            Debug.LogWarning($"[AssetRegistry] Missing sprite at Resources/{fullPath}");
            return null;
        }

        public static Sprite[] GetAnimationFrames(string prefix, int count, float ppu = 32f)
        {
            Sprite[] frames = new Sprite[count];
            for (int i = 0; i < count; i++)
            {
                frames[i] = GetSprite($"{prefix}_{i}", ppu);
            }
            return frames;
        }

        public static AudioClip GetAudio(string relativePath)
        {
            if (AudioCache.TryGetValue(relativePath, out AudioClip cached) && cached != null)
            {
                return cached;
            }

            string fullPath = "Audio/" + relativePath;
            AudioClip clip = Resources.Load<AudioClip>(fullPath);
            if (clip != null)
            {
                AudioCache[relativePath] = clip;
            }
            return clip;
        }
    }
}
