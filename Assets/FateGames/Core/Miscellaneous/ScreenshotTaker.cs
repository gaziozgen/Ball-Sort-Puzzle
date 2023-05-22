using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
namespace FateGames.Core
{
    public static class ScreenshotTaker
    {
#if UNITY_EDITOR
        [MenuItem("Fate/Take Screenshot %&s")]
        private static void HandleTakeScreenshotClick()
        {
            TakeScreenshot();
        }
        public static void TakeScreenshot()
        {
            string folderPath = Directory.GetCurrentDirectory() + "/Screenshots/";

            if (!Directory.Exists(folderPath))
                Directory.CreateDirectory(folderPath);

            var screenshotName =
                                    "Screenshot_" +
                                    System.DateTime.Now.ToString("dd-MM-yyyy-HH-mm-ss") +
                                    ".png";
            ScreenCapture.CaptureScreenshot(System.IO.Path.Combine(folderPath, screenshotName));
            Debug.Log(folderPath + screenshotName);
        }
#endif

    }

}
