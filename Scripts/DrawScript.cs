using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Data;
using UnityEditor.U2D.Path;

public class DrawScript : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerMoveHandler, IPointerEnterHandler, IPointerExitHandler
{
    public static DrawScript instance;
    [SerializeField] private GameObject cursorObject;
    [SerializeField] public Color standartColor;
    [SerializeField] public Color drawedColor;
    [SerializeField] public InputField inputField;
    [SerializeField] NetworkTrainer networkTrainer;
    public Texture2D currentTexture;
    public int ImageSize = 20;
    bool isDrawing = false;
    bool isInField = false;
    private List<Vector2Int> allDrawings = new List<Vector2Int>();
    private List<List<Vector2Int>> allActions = new List<List<Vector2Int>>();
    Texture2D previousTexture = null;
    Vector2Int blockedPixel;
    void Init()
    {
        instance = this;
        UnityEngine.Random.seed = DateTime.Now.GetHashCode();
        currentTexture = createEmptyField();
        previousTexture = currentTexture;
        GetComponent<Image>().sprite = Sprite.Create(currentTexture, new Rect(0, 0, ImageSize, ImageSize), new Vector2(0.5f,0.5f));

        
    }
    Texture2D createEmptyField()
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

    public Texture2D AlignDrawing()
    {
        var texture = currentTexture;
        var edges = ProgrammLogic.FindEdgePixels(texture, ImageSize);
        //currentTexture = ProgrammLogic.AddBiasToImage(currentTexture, ImageSize, ImageSize - 1 - edges.upper, -edges.lefted);
        
        int BiasX = 10 - ((edges.upper + edges.lower) / 2);
        int BiasY = 10 - ((edges.righted + edges.lefted) / 2);
        texture = ProgrammLogic.AddBiasToImage(texture, ImageSize, BiasX, BiasY);

        texture.filterMode = FilterMode.Point;
        Debug.Log($"Center of drawing: ({(edges.upper + edges.lower) / 2} , {(edges.righted + edges.lefted) / 2})\nBias for this Drawing: ({BiasX} , {BiasY})");
        return texture;

    }
    public void Clear()
    {
        allActions.Clear();
        allDrawings.Clear();
        Init();
    }
    private void Awake()
    {
        Init();
    }
    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.Z))
        {
            if (allActions.Count <= 0)
                return;
            var lastAction = allActions[allActions.Count - 1];
            foreach(var pix in lastAction)
            {
                currentTexture.SetPixel(pix.x, pix.y, standartColor);
            }
            allActions.Remove(lastAction);
            currentTexture.Apply();
            GetComponent<Image>().sprite = Sprite.Create(currentTexture, new Rect(0, 0, ImageSize, ImageSize), new Vector2(0.5f, 0.5f));
        }
    }
    public void UpdateTexture()
    {
        currentTexture.Apply();
        GetComponent<Image>().sprite = Sprite.Create(currentTexture, new Rect(0, 0, ImageSize, ImageSize), new Vector2(0.5f, 0.5f));
    }
    public void OnPointerMove(PointerEventData eventData)
    {
        if (!isDrawing)
            return;
        if (!isInField)
            return;
        var cursor = Input.mousePosition;
        float scaledWitdh = ImageSize * transform.localScale.x / 2f;
        float scaledHeight = ImageSize * transform.localScale.y / 2f;

        float offsetX = transform.position.x - scaledWitdh;
        float offsetY = transform.position.y - scaledHeight;

        cursor.x = (cursor.x - offsetX) / transform.localScale.x;
        cursor.y = (cursor.y - offsetY) / transform.localScale.y;

        int pixelX = Mathf.FloorToInt(cursor.x);//+ 0.5f * 32);
        int pixelY = Mathf.FloorToInt(cursor.y);//+ 0.5f * 32);
        //Debug.Log($"({pixelX} , {pixelY})");
        if (blockedPixel.x == pixelX && blockedPixel.y == pixelY)
            return;

        allDrawings.Add(new Vector2Int(pixelX, pixelY));
        blockedPixel = new Vector2Int(pixelX, pixelY);

        DrawColor(pixelX, pixelY);
        currentTexture.Apply();
        GetComponent<Image>().sprite = Sprite.Create(currentTexture, new Rect(0, 0, ImageSize, ImageSize), new Vector2(0.5f, 0.5f));
    }
    private void DrawColor(int x, int y)
    {
        List<Vector2Int> getAvaiblePixels()
        {
            List<Vector2Int> result = new List<Vector2Int>();
            int[] smejX = { -1, 1, 0, 0 };
            int[] smejY = { 0, 0, -1, 1 };
            for(int i = 0; i < 4; i++)
            {
                var newX = x + smejX[i];
                var newY = y + smejY[i];
                if (newX >= 0 && newX < ImageSize && newY >= 0 && newY < ImageSize)
                    result.Add(new Vector2Int(newX, newY));
            }
            return result;
        }
        currentTexture.SetPixel(x, y, drawedColor);
        foreach(var next in getAvaiblePixels())
        {
            var Color = currentTexture.GetPixel(next.x, next.y);
            Color.g += 0.215f;
            Color.r += 0.215f;
            Color.b += 0.215f;
            currentTexture.SetPixel(next.x, next.y, Color);
        }

    }
    public void OnPointerDown(PointerEventData eventData)
    {
        Debug.Log("PRESSED");
        isDrawing = true;
    }
    public void OnPointerUp(PointerEventData eventData)
    {
        Debug.Log("Unpressed");

        List<Vector2Int> vector2Ints= new List<Vector2Int>();
        foreach(var pix in allDrawings)
            vector2Ints.Add(pix);
        allActions.Add(vector2Ints);

        allDrawings.Clear();
        isDrawing = false;

    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        isInField = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isInField = false;
        isDrawing = false;
    }
}
