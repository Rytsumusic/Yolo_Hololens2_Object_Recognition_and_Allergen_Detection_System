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
using System.Runtime.CompilerServices;

public class yolo_hud : MonoBehaviour
{
    public Text summary;
    //public Text summary_description;
    //public Text allergens;
    // public Text name;
    System.Threading.Timer _timer;

    private IWorker _worker;
    [SerializeField] ModelAsset modelAsset;
    private float[] results;
    private Model runtimeModel;
    private TensorFloat tensor;
    private bool isPhotomodeActive = false;


    void Start()
    {
        summary.text = "Waiting for User";
        //summary_description.text = "Look at an object please to see a summary of the object";
        //name.text = "Waiting for User";
        //allergens.text = "Waiting for User";

        int seconds_interval = 60;
        _timer = new System.Threading.Timer(Tick, null, 0, seconds_interval * 1000);

        //runtimeModel = ModelLoader.Load(modelAsset);
        //_worker = WorkerFactory.CreateWorker(BackendType.GPUCompute, runtimeModel);
        AnalyzeScene();

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
        if (!isPhotomodeActive)
        {
            isPhotomodeActive = true;
            summary.text = "Analyzing Scene";
            PhotoCapture.CreateAsync(false, PhotoCaptureCreated);
        }

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
            summary.text = "Photomode Started";
        }
        else
        {
            summary.text = "SummaryXXXXXXXXXXXXXX Unable to start the photo mode";
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
            //AnalyzeImage(image);
            summary.text = "Image Captured sucessfully";
            Debug.Log("Image Captured sucessfully");
        }
        else
        {
            summary.text = "SummaryXXXXXXXXXXXXXX Unable to start the jpg to disk mode";
            summary.text = "Shutting Down";
        }
        if (_photoCaptureObject != null)
        {
            _photoCaptureObject.StopPhotoModeAsync(OnStoppedPhotoMode);
        }
    }

    void AnalyzeImage(byte[] image)
    {

        Texture2D texture = new Texture2D(2, 2);
        texture.LoadImage(image);

        Texture2D resizeTexture = ResizeTexture(texture, 640, 640);
        TensorFloat tensor = TextureConverter.ToTensor(resizeTexture);
        _worker.Execute(tensor);

        TensorFloat output = _worker.PeekOutput() as TensorFloat;
        output.MakeReadable();
        results = output.ToReadOnlyArray();
        List<string> objectNames = new List<string>();
        List<float> confidences = new List<float>();
        ;
        for (int i = 0; i < results.Length; i += 7)
        {
            float confidence = results[i + 4];
            if (confidence > 0.5f)
            {
                string className = GetNames((int)results[i + 5]);
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

            tensor.Dispose();
        }
        string GetNames(int ClassId)
        {
            if (ClassId == 0) return "Person";
            if (ClassId == 1) return "Bicycle";
            if (ClassId == 2) return "Car";
            if (ClassId == 3) return "Motorbike";
            if (ClassId == 4) return "Aeroplane";
            if (ClassId == 5) return "Bus";
            if (ClassId == 6) return "Train";
            if (ClassId == 7) return "Truck";
            if (ClassId == 8) return "Boat";
            if (ClassId == 9) return "Traffic Light";
            if (ClassId == 10) return "Fire Hydrant";
            if (ClassId == 11) return "Stop Sign";
            if (ClassId == 12) return "Parking Meter";
            if (ClassId == 13) return "Bench";
            if (ClassId == 14) return "Bird";
            if (ClassId == 15) return "Cat";
            if (ClassId == 16) return "Dog";
            if (ClassId == 17) return "Horse";
            if (ClassId == 18) return "Sheep";
            if (ClassId == 19) return "Cow";
            if (ClassId == 20) return "Elephant";
            if (ClassId == 21) return "Bear";
            if (ClassId == 22) return "Zebra";
            if (ClassId == 23) return "Giraffe";
            if (ClassId == 24) return "Backpack";
            if (ClassId == 25) return "Umbrella";
            if (ClassId == 26) return "Handbag";
            if (ClassId == 27) return "Tie";
            if (ClassId == 28) return "Suitcase";
            if (ClassId == 29) return "Frisbee";
            if (ClassId == 30) return "Skis";
            if (ClassId == 31) return "Snowboard";
            if (ClassId == 32) return "Sports Ball";
            if (ClassId == 33) return "Kite";
            if (ClassId == 34) return "Baseball Bat";
            if (ClassId == 35) return "Baseball Glove";
            if (ClassId == 36) return "Skateboard";
            if (ClassId == 37) return "Surfboard";
            if (ClassId == 38) return "Tennis Racket";
            if (ClassId == 39) return "Bottle";
            if (ClassId == 40) return "Wine Glass";
            if (ClassId == 41) return "Cup";
            if (ClassId == 42) return "Fork";
            if (ClassId == 43) return "Knife";
            if (ClassId == 44) return "Spoon";
            if (ClassId == 45) return "Bowl";
            if (ClassId == 46) return "Banana";
            if (ClassId == 47) return "Apple";
            if (ClassId == 48) return "Sandwich";
            if (ClassId == 49) return "Orange";
            if (ClassId == 50) return "Broccoli";
            if (ClassId == 51) return "Carrot";
            if (ClassId == 52) return "Hot Dog";
            if (ClassId == 53) return "Pizza";
            if (ClassId == 54) return "Donut";
            if (ClassId == 55) return "Cake";
            if (ClassId == 56) return "Chair";
            if (ClassId == 57) return "Sofa";
            if (ClassId == 58) return "Pottedplant";
            if (ClassId == 59) return "Bed";
            if (ClassId == 60) return "Diningtable";
            if (ClassId == 61) return "Toilet";
            if (ClassId == 62) return "TV Monitor";
            if (ClassId == 63) return "Laptop";
            if (ClassId == 64) return "Mouse";
            if (ClassId == 65) return "Remote";
            if (ClassId == 66) return "Keyboard";
            if (ClassId == 67) return "Cell Phone";
            if (ClassId == 68) return "Microwave";
            if (ClassId == 69) return "Oven";
            if (ClassId == 70) return "Toaster";
            if (ClassId == 71) return "Sink";
            if (ClassId == 72) return "Refrigerator";
            if (ClassId == 73) return "Book";
            if (ClassId == 74) return "Clock";
            if (ClassId == 75) return "Vase";
            if (ClassId == 76) return "Scissors";
            if (ClassId == 77) return "Teddy Bear";
            if (ClassId == 78) return "Hair Drier";
            if (ClassId == 79) return "Toothbrush";
            return "unknown";
        }

        //resizes the texture to the desired width and height
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


    
    }
    void OnStoppedPhotoMode(PhotoCapture.PhotoCaptureResult result)  
        {
            _photoCaptureObject.Dispose();
            _photoCaptureObject = null;
            isPhotomodeActive = false;
        }
    private void Update()
    {
        AnalyzeScene();
    }
}