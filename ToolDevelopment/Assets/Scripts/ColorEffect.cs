using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ColorEffect : MonoBehaviour
{
    Color color1, color2;
    float brightness, saturation, gamma, contrast;

    public Texture2D NormalColor(Texture2D texture, float _brightness, float _saturation, float _gamma, float _contrast)
    {
        brightness = _brightness;
        saturation = _saturation;
        gamma = _gamma;
        contrast = _contrast;
        Texture2D newTexture = new Texture2D(texture.width, texture.height);
        Color[] pixels = texture.GetPixels();
        for (int i = 0; i < pixels.Length; i++)
        {
            if (pixels[i].a == 0)
            {
                continue;
            }
            pixels[i] = ColorAdjustments(pixels[i]);
        }
        newTexture.SetPixels(pixels);
        newTexture.Apply();
        newTexture.filterMode = FilterMode.Point;

        return newTexture;
    }

    public Texture2D SingleColor(Texture2D texture, float _brightness, float _saturation, float _gamma, float _contrast)
    {
        Texture2D newTexture = new Texture2D(texture.width, texture.height);
        brightness = _brightness;
        saturation = _saturation;
        gamma = _gamma;
        contrast = _contrast;
        for (int y = 0; y < texture.height; y++)
        {
            for (int x = 0; x < texture.width; x++)
            {
                if (texture.GetPixel(x, y).a == 0)
                {
                    newTexture.SetPixel(x, y, new Color(0, 0, 0, 0), 0);
                }
                else
                {
                    float grayscaleAmount = texture.GetPixel(x, y).grayscale;
                    Color newColor = new Color(color1.r * grayscaleAmount, color1.g * grayscaleAmount, color1.b * grayscaleAmount, 1f);
                    newColor = ColorAdjustments(newColor);
                    newTexture.SetPixel(x, y, newColor, 0);
                }
            }
        }
        newTexture.Apply();
        newTexture.filterMode = FilterMode.Point;
        return newTexture;
    }

    public Texture2D GradientColor(Texture2D texture, float _brightness, float _saturation, float _gamma, float _contrast)
    {
        brightness = _brightness;
        saturation = _saturation;
        gamma = _gamma;
        contrast = _contrast;

        Texture2D newTexture = new Texture2D(texture.width, texture.height);
        var gradient = new Gradient();
        var colorKey = new GradientColorKey[2];

        colorKey[0].color = color1;
        colorKey[0].time = 1.0f;

        colorKey[1].color = color2;
        colorKey[1].time = 0.3f;

        gradient.colorKeys = colorKey;
        Color color;

        for (int y = 0; y < texture.height; y++)
        {
            for (int x = 0; x < texture.width; x++)
            {
                if (texture.GetPixel(x, y).a == 0)
                {
                    newTexture.SetPixel(x, y, new Color(0, 0, 0, 0), 0);
                }
                else
                {
                    float grayscaleAmount = texture.GetPixel(x, y).grayscale;
                    color = gradient.Evaluate(grayscaleAmount);
                    Color newColor = new Color(color.r * grayscaleAmount, color.g * grayscaleAmount, color.b * grayscaleAmount, 1f);
                    newColor = ColorAdjustments(newColor);
                    newTexture.SetPixel(x, y, newColor, 0);
                }
            }
        }
        newTexture.Apply();
        newTexture.filterMode = FilterMode.Point;
        return newTexture;
    }

    Color ColorAdjustments(Color c)
    {
        // apply the brightness adjustment
        Color adjustedColor = c + new Color(brightness, brightness, brightness);

        // apply the saturation adjustment
        float h, s, v;
        Color.RGBToHSV(adjustedColor, out h, out s, out v);
        adjustedColor = Color.HSVToRGB(h, s + saturation, v + saturation);

        // apply the gamma adjustment
        adjustedColor.r = Mathf.Pow(adjustedColor.r, 1f / gamma);
        adjustedColor.g = Mathf.Pow(adjustedColor.g, 1f / gamma);
        adjustedColor.b = Mathf.Pow(adjustedColor.b, 1f / gamma);

        // apply the contrast adjustment
        if (contrast <= 0.5f)
        {
            adjustedColor.r = ((adjustedColor.r - 0.5f) * contrast * 2) + 0.5f;
            adjustedColor.g = ((adjustedColor.g - 0.5f) * contrast * 2) + 0.5f;
            adjustedColor.b = ((adjustedColor.b - 0.5f) * contrast * 2) + 0.5f;
        }
        else
        {
            if (adjustedColor.grayscale >= 0.5f)
            {
                adjustedColor = Color.Lerp(adjustedColor, Color.white, (contrast - 0.5f) * 2);
            }
            else
            {
                adjustedColor = Color.Lerp(adjustedColor, Color.black, (contrast - 0.5f) * 2);
            }
        }

        return adjustedColor;
    }

    public void SetColor1(Color color)
    {
        color1 = color;
    }

    public void SetColor2(Color color)
    {
        color2 = color;
    }

    public void SwapColors()
    {
        Color previousColor = color1;
        color1 = color2;
        color2 = previousColor;
    }
}

//public enum ColorMode
//{
//    NORMAL = 0,
//    SINGLE,
//    GRADIENT,
//}