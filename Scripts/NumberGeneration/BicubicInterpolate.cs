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

                result.SetPixel(x, y, BicubicFilteredColor(source, sourceX, sourceY));
            }
        }

        result.Apply();
        return result;
    }
    static Color BicubicFilteredColor(Texture2D texture, float x, float y)
    {
        int xFloor = Mathf.FloorToInt(x);
        int yFloor = Mathf.FloorToInt(y);

        float u = x - xFloor;
        float v = y - yFloor;

        float[] weights = new float[4];
        for (int i = 0; i < 4; i++)
        {
            float d = Mathf.Abs(y - (yFloor - 1 + i));
            weights[i] = CubicFilter(d);
        }

        Color finalColor = Color.black;

        for (int i = 0; i < 4; i++)
        {
            int yIndex = Mathf.Clamp(yFloor - 1 + i, 0, texture.height - 1);

            for (int j = 0; j < 4; j++)
            {
                int xIndex = Mathf.Clamp(xFloor - 1 + j, 0, texture.width - 1);
                finalColor += texture.GetPixel(xIndex, yIndex) * weights[i] * weights[j];
            }
        }

        return finalColor;
    }

    static float CubicFilter(float x)
    {
        const float B = 0.04f;
        const float C = 0.16f;
        const float P0 = (6.0f - 2.0f * B) / 6.0f;
        const float P2 = (-18.0f + 12.0f * B + 6.0f * C) / 6.0f;
        const float P3 = (12.0f - 9.0f * B - 6.0f * C) / 6.0f;
        const float P4 = (-B - 6.0f * C) / 6.0f;

        x = Mathf.Abs(x);

        if (x < 1.0f)
        {
            return P0 + x * x * (P2 + x * P3);
        }
        else
        {
            return P4 * (Mathf.Pow((x - 1.0f), 3));
        }
    }

}
