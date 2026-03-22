🧠 VRibe Therapy
Neuro-Responsive VR for Focus & Flow

📌 Inspiration

VRibe Therapy began with a simple observation: our development environments are static, but our minds are dynamic. Inspired by the struggle of maintaining "flow state" during intense coding sessions and the challenges faced by developers with ADHD, we wanted to build a bridge between the brain and software so that programmers like us could thrive without getting burnt out.

🚀 What it does

VRibe Therapy is a neuro-responsive application built for the Meta Quest 3S. Using real-time EEG data from an Arduino, the app monitors the user’s cognitive state in alpha and beta waves.

Focus Mode: When the user is calm and focused, the environment is vibrant, clear, and full of life (birds chirping, steady water).

Stress Mode: When the user becomes frustrated or distracted, the environment shifts, with rain and darkening skies representing a need to destress. It transforms the abstract concept of "mindfulness" into a tangible, visual feedback loop.

🛠️ How we built it

We utilized a modular architecture to integrate hardware and human feelings to create a virtual environment:

Hardware: An Arduino Uno with EEG electrodes captures raw microvolt signals. We built this EEG device from scratch to go with our project.
The Bridge: A Python middleman server that uses Short-Term Fourier Transforms (STFT) to move data from the brain EEG domain to the frequency Unity domain. We use the Alpha and Beta waves outputted by the EEG to compute a Calm Score, detailing how relaxed the user is.
VR/MR Engine: Developed in Unity 6 using the Meta XR All-in-One SDK. We leveraged Meta VR Building Blocks for rapid passthrough integration and State-Driven Design to manage environmental transitions.

⚠️ Challenges we ran into

Signal-to-Noise Ratio: Raw EEG data is incredibly noisy; so much so that it became difficult to distinguish emotions in the data we streamed. We had to implement a Rolling Average Filter in Python to prevent the VR environment from flickering during natural movements. Additionally, the EEG inputs we used were not state-of-the-art and the outputs were noisy as a result.

Version Control: Joint development on Unity scenes is not easy, as even minute changes to a common object across branches led to massive merge conflicts down the line. This taught us that Git might not always be the best choice for version control.

🏆 Accomplishments that we're proud of
Custom EEG: We created an EEG with just electrodes, an Arduino and some cables, sauntering the cables together to create a working device that streams data.
Wireless Neuro-Streaming: Successfully streaming processed EEG brainwave data to a standalone mobile headset.
Neuro-Adaptive Environment: The environment in the VR adapts to how the user is feeling and changes the surroundings based on their emotions.
Functional MVP: Moving from raw hardware components to a working prototype in just 24 hours.
📚 What we learned

We discovered that Neurofeedback is a game-changer for spatial computing. We learned the intricacies of digital signal processing (DSP) and how to map biological markers to visual parameters. Most importantly, we learned that tools that help developers don't always have to do with code, but can also have to do with emotions and stress.

🔮 What's next for VRibe Therapy
LLM Integration: Using the EEG "Calm Score" to automatically prompt an LLM to simplify complex code blocks when the user's stress levels spike.
ADHD Training Modules: Developing specific "Gamified Focus" levels designed to help neurodivergent developers strengthen their concentration through biofeedback.
Hardware Miniaturization: Moving from a bulky Arduino setup to a sleek, 3D-printed headband integrated directly into the Quest 3S strap.

🧩 Project Structure (Suggested)
VRibe-Therapy/
│
├── unity/                 # Unity 6 VR project
├── python-bridge/         # EEG processing + STFT server
├── arduino/               # EEG signal acquisition code
├── assets/                # Media, demos, screenshots
├── docs/                  # Architecture diagrams
└── README.md



