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
    [Range(1, 100)]
    [SerializeField] private int GenerationCount;
    [SerializeField] private DrawScript ds;
    [SerializeField] private bool isScale;
    private int ImageSize;
    private void Awake()
    {
        UnityEngine.Random.seed = DateTime.Now.GetHashCode();
        ImageSize = ds.ImageSize;
    }
    public void Generate100()
    {
        for(int i = 0; i < GenerationCount; i++)
        {
            Generate();
        }
    }
    public void ScaleImage()
    {
        var scale = UnityEngine.Random.Range(imageSizeScale, 1);
        //scale = imageSizeScale;
        int newSize = (int)(ImageSize * scale);
        //Texture2D newTexture = BicubicInterpolate.ScaleImage(ds.currentTexture, newSize , newSize , ImageSize);
        Texture2D newTexture = SplineScale.SplineScaleTexture(ds.currentTexture, newSize, newSize, ImageSize);
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
            var edges = ProgrammLogic.FindEdgePixels(ds.currentTexture, ds.ImageSize);
            if (m < 0)
                m = Max(m, -edges.lefted);
            else
                m = Min(m, ds.ImageSize - edges.righted - 1);
            if (n < 0)
                n = Max(n, -edges.lower);
            else n = Min(n, ds.ImageSize - edges.upper - 1);
        }
        
        
        int a = UnityEngine.Random.Range(0, maxBias + 1);
        int b = UnityEngine.Random.Range(0, maxBias + 1);
        if (smej > 0)
        {
            a = smej;
            b = smej;
        }
        ClampNandM(ref a, ref b);
        ds.currentTexture = ProgrammLogic.AddBiasToImage(ds.currentTexture, ds.ImageSize , a , b);
        ds.UpdateTexture();
    }
    public void Generate()
    {
        var saveCurrent = ds.currentTexture;
        if (isScale)
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
