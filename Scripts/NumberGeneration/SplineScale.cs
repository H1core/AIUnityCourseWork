using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public static class SplineScale
{
    public static Texture2D SplineScaleTexture(Texture2D source, int targetWidth, int targetHeight, int resultSize)
    {
        Texture2D result = new Texture2D(resultSize, resultSize);

        for (int y = 0; y < resultSize; y++)
        {
            for (int x = 0; x < resultSize; x++)
            {
                float sourceX = (float)x / targetWidth * (source.width - 1);
                float sourceY = (float)y / targetHeight * (source.height - 1);

                result.SetPixel(x, y, EvaluateSpline(source, sourceX, sourceY));
            }
        }

        result.Apply();
        return result;
    }

    static Color EvaluateSpline(Texture2D texture, float x, float y)
    {
        int xFloor = Mathf.FloorToInt(x);
        int yFloor = Mathf.FloorToInt(y);

        float u = x - xFloor;
        float v = y - yFloor;

        Color p0 = GetPixelClamped(texture, xFloor - 1, yFloor - 1);
        Color p1 = GetPixelClamped(texture, xFloor, yFloor - 1);
        Color p2 = GetPixelClamped(texture, xFloor + 1, yFloor - 1);
        Color p3 = GetPixelClamped(texture, xFloor + 2, yFloor - 1);

        Color p4 = GetPixelClamped(texture, xFloor - 1, yFloor);
        Color p5 = GetPixelClamped(texture, xFloor, yFloor);
        Color p6 = GetPixelClamped(texture, xFloor + 1, yFloor);
        Color p7 = GetPixelClamped(texture, xFloor + 2, yFloor);

        Color p8 = GetPixelClamped(texture, xFloor - 1, yFloor + 1);
        Color p9 = GetPixelClamped(texture, xFloor, yFloor + 1);
        Color p10 = GetPixelClamped(texture, xFloor + 1, yFloor + 1);
        Color p11 = GetPixelClamped(texture, xFloor + 2, yFloor + 1);

        Color p12 = GetPixelClamped(texture, xFloor - 1, yFloor + 2);
        Color p13 = GetPixelClamped(texture, xFloor, yFloor + 2);
        Color p14 = GetPixelClamped(texture, xFloor + 1, yFloor + 2);
        Color p15 = GetPixelClamped(texture, xFloor + 2, yFloor + 2);

        Color row1 = CatmullRom(p0, p1, p2, p3, u);
        Color row2 = CatmullRom(p4, p5, p6, p7, u);
        Color row3 = CatmullRom(p8, p9, p10, p11, u);
        Color row4 = CatmullRom(p12, p13, p14, p15, u);

        return CatmullRom(row1, row2, row3, row4, v);
    }

    static Color CatmullRom(Color p0, Color p1, Color p2, Color p3, float t)
    {
        float tt = t * t;
        float ttt = tt * t;

        return 0.5f * (
            (2.0f * p1) +
            (p2 - p0) * t +
            (2.0f * p0 - 5.0f * p1 + 4.0f * p2 - p3) * tt +
            (3.0f * p1 - 3.0f * p2 + p3 - p0) * ttt
        );
    }

    static Color GetPixelClamped(Texture2D texture, int x, int y)
    {
        x = Mathf.Clamp(x, 0, texture.width - 1);
        y = Mathf.Clamp(y, 0, texture.height - 1);
        return texture.GetPixel(x, y);
    }
}

