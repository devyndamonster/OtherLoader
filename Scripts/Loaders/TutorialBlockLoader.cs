using FistVR;
using System.IO;

namespace OtherLoader.Loaders
{
    public class TutorialBlockLoader : BaseAssetLoader
    {
        protected override void LoadAsset(UnityEngine.Object asset, string bundleId)
        {
            TutorialBlock tutorialBlock = asset as TutorialBlock;

            if (UsesLocalVideo(tutorialBlock))
            {
                LinkVideoToTutorialBlock(tutorialBlock, bundleId);
            }

            IM.TutorialBlockDic[tutorialBlock.ID] = tutorialBlock;

            OtherLogger.Log("Loaded tutorial block with media path: " + tutorialBlock.MediaRef.MediaPath.Path, OtherLogger.LogType.Loading);
        }


        private void LinkVideoToTutorialBlock(TutorialBlock tutorialBlock, string bundleId)
        {
            string videoPath = GetVideoFilePath(bundleId, tutorialBlock.ID);
            tutorialBlock.MediaRef.MediaPath.Path = videoPath;
        }

        private bool UsesLocalVideo(TutorialBlock tutorialBlock)
        {
            return string.IsNullOrEmpty(tutorialBlock.MediaRef.MediaPath.Path);
        }

        private string GetVideoFilePath(string bundleId, string tutorialBlockId)
        {
            string videoPath = Path.Combine(LoaderUtils.GetModPathFromUniqueID(bundleId), tutorialBlockId + ".mp4");

            if (!File.Exists(videoPath))
            {
                throw new FileNotFoundException("Tutorial block had no assigned path, and an MP4 file for it could not be found", videoPath);
            }

            return videoPath;
        }

    }
}
