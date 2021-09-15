
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


        public static string GetModPathFromUniqueID(string uniqueAssetID) 
        {
            return uniqueAssetID.Split(':')[0].Trim();
        }

        public static string GetBundleNameFromUniqueID(string uniqueAssetID)
        {
            return uniqueAssetID.Split(':')[1].Trim();
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
