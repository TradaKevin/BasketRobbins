#if UNITY_EDITOR
using System.IO;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuUIBuilder : MonoBehaviour
{
    private const string AssetFolder = "Assets/UI/IntroMenuPolished";

    private static readonly Color MainOrange = Hex("#F58A22");
    private static readonly Color LightOrange = Hex("#FFB347");
    private static readonly Color DarkBrown = Hex("#2D1608");
    private static readonly Color CreamWhite = Hex("#FFF6E8");
    private static readonly Color DeepShadow = new Color(0.05f, 0.025f, 0.01f, 0.86f);

    [ContextMenu("Build Polished Intro Menu UI")]
    public void BuildFromContext()
    {
        BuildPolishedIntroMenu();
    }

    public static void BuildPolishedIntroMenu()
    {
        EnsureFolders();

        Sprite rounded = SaveSprite("Rounded_Panel", MakeRoundedTexture(128, 64, 22), 100f, new Vector4(22, 22, 22, 22));
        Sprite circle = SaveSprite("Circle_White", MakeCircleTexture(128, Color.white, false), 100f, Vector4.zero);
        Sprite glow = SaveSprite("Soft_Glow", MakeCircleTexture(192, new Color(1f, 0.48f, 0.06f, 0.38f), true), 100f, Vector4.zero);
        Sprite ball = SaveSprite("Basketball_Icon", MakeBasketballTexture(192), 100f, Vector4.zero);
        Sprite greenDot = SaveSprite("Trajectory_Dot", MakeCircleTexture(48, Hex("#33CC66"), true), 100f, Vector4.zero);
        Sprite coin = SaveSprite("Coin_Icon", MakeCoinTexture(128), 100f, Vector4.zero);
        Sprite gear = SaveSprite("Gear_Icon", MakeGearIcon(128), 100f, Vector4.zero);
        Sprite speaker = SaveSprite("Sound_Icon", MakeSpeakerIcon(128), 100f, Vector4.zero);
        Sprite power = SaveSprite("Power_Icon", MakePowerIcon(128), 100f, Vector4.zero);
        Sprite map = SaveSprite("Map_Icon", MakeMapIcon(128), 100f, Vector4.zero);
        Sprite cart = SaveSprite("Cart_Icon", MakeCartIcon(128), 100f, Vector4.zero);
        Sprite scoreFlash = SaveSprite("Score_Flash", MakeFlashTexture(128), 100f, Vector4.zero);

        GameObject menuObject = GameObject.Find("Menu");
        if (menuObject == null)
        {
            menuObject = new GameObject("Menu", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            menuObject.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
        }

        Canvas canvas = menuObject.GetComponent<Canvas>();
        if (canvas == null)
        {
            canvas = menuObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        }

        CanvasScaler scaler = menuObject.GetComponent<CanvasScaler>();
        if (scaler == null)
        {
            scaler = menuObject.AddComponent<CanvasScaler>();
        }

        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;

        if (menuObject.GetComponent<GraphicRaycaster>() == null)
        {
            menuObject.AddComponent<GraphicRaycaster>();
        }

        Transform menuTransform = menuObject.transform;
        DestroyChildIfExists(menuTransform, "ArcadeMainMenu");

        Transform oldMainMenu = FindChildRecursive(menuTransform, "Main Menu");
        if (oldMainMenu != null)
        {
            oldMainMenu.gameObject.SetActive(false);
        }

        Transform optionMenu = FindChildRecursive(menuTransform, "Option Menu");
        if (optionMenu != null)
        {
            optionMenu.gameObject.SetActive(false);
        }

        GameObject root = new GameObject("ArcadeMainMenu", typeof(RectTransform));
        root.transform.SetParent(menuTransform, false);
        RectTransform rootRect = root.GetComponent<RectTransform>();
        Stretch(rootRect, Vector2.zero, Vector2.zero);
        root.transform.SetAsLastSibling();

        Image overlay = AddImage("DarkReadableOverlay", root.transform, null, new Color(0f, 0f, 0f, 0.42f));
        Stretch(overlay.rectTransform, Vector2.zero, Vector2.zero);

        AudioSource audioSource = root.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;

        CreateCoinCounter(root.transform, rounded, coin, out TMP_Text coinText);
        CreateBackgroundLoop(root.transform, ball, greenDot, scoreFlash);
        CreateLogo(root.transform, ball, glow);

        Button playButton = CreateArcadeButton("PlayButton", root.transform, "PLAY", new Vector2(0f, -80f), new Vector2(600f, 126f), true, rounded, ball, audioSource, 70f);
        Button levelButton = CreateArcadeButton("LevelSelectButton", root.transform, "LEVEL SELECT", new Vector2(0f, -215f), new Vector2(420f, 76f), false, rounded, map, audioSource, 34f);
        Button shopButton = CreateArcadeButton("ShopButton", root.transform, "SHOP", new Vector2(0f, -305f), new Vector2(420f, 76f), false, rounded, cart, audioSource, 34f);
        Button optionsButton = CreateArcadeButton("OptionsButton", root.transform, "OPTIONS", new Vector2(0f, -395f), new Vector2(420f, 76f), false, rounded, gear, audioSource, 34f);
        Button quitButton = CreateArcadeButton("QuitButton", root.transform, "QUIT", new Vector2(155f, 74f), new Vector2(230f, 74f), false, rounded, power, audioSource, 32f);
        AnchorBottomLeft(quitButton.GetComponent<RectTransform>());

        Button settingsButton = CreateRoundButton("SettingsButton", root.transform, gear, new Vector2(-215f, 76f), circle, audioSource);
        Button soundButton = CreateRoundButton("SoundButton", root.transform, speaker, new Vector2(-115f, 76f), circle, audioSource);
        AnchorBottomRight(settingsButton.GetComponent<RectTransform>());
        AnchorBottomRight(soundButton.GetComponent<RectTransform>());

        MainMenuManager manager = root.AddComponent<MainMenuManager>();
        manager.playButton = playButton;
        manager.levelSelectButton = levelButton;
        manager.shopButton = shopButton;
        manager.optionsButton = optionsButton;
        manager.quitButton = quitButton;
        manager.settingsButton = settingsButton;
        manager.soundButton = soundButton;
        manager.coinText = coinText;
        manager.optionsPanel = optionMenu != null ? optionMenu.gameObject : null;
        manager.uiAudioSource = audioSource;
        manager.playSceneName = "Main GamePlay";
        manager.levelSelectSceneName = "LevelSelect";

        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        EditorSceneManager.SaveScene(SceneManager.GetActiveScene());
        Debug.Log("Polished BasketRobbins Intro Menu UI created.");
    }

    private static void CreateLogo(Transform parent, Sprite ball, Sprite glow)
    {
        GameObject group = new GameObject("LogoGroup", typeof(RectTransform));
        group.transform.SetParent(parent, false);
        RectTransform rect = group.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 1f);
        rect.anchorMax = new Vector2(0.5f, 1f);
        rect.pivot = new Vector2(0.5f, 1f);
        rect.anchoredPosition = new Vector2(0f, -32f);
        rect.sizeDelta = new Vector2(640f, 300f);

        Image logoGlow = AddImage("LogoGlow", group.transform, glow, Color.white);
        logoGlow.rectTransform.anchoredPosition = new Vector2(0f, -64f);
        logoGlow.rectTransform.sizeDelta = new Vector2(230f, 230f);
        logoGlow.raycastTarget = false;

        Image ballBadge = AddImage("LogoBasketballBadge", group.transform, ball, Color.white);
        ballBadge.rectTransform.anchoredPosition = new Vector2(0f, -72f);
        ballBadge.rectTransform.sizeDelta = new Vector2(178f, 178f);
        ballBadge.raycastTarget = false;

        TMP_Text logoText = AddText("LogoText", group.transform, "Basket\nRobbins", 78f, FontStyles.Bold, new Vector2(0f, -190f), new Vector2(620f, 165f), TextAlignmentOptions.Center, CreamWhite);
        logoText.lineSpacing = -20f;
        AddOutlineAndShadow(logoText.gameObject, DarkBrown, new Vector2(5f, -5f));

        Image subtitlePill = AddImage("SubtitlePill", group.transform, null, new Color(0.05f, 0.025f, 0.01f, 0.78f));
        subtitlePill.rectTransform.anchoredPosition = new Vector2(0f, -285f);
        subtitlePill.rectTransform.sizeDelta = new Vector2(455f, 42f);
        subtitlePill.raycastTarget = false;

        TMP_Text subtitle = AddText("Subtitle", group.transform, "Arcade Basketball Challenge", 24f, FontStyles.Bold, new Vector2(0f, -285f), new Vector2(440f, 38f), TextAlignmentOptions.Center, CreamWhite);
        AddShadow(subtitle.gameObject, new Color(0f, 0f, 0f, 0.75f), new Vector2(2f, -2f));
    }

    private static Button CreateArcadeButton(string name, Transform parent, string label, Vector2 position, Vector2 size, bool primary, Sprite rounded, Sprite icon, AudioSource audioSource, float fontSize)
    {
        GameObject buttonObject = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button), typeof(UIButtonAnimator));
        buttonObject.transform.SetParent(parent, false);
        RectTransform rect = buttonObject.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = position;
        rect.sizeDelta = size;

        Image border = buttonObject.GetComponent<Image>();
        border.sprite = rounded;
        border.type = Image.Type.Sliced;
        border.color = primary ? CreamWhite : LightOrange;

        Shadow shadow = buttonObject.AddComponent<Shadow>();
        shadow.effectColor = new Color(0f, 0f, 0f, 0.75f);
        shadow.effectDistance = new Vector2(0f, -7f);

        Image fill = AddImage("Fill", buttonObject.transform, rounded, primary ? MainOrange : DarkBrown);
        fill.type = Image.Type.Sliced;
        Stretch(fill.rectTransform, new Vector2(8f, 8f), new Vector2(-8f, -8f));
        fill.raycastTarget = false;

        Image gloss = AddImage("TopGloss", buttonObject.transform, rounded, primary ? new Color(1f, 0.85f, 0.45f, 0.28f) : new Color(1f, 0.55f, 0.14f, 0.18f));
        gloss.type = Image.Type.Sliced;
        gloss.rectTransform.anchorMin = new Vector2(0f, 0.52f);
        gloss.rectTransform.anchorMax = new Vector2(1f, 1f);
        gloss.rectTransform.offsetMin = new Vector2(14f, 0f);
        gloss.rectTransform.offsetMax = new Vector2(-14f, -10f);
        gloss.raycastTarget = false;

        if (icon != null)
        {
            Image iconImage = AddImage("Icon", buttonObject.transform, icon, Color.white);
            iconImage.rectTransform.anchorMin = new Vector2(0f, 0.5f);
            iconImage.rectTransform.anchorMax = new Vector2(0f, 0.5f);
            iconImage.rectTransform.pivot = new Vector2(0.5f, 0.5f);
            iconImage.rectTransform.anchoredPosition = new Vector2(primary ? 86f : 58f, 0f);
            iconImage.rectTransform.sizeDelta = primary ? new Vector2(78f, 78f) : new Vector2(46f, 46f);
            iconImage.raycastTarget = false;
        }

        TMP_Text text = AddText("Label", buttonObject.transform, label, fontSize, FontStyles.Bold, primary ? new Vector2(55f, 1f) : new Vector2(34f, 1f), size, TextAlignmentOptions.Center, CreamWhite);
        AddOutlineAndShadow(text.gameObject, DarkBrown, new Vector2(3f, -3f));

        Button button = buttonObject.GetComponent<Button>();
        button.targetGraphic = border;
        ColorBlock colors = button.colors;
        colors.normalColor = Color.white;
        colors.highlightedColor = primary ? LightOrange : new Color(1f, 0.72f, 0.32f, 1f);
        colors.pressedColor = new Color(0.82f, 0.31f, 0.03f, 1f);
        colors.selectedColor = Color.white;
        colors.disabledColor = new Color(0.35f, 0.3f, 0.26f, 1f);
        colors.fadeDuration = 0.08f;
        button.colors = colors;

        UIButtonAnimator animator = buttonObject.GetComponent<UIButtonAnimator>();
        animator.audioSource = audioSource;
        animator.hoverScale = 1.05f;
        animator.pressedScale = 0.95f;

        return button;
    }

    private static Button CreateRoundButton(string name, Transform parent, Sprite icon, Vector2 position, Sprite circle, AudioSource audioSource)
    {
        GameObject buttonObject = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button), typeof(UIButtonAnimator));
        buttonObject.transform.SetParent(parent, false);
        RectTransform rect = buttonObject.GetComponent<RectTransform>();
        rect.anchoredPosition = position;
        rect.sizeDelta = new Vector2(78f, 78f);

        Image border = buttonObject.GetComponent<Image>();
        border.sprite = circle;
        border.color = LightOrange;

        Image fill = AddImage("Fill", buttonObject.transform, circle, DarkBrown);
        Stretch(fill.rectTransform, new Vector2(6f, 6f), new Vector2(-6f, -6f));
        fill.raycastTarget = false;

        Image iconImage = AddImage("Icon", buttonObject.transform, icon, CreamWhite);
        iconImage.rectTransform.sizeDelta = new Vector2(43f, 43f);
        iconImage.raycastTarget = false;

        Shadow shadow = buttonObject.AddComponent<Shadow>();
        shadow.effectColor = new Color(0f, 0f, 0f, 0.65f);
        shadow.effectDistance = new Vector2(0f, -5f);

        Button button = buttonObject.GetComponent<Button>();
        button.targetGraphic = border;

        UIButtonAnimator animator = buttonObject.GetComponent<UIButtonAnimator>();
        animator.audioSource = audioSource;
        return button;
    }

    private static void CreateCoinCounter(Transform parent, Sprite rounded, Sprite coin, out TMP_Text coinText)
    {
        GameObject counter = new GameObject("CoinCounter", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        counter.transform.SetParent(parent, false);
        RectTransform rect = counter.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(1f, 1f);
        rect.anchorMax = new Vector2(1f, 1f);
        rect.pivot = new Vector2(1f, 1f);
        rect.anchoredPosition = new Vector2(-78f, -48f);
        rect.sizeDelta = new Vector2(265f, 78f);

        Image border = counter.GetComponent<Image>();
        border.sprite = rounded;
        border.type = Image.Type.Sliced;
        border.color = LightOrange;

        Image fill = AddImage("Fill", counter.transform, rounded, DeepShadow);
        fill.type = Image.Type.Sliced;
        Stretch(fill.rectTransform, new Vector2(7f, 7f), new Vector2(-7f, -7f));
        fill.raycastTarget = false;

        Image coinImage = AddImage("CoinIcon", counter.transform, coin, Color.white);
        coinImage.rectTransform.anchorMin = new Vector2(0f, 0.5f);
        coinImage.rectTransform.anchorMax = new Vector2(0f, 0.5f);
        coinImage.rectTransform.pivot = new Vector2(0.5f, 0.5f);
        coinImage.rectTransform.anchoredPosition = new Vector2(42f, 0f);
        coinImage.rectTransform.sizeDelta = new Vector2(64f, 64f);
        coinImage.raycastTarget = false;

        coinText = AddText("CoinAmount", counter.transform, "250", 38f, FontStyles.Bold, new Vector2(42f, 0f), new Vector2(210f, 62f), TextAlignmentOptions.Center, CreamWhite);
        AddOutlineAndShadow(coinText.gameObject, DarkBrown, new Vector2(2f, -2f));

        Image plus = AddImage("PlusBadge", counter.transform, null, Hex("#33CC66"));
        plus.rectTransform.anchorMin = new Vector2(1f, 0.5f);
        plus.rectTransform.anchorMax = new Vector2(1f, 0.5f);
        plus.rectTransform.pivot = new Vector2(0.5f, 0.5f);
        plus.rectTransform.anchoredPosition = new Vector2(-35f, 0f);
        plus.rectTransform.sizeDelta = new Vector2(42f, 42f);
        TMP_Text plusText = AddText("Plus", plus.transform, "+", 34f, FontStyles.Bold, Vector2.zero, new Vector2(42f, 42f), TextAlignmentOptions.Center, Color.white);
        plusText.raycastTarget = false;
    }

    private static void CreateBackgroundLoop(Transform parent, Sprite ball, Sprite dot, Sprite flashSprite)
    {
        GameObject group = new GameObject("BackgroundShotLoop", typeof(RectTransform), typeof(BasketballMenuLoop));
        group.transform.SetParent(parent, false);
        Stretch(group.GetComponent<RectTransform>(), Vector2.zero, Vector2.zero);

        RectTransform[] dots = new RectTransform[13];
        for (int i = 0; i < dots.Length; i++)
        {
            Image dotImage = AddImage("TrajectoryDot_" + (i + 1), group.transform, dot, Color.white);
            dotImage.rectTransform.sizeDelta = new Vector2(24f, 24f);
            dotImage.raycastTarget = false;
            dots[i] = dotImage.rectTransform;
        }

        Image score = AddImage("ScoreFlash", group.transform, flashSprite, Color.clear);
        score.rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        score.rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        score.rectTransform.anchoredPosition = new Vector2(790f, 170f);
        score.rectTransform.sizeDelta = new Vector2(120f, 120f);
        score.raycastTarget = false;

        Image ballImage = AddImage("AnimatedBasketball", group.transform, ball, Color.white);
        ballImage.rectTransform.sizeDelta = new Vector2(78f, 78f);
        ballImage.raycastTarget = false;

        BasketballMenuLoop loop = group.GetComponent<BasketballMenuLoop>();
        loop.ball = ballImage.rectTransform;
        loop.trajectoryDots = dots;
        loop.scoreFlash = score;
    }

    private static TMP_Text AddText(string name, Transform parent, string value, float size, FontStyles style, Vector2 position, Vector2 rectSize, TextAlignmentOptions alignment, Color color)
    {
        GameObject textObject = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        textObject.transform.SetParent(parent, false);
        RectTransform rect = textObject.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = position;
        rect.sizeDelta = rectSize;

        TextMeshProUGUI text = textObject.GetComponent<TextMeshProUGUI>();
        text.text = value;
        text.fontSize = size;
        text.fontStyle = style;
        text.alignment = alignment;
        text.color = color;
        text.enableWordWrapping = false;
        text.raycastTarget = false;
        return text;
    }

    private static Image AddImage(string name, Transform parent, Sprite sprite, Color color)
    {
        GameObject imageObject = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        imageObject.transform.SetParent(parent, false);
        RectTransform rect = imageObject.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        Image image = imageObject.GetComponent<Image>();
        image.sprite = sprite;
        image.color = color;
        return image;
    }

    private static void AddOutlineAndShadow(GameObject target, Color outlineColor, Vector2 shadowDistance)
    {
        Outline outline = target.AddComponent<Outline>();
        outline.effectColor = outlineColor;
        outline.effectDistance = new Vector2(3f, -3f);
        AddShadow(target, new Color(0f, 0f, 0f, 0.75f), shadowDistance);
    }

    private static void AddShadow(GameObject target, Color color, Vector2 distance)
    {
        Shadow shadow = target.AddComponent<Shadow>();
        shadow.effectColor = color;
        shadow.effectDistance = distance;
    }

    private static void AnchorBottomLeft(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.zero;
        rect.pivot = new Vector2(0.5f, 0.5f);
    }

    private static void AnchorBottomRight(RectTransform rect)
    {
        rect.anchorMin = new Vector2(1f, 0f);
        rect.anchorMax = new Vector2(1f, 0f);
        rect.pivot = new Vector2(0.5f, 0.5f);
    }

    private static void Stretch(RectTransform rect, Vector2 minOffset, Vector2 maxOffset)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = minOffset;
        rect.offsetMax = maxOffset;
    }

    private static Transform FindChildRecursive(Transform parent, string childName)
    {
        foreach (Transform child in parent)
        {
            if (child.name == childName)
            {
                return child;
            }

            Transform result = FindChildRecursive(child, childName);
            if (result != null)
            {
                return result;
            }
        }
        return null;
    }

    private static void DestroyChildIfExists(Transform parent, string childName)
    {
        Transform child = FindChildRecursive(parent, childName);
        if (child != null)
        {
            UnityEngine.Object.DestroyImmediate(child.gameObject);
        }
    }

    private static void EnsureFolders()
    {
        CreateFolder("Assets", "UI");
        CreateFolder("Assets/UI", "IntroMenuPolished");
    }

    private static void CreateFolder(string parent, string child)
    {
        string path = parent + "/" + child;
        if (!AssetDatabase.IsValidFolder(path))
        {
            AssetDatabase.CreateFolder(parent, child);
        }
    }

    private static Sprite SaveSprite(string name, Texture2D texture, float pixelsPerUnit, Vector4 border)
    {
        string path = AssetFolder + "/" + name + ".png";
        File.WriteAllBytes(path, texture.EncodeToPNG());
        AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport);

        TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
        if (importer != null)
        {
            importer.textureType = TextureImporterType.Sprite;
            importer.spritePixelsPerUnit = pixelsPerUnit;
            importer.alphaIsTransparency = true;
            importer.mipmapEnabled = false;
            importer.spriteBorder = border;
            importer.SaveAndReimport();
        }

        return AssetDatabase.LoadAssetAtPath<Sprite>(path);
    }

    private static Texture2D MakeRoundedTexture(int width, int height, int radius)
    {
        Texture2D texture = NewClearTexture(width, height);
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                Vector2 corner = new Vector2(Mathf.Clamp(x, radius, width - radius - 1), Mathf.Clamp(y, radius, height - radius - 1));
                bool inside = x >= radius && x < width - radius || y >= radius && y < height - radius || Vector2.Distance(new Vector2(x, y), corner) <= radius;
                texture.SetPixel(x, y, inside ? Color.white : Color.clear);
            }
        }
        texture.Apply();
        return texture;
    }

    private static Texture2D MakeCircleTexture(int size, Color color, bool soft)
    {
        Texture2D texture = NewClearTexture(size, size);
        Vector2 center = new Vector2(size * 0.5f, size * 0.5f);
        float radius = size * 0.46f;
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float distance = Vector2.Distance(new Vector2(x, y), center);
                float alpha = soft ? Mathf.Clamp01((radius - distance) / 20f) : (distance <= radius ? 1f : 0f);
                texture.SetPixel(x, y, new Color(color.r, color.g, color.b, color.a * alpha));
            }
        }
        texture.Apply();
        return texture;
    }

    private static Texture2D MakeBasketballTexture(int size)
    {
        Texture2D texture = NewClearTexture(size, size);
        Vector2 center = new Vector2(size * 0.5f, size * 0.5f);
        float radius = size * 0.46f;
        Color seam = Hex("#4A1A08");
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float distance = Vector2.Distance(new Vector2(x, y), center);
                if (distance > radius)
                {
                    texture.SetPixel(x, y, Color.clear);
                    continue;
                }

                float shade = 1f - Mathf.Clamp01(distance / radius) * 0.2f;
                Color pixel = new Color(MainOrange.r * shade, MainOrange.g * shade, MainOrange.b * shade, 1f);
                bool line = Mathf.Abs(x - center.x) < 3f || Mathf.Abs(y - center.y) < 3f;
                bool curveA = Mathf.Abs((x - center.x) * 0.5f + Mathf.Sin((y - center.y) / 25f) * 21f) < 3.5f;
                bool curveB = Mathf.Abs((x - center.x) * 0.5f - Mathf.Sin((y - center.y) / 25f) * 21f) < 3.5f;
                texture.SetPixel(x, y, line || curveA || curveB ? seam : pixel);
            }
        }
        texture.Apply();
        return texture;
    }

    private static Texture2D MakeCoinTexture(int size)
    {
        Texture2D texture = MakeCircleTexture(size, LightOrange, false);
        Vector2 center = new Vector2(size * 0.5f, size * 0.5f);
        DrawRing(texture, center, size * 0.35f, size * 0.04f, MainOrange);
        DrawStar(texture, center, size * 0.19f, CreamWhite);
        texture.Apply();
        return texture;
    }

    private static Texture2D MakeFlashTexture(int size)
    {
        Texture2D texture = NewClearTexture(size, size);
        Vector2 center = new Vector2(size * 0.5f, size * 0.5f);
        for (int i = 0; i < 8; i++)
        {
            float angle = i * Mathf.PI * 2f / 8f;
            Vector2 end = center + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * size * 0.44f;
            DrawLine(texture, center, end, Color.white, 4f);
        }
        texture.Apply();
        return texture;
    }

    private static Texture2D MakePowerIcon(int size)
    {
        Texture2D texture = NewClearTexture(size, size);
        Vector2 center = new Vector2(size * 0.5f, size * 0.48f);
        DrawRing(texture, center, size * 0.30f, 7f, Color.white);
        DrawLine(texture, new Vector2(size * 0.5f, size * 0.78f), new Vector2(size * 0.5f, size * 0.46f), Color.white, 9f);
        texture.Apply();
        return texture;
    }

    private static Texture2D MakeSpeakerIcon(int size)
    {
        Texture2D texture = NewClearTexture(size, size);
        FillRect(texture, 24, 48, 24, 32, Color.white);
        FillTriangle(texture, new Vector2(46, 48), new Vector2(76, 28), new Vector2(76, 100), Color.white);
        DrawArc(texture, new Vector2(70, 64), 28, -45, 45, Color.white, 5f);
        DrawArc(texture, new Vector2(70, 64), 43, -45, 45, Color.white, 5f);
        texture.Apply();
        return texture;
    }

    private static Texture2D MakeGearIcon(int size)
    {
        Texture2D texture = NewClearTexture(size, size);
        Vector2 center = new Vector2(size * 0.5f, size * 0.5f);
        for (int i = 0; i < 8; i++)
        {
            float angle = i * Mathf.PI * 2f / 8f;
            Vector2 p = center + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * 39f;
            FillCircle(texture, p, 9f, Color.white);
        }
        DrawRing(texture, center, 31f, 12f, Color.white);
        DrawRing(texture, center, 12f, 5f, Color.white);
        texture.Apply();
        return texture;
    }

    private static Texture2D MakeMapIcon(int size)
    {
        Texture2D texture = NewClearTexture(size, size);
        DrawLine(texture, new Vector2(22, 35), new Vector2(48, 24), Color.white, 5f);
        DrawLine(texture, new Vector2(48, 24), new Vector2(78, 36), Color.white, 5f);
        DrawLine(texture, new Vector2(78, 36), new Vector2(106, 25), Color.white, 5f);
        DrawLine(texture, new Vector2(22, 35), new Vector2(22, 96), Color.white, 5f);
        DrawLine(texture, new Vector2(48, 24), new Vector2(48, 86), Color.white, 5f);
        DrawLine(texture, new Vector2(78, 36), new Vector2(78, 99), Color.white, 5f);
        DrawLine(texture, new Vector2(106, 25), new Vector2(106, 88), Color.white, 5f);
        DrawLine(texture, new Vector2(22, 96), new Vector2(48, 86), Color.white, 5f);
        DrawLine(texture, new Vector2(48, 86), new Vector2(78, 99), Color.white, 5f);
        DrawLine(texture, new Vector2(78, 99), new Vector2(106, 88), Color.white, 5f);
        texture.Apply();
        return texture;
    }

    private static Texture2D MakeCartIcon(int size)
    {
        Texture2D texture = NewClearTexture(size, size);
        DrawLine(texture, new Vector2(25, 34), new Vector2(40, 34), Color.white, 7f);
        DrawLine(texture, new Vector2(40, 34), new Vector2(50, 78), Color.white, 7f);
        DrawLine(texture, new Vector2(50, 78), new Vector2(98, 78), Color.white, 7f);
        DrawLine(texture, new Vector2(48, 49), new Vector2(104, 49), Color.white, 7f);
        DrawLine(texture, new Vector2(104, 49), new Vector2(96, 78), Color.white, 7f);
        FillCircle(texture, new Vector2(58, 93), 8f, Color.white);
        FillCircle(texture, new Vector2(92, 93), 8f, Color.white);
        texture.Apply();
        return texture;
    }

    private static Texture2D NewClearTexture(int width, int height)
    {
        Texture2D texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                texture.SetPixel(x, y, Color.clear);
            }
        }
        return texture;
    }

    private static void FillRect(Texture2D texture, int x, int y, int width, int height, Color color)
    {
        for (int py = y; py < y + height; py++)
        {
            for (int px = x; px < x + width; px++)
            {
                SetPixelSafe(texture, px, py, color);
            }
        }
    }

    private static void FillTriangle(Texture2D texture, Vector2 a, Vector2 b, Vector2 c, Color color)
    {
        for (int y = 0; y < texture.height; y++)
        {
            for (int x = 0; x < texture.width; x++)
            {
                Vector2 p = new Vector2(x, y);
                float area = TriangleArea(a, b, c);
                float a1 = TriangleArea(p, b, c);
                float a2 = TriangleArea(a, p, c);
                float a3 = TriangleArea(a, b, p);
                if (Mathf.Abs(area - (a1 + a2 + a3)) < 0.5f)
                {
                    texture.SetPixel(x, y, color);
                }
            }
        }
    }

    private static float TriangleArea(Vector2 a, Vector2 b, Vector2 c)
    {
        return Mathf.Abs((a.x * (b.y - c.y) + b.x * (c.y - a.y) + c.x * (a.y - b.y)) * 0.5f);
    }

    private static void FillCircle(Texture2D texture, Vector2 center, float radius, Color color)
    {
        for (int y = Mathf.FloorToInt(center.y - radius); y <= Mathf.CeilToInt(center.y + radius); y++)
        {
            for (int x = Mathf.FloorToInt(center.x - radius); x <= Mathf.CeilToInt(center.x + radius); x++)
            {
                if (Vector2.Distance(new Vector2(x, y), center) <= radius)
                {
                    SetPixelSafe(texture, x, y, color);
                }
            }
        }
    }

    private static void DrawRing(Texture2D texture, Vector2 center, float radius, float thickness, Color color)
    {
        for (int y = 0; y < texture.height; y++)
        {
            for (int x = 0; x < texture.width; x++)
            {
                float distance = Vector2.Distance(new Vector2(x, y), center);
                if (Mathf.Abs(distance - radius) <= thickness)
                {
                    texture.SetPixel(x, y, color);
                }
            }
        }
    }

    private static void DrawStar(Texture2D texture, Vector2 center, float radius, Color color)
    {
        Vector2 previous = center + Vector2.up * radius;
        for (int i = 1; i <= 5; i++)
        {
            float angle = Mathf.Deg2Rad * (90f + i * 144f);
            Vector2 next = center + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius;
            DrawLine(texture, previous, next, color, 4f);
            previous = next;
        }
    }

    private static void DrawArc(Texture2D texture, Vector2 center, float radius, float startDegrees, float endDegrees, Color color, float thickness)
    {
        Vector2 previous = center + DegreeVector(startDegrees) * radius;
        for (int i = 1; i <= 24; i++)
        {
            float t = i / 24f;
            float angle = Mathf.Lerp(startDegrees, endDegrees, t);
            Vector2 next = center + DegreeVector(angle) * radius;
            DrawLine(texture, previous, next, color, thickness);
            previous = next;
        }
    }

    private static Vector2 DegreeVector(float degrees)
    {
        float radians = degrees * Mathf.Deg2Rad;
        return new Vector2(Mathf.Cos(radians), Mathf.Sin(radians));
    }

    private static void DrawLine(Texture2D texture, Vector2 a, Vector2 b, Color color, float thickness)
    {
        int steps = Mathf.CeilToInt(Vector2.Distance(a, b));
        if (steps <= 0)
        {
            return;
        }

        for (int i = 0; i <= steps; i++)
        {
            Vector2 p = Vector2.Lerp(a, b, i / (float)steps);
            for (int y = -Mathf.CeilToInt(thickness); y <= Mathf.CeilToInt(thickness); y++)
            {
                for (int x = -Mathf.CeilToInt(thickness); x <= Mathf.CeilToInt(thickness); x++)
                {
                    if (x * x + y * y <= thickness * thickness)
                    {
                        SetPixelSafe(texture, Mathf.RoundToInt(p.x + x), Mathf.RoundToInt(p.y + y), color);
                    }
                }
            }
        }
    }

    private static void SetPixelSafe(Texture2D texture, int x, int y, Color color)
    {
        if (x >= 0 && x < texture.width && y >= 0 && y < texture.height)
        {
            texture.SetPixel(x, y, color);
        }
    }

    private static Color Hex(string value)
    {
        ColorUtility.TryParseHtmlString(value, out Color color);
        return color;
    }
}
#endif
