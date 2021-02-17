using UnityEngine;
using UnityEditor;
using System.IO;

public class TextureCreatorWindow : EditorWindow
{
    string savingFolder = "SavedTextures";
    float windowPadding = 100;

    string fileName = "myProceduralTexture";
    float perlinXScale;
    float perlinYScale;
    int perlinOctaves;
    float perlinPersistence;
    float perlinHeightScale;
    int PerlinOffsetX;
    int perlinOffsetY;
    bool alphaToggle = false;
    bool seamlessToggle = false;
    bool remapValuesToggle = false;

    float brightness = 0.5f;
    float contrast = 0.5f;

    Texture2D proceduralTexture;

    [MenuItem("Window/Texture Creator Window")]
    public static void ShowWindow()
    {
        EditorWindow.GetWindow(typeof(TextureCreatorWindow));
    }

    private void OnEnable()
    {
        proceduralTexture = new Texture2D(513, 513, TextureFormat.ARGB32, false);
    }

    private void OnGUI()
    {
        GUILayout.Label("Settings", EditorStyles.largeLabel);

        fileName = EditorGUILayout.TextField("Texture Name", fileName);

        int windowWidth = (int)(EditorGUIUtility.currentViewWidth - windowPadding);

        perlinXScale = EditorGUILayout.Slider("X Scale", perlinXScale, 0, 0.1f);
        perlinYScale = EditorGUILayout.Slider("Y Scale", perlinYScale, 0, 0.1f);
        perlinOctaves = EditorGUILayout.IntSlider("Octaves", perlinOctaves, 0, 10);
        perlinPersistence = EditorGUILayout.Slider("Persistance", perlinPersistence, 1f, 10f);
        perlinHeightScale = EditorGUILayout.Slider("Height Scale", perlinHeightScale, 0f, 1f);
        PerlinOffsetX = EditorGUILayout.IntSlider("X Offset", PerlinOffsetX, 0, 1000);
        perlinOffsetY = EditorGUILayout.IntSlider("Y Offset", perlinOffsetY, 0, 1000);

        GUILayout.Space(10);

        alphaToggle = EditorGUILayout.Toggle("Alpha", alphaToggle);
        seamlessToggle = EditorGUILayout.Toggle("Seamless", seamlessToggle);
        remapValuesToggle = EditorGUILayout.Toggle("Remap Values", remapValuesToggle);

        GUILayout.Space(10);

        brightness = EditorGUILayout.Slider("Brightness", brightness, 0f, 2f);
        contrast = EditorGUILayout.Slider("Contrast", contrast, 0f, 2f);

        GUILayout.Space(10);

        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        if (GUILayout.Button("Generate", GUILayout.Width(windowWidth)))
        {
            GenerateTexture();
        }
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        GUILayout.Label(proceduralTexture, GUILayout.Width(windowWidth), GUILayout.Height(windowWidth));
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        if (GUILayout.Button("Save", GUILayout.Width(windowWidth)))
        {
            SaveTexture();
        }
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();

    }


    private void GenerateTexture()
    {
        int width = 513;
        int height = 513;
        float pixelValue;
        Color pixelColour = Color.white;

        float minColour = 1;
        float maxColour = 0;

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (seamlessToggle)
                {
                    float horizontalPosition = (float)x / (float)width;
                    float verticalPosition = (float)y / (float)height;

                    float noise00 = Utils.FractalBrownianMotion((x + PerlinOffsetX) * perlinXScale,
                                                                (y + perlinOffsetY) * perlinYScale,
                                                                perlinOctaves,
                                                                perlinPersistence)
                                                                * perlinHeightScale;

                    float noise10 = Utils.FractalBrownianMotion((x + PerlinOffsetX + width) * perlinXScale,
                                                                (y + perlinOffsetY) * perlinYScale,
                                                                perlinOctaves,
                                                                perlinPersistence)
                                                                * perlinHeightScale;

                    float noise01 = Utils.FractalBrownianMotion((x + PerlinOffsetX) * perlinXScale,
                                                                (y + perlinOffsetY + height) * perlinYScale,
                                                                perlinOctaves,
                                                                perlinPersistence)
                                                                * perlinHeightScale;

                    float noise11 = Utils.FractalBrownianMotion((x + PerlinOffsetX + width) * perlinXScale,
                                                                (y + perlinOffsetY + height) * perlinYScale,
                                                                perlinOctaves,
                                                                perlinPersistence)
                                                                * perlinHeightScale;

                    pixelValue = noise00 * horizontalPosition * verticalPosition
                                    + noise10 * (1 - horizontalPosition) * verticalPosition
                                    + noise01 * horizontalPosition * (1 - verticalPosition)
                                    + noise11 * (1 - horizontalPosition) * (1 - verticalPosition);
                }
                else
                {
                    pixelValue = Utils.FractalBrownianMotion((x + PerlinOffsetX) * perlinXScale,
                                         (y + perlinOffsetY) * perlinYScale,
                                         perlinOctaves,
                                         perlinPersistence)
                                         * perlinHeightScale;

                }
                // for mapping function
                if (minColour > pixelValue) { minColour = pixelValue; }
                if (maxColour < pixelValue) { maxColour = pixelValue; }

                float colourValue = (pixelValue - 0.5f) * contrast
                                    + 0.5f * brightness;              // given equation to implement brightness/contrast

                pixelColour = new Color(colourValue, colourValue, colourValue, alphaToggle ? colourValue : 1);
                proceduralTexture.SetPixel(x, y, pixelColour);
            }
        }

        if (remapValuesToggle)
        {
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    pixelColour = proceduralTexture.GetPixel(x, y);
                    float colourValue = Utils.Map(pixelColour.r,
                                                  minColour, maxColour,
                                                  0, 1);
                    pixelColour = new Color(colourValue, colourValue, colourValue, alphaToggle ? colourValue : 1);
                    proceduralTexture.SetPixel(x, y, pixelColour);
                }
            }
        }

        proceduralTexture.Apply(false, false);
    }

    private void SaveTexture()
    {
        string savingPath = System.IO.Path.Combine(Application.dataPath, savingFolder);
        byte[] bytes = proceduralTexture.EncodeToPNG();
        System.IO.Directory.CreateDirectory(savingPath);

        string filePath = System.IO.Path.Combine(savingPath, fileName + ".png");
        File.WriteAllBytes(filePath, bytes);
    }
}
