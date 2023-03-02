using SFB;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    private string _path;
    public RawImage image;
    Texture2D mainTexture;
    Texture2D pixelateLayer;
    Texture2D colorLayer;
    Texture2D outlineLayer;

    int outlineThickness;
    public TextMeshProUGUI thicknessUI;

    PixelateMode pixelateMode;
    ColorMode colorMode;
    OutlineMode outlineMode;

    public List<Image> checkmarksPixelate;
    public List<Image> checkmarksColor;
    public List<Image> checkmarksOutline;
    public Image checkmarkOriginal;


    private void Start()
    {
        outlineThickness = 1;
        UpdateThicknessUINumber();
        Downscale64x64();
        ClearOutline();
    }

    public void LoadImage()
    {
        WriteResult(StandaloneFileBrowser.OpenFilePanel("Open File", "", "", false));
        mainTexture = ImageLoader.LoadImage(_path.Replace(@"\", "/").Replace("\n", ""));
        image.texture = mainTexture;
        switch (pixelateMode)
        {
            case PixelateMode.X64:
                Downscale64x64();
                break;
            case PixelateMode.X32:
                Downscale32x32();
                break;
            case PixelateMode.X16:
                Downscale16x16();
                break;
            case PixelateMode.X8:
                Downscale8x8();
                break;
            default:
                break;
        }
    }

    public void ExportImageToPNG()
    {
        ImageLoader.SaveImage((Texture2D)image.texture);
    }

    public void WriteResult(string[] paths)
    {
        if (paths.Length == 0)
        {
            return;
        }

        _path = "";
        foreach (var p in paths)
        {
            _path += p + "\n";
        }
    }

    public void WriteResult(string path)
    {
        _path = path;
    }

    public void ToggleOriginalImage()
    {
        if (image.texture != null)
        {
            if (image.texture != mainTexture)
            {
                image.texture = mainTexture;
                checkmarkOriginal.gameObject.SetActive(true);
            }
            else
            {
                image.texture = pixelateLayer;
                checkmarkOriginal.gameObject.SetActive(false);
            }
        }
    }

    void DownscaleImage(int size)
    {
        Texture2D newTexture = new Texture2D(size, size);

        float xScale = mainTexture.width / (float)size;
        float yScale = mainTexture.height / (float)size;

        // Loop through each pixel in the downscaled texture and set its color to the average color of the corresponding 8x8 block of pixels in the original texture
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                Color pixelColor = mainTexture.GetPixel((int)((x + 0.5f) * xScale), (int)((y + 0.5f) * yScale));
                if (pixelColor.a > 0)
                {
                    pixelColor.a = 1;
                }
                newTexture.SetPixel(x, y, pixelColor, 0);
            }
        }

        // Apply the changes to the downscaled texture
        newTexture.Apply();
        newTexture.filterMode = FilterMode.Point;
        pixelateLayer = newTexture;
        image.texture = pixelateLayer;

        UpdateColorMode();
    }

    public void Downscale64x64()
    {
        pixelateMode = PixelateMode.X64;
        CheckmarkUIElements(0, checkmarksPixelate);
        if (image.texture != null) DownscaleImage(64);
    }

    public void Downscale32x32()
    {
        pixelateMode = PixelateMode.X32;
        CheckmarkUIElements(1, checkmarksPixelate);
        if (image.texture != null) DownscaleImage(32);
    }

    public void Downscale16x16()
    {
        pixelateMode = PixelateMode.X16;
        CheckmarkUIElements(2, checkmarksPixelate);
        if (image.texture != null) DownscaleImage(16);
    }

    public void Downscale8x8()
    {
        pixelateMode = PixelateMode.X8;
        CheckmarkUIElements(3, checkmarksPixelate);
        if (image.texture != null) DownscaleImage(8);
    }

    public void NormalColor()
    {
        colorMode = ColorMode.NORMAL;
        CheckmarkUIElements(0, checkmarksColor);
        if (image.texture != null) colorLayer = pixelateLayer;
    }

    public void BlackAndWhite()
    {
        colorMode = ColorMode.BLACKANDWHITE;
        CheckmarkUIElements(1, checkmarksColor);

        if (image.texture != null)
        {
            Texture2D newTexture = new Texture2D(pixelateLayer.width, pixelateLayer.height);

            for (int y = 0; y < pixelateLayer.height; y++)
            {
                for (int x = 0; x < pixelateLayer.width; x++)
                {
                    float grayscaleAmount = pixelateLayer.GetPixel(x, y).grayscale;
                    newTexture.SetPixel(x, y, new Color(grayscaleAmount, grayscaleAmount, grayscaleAmount, 1), 0);
                }
            }
            newTexture.Apply();
            newTexture.filterMode = FilterMode.Point;
            colorLayer = newTexture;
            image.texture = colorLayer;
        }
    }

    void UpdateColorMode()
    {
        switch (colorMode)
        {
            case ColorMode.NORMAL:
                NormalColor();
                break;
            case ColorMode.BLACKANDWHITE:
                BlackAndWhite();
                break;
            default:
                break;
        }

        UpdateOutline();
    }

    public void ClearOutline()
    {
        outlineMode = OutlineMode.NONE;
        CheckmarkUIElements(0, checkmarksOutline);
        if(image.texture != null) image.texture = pixelateLayer;
    }

    public void OutlineOutsideThin()
    {
        OutlineOutside(OutlineMode.OUTSIDE_THIN);
        CheckmarkUIElements(1, checkmarksOutline);
    }

    public void OutlineOutsideThick()
    {
        OutlineOutside(OutlineMode.OUTSIDE_THICK);
        CheckmarkUIElements(2, checkmarksOutline);
    }

    void OutlineOutside(OutlineMode mode)
    {
        outlineMode = mode;
        if (image.texture == null) return;

        Texture2D currentTexture = colorLayer;
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
        image.texture = newTexture;
    }

    public void OutlineInsideThin()
    {
        OutlineInside(OutlineMode.INSIDE_THIN);
        CheckmarkUIElements(3, checkmarksOutline);
    }

    public void OutlineInsideThick()
    {
        OutlineInside(OutlineMode.INSIDE_THICK);
        CheckmarkUIElements(4, checkmarksOutline);
    }

    void OutlineInside(OutlineMode mode)
    {
        outlineMode = mode;
        if (image.texture == null) return;

        Texture2D currentTexture = colorLayer;
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
        image.texture = newTexture;
    }

    void CheckmarkUIElements(int position, List<Image> checkmarkList)
    {
        foreach (Image checkmark in checkmarkList)
        {
            checkmark.gameObject.SetActive(false);
        }
        checkmarkList[position].gameObject.SetActive(true);
    }
    public void IncreaseThickness()
    {
        if (outlineThickness < 16)
        {
            outlineThickness++;
            UpdateThicknessUINumber();
            UpdateOutline();
        }
    }

    public void DecreaseThickness()
    {
        if (outlineThickness > 1)
        {
            outlineThickness--;
            UpdateThicknessUINumber();
            UpdateOutline();
        }
    }

    void UpdateThicknessUINumber()
    {
        thicknessUI.text = outlineThickness.ToString();
    }

    void UpdateOutline()
    {
        switch (outlineMode)
        {
            case OutlineMode.NONE:
                ClearOutline();
                break;
            case OutlineMode.OUTSIDE_THIN:
                OutlineOutsideThin();
                break;
            case OutlineMode.INSIDE_THIN:
                OutlineInsideThin();
                break;
            case OutlineMode.OUTSIDE_THICK:
                OutlineOutsideThick();
                break;
            case OutlineMode.INSIDE_THICK:
                OutlineInsideThick();
                break;
            default:
                break;
        }
    }
}


public enum PixelateMode
{
    X64 = 0,
    X32,
    X16,
    X8,
}

public enum OutlineMode
{
    NONE = 0,
    OUTSIDE_THIN,
    INSIDE_THIN,
    OUTSIDE_THICK,
    INSIDE_THICK,
}

public enum ColorMode
{
    NORMAL = 0,
    BLACKANDWHITE,
}