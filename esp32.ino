int pins[12] = {23, 22, 21, 19, 18, 17, 16, 4, 25, 26, 27, 14};
bool lastState[12];
unsigned long lastPress[12];
const unsigned long DEBOUNCE = 50;

void setup() {
  Serial.begin(115200);
  for (int i = 0; i < 12; i++) {
    pinMode(pins[i], INPUT_PULLUP);
    lastState[i] = digitalRead(pins[i]);
    lastPress[i] = 0;
  }
}

void loop() {
  if (Serial.available()) {
    String msg = Serial.readStringUntil('\n');
    msg.trim();
    if (msg == "MYSTREAMDECK:HELLO") {
      Serial.println("MYSTREAMDECK:WAZZUP");
    }
  }

  for (int i = 0; i < 12; i++) {
    bool state = digitalRead(pins[i]);
    if (lastState[i] == HIGH && state == LOW) {
      unsigned long now = millis();
      if (now - lastPress[i] > DEBOUNCE) {
        Serial.print("MYSTREAMDECK;BUTTON");
        Serial.print(i + 1);
        Serial.print('\n');
        lastPress[i] = now;
      }
    }
    lastState[i] = state;
  }
}
