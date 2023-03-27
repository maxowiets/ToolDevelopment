using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Downscaler : MonoBehaviour
{
    public Texture2D DownscaleImage(Texture2D texture, int size, float widthMultiplier, float heightMultiplier, int totalFrames)
    {
        Texture2D newTexture = new Texture2D((int)(size * totalFrames * widthMultiplier), (int)(size * heightMultiplier));

        Debug.Log(texture);
        float xScale = texture.width / (float)size;
        float yScale = texture.height / (float)size;

        for (int i = 0; i < totalFrames; i++)
        {
            for (int y = 0; y < Mathf.FloorToInt(size * heightMultiplier); y++)
            {
                for (int x = i; x < Mathf.FloorToInt(size * (i + 1) * widthMultiplier); x++)
                {
                    Color pixelColor = texture.GetPixel((int)((x + 0.5f) * xScale / totalFrames / widthMultiplier), (int)((y + 0.5f) * yScale / heightMultiplier));
                    if (pixelColor.a > 0)
                    {
                        pixelColor.a = 1;
                    }
                    newTexture.SetPixel(x, y, pixelColor, 0);
                }
            }
        }

        // Apply the changes to the downscaled texture
        newTexture.Apply();
        newTexture.filterMode = FilterMode.Point;
        return newTexture;
    }

    public Texture2D DownscaleImage(Texture2D texture, int size)
    {
        return DownscaleImage(texture, size, 1, 1, 1);
    }

    public Texture2D DownscaleImage(Texture2D texture, int size, float widthMultiplier, float heightMultiplier)
    {
        return DownscaleImage(texture, size, widthMultiplier, heightMultiplier, 1);
    }
}

public enum DownscaleMode
{
    X128 = 0,
    X64,
    X32,
    X16,
    X8,
}