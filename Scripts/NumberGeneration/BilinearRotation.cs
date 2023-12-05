using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class BilinearRotation
{
    public static Texture2D RotateTexture(Texture2D original, float angle)
    {
        int width = original.width;
        int height = original.height;

        Color[] originalPixels = original.GetPixels();
        Color[] rotatedPixels = new Color[width * height];

        float centerX = width / 2.0f;
        float centerY = height / 2.0f;

        // Переводим угол в радианы
        float radians = angle * Mathf.Deg2Rad;
        float cos = Mathf.Cos(radians);
        float sin = Mathf.Sin(radians);

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                // Поворачиваем пиксель вокруг центра изображения с билинейной интерполяцией
                float offsetX = x - centerX;
                float offsetY = y - centerY;

                float rotatedX = cos * offsetX - sin * offsetY + centerX;
                float rotatedY = sin * offsetX + cos * offsetY + centerY;

                rotatedPixels[y * width + x] = BilinearInterpolation(original, rotatedX, rotatedY);
            }
        }

        Texture2D rotatedTexture = new Texture2D(width, height);
        rotatedTexture.SetPixels(rotatedPixels);
        rotatedTexture.Apply();

        return rotatedTexture;
    }

    private static Color BilinearInterpolation(Texture2D texture, float x, float y)
    {
        int xFloor = Mathf.FloorToInt(x);
        int xCeil = Mathf.CeilToInt(x);
        int yFloor = Mathf.FloorToInt(y);
        int yCeil = Mathf.CeilToInt(y);

        Color topLeft = texture.GetPixel(xFloor, yCeil);
        Color topRight = texture.GetPixel(xCeil, yCeil);
        Color bottomLeft = texture.GetPixel(xFloor, yFloor);
        Color bottomRight = texture.GetPixel(xCeil, yFloor);

        float xLerp = x - xFloor;
        float yLerp = y - yFloor;

        Color top = Color.Lerp(topLeft, topRight, xLerp);
        Color bottom = Color.Lerp(bottomLeft, bottomRight, xLerp);

        return Color.Lerp(bottom, top, yLerp);
    }
}
