using System;
using UnityEngine;
using UnityEngine.UI;
using System.IO;

public class ProcessImage : MonoBehaviour
{
    [SerializeField] private float maxAngle = 6f;
    [SerializeField] private int maxBias = 4;
    [Range(0.0f, 0.35f)]
    [SerializeField] private float noiseScale = 0.2f;
    [Range(0.0f, 1f)]
    [SerializeField] private float imageSizeScale = 0.5f;
    [SerializeField] private DrawScript ds;
    private int ImageSize;
    private void Awake()
    {
        UnityEngine.Random.seed = DateTime.Now.GetHashCode();
        ImageSize = ds.ImageSize;
    }
    public void Generate100()
    {
        for(int i = 0; i < 100; i++)
        {
            Generate();
        }
    }
    public void ScaleImage()
    {
        var scale = UnityEngine.Random.Range(imageSizeScale, 1);
        //scale = imageSizeScale;
        int newSize = (int)(ImageSize * scale);
        Texture2D newTexture = BicubicInterpolate.ScaleImage(ds.currentTexture, newSize , newSize , ImageSize);
        ds.currentTexture = newTexture;
        int smej = (ImageSize - (int)(ImageSize * scale));
        AddBias(smej / 2);
        ds.UpdateTexture();
    }
    public void RotateDrawing()
    {
        float theta = UnityEngine.Random.Range(-maxAngle, maxAngle);
        var rotatedTexutre = BilinearRotation.RotateTexture(ds.currentTexture, theta);
        rotatedTexutre.filterMode = FilterMode.Point;
        rotatedTexutre.Apply();
        ds.currentTexture = rotatedTexutre;
        ds.UpdateTexture();
    }
    Texture2D createEmptyField()
    {
        Texture2D startTexture = new Texture2D(ds.ImageSize, ImageSize);
        for (int i = 0; i < ImageSize; i++)
        {
            for (int j = 0; j < ImageSize; j++)
            {
                startTexture.SetPixel(i, j, ds.standartColor);
            }
        }
        startTexture.Apply();
        startTexture.filterMode = FilterMode.Point;
        return startTexture;
    }
    public void AddParticles()
    {
        float whiteNoiseFunction(float minimalValue, float maximalValue)
        {
            return UnityEngine.Random.Range(minimalValue, maximalValue);
        }

        for (int i = 0; i < ImageSize; i++)
        {
            for (int j = 0; j < ImageSize; j++)
            {
                var param = whiteNoiseFunction(0, 1);
                var Color = ds.currentTexture.GetPixel(i, j);
                Color.g += param * noiseScale;
                Color.b += param * noiseScale;
                Color.r += param * noiseScale;
                ds.currentTexture.SetPixel(i, j, Color);
            }
        }
        ds.UpdateTexture();
    }
    public void AddBias(int smej)
    {
        void ClampNandM(ref int n, ref int m)
        {
            int lefted = 20, righted = 0, upper = 0, lower = 20;
            for (int i = 0; i < ds.ImageSize; i++)
            {
                for (int j = 0; j < ds.ImageSize; j++)
                {
                    if (ds.currentTexture.GetPixel(i, j).r + 0.05 >= 0.215)
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
            //Debug.Log($"L{lefted}, R{righted} , U{upper} , LOWER{lower}");
            if (m < 0)
                m = Max(m, -lefted);
            else
                m = Min(m, ds.ImageSize - righted - 1);
            if (n < 0)
                n = Max(n, -lower);
            else n = Min(n, ds.ImageSize - upper - 1);
        }
        Texture2D biasedTexture = createEmptyField();
        
        int a = UnityEngine.Random.Range(-maxBias, maxBias);
        int b = UnityEngine.Random.Range(-maxBias, maxBias);
        if (smej > 0)
        {
            a = smej;
            b = smej;
        }
        ClampNandM(ref a, ref b);


        //processNandM(ref n, ref m);
        for (int i = 0; i < ds.ImageSize; i++)
        {
            for (int j = 0; j < ds.ImageSize; j++)
            {
                int ShiftedX = (i + b + ds.ImageSize) % ds.ImageSize;
                int ShiftedY = (j + a + ds.ImageSize) % ds.ImageSize;
                biasedTexture.SetPixel(ShiftedX, ShiftedY, ds.currentTexture.GetPixel(i, j));
            }
        }
        biasedTexture.Apply();
        ds.currentTexture = biasedTexture;
        ds.UpdateTexture();
    }
    public void Generate()
    {
        var saveCurrent = ds.currentTexture;
        ScaleImage();
        RotateDrawing();
        AddBias(0);
        AddParticles();
        ds.UpdateTexture();
        Save();
        ds.currentTexture = saveCurrent;
    }
    public void Save()
    {
        var savingTexture = ds.currentTexture;
        byte[] savingTextureBytes = savingTexture.EncodeToPNG();
        Debug.Log($"{Application.dataPath}/Dataset/{System.Convert.ToInt16(ds.inputField.text)}_{savingTextureBytes.GetHashCode()}.png");
        File.WriteAllBytes($"{Application.dataPath}/Dataset/{System.Convert.ToInt16(ds.inputField.text)}_{savingTextureBytes.GetHashCode()}.png", savingTextureBytes);
    }
    int Max(int a, int b)
    {
        if (a > b)
            return a;
        else return b;
    }
    int Min(int a, int b)
    {
        if (a < b)
            return a;
        else return b;
    }

}
