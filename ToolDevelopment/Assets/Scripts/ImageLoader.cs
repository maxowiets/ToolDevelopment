using SFB;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class ImageLoader
{
    public static Texture2D LoadImage(string filePath)
    {
        Texture2D tex = null;
        byte[] fileData;

        if (File.Exists(filePath))
        {
            fileData = File.ReadAllBytes(filePath);
            tex = new Texture2D(2, 2);
            tex.LoadImage(fileData);
        }
        return tex;
    }

    public static void SaveImage(Texture2D texture)
    {
        byte[] bytes = texture.EncodeToPNG();
        var path = StandaloneFileBrowser.SaveFilePanel("Save File", "", "image", "png");
        File.WriteAllBytes(path, bytes);
    }
}
