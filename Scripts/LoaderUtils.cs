using Deli.Immediate;
using Deli.Runtime;
using Deli.Runtime.Yielding;
using Deli.VFS;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;

namespace OtherLoader
{
    public static class LoaderUtils
    {

        public static DelayedReader<byte[]> DelayedByteReader;
        public static ImmediateReader<byte[]> ImmediateByteReader;

        /*
        public static AnvilCallback<AssetBundle> LoadAssetBundleFromFileAsync(IFileHandle file)
        {
            //Want to read bytes asyncronously
            ResultYieldInstruction<byte[]> readBytes = DelayedByteReader(file);
            //yield return readBytes;
            byte[] bundleBytes = readBytes.Result;

            //We need an AsyncOperation for loading the AssetBundle
            AsyncOperation request = AssetBundle.LoadFromMemoryAsync(bundleBytes);

            //Everything needs to be packaged in one happy AnvilCallback
            AnvilCallbackBase anvilCallbackBase = new AnvilCallback<AssetBundle>(request, null);
            return (AnvilCallback<AssetBundle>)anvilCallbackBase;
        }
        */


        public static AnvilCallback<AssetBundle> LoadAssetBundleFromFile(IFileHandle file)
        {
            byte[] bundleBytes = ImmediateByteReader(file);

            AsyncOperation request = AssetBundle.LoadFromMemoryAsync(bundleBytes);

            AnvilCallbackBase anvilCallbackBase = new AnvilCallback<AssetBundle>(request, null);
            return (AnvilCallback<AssetBundle>)anvilCallbackBase;
        }


        public static AnvilCallback<AssetBundle> LoadAssetBundleFromBytes(byte[] bundleBytes)
        {
            AsyncOperation request = AssetBundle.LoadFromMemoryAsync(bundleBytes);

            AnvilCallbackBase anvilCallbackBase = new AnvilCallback<AssetBundle>(request, null);
            return (AnvilCallback<AssetBundle>)anvilCallbackBase;
        }

        public static AnvilCallback<AssetBundle> LoadAssetBundleFromPath(string path)
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

    }
}
