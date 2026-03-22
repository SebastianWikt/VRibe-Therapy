Breathing Exercise Scene Setup

This file explains how to create and run the `BreathingExercise` scene in the Unity Editor using the script provided in this repo.

Steps to set up the scene (in Unity Editor):

1. Open Unity and your project.
2. In the Project window, go to `Assets/Scenes`.
3. Create a new Scene: `File -> New Scene` and save it as `BreathingExercise.unity` inside `Assets/Scenes`.
4. Add an empty GameObject to the scene: `GameObject -> Create Empty` and name it `BreathingController`.
5. In the Project window, locate the script: `Assets/Scripts/BreathingController.cs` and drag it onto the `BreathingController` GameObject (or use Add Component).
6. Optionally adjust `inhaleDuration`, `holdDuration`, and `exhaleDuration` on the `BreathingController` component in the Inspector.
7. Press Play to test the breathing exercise. The script will create a simple UI (circle + phase text) at runtime.

Notes:
- The script uses built-in `Arial` font and the built-in UI sprite when available.
- If you want a circular graphic, replace the `BreathingCircle` Image sprite with a round sprite in the Inspector.
- To make the scene start from a menu, add the `BreathingExercise.unity` scene to `File -> Build Settings -> Scenes In Build`.

Menu & example ready-made scene steps:

1. Create a new Scene and save as `BreathingMenu.unity` in `Assets/Scenes`.
2. Create an empty GameObject called `SceneLoader` and add the `Assets/Scripts/SceneLoader.cs` component.
3. Create a Canvas (`GameObject -> UI -> Canvas`) and inside it add a Button (`GameObject -> UI -> Button`) named `StartButton`.
4. Optionally style the button. `SceneLoader` will automatically find `StartButton` by name and hook the click to load `BreathingExercise`.
5. Add both `BreathingMenu.unity` and `BreathingExercise.unity` to `File -> Build Settings -> Scenes In Build`.

Artwork placeholder:

I added `Assets/Art/round_placeholder.txt` as a placeholder. To use real artwork, add `Assets/Art/round.png` (recommended 512x512, transparent background) and then in the `BreathingExercise` scene assign it to the `BreathingCircle` Image.

I created `Assets/Scripts/SceneLoader.cs` which loads `BreathingExercise` by default. If you want, I can also generate ready-made `.unity` scene files pre-populated with objects; Unity will create .meta GUIDs when you open them. Say "create scenes" if you want those actual `.unity` files added here.
