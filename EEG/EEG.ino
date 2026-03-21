const int eegPin = A0;   // Analog input pin
const float Vref = 5.0;  // Reference voltage (Uno R4 default)
const int ADC_RES = 4095; // 12-bit ADC (0–4095)

void setup() {
  Serial.begin(115200);

  // Set ADC resolution to 12 bits (important for Uno R4)
  analogReadResolution(12);

  Serial.println("EEG Read Start");
}

void loop() {
  int raw = analogRead(eegPin);

  // Convert to voltage
  float voltage = (raw * Vref) / ADC_RES;

  // Print both raw and voltage
  Serial.print("Raw: ");
  Serial.print(raw);
  Serial.print("  Voltage: ");
  Serial.println(voltage, 6);

  delay(5);  // ~200 Hz sampling (adjust as needed)
}