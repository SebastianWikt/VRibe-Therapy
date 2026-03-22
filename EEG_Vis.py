import sys
import serial
from collections import deque
import numpy as np
import pyqtgraph as pg
from pyqtgraph.Qt import QtWidgets, QtCore

# 🔌 Serial config
PORT = 'COM6'
BAUD = 115200

ser = serial.Serial(PORT, BAUD, timeout=1)

# 📊 Parameters
max_points = 500          # waveform buffer
fs = 200                 # sampling rate (Hz) → match Arduino (~delay(5))
window_size = 128        # STFT window
overlap = 64

data = deque([0]*max_points, maxlen=max_points)

# Spectrogram buffer (time x frequency)
spec_history = 100
spec = np.zeros((window_size//2 + 1, spec_history))

# 🖥️ Qt App
app = QtWidgets.QApplication(sys.argv)
win = pg.GraphicsLayoutWidget(title="Live EEG Signal")

# --- Time domain plot ---
plot_time = win.addPlot(title="EEG Voltage")
curve = plot_time.plot(pen='y')
plot_time.setYRange(0, 5)
plot_time.setLabel('left', 'Voltage', 'V')
plot_time.setLabel('bottom', 'Samples')

# --- Spectrogram plot ---
win.nextRow()
plot_spec = win.addPlot(title="Spectrogram (STFT)")
img = pg.ImageItem()
plot_spec.addItem(img)

plot_spec.setLabel('left', 'Frequency', 'Hz')
plot_spec.setLabel('bottom', 'Time')

# Color map (nice visualization)
colormap = pg.colormap.get('viridis')
img.setLookupTable(colormap.getLookupTable())

win.show()

# 🔁 Update function
def update():
    global spec

    try:
        # Read serial data
        while ser.in_waiting:
            line_raw = ser.readline().decode('utf-8').strip()

            if "Voltage:" in line_raw:
                voltage = float(line_raw.split("Voltage:")[1])
                data.append(voltage)

        # Update waveform
        curve.setData(list(data))

        # --- STFT ---
        if len(data) >= window_size:
            signal = np.array(data)

            # Take latest window
            window = signal[-window_size:]

            # Apply window function
            windowed = window * np.hanning(window_size)

            # FFT
            fft = np.fft.rfft(windowed)
            power = np.abs(fft)**2
            power_db = 10 * np.log10(np.abs(np.fft.rfft(windowed))**2 + 1e-12)

            # Shift spectrogram (scrolling effect)
            spec = np.roll(spec, -1, axis=1)
            spec[:, -1] = power_db

            # Update image  ← was: img.setImage(spec, autoLevels=True)
            img.setImage(spec.T, autoLevels=True)

    except Exception:
        pass

# ⏱️ Timer
timer = QtCore.QTimer()
timer.timeout.connect(update)
timer.start(20)

# ▶️ Run app
try:
    sys.exit(app.exec())
except KeyboardInterrupt:
    print("Stopping...")
finally:
    ser.close()