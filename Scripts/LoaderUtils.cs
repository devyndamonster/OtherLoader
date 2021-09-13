
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;

namespace OtherLoader
{
    public static class LoaderUtils
    {

        public static AnvilCallback<AssetBundle> LoadAssetBundle(string path)
        {
            AsyncOperation request = AssetBundle.LoadFromFileAsync(path);

            AnvilCallbackBase anvilCallbackBase = new AnvilCallback<AssetBundle>(request, null);
            return (AnvilCallback<AssetBundle>)anvilCallbackBase;
        }


        public static Sprite LoadSprite(string path)
        {
            Texture2D spriteTexture = LoadTexture(path);
            if (spriteTexture == null) return null;
            Sprite sprite = Sprite.Create(spriteTexture, new Rect(0, 0, spriteTexture.width, spriteTexture.height), new Vector2(0, 0), 100f);
            sprite.name = Path.GetFileName(path);
            return sprite;
        }

        public static Texture2D LoadTexture(string path)
        {
            // Load a PNG or JPG file from disk to a Texture2D
            // Returns null if load fails

            Stream fileStream = File.OpenRead(path);
            MemoryStream mem = new MemoryStream();

            CopyStream(fileStream, mem);

            byte[] fileData = mem.ToArray();

            Texture2D tex2D = new Texture2D(2, 2);
            if (tex2D.LoadImage(fileData)) return tex2D;

            return null;
        }

        public static void SaveSpriteToPNG(Sprite image, string path)
        {
            SaveTextureToPNG(image.texture, path);
        }

        public static void SaveTextureToPNG(Texture2D texture, string path)
        {
            byte[] imageBytes = texture.EncodeToPNG();
            File.WriteAllBytes(path, imageBytes);
        }

        public static void ForceSaveSpriteToPNG(Sprite image, string path)
        {
            ForceSaveTextureToPNG(image.texture, path);
        }

        public static void ForceSaveTextureToPNG(Texture2D texture, string path)
        {
            //This fun bundle of code was found here: https://answers.unity.com/questions/639947/how-to-get-the-pixels-of-a-texture-which-is-in-mem.html
            texture.filterMode = FilterMode.Point;
            RenderTexture rt = RenderTexture.GetTemporary(texture.width, texture.height);
            rt.filterMode = FilterMode.Point;
            RenderTexture.active = rt;
            Graphics.Blit(texture, rt);
            Texture2D img2 = new Texture2D(texture.width, texture.height);
            img2.ReadPixels(new Rect(0, 0, texture.width, texture.height), 0, 0);
            img2.Apply();
            RenderTexture.active = null;
            texture = img2;


            byte[] imageBytes = texture.EncodeToPNG();
            File.WriteAllBytes(path, imageBytes);
        }

        public static void CopyStream(Stream input, Stream output)
        {
            byte[] b = new byte[32768];
            int r;
            while ((r = input.Read(b, 0, b.Length)) > 0)
                output.Write(b, 0, r);
        }



        /// <summary>
        /// Code borrowed from the Sodalite repo untill it officially releases
        /// </summary>
        public static IEnumerator TryCatch<T>(this IEnumerator @this, Action<T> handler) where T : Exception
        {
            bool MoveNext()
            {
                try
                {
                    return @this.MoveNext();
                }
                catch (T e)
                {
                    handler(e);
                    return false;
                }
            }

            while (MoveNext())
                yield return @this.Current;
        }


    }
}
