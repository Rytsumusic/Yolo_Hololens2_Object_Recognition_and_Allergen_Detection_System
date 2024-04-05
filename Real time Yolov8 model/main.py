import cv2
from ultralytics import YOLO
import math
import tkinter as tk
from tkinter import simpledialog

cap = cv2.VideoCapture(0)
cap.set(3, 640)
cap.set(4, 480)

allergies =[]


model = YOLO("yolo-weights/yolov8n.pt")

objectclasses = ["person", "bicycle", "car", "motorbike", "aeroplane", "bus", "train", "truck", "boat",
              "traffic light", "fire hydrant", "stop sign", "parking meter", "bench", "bird", "cat",
              "dog", "horse", "sheep", "cow", "elephant", "bear", "zebra", "giraffe", "backpack", "umbrella",
              "handbag", "tie", "suitcase", "frisbee", "skis", "snowboard", "sports ball", "kite", "baseball bat",
              "baseball glove", "skateboard", "surfboard", "tennis racket", "bottle", "wine glass", "cup",
              "fork", "knife", "spoon", "bowl", "banana", "apple", "sandwich", "orange", "broccoli",
              "carrot", "hot dog", "pizza", "donut", "cake", "chair", "sofa", "pottedplant", "bed",
              "diningtable", "toilet", "tvmonitor", "laptop", "mouse", "remote", "keyboard", "cell phone",
              "microwave", "oven", "toaster", "sink", "refrigerator", "book", "clock", "vase", "scissors",
              "teddy bear", "hair drier", "toothbrush"
              ]


root = tk.Tk()
root.withdraw()
allergy = simpledialog.askstring("Enter an allergy", "Enter an allergy:")
allergies.append(allergy)
severity = simpledialog.askstring("Enter severity", "Enter severity (mild, middle, severe):")
if severity == "mild":
    severity_level = 1
elif severity == "middle":
    severity_level = 2
elif severity == "severe":
    severity_level = 3
else:
    severity_level = 0

allergy_severity = {allergy: severity_level}

while True:
    sucess,img = cap.read()
    results =model(img,stream=True)
    
        
    for r in results:
        boxes = r.boxes
        
        for box in boxes:
            x1, y1, x2, y2 = box.xyxy[0]
            x1, y1, x2, y2 = int(x1), int(y1), int(x2), int(y2) # convert to int values
            
            if objectclasses[int(box.cls[0])] == allergy:
                cv2.putText(img, "Allergy Detected! Severity: {}".format(allergy_severity[allergy]), (x1, y1 - 10), cv2.FONT_HERSHEY_SIMPLEX, 0.9, (0, 0, 255), 2)
                cv2.rectangle(img, (x1, y1), (x2, y2), (255, 0, 255), 3)
                confidence = math.ceil((box.conf[0]*100))/100
                print("Confidence --->",confidence)
                cls = int(box.cls[0])
                print("Class name -->", objectclasses[cls])
                org = [x1, y1]
                font = cv2.FONT_HERSHEY_SIMPLEX
                fontScale = 1
                color = (255, 0, 0)
                thickness = 2
                cv2.putText(img, objectclasses[cls], org, font, fontScale, color, thickness)
                
                # Break out of the loop to prevent other boxes from being drawn
                break
            else:
                cv2.rectangle(img, (x1, y1), (x2, y2), (0, 255, 0), 3)
                # confidence
                confidence = math.ceil((box.conf[0]*100))/100
                print("Confidence --->",confidence)

                # class name
                cls = int(box.cls[0])
                print("Class name -->", objectclasses[cls])

                # object details
                org = [x1, y1]
                font = cv2.FONT_HERSHEY_SIMPLEX
                fontScale = 1
                color = (255, 0, 0)
                thickness = 2

                cv2.putText(img, objectclasses[cls], org, font, fontScale, color, thickness)

    cv2.imshow('Webcam', img)
    if cv2.waitKey(1) == ord('q'):
        break
cap.release()
cv2.destroyAllWindows()
