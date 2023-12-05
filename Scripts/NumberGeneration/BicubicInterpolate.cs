using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class BicubicInterpolate
{
    public static Texture2D ScaleImage(Texture2D source, int targetWidth, int targetHeight , int ImageSize)
    {
        Texture2D result = new Texture2D(ImageSize, ImageSize);
        for(int i = 0; i < ImageSize; i++)
        {
            for(int j = 0; j < ImageSize; j++)
            {
                result.SetPixel(i, j, new Color(0, 0, 0));
            }
        }

        for (int y = 0; y < targetHeight; y++)
        {
            for (int x = 0; x < targetWidth; x++)
            {
                float sourceX = x * 1.0f / targetWidth * (source.width - 1);
                float sourceY = y * 1.0f / targetHeight * (source.height - 1);

                result.SetPixel(x, y, GetBilinearFilteredColor(source, sourceX, sourceY));
            }
        }

        result.Apply();
        return result;
    }

    static Color GetBilinearFilteredColor(Texture2D texture, float x, float y)
    {
        int xFloor = Mathf.FloorToInt(x);
        int yFloor = Mathf.FloorToInt(y);
        int xCeil = Mathf.CeilToInt(x);
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
