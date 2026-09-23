using System.IO;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;

public static class BuildFlappyBirdGame
{
    public static void Main()
    {
        Debug.Log("Starting Flappy Bird automated build...");

        EnsureDirectories();
        AddRequiredTags();
        GenerateSprites();
        GameObject pipePrefab = CreatePipePrefab();
        CreateFlappyScene(pipePrefab);

        Debug.Log("Flappy Bird build complete! Scene saved to Assets/Scenes/FlappyBird.unity");
    }

    private static void EnsureDirectories()
    {
        if (!AssetDatabase.IsValidFolder("Assets/Sprites")) AssetDatabase.CreateFolder("Assets", "Sprites");
        if (!AssetDatabase.IsValidFolder("Assets/Prefabs")) AssetDatabase.CreateFolder("Assets", "Prefabs");
        if (!AssetDatabase.IsValidFolder("Assets/Scenes")) AssetDatabase.CreateFolder("Assets", "Scenes");
        AssetDatabase.Refresh();
    }

    private static void AddRequiredTags()
    {
        SerializedObject tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
        SerializedProperty tagsProp = tagManager.FindProperty("tags");

        string[] requiredTags = { "Obstacle", "Ground", "ScoreTrigger" };
        foreach (string tag in requiredTags)
        {
            bool exists = false;
            for (int i = 0; i < tagsProp.arraySize; i++)
            {
                if (tagsProp.GetArrayElementAtIndex(i).stringValue == tag)
                {
                    exists = true;
                    break;
                }
            }

            if (!exists)
            {
                tagsProp.InsertArrayElementAtIndex(tagsProp.arraySize);
                tagsProp.GetArrayElementAtIndex(tagsProp.arraySize - 1).stringValue = tag;
                Debug.Log($"Added tag: {tag}");
            }
        }
        tagManager.ApplyModifiedProperties();
    }

    private static void GenerateSprites()
    {
        // 1. Bird Sprite (64x64)
        CreateBirdSprite("Assets/Sprites/Bird.png");

        // 2. Pipe Sprite (64x256)
        CreatePipeSprite("Assets/Sprites/Pipe.png");

        // 3. Ground Sprite (128x64)
        CreateGroundSprite("Assets/Sprites/Ground.png");

        // 4. Cloud Sprite (128x64)
        CreateCloudSprite("Assets/Sprites/Cloud.png");

        AssetDatabase.Refresh();

        ConfigureAsSprite("Assets/Sprites/Bird.png", 64);
        ConfigureAsSprite("Assets/Sprites/Pipe.png", 64);
        ConfigureAsSprite("Assets/Sprites/Ground.png", 64);
        ConfigureAsSprite("Assets/Sprites/Cloud.png", 64);
    }

    private static void ConfigureAsSprite(string path, float pixelsPerUnit)
    {
        TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
        if (importer != null)
        {
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.spritePixelsPerUnit = pixelsPerUnit;
            importer.filterMode = FilterMode.Point;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.SaveAndReimport();
        }
    }

    private static void CreateBirdSprite(string path)
    {
        int width = 64, height = 64;
        Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, false);
        Color clear = new Color(0, 0, 0, 0);
        Color bodyColor = new Color(1.0f, 0.85f, 0.1f);      // Bright Yellow
        Color bodyOutline = new Color(0.7f, 0.45f, 0.0f);    // Dark Yellow/Brown
        Color eyeWhite = Color.white;
        Color pupil = Color.black;
        Color beakColor = new Color(1.0f, 0.45f, 0.0f);      // Orange Beak
        Color wingColor = new Color(1.0f, 0.95f, 0.5f);      // Light yellow wing

        // Clear
        for (int y = 0; y < height; y++)
            for (int x = 0; x < width; x++)
                tex.SetPixel(x, y, clear);

        Vector2 center = new Vector2(28, 32);
        float rx = 20, ry = 18;

        // Draw body
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                float dx = (x - center.x) / rx;
                float dy = (y - center.y) / ry;
                float distSq = dx * dx + dy * dy;

                if (distSq <= 1.0f)
                {
                    if (distSq > 0.78f)
                        tex.SetPixel(x, y, bodyOutline);
                    else
                        tex.SetPixel(x, y, bodyColor);
                }
            }
        }

        // Draw Wing
        Vector2 wingCenter = new Vector2(20, 28);
        for (int y = 18; y <= 34; y++)
        {
            for (int x = 12; x <= 28; x++)
            {
                float dx = (x - wingCenter.x) / 8f;
                float dy = (y - wingCenter.y) / 6f;
                if (dx * dx + dy * dy <= 1.0f)
                {
                    tex.SetPixel(x, y, wingColor);
                }
            }
        }

        // Draw Beak (x: 44 to 58, y: 26 to 36)
        for (int y = 24; y <= 36; y++)
        {
            int startX = 42;
            int endX = startX + (int)((1f - Mathf.Abs(y - 30) / 6f) * 16);
            for (int x = startX; x <= endX; x++)
            {
                tex.SetPixel(x, y, beakColor);
            }
        }

        // Draw Eye (center at 38, 38)
        Vector2 eyeCenter = new Vector2(38, 38);
        for (int y = 30; y <= 46; y++)
        {
            for (int x = 30; x <= 46; x++)
            {
                float d = Vector2.Distance(new Vector2(x, y), eyeCenter);
                if (d <= 6f)
                {
                    tex.SetPixel(x, y, eyeWhite);
                }
                if (d <= 3f && x >= 39)
                {
                    tex.SetPixel(x, y, pupil);
                }
            }
        }

        tex.Apply();
        File.WriteAllBytes(path, tex.EncodeToPNG());
        Object.DestroyImmediate(tex);
    }

    private static void CreatePipeSprite(string path)
    {
        int width = 64, height = 256;
        Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, false);

        Color rimGreen = new Color(0.48f, 0.82f, 0.22f);
        Color rimHighlight = new Color(0.68f, 0.95f, 0.38f);
        Color rimShadow = new Color(0.24f, 0.48f, 0.12f);
        Color pipeBorder = new Color(0.12f, 0.28f, 0.08f);

        int rimHeight = 36;
        int rimStart = height - rimHeight;

        for (int y = 0; y < height; y++)
        {
            bool isRim = (y >= rimStart);
            int margin = isRim ? 0 : 5;

            for (int x = 0; x < width; x++)
            {
                if (x < margin || x >= width - margin)
                {
                    tex.SetPixel(x, y, new Color(0, 0, 0, 0));
                    continue;
                }

                // Border
                if (x == margin || x == width - margin - 1 || (isRim && y == rimStart) || y == height - 1)
                {
                    tex.SetPixel(x, y, pipeBorder);
                }
                // Highlight strip on left
                else if (x < margin + 10)
                {
                    tex.SetPixel(x, y, rimHighlight);
                }
                // Shadow strip on right
                else if (x > width - margin - 12)
                {
                    tex.SetPixel(x, y, rimShadow);
                }
                // Main body
                else
                {
                    tex.SetPixel(x, y, rimGreen);
                }
            }
        }

        tex.Apply();
        File.WriteAllBytes(path, tex.EncodeToPNG());
        Object.DestroyImmediate(tex);
    }

    private static void CreateGroundSprite(string path)
    {
        int width = 128, height = 64;
        Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, false);

        Color grassTop = new Color(0.52f, 0.85f, 0.24f);
        Color grassShadow = new Color(0.36f, 0.65f, 0.16f);
        Color dirtLight = new Color(0.85f, 0.78f, 0.52f);
        Color dirtDark = new Color(0.72f, 0.62f, 0.40f);
        Color borderDark = new Color(0.25f, 0.35f, 0.10f);

        int grassHeight = 16;

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                if (y == height - 1)
                {
                    tex.SetPixel(x, y, borderDark);
                }
                else if (y >= height - grassHeight)
                {
                    // Grass top with subtle stripe
                    bool tooth = ((x / 8) % 2 == 0) && (y == height - grassHeight);
                    tex.SetPixel(x, y, tooth ? grassShadow : grassTop);
                }
                else if (y == height - grassHeight - 1)
                {
                    tex.SetPixel(x, y, borderDark);
                }
                else
                {
                    // Dirt with diagonal pattern
                    bool stripe = ((x + y) / 8) % 2 == 0;
                    tex.SetPixel(x, y, stripe ? dirtDark : dirtLight);
                }
            }
        }

        tex.Apply();
        File.WriteAllBytes(path, tex.EncodeToPNG());
        Object.DestroyImmediate(tex);
    }

    private static void CreateCloudSprite(string path)
    {
        int width = 128, height = 64;
        Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, false);
        Color clear = new Color(0, 0, 0, 0);
        Color cloudColor = new Color(1f, 1f, 1f, 0.85f);

        for (int y = 0; y < height; y++)
            for (int x = 0; x < width; x++)
                tex.SetPixel(x, y, clear);

        // Draw 3 overlapping circles for cloud
        Vector2[] centers = { new Vector2(40, 28), new Vector2(64, 34), new Vector2(88, 28), new Vector2(64, 22) };
        float[] radii = { 20f, 25f, 20f, 20f };

        for (int i = 0; i < centers.Length; i++)
        {
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    if (Vector2.Distance(new Vector2(x, y), centers[i]) <= radii[i])
                    {
                        tex.SetPixel(x, y, cloudColor);
                    }
                }
            }
        }

        tex.Apply();
        File.WriteAllBytes(path, tex.EncodeToPNG());
        Object.DestroyImmediate(tex);
    }

    private static GameObject CreatePipePrefab()
    {
        Sprite pipeSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Pipe.png");

        GameObject root = new GameObject("PipePair");
        Pipe pipeScript = root.AddComponent<Pipe>();
        pipeScript.Speed = 2.6f;
        pipeScript.DestroyX = -12f;

        // Gap is ~3.2 units
        float pipeOffset = 3.6f;

        // Top Pipe (upside down)
        GameObject topPipe = new GameObject("TopPipe");
        topPipe.transform.SetParent(root.transform);
        topPipe.transform.localPosition = new Vector3(0f, pipeOffset, 0f);
        topPipe.transform.localRotation = Quaternion.Euler(0, 0, 180f);
        SpriteRenderer topSr = topPipe.AddComponent<SpriteRenderer>();
        topSr.sprite = pipeSprite;
        topSr.sortingOrder = 1;
        BoxCollider2D topCol = topPipe.AddComponent<BoxCollider2D>();
        topPipe.tag = "Obstacle";

        // Bottom Pipe
        GameObject bottomPipe = new GameObject("BottomPipe");
        bottomPipe.transform.SetParent(root.transform);
        bottomPipe.transform.localPosition = new Vector3(0f, -pipeOffset, 0f);
        SpriteRenderer botSr = bottomPipe.AddComponent<SpriteRenderer>();
        botSr.sprite = pipeSprite;
        botSr.sortingOrder = 1;
        BoxCollider2D botCol = bottomPipe.AddComponent<BoxCollider2D>();
        bottomPipe.tag = "Obstacle";

        // Score Trigger Zone
        GameObject scoreTrigger = new GameObject("ScoreTrigger");
        scoreTrigger.transform.SetParent(root.transform);
        scoreTrigger.transform.localPosition = Vector3.zero;
        BoxCollider2D triggerCol = scoreTrigger.AddComponent<BoxCollider2D>();
        triggerCol.isTrigger = true;
        triggerCol.size = new Vector2(0.6f, 3.2f);
        scoreTrigger.tag = "ScoreTrigger";

        string prefabPath = "Assets/Prefabs/PipePair.prefab";
        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
        Object.DestroyImmediate(root);
        Debug.Log("Created PipePair prefab at " + prefabPath);
        return prefab;
    }

    private static void CreateFlappyScene(GameObject pipePrefab)
    {
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        Sprite birdSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Bird.png");
        Sprite groundSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Ground.png");
        Sprite cloudSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Cloud.png");

        // 1. Camera
        GameObject camGo = new GameObject("Main Camera");
        Camera cam = camGo.AddComponent<Camera>();
        cam.orthographic = true;
        cam.orthographicSize = 5f;
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = new Color(0.44f, 0.77f, 0.81f); // Sky Cyan
        camGo.transform.position = new Vector3(0f, 0f, -10f);
        camGo.tag = "MainCamera";
        camGo.AddComponent<AudioListener>();

        // 2. Background Clouds
        GameObject cloudsRoot = new GameObject("Environment_Clouds");
        cloudsRoot.transform.position = Vector3.zero;
        Vector3[] cloudPositions = { new Vector3(-4f, 2.5f, 5f), new Vector3(2f, 3.8f, 5f), new Vector3(7f, 1.8f, 5f) };
        for (int i = 0; i < cloudPositions.Length; i++)
        {
            GameObject cloud = new GameObject("Cloud_" + i);
            cloud.transform.SetParent(cloudsRoot.transform);
            cloud.transform.position = cloudPositions[i];
            SpriteRenderer cloudSr = cloud.AddComponent<SpriteRenderer>();
            cloudSr.sprite = cloudSprite;
            cloudSr.sortingOrder = -5;
        }

        // 3. Ground & Ceiling
        GameObject groundRoot = new GameObject("GroundScroller");
        groundRoot.transform.position = new Vector3(0f, -4.5f, 0f);
        GroundScroller groundScroller = groundRoot.AddComponent<GroundScroller>();
        groundScroller.ScrollSpeed = 2.6f;
        groundScroller.TileWidth = 14f;

        GameObject groundA = new GameObject("GroundA");
        groundA.transform.SetParent(groundRoot.transform);
        groundA.transform.localPosition = Vector3.zero;
        SpriteRenderer gaSr = groundA.AddComponent<SpriteRenderer>();
        gaSr.sprite = groundSprite;
        gaSr.drawMode = SpriteDrawMode.Tiled;
        gaSr.size = new Vector2(14f, 2f);
        gaSr.sortingOrder = 5;
        BoxCollider2D gaCol = groundA.AddComponent<BoxCollider2D>();
        gaCol.size = new Vector2(14f, 2f);
        groundA.tag = "Ground";

        GameObject groundB = new GameObject("GroundB");
        groundB.transform.SetParent(groundRoot.transform);
        groundB.transform.localPosition = new Vector3(14f, 0f, 0f);
        SpriteRenderer gbSr = groundB.AddComponent<SpriteRenderer>();
        gbSr.sprite = groundSprite;
        gbSr.drawMode = SpriteDrawMode.Tiled;
        gbSr.size = new Vector2(14f, 2f);
        gbSr.sortingOrder = 5;
        BoxCollider2D gbCol = groundB.AddComponent<BoxCollider2D>();
        gbCol.size = new Vector2(14f, 2f);
        groundB.tag = "Ground";

        groundScroller.GroundA = groundA.transform;
        groundScroller.GroundB = groundB.transform;

        // Ceiling Collider (so bird cannot fly above screen)
        GameObject ceiling = new GameObject("Ceiling");
        ceiling.transform.position = new Vector3(0f, 5.5f, 0f);
        BoxCollider2D ceilCol = ceiling.AddComponent<BoxCollider2D>();
        ceilCol.size = new Vector2(20f, 1f);
        ceiling.tag = "Obstacle";

        // 4. Bird
        GameObject birdGo = new GameObject("Bird");
        birdGo.transform.position = new Vector3(-2f, 0f, 0f);
        SpriteRenderer birdSr = birdGo.AddComponent<SpriteRenderer>();
        birdSr.sprite = birdSprite;
        birdSr.sortingOrder = 3;

        CircleCollider2D birdCol = birdGo.AddComponent<CircleCollider2D>();
        birdCol.radius = 0.4f;

        Rigidbody2D birdRb = birdGo.AddComponent<Rigidbody2D>();
        birdRb.gravityScale = 2.4f;
        birdRb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

        BirdController birdCtrl = birdGo.AddComponent<BirdController>();
        birdCtrl.FlapForce = 6.2f;

        // 5. Pipe Spawner
        GameObject spawnerGo = new GameObject("PipeSpawner");
        PipeSpawner spawner = spawnerGo.AddComponent<PipeSpawner>();
        spawner.PipePrefab = pipePrefab;
        spawner.SpawnInterval = 1.7f;
        spawner.MinY = -1.4f;
        spawner.MaxY = 1.8f;
        spawner.SpawnX = 7f;

        // 6. UI Canvas
        GameObject canvasGo = new GameObject("Canvas");
        Canvas canvas = canvasGo.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceCamera;
        canvas.worldCamera = cam;
        canvas.planeDistance = 5f;
        canvas.sortingOrder = 20;

        CanvasScaler scaler = canvasGo.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(800, 800);
        scaler.matchWidthOrHeight = 0.5f;
        canvasGo.AddComponent<GraphicRaycaster>();

        // EventSystem
        GameObject eventSysGo = new GameObject("EventSystem");
        eventSysGo.AddComponent<UnityEngine.EventSystems.EventSystem>();
#if ENABLE_INPUT_SYSTEM
        eventSysGo.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
#else
        eventSysGo.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
#endif

        FlappyUI flappyUI = canvasGo.AddComponent<FlappyUI>();

        // UI Panels
        Font defaultFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

        // Ready Panel
        GameObject readyPanel = CreateUIPanel(canvasGo.transform, "ReadyPanel");
        Text readyTitle = CreateUIText(readyPanel.transform, "Title", "FLAPPY BIRD", defaultFont, 64, FontStyle.Bold, new Color(1f, 0.85f, 0.1f), new Vector2(0, 180), new Vector2(600, 120));
        AddOutline(readyTitle.gameObject, Color.black, 3f);

        Text readyPrompt = CreateUIText(readyPanel.transform, "Prompt", "TAP / SPACE TO FLY", defaultFont, 36, FontStyle.Bold, Color.white, new Vector2(0, -120), new Vector2(600, 80));
        AddOutline(readyPrompt.gameObject, Color.black, 2f);

        // Playing HUD
        GameObject playingPanel = CreateUIPanel(canvasGo.transform, "PlayingPanel");
        Text hudScore = CreateUIText(playingPanel.transform, "HUDScore", "0", defaultFont, 72, FontStyle.Bold, Color.white, new Vector2(0, 310), new Vector2(300, 120));
        AddOutline(hudScore.gameObject, Color.black, 4f);

        // GameOver Panel
        GameObject gameOverPanel = CreateUIPanel(canvasGo.transform, "GameOverPanel");

        // Dark backdrop for game over
        GameObject goBackdrop = new GameObject("Backdrop");
        goBackdrop.transform.SetParent(gameOverPanel.transform, false);
        RectTransform bdRt = goBackdrop.AddComponent<RectTransform>();
        bdRt.sizeDelta = new Vector2(440, 460);
        bdRt.anchoredPosition = new Vector2(0, 30);
        Image bdImg = goBackdrop.AddComponent<Image>();
        bdImg.color = new Color(0f, 0f, 0f, 0.45f);

        Text goTitle = CreateUIText(gameOverPanel.transform, "GOTitle", "GAME OVER", defaultFont, 56, FontStyle.Bold, new Color(0.95f, 0.25f, 0.25f), new Vector2(0, 180), new Vector2(500, 100));
        AddOutline(goTitle.gameObject, Color.black, 3f);

        Text goScore = CreateUIText(gameOverPanel.transform, "GOScore", "SCORE: 0", defaultFont, 38, FontStyle.Bold, Color.white, new Vector2(0, 85), new Vector2(400, 70));
        AddOutline(goScore.gameObject, Color.black, 2f);

        Text goBest = CreateUIText(gameOverPanel.transform, "GOBest", "BEST: 0", defaultFont, 34, FontStyle.Bold, new Color(1f, 0.88f, 0.2f), new Vector2(0, 25), new Vector2(400, 70));
        AddOutline(goBest.gameObject, Color.black, 2f);

        // Restart Prompt Button
        GameObject restartBtnGo = CreateUIButton(gameOverPanel.transform, "RestartButton", "PLAY AGAIN", defaultFont, new Vector2(0, -75), new Vector2(280, 70));

        // Default state: only ReadyPanel is visible initially
        readyPanel.SetActive(true);
        playingPanel.SetActive(false);
        gameOverPanel.SetActive(false);

        flappyUI.ReadyPanel = readyPanel;
        flappyUI.PlayingPanel = playingPanel;
        flappyUI.GameOverPanel = gameOverPanel;
        flappyUI.ScoreText = hudScore;
        flappyUI.GameOverScoreText = goScore;
        flappyUI.HighScoreText = goBest;
        flappyUI.RestartButton = restartBtnGo.GetComponent<Button>();

        // 7. GameManager
        GameObject gmGo = new GameObject("GameManager");
        GameManager gm = gmGo.AddComponent<GameManager>();
        gm.Bird = birdCtrl;
        gm.Spawner = spawner;
        gm.Ground = groundScroller;
        gm.UI = flappyUI;

        // Save Scene
        string scenePath = "Assets/Scenes/FlappyBird.unity";
        EditorSceneManager.SaveScene(scene, scenePath);
        Debug.Log("Scene saved to " + scenePath);

        // Add Scene to EditorBuildSettings
        EditorBuildSettingsScene[] scenes = new EditorBuildSettingsScene[]
        {
            new EditorBuildSettingsScene(scenePath, true)
        };
        EditorBuildSettings.scenes = scenes;
    }

    private static GameObject CreateUIPanel(Transform parent, string name)
    {
        GameObject panel = new GameObject(name);
        panel.transform.SetParent(parent, false);
        RectTransform rt = panel.AddComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.sizeDelta = Vector2.zero;
        rt.anchoredPosition = Vector2.zero;
        return panel;
    }

    private static Text CreateUIText(Transform parent, string name, string text, Font font, int size, FontStyle style, Color color, Vector2 pos, Vector2 dimensions)
    {
        GameObject textGo = new GameObject(name);
        textGo.transform.SetParent(parent, false);
        RectTransform rt = textGo.AddComponent<RectTransform>();
        rt.anchoredPosition = pos;
        rt.sizeDelta = dimensions;

        Text t = textGo.AddComponent<Text>();
        t.text = text;
        t.font = font;
        t.fontSize = size;
        t.fontStyle = style;
        t.color = color;
        t.alignment = TextAnchor.MiddleCenter;
        return t;
    }

    private static GameObject CreateUIButton(Transform parent, string name, string label, Font font, Vector2 pos, Vector2 dimensions)
    {
        GameObject btnGo = new GameObject(name);
        btnGo.transform.SetParent(parent, false);
        RectTransform rt = btnGo.AddComponent<RectTransform>();
        rt.anchoredPosition = pos;
        rt.sizeDelta = dimensions;

        Image img = btnGo.AddComponent<Image>();
        img.color = new Color(0.92f, 0.45f, 0.15f);

        Button btn = btnGo.AddComponent<Button>();
        ColorBlock colors = btn.colors;
        colors.highlightedColor = new Color(1.0f, 0.6f, 0.2f);
        colors.pressedColor = new Color(0.75f, 0.35f, 0.1f);
        btn.colors = colors;

        GameObject textGo = new GameObject("Text");
        textGo.transform.SetParent(btnGo.transform, false);
        RectTransform textRt = textGo.AddComponent<RectTransform>();
        textRt.anchorMin = Vector2.zero;
        textRt.anchorMax = Vector2.one;
        textRt.sizeDelta = Vector2.zero;

        Text t = textGo.AddComponent<Text>();
        t.text = label;
        t.font = font;
        t.fontSize = 38;
        t.fontStyle = FontStyle.Bold;
        t.color = Color.white;
        t.alignment = TextAnchor.MiddleCenter;

        AddOutline(textGo, Color.black, 2f);

        return btnGo;
    }

    private static void AddOutline(GameObject go, Color outlineColor, float distance)
    {
        Outline outline = go.AddComponent<Outline>();
        outline.effectColor = outlineColor;
        outline.effectDistance = new Vector2(distance, -distance);
    }
}
