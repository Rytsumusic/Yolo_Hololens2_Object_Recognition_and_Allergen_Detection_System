using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Sentis;
using UnityEngine.UI;
using Lays = Unity.Sentis.Layers;
using Unity.VisualScripting;

public class Object_recognition_text : MonoBehaviour
{
    private WebCamTexture camTexture;   
    public Text summary;
    public Text allergen;
    [SerializeField]ModelAsset modelAsset;
    Model runtimeModel;
    IWorker worker;
    float[] modelresults;
    private const int imageWidth = 640;
    private const int imageHeight = 640;
    private Input_Field_Allergen inputFieldAllergen;
    // Start is called before the first frame update
    void Start()
    {
        Vector2Int Camerasize = new Vector2Int(896,540);
        int cameraFPS = 30;

        camTexture = new WebCamTexture (Camerasize.x, Camerasize.y, cameraFPS);

        camTexture.Play();

        runtimeModel = ModelLoader.Load(modelAsset);
        worker = WorkerFactory.CreateWorker(BackendType.GPUCompute,runtimeModel,verbose:true);
        if (worker == null)
        {
            Debug.LogError("Failed to create worker.");
            return;
        }
        inputFieldAllergen = GetComponent<Input_Field_Allergen>();

    }

    // Update is called once per frame
    void Update()
    {             
       analyzeimage();
    }
    public void analyzeimage() 
    {
        if(camTexture.isPlaying&& camTexture.didUpdateThisFrame)
        {
            Texture2D frame = new Texture2D(camTexture.width, camTexture.height);
            frame.SetPixels32(camTexture.GetPixels32());
            frame.Apply();
            Texture2D resized = ResizeTexture(frame, imageWidth, imageHeight);
            resized.Apply();
            summary.text = "Object recognition in progress";
            allergen.text = "Allergen detection in progress";

            if (worker == null)
            {
                Debug.LogError("Worker is null.");
                return;
            }
            TensorFloat output = TextureConverter.ToTensor(resized);
            worker.Execute(output);
            TensorFloat result = worker.PeekOutput() as TensorFloat;
            result.MakeReadable();
            modelresults = result.ToReadOnlyArray();
            List<string> allergens = new List<string>();
            //Debug.Log("Model results: " + modelresults[0]);
            List<string> objectNames = new List<string>();
            List<float> confidences = new List<float>();

            for(int i = 0; i < modelresults.Length; i += 7)
            {
                float confidence = modelresults[i + 2];
                if (confidence < 0.5)
                {
                    continue;
                }
                int classId = (int)modelresults[i + 1];
                float x = modelresults[i + 3];
                float y = modelresults[i + 4];
                float width = modelresults[i + 5];
                float height = modelresults[i + 6];
                float xMin = x - width / 2;
                float yMin = y - height / 2;
                float xMax = x + width / 2;
                float yMax = y + height / 2;
                objectNames.Add(GetNames(classId));
                confidences.Add(confidence);
                allergens.Add(GetNames(classId));
            }
            foreach (string allergenName in inputFieldAllergen.getAllergens()) 
            {
                if (allergens.Contains(allergenName))
                {
                    allergen.text = "Allergen detected: " + allergenName;
                    break;
                }
                else 
                {
                    allergen.text = "No allergens detected";
                }
            }
            if(objectNames.Count > 0)
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

            // Clean up
            if (output!= null)
            {
                output.Dispose();
            }
            if (result != null)
            {
                result.Dispose();
            }       
        }
    }
    Texture2D ResizeTexture(Texture2D source, int newWidth, int newHeight)
    {
        RenderTexture rt = RenderTexture.GetTemporary(newWidth, newHeight);
        rt.filterMode = FilterMode.Bilinear;
        RenderTexture.active = rt;
        Graphics.Blit(source, rt);

        Texture2D resizedTexture = new Texture2D(newWidth, newHeight,TextureFormat.RGB24,false);
        resizedTexture.ReadPixels(new Rect(0, 0, newWidth, newHeight), 0, 0);
        resizedTexture.Apply();

        RenderTexture.active = null;
        RenderTexture.ReleaseTemporary(rt);

        return resizedTexture;
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


    private void OnDestroy()
    {
        if(camTexture != null)
        {
            camTexture.Stop();
            camTexture = null;
        }
        if(worker != null)
        {
            worker.Dispose();
            worker = null;
        }
    }
}


