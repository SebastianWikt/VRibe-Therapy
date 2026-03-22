import serial
import numpy as np
from pylsl import StreamInfo, StreamOutlet
import time
from scipy.fft import rfft, rfftfreq
from collections import deque

# --- SETUP ---
# Update 'COM3' to your actual Arduino port (e.g., '/dev/tty.usbmodem' on Mac)
SERIAL_PORT = 'COM9' 
BAUD_RATE = 115200 # High baud rate for EEG sampling
STREAM_NAME = 'VibeStream'

# --- SETTINGS ---
FS = 100  # Sampling rate (Hz) - matches your Arduino delay(10)
WINDOW_SIZE = 128 # Must be a power of 2 for speed

def get_bands(signal):
    # 1. Apply FFT
    fft_vals = np.abs(rfft(signal))
    freqs = rfftfreq(WINDOW_SIZE, 1/FS)
    
    # 2. Extract Power in specific ranges
    alpha_idx = np.where((freqs >= 8) & (freqs <= 12))[0]
    beta_idx = np.where((freqs >= 13) & (freqs <= 30))[0]
    
    alpha_pwr = np.mean(fft_vals[alpha_idx]) if len(alpha_idx) > 0 else 0
    beta_pwr = np.mean(fft_vals[beta_idx]) if len(beta_idx) > 0 else 0  
      
    return alpha_pwr, beta_pwr

def main():
    data_buffer = deque(maxlen=WINDOW_SIZE)
    
    # 1. Initialize LSL Stream (2 channels: Alpha and Beta)
    info = StreamInfo(STREAM_NAME, 'EEG_Processed', 2, 10, 'float32', 'vribecoder-001')
    outlet = StreamOutlet(info)
    
    print(f"--- LSL Stream '{STREAM_NAME}' is LIVE ---")

    try:
        ser = serial.Serial(SERIAL_PORT, BAUD_RATE, timeout=1)
        print(f"Connected to Arduino on {SERIAL_PORT}")
        
        while True:
            if ser.in_waiting > 500:
                ser.reset_input_buffer()
                continue
            
            if ser.in_waiting > 0:
                line = ser.readline().decode('utf-8', errors='ignore').strip()
                
                try:
                    # If line is "Raw: 512 Voltage: 2.50"
                    # Split by space and take the 2nd element (index 1)
                    parts = line.split()
                    if "Raw:" in parts:
                        raw_value = float(parts[parts.index("Raw:") + 1])
                        
                        data_buffer.append(raw_value)

                        if len(data_buffer) >= WINDOW_SIZE:
                            alpha, beta = get_bands(list(data_buffer))
                            outlet.push_sample([alpha, beta])
                            print(f"Bands Sent -> Alpha: {alpha:.2f} | Beta: {beta:.2f}")
                            
                            # Slide the window (keep half for smoothness)
                            # data_buffer = data_buffer[WINDOW_SIZE//4:]
    
                except (ValueError, IndexError):
                    continue

    except Exception as e:
        print(f"Error: {e}")
    finally:
        if 'ser' in locals(): ser.close()

if __name__ == "__main__":
    main()

# import serial
# import numpy as np
# from pylsl import StreamInfo, StreamOutlet
# import time

# # --- SETUP ---
# # Update 'COM3' to your actual Arduino port (e.g., '/dev/tty.usbmodem' on Mac)
# SERIAL_PORT = 'COM9' 
# BAUD_RATE = 115200 # High baud rate for EEG sampling
# STREAM_NAME = 'VibeStream'

# def main():
#     # 1. Initialize LSL Stream (2 channels: Alpha and Beta)
#     info = StreamInfo(STREAM_NAME, 'EEG_Processed', 2, 10, 'float32', 'vribecoder-001')
#     outlet = StreamOutlet(info)
    
#     print(f"--- LSL Stream '{STREAM_NAME}' is LIVE ---")

#     try:
#         ser = serial.Serial(SERIAL_PORT, BAUD_RATE, timeout=1)
#         print(f"Connected to Arduino on {SERIAL_PORT}")
        
#         while True:
#             if ser.in_waiting > 0:
#                 line = ser.readline().decode('utf-8', errors='ignore').strip()
                
#                 try:
#                     # Split the string "Raw: 864 Voltage: 4.21" into a list
#                     parts = line.split()
#                     if "Raw:" in parts:
#                         # Get the number right after "Raw:"
#                         raw_value = float(parts[parts.index("Raw:") + 1])
                        
#                         # Simple placeholder math
#                         alpha = raw_value * 0.6  
#                         beta = raw_value * 0.4   
                        
#                         outlet.push_sample([alpha, beta])
#                         print(f"Broadcasting -> Alpha: {alpha:.2f} | Beta: {beta:.2f}")
#                 except (ValueError, IndexError):
#                     continue

#     except Exception as e:
#         print(f"Error: {e}")
#     finally:
#         if 'ser' in locals(): ser.close()

# if __name__ == "__main__":
#     main()