using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
public static class ProgrammLogic
{
    public static (int lefted, int righted, int upper, int lower) FindEdgePixels(Texture2D texture, int ImageSize)
    {
        int lefted = 20, righted = 0, upper = 0, lower = 20;
        //int countOfWhiteNeighborg = 0;
        for (int i = 0; i < ImageSize; i++)
        {
            for (int j = 0; j < ImageSize; j++)
            {
                if (texture.GetPixel(i, j).grayscale >= 0.37f)
                {
                    
                    if (lefted > i)
                    {
                        lefted = i;
                    }
                    if (righted < i)
                    {
                        righted = i;
                    }
                    if (upper < j)
                        upper = j;
                    if (lower > j)
                        lower = j;
                }
            }
        }
        return (lefted, righted, upper, lower);
    }
    public static Texture2D AddBiasToImage(Texture2D texture , int ImageSize , int a, int b)
    {
        Texture2D result = CreateEmptyField(Color.black , ImageSize);
        for (int i = 0; i < ImageSize; i++)
        {
            for (int j = 0; j < ImageSize; j++)
            {
                int ShiftedX = (i + b + ImageSize) % ImageSize;
                int ShiftedY = (j + a + ImageSize) % ImageSize;
                result.SetPixel(ShiftedX, ShiftedY, texture.GetPixel(i, j));
            }
        }
        result.Apply();
        return result;
    }
    public static Texture2D CreateEmptyField(Color standartColor , int ImageSize)
    {
        Texture2D startTexture = new Texture2D(ImageSize, ImageSize);
        for (int i = 0; i < ImageSize; i++)
        {
            for (int j = 0; j < ImageSize; j++)
            {
                startTexture.SetPixel(i, j, standartColor);
            }
        }
        startTexture.Apply();
        startTexture.filterMode = FilterMode.Point;
        return startTexture;
    }
}
