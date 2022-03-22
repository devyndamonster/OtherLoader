
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


        public static string GetModPathFromUniqueID(string bundleID) 
        {
            return bundleID.Split(new string[] { " : " }, StringSplitOptions.None)[0].Trim();
        }

        public static string GetBundleNameFromUniqueID(string bundleID)
        {
            return bundleID.Split(new string[] { " : " }, StringSplitOptions.None)[1].Trim();
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


        public static IEnumerator WaitUntilBundleCanLoad(string bundleId)
        {
            bool overTime = false;
            while (!LoaderStatus.CanOrderedModLoad(bundleId))
            {
                if (!overTime && Time.time - LoaderStatus.LastLoadEventTime > 30)
                {
                    OtherLogger.Log("Bundle has been waiting a long time to load! (" + bundleId + ")", OtherLogger.LogType.General);
                    LoaderStatus.PrintWaitingBundles(bundleId);
                    overTime = true;
                }

                yield return null;
            }
        }


    }
}
