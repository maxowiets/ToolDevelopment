using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ColorPicker : MonoBehaviour
{
    public RectTransform texture;
    public Image actualColor;
    public NewUIManager manager;
    public Texture2D refSprite;

    public void OnClickPickerColor()
    {
        SetColor();
    }

    void SetColor()
    {
        Vector3 imagePos = texture.position;
        float globalPosX = Input.mousePosition.x - imagePos.x;
        float globalPosY = Input.mousePosition.y - imagePos.y;

        int localPosX = (int)(globalPosX * (refSprite.width / texture.rect.width));
        int localPosY = (int)(globalPosY * (refSprite.height / texture.rect.height));

        Color color = refSprite.GetPixel(localPosX, localPosY);
        SetActualColor(color);
        
    }
    Color GetNewColor()
    {
        Vector3 imagePos = texture.position;
        float globalPosX = Input.mousePosition.x - imagePos.x;
        float globalPosY = Input.mousePosition.y - imagePos.y;

        int localPosX = (int)(globalPosX * (refSprite.width / texture.rect.width));
        int localPosY = (int)(globalPosY * (refSprite.height / texture.rect.height));

        Color color = refSprite.GetPixel(localPosX, localPosY);
        return color;
    }

    void SetActualColor(Color c)
    {
        actualColor.color = c;
    }

    public void SetColor1()
    {
        manager.SetColor1(GetNewColor());
    }
    public void SetColor2()
    {
        manager.SetColor2(GetNewColor());
    }

    public void Initialize()
    {
        manager.SetColor1(Color.white);
        manager.SetColor2(Color.white);
        gameObject.SetActive(false);
    }

    public void DisableColorPicker()
    {
        gameObject.SetActive(false);
    }
}
