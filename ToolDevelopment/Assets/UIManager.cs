using SFB;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
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

    int currentFrame;
    int totalFrames;
    public GameObject animationScrollingButtons;
    public TextMeshProUGUI animationFrameCounter;
    public GameObject playButton;
    public GameObject pauseButton;

    public Slider hueSlider;
    public Slider saturationSlider;
    public Slider valueSlider;

    public Slider hueSliderGradient;
    public Slider saturationSliderGradient;
    public Slider valueSliderGradient;

    public GameObject swapColorButton;

    float widthMultiplier;
    float heightMultiplier;

    private void Start()
    {
        totalFrames = 1;
        currentFrame = 0;
        animationScrollingButtons.SetActive(false);
        outlineThickness = 1;
        ClearOutline();
        UpdateThicknessUINumber();
        NormalColor();
        Downscale128x128();
    }

    public void LoadImage()
    {
        WriteResult(StandaloneFileBrowser.OpenFilePanel("Open File", "", "", false));
        mainTexture = ImageLoader.LoadImage(_path.Replace(@"\", "/").Replace("\n", ""));
        if (mainTexture.width % mainTexture.height == 0)
        {
            widthMultiplier = 1;
            heightMultiplier = 1;
            SetImageSize();
            currentFrame = 0;
            totalFrames = mainTexture.width / mainTexture.height;
            if (totalFrames > 1)
            {
                animationScrollingButtons.SetActive(true);
            }
            else
            {
                animationScrollingButtons.SetActive(false);
            }
            VisualizeAnimationFrame();
        }
        else
        {
            totalFrames = 1;
            VisualizeAnimationFrame();
            animationScrollingButtons.SetActive(false);

            if (mainTexture.width > mainTexture.height)
            {
                widthMultiplier = 1;
                heightMultiplier = (float)mainTexture.height / (float)mainTexture.width;
            }
            else
            {
                heightMultiplier = 1;
                widthMultiplier = (float)mainTexture.width / (float)mainTexture.height;
            }
            SetImageSize();
        }

        image.texture = mainTexture;
        PauseAnimation();
        switch (pixelateMode)
        {
            case PixelateMode.X128:
                Downscale128x128();
                break;
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

    void SetImageSize()
    {
        image.GetComponent<RectTransform>().sizeDelta = new Vector2(512 * widthMultiplier, 512 * heightMultiplier);
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
                image.texture = outlineLayer;
                checkmarkOriginal.gameObject.SetActive(false);
            }
        }
    }

    void DownscaleImage(int size)
    {
        Texture2D newTexture = new Texture2D((int)(size * totalFrames * widthMultiplier), (int)(size * heightMultiplier));

        float xScale = mainTexture.width / (float)size;
        float yScale = mainTexture.height / (float)size;

        for (int i = 0; i < totalFrames; i++)
        {
            for (int y = 0; y < Mathf.FloorToInt(size * heightMultiplier); y++)
            {
                for (int x = i; x < Mathf.FloorToInt(size * (i + 1) * widthMultiplier); x++)
                {
                    Color pixelColor = mainTexture.GetPixel((int)((x + 0.5f) * xScale / totalFrames / widthMultiplier), (int)((y + 0.5f) * yScale / heightMultiplier));
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
        pixelateLayer = newTexture;
        image.texture = pixelateLayer;

        UpdateColorMode();
    }

    public void Downscale128x128()
    {
        pixelateMode = PixelateMode.X128;
        CheckmarkUIElements(0, checkmarksPixelate);
        if (image.texture != null) DownscaleImage(128);

    }

    public void Downscale64x64()
    {
        pixelateMode = PixelateMode.X64;
        CheckmarkUIElements(1, checkmarksPixelate);
        if (image.texture != null) DownscaleImage(64);
    }

    public void Downscale32x32()
    {
        pixelateMode = PixelateMode.X32;
        CheckmarkUIElements(2, checkmarksPixelate);
        if (image.texture != null) DownscaleImage(32);
    }

    public void Downscale16x16()
    {
        pixelateMode = PixelateMode.X16;
        CheckmarkUIElements(3, checkmarksPixelate);
        if (image.texture != null) DownscaleImage(16);
    }

    public void Downscale8x8()
    {
        pixelateMode = PixelateMode.X8;
        CheckmarkUIElements(4, checkmarksPixelate);
        if (image.texture != null) DownscaleImage(8);
    }

    public void NormalColor()
    {
        colorMode = ColorMode.NORMAL;
        CheckmarkUIElements(0, checkmarksColor);

        hueSlider.gameObject.SetActive(false);
        saturationSlider.gameObject.SetActive(false);
        valueSlider.gameObject.SetActive(false);

        hueSliderGradient.gameObject.SetActive(false);
        saturationSliderGradient.gameObject.SetActive(false);
        valueSliderGradient.gameObject.SetActive(false);

        swapColorButton.SetActive(false);

        if (image.texture != null)
        {
            colorLayer = pixelateLayer;
            image.texture = colorLayer;
        }
        UpdateOutline();
    }

    public void HSVColor()
    {
        colorMode = ColorMode.HSV;
        CheckmarkUIElements(1, checkmarksColor);
        
        hueSlider.gameObject.SetActive(true);
        saturationSlider.gameObject.SetActive(true);
        valueSlider.gameObject.SetActive(true);

        hueSliderGradient.gameObject.SetActive(false);
        saturationSliderGradient.gameObject.SetActive(false);
        valueSliderGradient.gameObject.SetActive(false);

        swapColorButton.SetActive(false);

        ChangeColorFromHSV();
    }

    public void GradientColor()
    {
        colorMode = ColorMode.GRADIENT;
        CheckmarkUIElements(2, checkmarksColor);

        hueSlider.gameObject.SetActive(true);
        saturationSlider.gameObject.SetActive(true);
        valueSlider.gameObject.SetActive(true);

        hueSliderGradient.gameObject.SetActive(true);
        saturationSliderGradient.gameObject.SetActive(true);
        valueSliderGradient.gameObject.SetActive(true);

        swapColorButton.SetActive(true);

        ChangeGradientFromHSV();
    }

    public void SwapColors()
    {
        float h = hueSlider.value;
        float s = saturationSlider.value;
        float v = valueSlider.value;

        hueSlider.value = hueSliderGradient.value;
        saturationSlider.value = saturationSliderGradient.value;
        valueSlider.value = valueSliderGradient.value;

        hueSliderGradient.value = h;
        saturationSliderGradient.value = s;
        valueSliderGradient.value = v;

        ChangeGradientFromHSV();
    }

    void UpdateColorMode()
    {
        switch (colorMode)
        {
            case ColorMode.NORMAL:
                NormalColor();
                break;
            case ColorMode.HSV:
                ChangeColorFromHSV();
                break;
            case ColorMode.GRADIENT:
                ChangeGradientFromHSV();
                break;
        }
    }

    public void ChangeColorFromHSV()
    {
        if (colorMode == ColorMode.GRADIENT)
        {
            ChangeGradientFromHSV();
            return;
        }
        if (image.texture != null)
        {
            Texture2D newTexture = new Texture2D(pixelateLayer.width, pixelateLayer.height);
            Color color = Color.HSVToRGB(hueSlider.value, saturationSlider.value, valueSlider.value);

            for (int y = 0; y < pixelateLayer.height; y++)
            {
                for (int x = 0; x < pixelateLayer.width; x++)
                {
                    if (pixelateLayer.GetPixel(x, y).a == 0)
                    {
                        newTexture.SetPixel(x, y, new Color(0, 0, 0, 0), 0);
                    }
                    else
                    {
                        float grayscaleAmount = pixelateLayer.GetPixel(x, y).grayscale;
                        newTexture.SetPixel(x, y, new Color(color.r * grayscaleAmount, color.g * grayscaleAmount, color.b * grayscaleAmount, 1f), 0);
                    }
                }
            }
            newTexture.Apply();
            newTexture.filterMode = FilterMode.Point;
            colorLayer = newTexture;
            image.texture = colorLayer;
        }
        UpdateOutline();
    }

    public void ChangeGradientFromHSV()
    {
        colorMode = ColorMode.GRADIENT;
        CheckmarkUIElements(2, checkmarksColor);

        if (image.texture != null)
        {
            Texture2D newTexture = new Texture2D(pixelateLayer.width, pixelateLayer.height);
            var gradient = new Gradient();
            var colorKey = new GradientColorKey[2];

            colorKey[0].color = Color.HSVToRGB(hueSlider.value, saturationSlider.value, valueSlider.value);
            colorKey[0].time = 1.0f;

            colorKey[1].color = Color.HSVToRGB(hueSliderGradient.value, saturationSliderGradient.value, valueSliderGradient.value);
            colorKey[1].time = 0.3f;

            gradient.colorKeys = colorKey;
            Color color;

            for (int y = 0; y < pixelateLayer.height; y++)
            {
                for (int x = 0; x < pixelateLayer.width; x++)
                {
                    if (pixelateLayer.GetPixel(x, y).a == 0)
                    {
                        newTexture.SetPixel(x, y, new Color(0, 0, 0, 0), 0);
                    }
                    else
                    {
                        float grayscaleAmount = pixelateLayer.GetPixel(x, y).grayscale;
                        color = gradient.Evaluate(grayscaleAmount);
                        newTexture.SetPixel(x, y, new Color(color.r * grayscaleAmount, color.g * grayscaleAmount, color.b * grayscaleAmount, 1f), 0);
                    }
                }
            }
            newTexture.Apply();
            newTexture.filterMode = FilterMode.Point;
            colorLayer = newTexture;
            image.texture = colorLayer;
        }
        UpdateOutline();
    }

    public void ClearOutline()
    {
        outlineMode = OutlineMode.NONE;
        CheckmarkUIElements(0, checkmarksOutline);
        if (image.texture != null)
        {
            outlineLayer = colorLayer;
            image.texture = outlineLayer;
        }
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

        Debug.Log(newTexture.width + "   " + newTexture.height);

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
        outlineLayer = newTexture;
        image.texture = outlineLayer;
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

        Debug.Log(newTexture.width + "   " + newTexture.height);

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
        outlineLayer = newTexture;
        image.texture = outlineLayer;
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

        checkmarkOriginal.gameObject.SetActive(false);
    }

    void VisualizeAnimationFrame()
    {
        image.uvRect = new Rect(1f / totalFrames * currentFrame, 0, 1f / totalFrames, 1);
        animationFrameCounter.text = (currentFrame + 1).ToString();
    }

    public void NextAnimationFrame()
    {
        currentFrame++;
        if (currentFrame >= totalFrames)
        {
            currentFrame = 0;
        }
        VisualizeAnimationFrame();
    }

    public void PreviousAnimationFrame()
    {
        currentFrame--;
        if (currentFrame <= 0)
        {
            currentFrame = totalFrames - 1;
        }
        VisualizeAnimationFrame();
    }

    public void PlayAnimation()
    {
        playButton.SetActive(false);
        pauseButton.SetActive(true);
        StartCoroutine("PlayAnimationCoroutine");
    }

    public void PauseAnimation()
    {
        playButton.SetActive(true);
        pauseButton.SetActive(false);
        StopAllCoroutines();
    }

    IEnumerator PlayAnimationCoroutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(0.125f);
            NextAnimationFrame();
        }
    }
}


public enum PixelateMode
{
    X128 = 0,
    X64,
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
    HSV,
    GRADIENT,
}