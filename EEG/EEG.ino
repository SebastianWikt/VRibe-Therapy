const int eegPin = A1;   // Analog input pin
const float Vref = 5.0;  // Reference voltage (default)
const int ADC_RES = 1023; // 10-bit ADC (0–1023)

void setup() {
  Serial.begin(115200);

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

  delay(5);  // ~200 Hz sampling
}

//Raw: # Voltage: #