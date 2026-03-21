import sys
import serial
from collections import deque
import pyqtgraph as pg
from pyqtgraph.Qt import QtWidgets, QtCore

# 🔌 Serial config
PORT = 'COM5'
BAUD = 115200

ser = serial.Serial(PORT, BAUD, timeout=1)

# 📊 Data buffer
max_points = 200
data = deque([0]*max_points, maxlen=max_points)

# 🖥️ Qt App
app = QtWidgets.QApplication(sys.argv)
win = pg.GraphicsLayoutWidget(title="Live EEG Signal")
plot = win.addPlot(title="EEG Voltage")
curve = plot.plot(pen='y')

plot.setYRange(0, 5)  # Voltage range
plot.setLabel('left', 'Voltage', 'V')
plot.setLabel('bottom', 'Samples')

win.show()

# 🔁 Update function
def update():
    try:
        while ser.in_waiting:  # Read all available data
            line_raw = ser.readline().decode('utf-8').strip()

            if "Voltage:" in line_raw:
                voltage = float(line_raw.split("Voltage:")[1])
                data.append(voltage)

        curve.setData(list(data))

    except Exception:
        pass

# ⏱️ Timer (update every 20 ms ~50 FPS)
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