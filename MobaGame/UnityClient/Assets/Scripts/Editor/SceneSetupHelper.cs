using UnityEngine;
using UnityEditor;
using UnityEngine.SceneManagement;
using UnityEditor.SceneManagement;

namespace MobaGame.Editor
{
    public class SceneSetupHelper : EditorWindow
    {
        [MenuItem("MOBA/Setup/Create All Scenes")]
        public static void CreateAllScenes()
        {
            CreateScene("LoginScene");
            CreateScene("LobbyScene");
            CreateScene("HeroSelectionScene");
            CreateScene("GameScene");
            CreateScene("ResultScene");

            Debug.Log("All scenes created in Assets/Scenes/");
        }

        private static void CreateScene(string sceneName)
        {
            Scene newScene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            
            // Add a camera
            GameObject camera = new GameObject("Main Camera");
            camera.AddComponent<Camera>();
            camera.tag = "MainCamera";
            camera.AddComponent<AudioListener>();

            // Add a directional light
            GameObject light = new GameObject("Directional Light");
            Light lightComponent = light.AddComponent<Light>();
            lightComponent.type = LightType.Directional;
            light.transform.rotation = Quaternion.Euler(50, -30, 0);

            // Save the scene
            string scenePath = $"Assets/Scenes/{sceneName}.unity";
            System.IO.Directory.CreateDirectory("Assets/Scenes");
            EditorSceneManager.SaveScene(newScene, scenePath);

            Debug.Log($"Created scene: {scenePath}");
        }

        [MenuItem("MOBA/Setup/Add Scenes to Build Settings")]
        public static void AddScenesToBuild()
        {
            string[] sceneNames = { "LoginScene", "LobbyScene", "HeroSelectionScene", "GameScene", "ResultScene" };
            var scenes = new System.Collections.Generic.List<EditorBuildSettingsScene>();

            foreach (string sceneName in sceneNames)
            {
                string scenePath = $"Assets/Scenes/{sceneName}.unity";
                if (System.IO.File.Exists(scenePath))
                {
                    scenes.Add(new EditorBuildSettingsScene(scenePath, true));
                }
            }

            EditorBuildSettings.scenes = scenes.ToArray();
            Debug.Log("Added scenes to Build Settings");
        }
    }
}
