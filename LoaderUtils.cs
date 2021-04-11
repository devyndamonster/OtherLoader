using Deli.Immediate;
using Deli.Runtime;
using Deli.Runtime.Yielding;
using Deli.VFS;
using System;
using System.Collections.Generic;
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

    }


    public static class LoaderStatus
    {
        private static Dictionary<string, float> activeLoaders = new Dictionary<string, float>();

        public static float GetLoaderProgress()
        {
            if (activeLoaders.Count == 0) return 1;

            float minProgress = activeLoaders.Values.First();

            foreach(float prog in activeLoaders.Values)
            {
                minProgress = Mathf.Min(minProgress, prog);
            }

            return minProgress;
        }

        public static void AddLoader(string modID)
        {
            if (!activeLoaders.ContainsKey(modID)) activeLoaders.Add(modID, 0);
            else OtherLogger.LogError("Tried to track progress on a mod that is already being tracked! ModID: " + modID);
        }

        public static void RemoveLoader(string modID)
        {
            if (activeLoaders.ContainsKey(modID)) activeLoaders.Remove(modID);
        }

        public static void UpdateProgress(string modID, float progress)
        {
            if(activeLoaders.ContainsKey(modID)) activeLoaders[modID] = progress;
        }

    }


    /// <summary>
    /// Credit to BlockBuilder57 for this incredibly useful extension
    /// https://github.com/BlockBuilder57/LSIIC/blob/527927cb921c360d9c158008e24bdeaf2059440e/LSIIC/LSIIC.VirtualObjectsInjector/VirtualObjectsInjectorPlugin.cs#L146
    /// </summary>
    public static class DictionaryExtension
    {
        public static TValue AddOrCreate<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, TKey key) where TValue : new()
        {
            if (!dictionary.ContainsKey(key))
                dictionary.Add(key, new TValue());
            return dictionary[key];
        }
    }

}
