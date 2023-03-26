using SFB;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NewUIManager : MonoBehaviour
{
    //Image variables
    private string _path;
    public RawImage image;
    Texture2D mainTexture;
    Texture2D pixelateLayer;
    Texture2D colorLayer;
    Texture2D outlineLayer;

    int outlineThickness;
    public TextMeshProUGUI thicknessUI;
    public GameObject importText;

    //Effects
    PixelateMode pixelateMode;
    Downscaler downscaler;
    ColorMode colorMode;
    ColorEffect colorEffect;
    OutlineMode outlineMode;
    Outline outline;

    //UI Checkmarks
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

    //Color Effect
    public Slider contrastSlider;
    public Slider gammaSlider;
    public Slider brightnessSlider;
    public Slider saturationSlider;
    //------------
    public GameObject colorPreview1;
    public GameObject colorPreview2;
    public ColorPicker colorPicker1;
    public ColorPicker colorPicker2;
    //------------
    Color color1;
    Color color2;
    //------------
    public GameObject swapColorButton;

    float widthMultiplier;
    float heightMultiplier;

    private void Awake()
    {
        downscaler = new Downscaler();
        colorEffect = new ColorEffect();
        outline = new Outline();
    }

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
        colorPicker1.Initialize();
        colorPicker2.Initialize();
    }

    public void LoadImage()
    {
        WriteResult(StandaloneFileBrowser.OpenFilePanel("Open File", "", "", false));
        mainTexture = ImageLoader.LoadImage(_path.Replace(@"\", "/").Replace("\n", ""));
        Debug.Log(mainTexture);
        importText.SetActive(false);
        image.color = Color.white;
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

        UpdateScaleImage();
        UpdateColorMode();
        UpdateOutline();
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

    void UpdateScaleImage()
    {
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

    public void Downscale128x128()
    {
        pixelateMode = PixelateMode.X128;
        CheckmarkUIElements(0, checkmarksPixelate);
        if (image.texture == null) return;
        pixelateLayer = downscaler.DownscaleImage(mainTexture, 128, widthMultiplier, heightMultiplier, totalFrames);
        image.texture = pixelateLayer;
    }

    public void Downscale64x64()
    {
        pixelateMode = PixelateMode.X64;
        CheckmarkUIElements(1, checkmarksPixelate);
        if (image.texture == null) return;
        pixelateLayer = downscaler.DownscaleImage(mainTexture, 64, widthMultiplier, heightMultiplier, totalFrames);
        image.texture = pixelateLayer;
    }

    public void Downscale32x32()
    {
        pixelateMode = PixelateMode.X32;
        CheckmarkUIElements(2, checkmarksPixelate);
        if (image.texture == null) return;
        pixelateLayer = downscaler.DownscaleImage(mainTexture, 32, widthMultiplier, heightMultiplier, totalFrames);
        image.texture = pixelateLayer;
    }

    public void Downscale16x16()
    {
        pixelateMode = PixelateMode.X16;
        CheckmarkUIElements(3, checkmarksPixelate);
        if (image.texture == null) return;
        pixelateLayer = downscaler.DownscaleImage(mainTexture, 16, widthMultiplier, heightMultiplier, totalFrames);
        image.texture = pixelateLayer;
    }

    public void Downscale8x8()
    {
        pixelateMode = PixelateMode.X8;
        CheckmarkUIElements(4, checkmarksPixelate);
        if (image.texture == null) return;
        pixelateLayer = downscaler.DownscaleImage(mainTexture, 8, widthMultiplier, heightMultiplier, totalFrames);
        image.texture = pixelateLayer;
    }

    public void NormalColor()
    {
        colorMode = ColorMode.NORMAL;
        CheckmarkUIElements(0, checkmarksColor);

        swapColorButton.SetActive(false);
        colorPreview1.SetActive(false);
        colorPreview2.SetActive(false);

        if (image.texture == null) return;

        colorLayer = colorEffect.NormalColor(pixelateLayer, brightnessSlider.value, saturationSlider.value, gammaSlider.value, contrastSlider.value);
        image.texture = colorLayer;
    }

    public void SingleColor()
    {
        colorMode = ColorMode.SINGLE;
        CheckmarkUIElements(1, checkmarksColor);

        swapColorButton.SetActive(false);
        colorPreview1.SetActive(true);
        colorPreview2.SetActive(false);

        if (image.texture == null) return;

        colorLayer = colorEffect.SingleColor(pixelateLayer, brightnessSlider.value, saturationSlider.value, gammaSlider.value, contrastSlider.value);
        image.texture = colorLayer;
    }

    public void GradientColor()
    {
        colorMode = ColorMode.GRADIENT;
        CheckmarkUIElements(2, checkmarksColor);

        swapColorButton.SetActive(true);
        colorPreview1.SetActive(true);
        colorPreview2.SetActive(true);

        if (image.texture == null) return;

        colorLayer = colorEffect.GradientColor(pixelateLayer, brightnessSlider.value, saturationSlider.value, gammaSlider.value, contrastSlider.value);
        image.texture = colorLayer;
    }

    public void SwapColors()
    {
        colorEffect.SwapColors();
    }

    public void ResetContrast()
    {
        contrastSlider.value = 0.5f;
    }

    public void ResetGamma()
    {
        gammaSlider.value = 1f;
    }

    public void ResetBrightness()
    {
        brightnessSlider.value = 0f;
    }

    public void ResetSaturation()
    {
        saturationSlider.value = 0f;
    }

    public void SetColor1(Color newColor)
    {
        colorEffect.SetColor1(newColor);
        UpdateColorMode();
        UpdateOutline();
    }

    public void SetColor2(Color newColor)
    {
        colorEffect.SetColor2(newColor);
        UpdateColorMode();
        UpdateOutline();
    }

    public void UpdateColorMode()
    {
        switch (colorMode)
        {
            case ColorMode.NORMAL:
                NormalColor();
                break;
            case ColorMode.SINGLE:
                SingleColor();
                break;
            case ColorMode.GRADIENT:
                GradientColor();
                break;
        }
    }

    public void ClearOutline()
    {
        outlineMode = OutlineMode.NONE;
        CheckmarkUIElements(0, checkmarksOutline);
        if (image.texture == null) return;
        
        outlineLayer = outline.ClearOutline(colorLayer);
        image.texture = outlineLayer;
    }

    public void OutlineOutsideThin()
    {
        outlineMode = OutlineMode.OUTSIDE_THIN;
        CheckmarkUIElements(1, checkmarksOutline);
        if (image.texture == null) return;

        outlineLayer = outline.OutlineOutsideThin(colorLayer);
        image.texture = outlineLayer;
    }

    public void OutlineOutsideThick()
    {
        outlineMode = OutlineMode.OUTSIDE_THICK;
        CheckmarkUIElements(2, checkmarksOutline);
        if (image.texture == null) return;

        outlineLayer = outline.OutlineOutsideThick(colorLayer);
        image.texture = outlineLayer;
    }


    public void OutlineInsideThin()
    {
        outlineMode = OutlineMode.INSIDE_THIN;
        CheckmarkUIElements(3, checkmarksOutline);
        if (image.texture == null) return;

        outlineLayer = outline.OutlineInsideThin(colorLayer);
        image.texture = outlineLayer;
    }

    public void OutlineInsideThick()
    {
        outlineMode = OutlineMode.INSIDE_THICK;
        CheckmarkUIElements(4, checkmarksOutline); 
        if (image.texture == null) return;

        outlineLayer = outline.OutlineInsideThick(colorLayer);
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
            outline.IncreaseThickness();
        }
    }

    public void DecreaseThickness()
    {
        if (outline.outlineThickness > 1)
        {
            outlineThickness--;
            outline.DecreaseThickness();
        }
    }

    void UpdateThicknessUINumber()
    {
        thicknessUI.text = outline.outlineThickness.ToString();
    }

    public void UpdateOutline()
    {
        UpdateThicknessUINumber();
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
