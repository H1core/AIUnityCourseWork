using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
public class NetworkDecision : MonoBehaviour
{
    //static NetworkDecision instance = null;
    [Header("OUTPUT PARAMETERS")]
    [SerializeField] private Text[] outputFields;
    [SerializeField] private Text finalDecision;

    [SerializeField] private NetworkTrainer networkTrainer;
    [SerializeField] private DrawScript drawScript;

    private int ImageSize;
    private void Awake()
    {
        //if (instance == null)
        //    instance = this;
        //else Destroy(gameObject);
        //DontDestroyOnLoad(gameObject);
        ImageSize = drawScript.ImageSize;
    }
    private void Update()
    {
        ClassifyInput();
        //drawScript.AlignDrawing();
    }
    public class OutputData : IComparable<OutputData>
    {
        public int value;
        public double percent;
        public OutputData(int value, double percent)
        {
            this.value = value;
            this.percent = percent;
        }

        public int CompareTo(OutputData other)
        {
            return other.percent.CompareTo(percent);
        }
    }
    public void ClassifyInput()
    {
        double[] pixelsInputs = new double[ImageSize * ImageSize];
        var texture = drawScript.AlignDrawing();
        for (int i = 0; i < ImageSize; i++)
        {
            for (int j = 0; j < ImageSize; j++)
            {
                pixelsInputs[i * ImageSize + j] = texture.GetPixel(i, j).r;
            }
        }
        var res = networkTrainer.neuralNetwork.Classify(pixelsInputs);
        finalDecision.text = $"I think it's: {res.predictedClass}";

        List<OutputData> outputData = new List<OutputData>();
        for(int i = 0; i < res.outputs.Length; i++)
        {
            outputData.Add(new OutputData(i, res.outputs[i]));
        }

        outputData.Sort();
        
        for(int i = 0; i < outputData.Count; i++)
        {
            outputFields[i].text = $"Number {outputData[i].value}: {Mathf.FloorToInt((float)outputData[i].percent * 1000)/10f}%";
        }
        /*StringBuilder data = new StringBuilder();
        Texture2D texture = GetComponent<Image>().sprite.texture;
        for (int i = 0; i < ImageSize; i++)
        {
            for (int j = 0; j < ImageSize; j++)
            {
                data.Append(texture.GetPixel(i, j) == drawedColor ? 1 : 0);
                data.Append(' ');
            }
        }
        data.Append('\n');
        for (int i = 0; i < 10; i++)
        {
            if (i == 0)
            {
                data.Append(1);
            }
            else
            {
                data.Append(0);
            }
            data.Append(' ');
        }
        DataPoint currentPoint = new DataPoint(data.ToString());*/
        //currentPoint.inputs);
    }
}
