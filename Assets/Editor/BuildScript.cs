// Assets/Editor/BuildScript.cs
using UnityEditor;
using UnityEngine;

public class BuildScript
{
    public static void BuildAndroid()
    {
        string[] scenes = { "Assets/Scenes/FlappyBird.unity" }; // adjust to your scene(s)

        string outputPath = "Builds/Android/app.apk";

        BuildPlayerOptions buildOptions = new BuildPlayerOptions
        {
            scenes = scenes,
            locationPathName = outputPath,
            target = BuildTarget.Android,
            options = BuildOptions.None
        };

        BuildPipeline.BuildPlayer(buildOptions);
    }
}