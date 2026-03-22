import sys
import json
import cv2
import mediapipe as mp
from mediapipe.tasks import python
from mediapipe.tasks.python import vision
import os

os.environ['TF_CPP_MIN_LOG_LEVEL'] = '3' 
os.environ['ABSL_LOGGING_LEVEL'] = 'error'

# 1. Path Setup
# This ensures it finds the model file regardless of where you start the script
model_path = os.path.join(os.path.dirname(__file__), 'face_detector.tflite')

# 2. Initialize the Task Detector (Forcing CPU)
base_options = python.BaseOptions(
    model_asset_path=model_path,
    delegate=python.BaseOptions.Delegate.CPU # Important for WSL stability
)
options = vision.FaceDetectorOptions(base_options=base_options)
detector = vision.FaceDetector.create_from_options(options)

cap = cv2.VideoCapture(0, cv2.CAP_V4L2)
cap.set(cv2.CAP_PROP_FOURCC, cv2.VideoWriter_fourcc('M', 'J', 'P', 'G'))
cap.set(cv2.CAP_PROP_FRAME_WIDTH, 640)
cap.set(cv2.CAP_PROP_FRAME_HEIGHT, 480)
cap.set(cv2.CAP_PROP_FPS, 30)

def get_vision_data():
    # If camera didn't open at all
    if not cap.isOpened():
        return {"mood": "Hardware Error", "confidence": 0}

    success, image = cap.read()
    if not success:
        return {"mood": "Timeout/Retry", "confidence": 0}

    image_rgb = cv2.cvtColor(image, cv2.COLOR_BGR2RGB)
    mp_image = mp.Image(image_format=mp.ImageFormat.SRGB, data=image_rgb)
    detection_result = detector.detect(mp_image)

    if detection_result.detections:
        score = detection_result.detections[0].categories[0].score
        return {"mood": "Focused", "confidence": round(float(score), 2)}
    return {"mood": "Not Focused", "confidence": 0.0}

for line in sys.stdin:
    try:
        request = json.loads(line)
        if request.get("command") == "get_data":
            print(json.dumps(get_vision_data()))
            sys.stdout.flush()
    except:
        pass

cap.release()