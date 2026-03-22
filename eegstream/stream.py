import serial
import time
import numpy as np
from pylsl import StreamInfo, StreamOutlet

# --- CONFIGURATION ---
SERIAL_PORT = 'COM3'  # Update this to your Arduino port
BAUD_RATE = 9600
STREAM_NAME = 'VibeStream'
STREAM_TYPE = 'EEG_Processed'
CHANNELS = 2  # [0] = Alpha Power, [1] = Beta Power

def main():
    # 1. Setup LSL Outlet
    # Arguments: Name, Type, Channel Count, Nominal Srate, Format, Unique ID
    info = StreamInfo(STREAM_NAME, STREAM_TYPE, CHANNELS, 10, 'float32', 'myuidw001')
    outlet = StreamOutlet(info)
    
    print(f"LSL Outlet started: {STREAM_NAME}. Waiting for Arduino...")

    try:
        ser = serial.Serial(SERIAL_PORT, BAUD_RATE, timeout=1)
        time.sleep(2) # Wait for Arduino to reset
        
        while True:
            if ser.in_waiting > 0:
                line = ser.readline().decode('utf-8').strip()
                
                try:
                    # Expecting Arduino to print: "alpha_val,beta_val"
                    values = [float(x) for x in line.split(',')]
                    
                    if len(values) == CHANNELS:
                        # 2. Push to LSL
                        outlet.push_sample(values)
                        print(f"Sent to VR -> Alpha: {values[0]}, Beta: {values[1]}")
                
                except ValueError:
                    # Handle noisy serial data
                    continue
                    
    except KeyboardInterrupt:
        print("\nStopping bridge...")
    finally:
        if 'ser' in locals():
            ser.close()

if __name__ == "__main__":
    main()