using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Windows.WebCam;
using Unity.Sentis;
using UnityEngine.InputSystem;
using UnityEngine.Timeline;

public class yolo_hud : MonoBehaviour
{
    public Text summary;
    //public Text summary_description;
    //public Text allergens;
    // public Text name;
    System.Threading.Timer _timer;
    private IWorker _worker;

    void Start()
    {
        summary.text = "Waiting for User";
        //summary_description.text = "Look at an object please to see a summary of the object";
        //name.text = "Waiting for User";
        //allergens.text = "Waiting for User";

        int seconds_interval = 60;
        _timer = new System.Threading.Timer(Tick, null, 0, seconds_interval * 1000);

        Model model = ModelLoader.Load("Assets/Model/Test_rabbit.onnx");

        _worker = WorkerFactory.CreateWorker(BackendType.GPUCompute, model);
    }

    private void Destroy_worker()
    {
        if (_worker != null)
        {
            _worker.Dispose();
        }
    }
    private void Tick(object state)
    {
        AnalyzeScene();
    }

    void AnalyzeScene()
    {
        // summary_description.text = "Description Pending";
        PhotoCapture.CreateAsync(false, PhotoCaptureCreated);
    }

    PhotoCapture _photoCaptureObject = null;
    void PhotoCaptureCreated(PhotoCapture captureObject)
    {
        _photoCaptureObject = captureObject;
        Resolution cameraResolution = PhotoCapture.SupportedResolutions.OrderByDescending((res) => res.width * res.height).First();

        CameraParameters x = new CameraParameters();
        x.hologramOpacity = 0.0f;
        x.cameraResolutionWidth = cameraResolution.width;
        x.cameraResolutionHeight = cameraResolution.height;
        x.pixelFormat = CapturePixelFormat.BGRA32;
        captureObject.StartPhotoModeAsync(x, photomodestarted);
    }

    private void photomodestarted(PhotoCapture.PhotoCaptureResult result)
    {
        if (result.success)
        {
            string filename = string.Format(@"analysis.jpg");
            string filePath = Path.Combine(Application.persistentDataPath, filename);
            _photoCaptureObject.TakePhotoAsync(filePath, PhotoCaptureFileOutputFormat.JPG, JPGToDisk);
        }
        else
        {
            //summary_description.text = "SummaryXXXXXXXXXXXXXX Unable to start the photo mode";
            summary.text = "Shutting Down";
        }
    }
    void JPGToDisk(PhotoCapture.PhotoCaptureResult result)
    {
        if (result.success)
        {
            string filename = string.Format(@"analysis.jpg");
            string filepath = Path.Combine(Application.persistentDataPath, filename);

            byte[] image = File.ReadAllBytes(filepath);
            AnalyzeImage(image);
        }
        else
        {
            //summary_description.text = "SummaryXXXXXXXXXXXXXX Unable to start the photo mode";
            summary.text = "Shutting Down";
        }
        _photoCaptureObject.StopPhotoModeAsync(OnStoppedPhotoMode);
    }
    void AnalyzeImage(byte[] image)
    {
        //This is where we process the image using the v8 model 

        Texture2D texture = new Texture2D(2, 2);
        texture.LoadImage(image);

        Texture2D resizeTexture = ResizeTexture(texture, 416, 416);

        TensorShape shape = new TensorShape(new TensorShape(4));
        int[] data = new int[] { 1, 3, 416, 416 };
        TensorInt tensor = new TensorInt(shape, data);
        
        TensorShape newShape = new TensorShape(new TensorShape(4));
        
        Tensor reshapedTensor = tensor.ShallowReshape(newShape);
        _worker.Execute(reshapedTensor);

        TensorInt output = _worker.PeekOutput() as TensorInt;

        List<string> objectNames = new List<string>();
        List<float> confidences = new List<float>();
        int outputLength = output.shape.length;
        for (int i = 0; i < outputLength; i += 7)
        {
            float confidence = output[i + 4];
            if (confidence > 0.5f)
            {
                string className = GetNames((int)output[i + 5]);
                objectNames.Add(className);
                confidences.Add(confidence * 100);
            }
        }

        if (objectNames.Count > 0)
        {
            string summaryText = "Detected: ";
            for (int i = 0; i < objectNames.Count; i++)
            {
                summaryText += objectNames[i] + " (" + confidences[i].ToString("0.00") + "%)";
                if (i < objectNames.Count - 1)
                {
                    summaryText += ", ";
                }
            }

            summary.text = summaryText;
        }
        else
        {
            summary.text = "No objects detected";
        }
    }
    string GetNames(int ClassId)
    {
        if (ClassId == 1) return "Bunny";
        return "unknown";
    }
    Texture2D ResizeTexture(Texture2D source, int newWidth, int newHeight)
    {
        RenderTexture rt = RenderTexture.GetTemporary(newWidth, newHeight);
        rt.filterMode = FilterMode.Bilinear;
        RenderTexture.active = rt;
        Graphics.Blit(source, rt);
        Texture2D resizedTexture = new Texture2D(newWidth, newHeight);
        resizedTexture.ReadPixels(new Rect(0, 0, newWidth, newHeight), 0, 0);
        resizedTexture.Apply();
        RenderTexture.active = null;
        RenderTexture.ReleaseTemporary(rt);
        return resizedTexture;
    }
    void OnStoppedPhotoMode(PhotoCapture.PhotoCaptureResult result)
    {
        _photoCaptureObject.Dispose();
        _photoCaptureObject = null;
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            AnalyzeScene();
        }
    }
}