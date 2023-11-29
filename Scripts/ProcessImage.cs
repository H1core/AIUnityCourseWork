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
    public void ScaleImage()
    {
        var scale = UnityEngine.Random.Range(imageSizeScale, 1);
        int newWidth = (int)(ImageSize * scale);
        int newHeigth = (int)(ImageSize * scale);
        Texture2D newTexture = createEmptyField();
        for(int y = 0; y < newHeigth; y++)
        {
            for(int x =0; x < newWidth; x++)
            {
                int x1 = (int)Mathf.Floor(x / scale);
                int x2 = Min(x1 + 1, ImageSize - 1);
                int y1 = (int)Mathf.Floor(y / scale);
                int y2 = Math.Min(y1 + 1 , ImageSize - 1);

                float topLeft = ds.currentTexture.GetPixel(x1, y1).grayscale;
                float topRight = ds.currentTexture.GetPixel(x2, y1).grayscale;
                float downLeft = ds.currentTexture.GetPixel(x1, y2).grayscale;
                float downRight = ds.currentTexture.GetPixel(x2, y2).grayscale;

                float fractionX = (x / scale) - x1;
                float fractionY = (y / scale) - y1;

                /*                float gray = (topLeft * (1 - fractionX) * (1 - fractionY) +
                                    topRight * fractionX * (1 - fractionY) +
                                    downLeft * (1 - fractionX) * fractionY +
                                    downRight * fractionX * fractionY
                                    );
                */
                float gray = Mathf.Min(Mathf.Max(topLeft, downRight) + Mathf.Max(topRight, downLeft));
                Debug.Log(gray);
                newTexture.SetPixel(x, y, new Color(gray, gray, gray));
            }
        }
        newTexture.Apply();
        ds.currentTexture = newTexture;
        ds.UpdateTexture();
    }
    public void RotateDrawing()
    {
        float theta = UnityEngine.Random.RandomRange(-maxAngle * Mathf.PI / 180f, maxAngle * Mathf.PI / 180f);

        Texture2D rotatedTexutre = createEmptyField();
        int rows = ImageSize;
        int cols = ImageSize;
        float cosTheta = Mathf.Cos(theta);
        float sinTheta = Mathf.Sin(theta);

        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < cols; j++)
            {
                // Вычисление новых координат с учетом поворота
                float x = j - cols / 2.0f;  // Смещение к началу координат по столбцам
                float y = i - rows / 2.0f;  // Смещение к началу координат по строкам

                // Поворот вокруг начала координат
                float newX = x * cosTheta - y * sinTheta;
                float newY = x * sinTheta + y * cosTheta;

                // Возвращение координат в исходную систему координат и смещение обратно
                int rotatedX = Mathf.RoundToInt(newX + cols / 2.0f);
                int rotatedY = Mathf.RoundToInt(newY + rows / 2.0f);

                // Проверка, чтобы избежать выхода за границы массива
                if (rotatedX >= 0 && rotatedX < cols && rotatedY >= 0 && rotatedY < rows)
                {
                    rotatedTexutre.SetPixel(i, j, ds.currentTexture.GetPixel(rotatedY, rotatedX));
                }
            }
        }

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
    public void AddBias()
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
        int n = UnityEngine.Random.Range(-maxBias, maxBias);
        int m = UnityEngine.Random.Range(-maxBias, maxBias);
        ClampNandM(ref n, ref m);


        //processNandM(ref n, ref m);
        for (int i = 0; i < ds.ImageSize; i++)
        {
            for (int j = 0; j < ds.ImageSize; j++)
            {
                int ShiftedX = (i + m + ds.ImageSize) % ds.ImageSize;
                int ShiftedY = (j + n + ds.ImageSize) % ds.ImageSize;
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
        RotateDrawing();
        AddBias();
        AddParticles();
        ds.currentTexture = saveCurrent;
    }
    public void Save()
    {
        var savingTexture = GetComponent<Image>().sprite.texture;
        byte[] savingTextureBytes = savingTexture.EncodeToPNG();
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
