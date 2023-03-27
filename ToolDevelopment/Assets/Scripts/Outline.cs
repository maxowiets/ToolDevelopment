using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Outline : MonoBehaviour
{
    public int outlineThickness = 1;

    public Texture2D ClearOutline(Texture2D texture)
    {
        return texture;
    }

    public Texture2D OutlineOutsideThin(Texture2D texture)
    {
        return OutlineOutside(texture, OutlineMode.OUTSIDE_THIN);
    }

    public Texture2D OutlineOutsideThick(Texture2D texture)
    {
        return OutlineOutside(texture, OutlineMode.OUTSIDE_THICK);
    }

    Texture2D OutlineOutside(Texture2D texture, OutlineMode mode)
    {
        Texture2D currentTexture = texture;
        Texture2D newTexture = new Texture2D(currentTexture.width, currentTexture.height, TextureFormat.RGBA32, false);

        for (int x = 0; x < currentTexture.width; x++)
        {
            for (int y = 0; y < currentTexture.height; y++)
            {
                Color pixelColor = currentTexture.GetPixel(x, y);
                if (pixelColor.a == 0)
                {
                    bool outlinePixel = false;
                    //loop through neighbouring pixels based on thickness
                    for (int i = -outlineThickness; i < outlineThickness + 1; i++)
                    {
                        for (int j = -outlineThickness; j < outlineThickness + 1; j++)
                        {
                            if (i == 0 && j == 0) continue;
                            //this makes the outline a bit thinner
                            if (mode == OutlineMode.OUTSIDE_THIN)
                            {
                                if (Mathf.Abs(i) + Mathf.Abs(j) > outlineThickness) continue;
                            }
                            //keep the calculations inside the texture width and height
                            if (x + i >= 0 && x + i < currentTexture.width && y + j >= 0 && y + j < currentTexture.height)
                            {
                                if (currentTexture.GetPixel(x + i, y + j).a > 0)
                                {
                                    outlinePixel = true;
                                    break;
                                }
                            }
                        }
                        if (outlinePixel) break;
                    }
                    if (outlinePixel) pixelColor = Color.black;
                    newTexture.SetPixel(x, y, pixelColor, 0);
                }
                //make pixels at the edge black as well if alpha != 0
                else if (x < outlineThickness || x >= currentTexture.width - outlineThickness || y < outlineThickness || y >= currentTexture.height - outlineThickness)
                {
                    pixelColor = Color.black;
                    newTexture.SetPixel(x, y, pixelColor, 0);
                }
                else
                {
                    newTexture.SetPixel(x, y, pixelColor, 0);
                }
            }
        }

        newTexture.Apply();
        newTexture.filterMode = FilterMode.Point;
        return newTexture;
    }

    public Texture2D OutlineInsideThin(Texture2D texture)
    {
        return OutlineInside(texture, OutlineMode.INSIDE_THIN);
    }

    public Texture2D OutlineInsideThick(Texture2D texture)
    {
        return OutlineInside(texture, OutlineMode.INSIDE_THICK);
    }

    Texture2D OutlineInside(Texture2D texture, OutlineMode mode)
    {
        Texture2D currentTexture = texture;
        Texture2D newTexture = new Texture2D(currentTexture.width, currentTexture.height, TextureFormat.RGBA32, false);

        for (int x = 0; x < currentTexture.width; x++)
        {
            for (int y = 0; y < currentTexture.height; y++)
            {
                Color pixelColor = currentTexture.GetPixel(x, y);
                if (pixelColor.a != 0)
                {
                    bool outlinePixel = false;
                    if (x < outlineThickness || x >= currentTexture.width - outlineThickness || y < outlineThickness || y >= currentTexture.height - outlineThickness)
                    {
                        pixelColor = Color.black;
                        newTexture.SetPixel(x, y, pixelColor, 0);
                        continue;
                    }
                    //loop through neighbouring pixels based on thickness
                    for (int i = -outlineThickness; i < outlineThickness + 1; i++)
                    {
                        for (int j = -outlineThickness; j < outlineThickness + 1; j++)
                        {
                            if (i == 0 && j == 0) continue;
                            //this makes the outline a bit thinner
                            if (mode == OutlineMode.INSIDE_THIN)
                            {
                                if (Mathf.Abs(i) + Mathf.Abs(j) > outlineThickness) continue;
                            }
                            //keep the calculations inside the texture width and height
                            if (x + i >= 0 && x + i < currentTexture.width && y + j >= 0 && y + j < currentTexture.height)
                            {
                                if (currentTexture.GetPixel(x + i, y + j).a == 0)
                                {
                                    outlinePixel = true;
                                    break;
                                }
                            }
                        }
                        if (outlinePixel) break;
                    }
                    if (outlinePixel) pixelColor = Color.black;
                    newTexture.SetPixel(x, y, pixelColor, 0);
                }
                else
                {
                    newTexture.SetPixel(x, y, pixelColor, 0);
                }
            }
        }

        newTexture.Apply();
        newTexture.filterMode = FilterMode.Point;
        return newTexture;
    }

    public void IncreaseThickness()
    {
        if (outlineThickness < 16)
        {
            outlineThickness++;
        }
    }

    public void DecreaseThickness()
    {
        if (outlineThickness > 1)
        {
            outlineThickness--;
        }
    }
}

public enum OutlineMode
{
    NONE = 0,
    OUTSIDE_THIN,
    INSIDE_THIN,
    OUTSIDE_THICK,
    INSIDE_THICK,
}