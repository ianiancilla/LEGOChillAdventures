using UnityEngine;
using UnityEditor;
using EditorGUITable;
using System.IO;

[CustomEditor(typeof(CustomTerrain))]
[CanEditMultipleObjects]

public class CustomTerrainEditor : Editor
{
    #region properties
    int windowWidth;

    // foldouts ------------------------------
    bool showTerrainHeight, showRandom, showLoadHeightMapImage, showAddSineWave, showPerlinHeight, showfBMHeight,
        showMultiplefBM, showVoronoi, showMPD, showSmoothing, showExtractHeightMap = false;
    bool showErosion, showErosionWind = false;
    bool showTerrainTextures = false;
    bool showVegetation = false;
    bool showDetails = false;
    bool showWater = false;

    // TERRAIN HEIGHT PROPERTIES -----------------------------------------------------------------------
    SerializedProperty resetHeightMapOnChange;
    // height randomiser -------------------------------------------------------
    SerializedProperty randomHeightRange;
    // heigtmap image loader ---------------------------------------------------
    SerializedProperty heightMapScale;
    SerializedProperty heightMapImage;
    // sine wave height --------------------------------------------------------
    SerializedProperty sinAmplitude;
    SerializedProperty sineOffsetX;
    SerializedProperty sinWavelength;
    SerializedProperty sineOffsetY;
    SerializedProperty xValueForSine;
    // perlin noise height -----------------------------------------------------
    SerializedProperty perlinXScale;
    SerializedProperty perlinZScale;
    SerializedProperty perlinYScale;
    SerializedProperty perlinOffsetX;
    SerializedProperty perlinOffsetZ;
    // Fractal Brownian Motion Height-------------------------------------------
    SerializedProperty fBMOctaves;
    SerializedProperty fBMPersistence;
    SerializedProperty fBMXScale;
    SerializedProperty fBMZScale;
    SerializedProperty fBMOffsetX;
    SerializedProperty fBMOffsetZ;
    SerializedProperty fBMFrequencyMultiplier;
    SerializedProperty fBMHeighScaler;
    // Multiple Fractal Brownian Motion Height-------------------------------------------
    GUITableState multiplefBMTable;
    SerializedProperty multiplefBMParameters;
    // Voronoi --------------------------------------------------------------------------
    SerializedProperty voronoiHeightRange;
    SerializedProperty voronoiSteepness;
    SerializedProperty voronoiCurvature;
    SerializedProperty voronoiPeakCount;
    SerializedProperty voronoiType;
    // Mid Point Displacement -----------------------------------------------------------
    SerializedProperty mpdSmoothness;
    SerializedProperty hmpdHeightRandomFactor;
    SerializedProperty mpdDampenerBase;
    SerializedProperty mpdMinTerrainHeight;
    SerializedProperty mpdMaxTerrainHeight;
    // Smoothing ------------------------------------------------------------------------
    SerializedProperty smothingFactor;
    // Extract Heightmap ----------------------------------------------------------------
    Texture2D currentHeightMapTexture;
    string saveMapDirectoryPath = "SavedTextures";
    string saveMapFileName = "filename";

    // EROSION PROPERTIES ------------------------------------------------------------------------------
    SerializedProperty erosionType;
    SerializedProperty erosionSmoothing;
    // Rain -----------------------------------------------------------------------------
    SerializedProperty erosionRainDroplets;
    SerializedProperty erosionRainStrength;
    // river
    SerializedProperty erosionRiverDroplets;
    SerializedProperty erosionRiverStrength;
    SerializedProperty erosionRiverPasses;
    SerializedProperty erosionRiverSolubility;
    // slides
    SerializedProperty erosionSlidesStrength;
    SerializedProperty erosionSlidesMinimum;
    SerializedProperty erosionSlidesPasses;
    // wind
    SerializedProperty erosionWindStrength;
    SerializedProperty erosionWindStepWidth;
    SerializedProperty erosionWindNoiseX;
    SerializedProperty erosionWindNoiseZ;
    SerializedProperty erosionWindNoiseAmplitude;
    SerializedProperty erosionWindDirection;
    // canyon
    SerializedProperty erosionCanyonBottomHeight;
    SerializedProperty erosionCanyonMeandering;
    SerializedProperty erosionCanyonBottomSteepness;
    SerializedProperty erosionCanyonBottomWidth;
    SerializedProperty erosionCanyonTopStepHeight;
    SerializedProperty erosionCanyonStepNumber;
    SerializedProperty erosionCanyonMaxStepWidth;



    // TEXTURE PROPERTIES ------------------------------------------------------------------------------
    GUITableState textureTable;
    SerializedProperty textureParametersList;
    SerializedProperty terrainLayersBlending;
    SerializedProperty terrainLayersNoiseFrequency;
    SerializedProperty terrainLayersNoiseFactor;

    // VEGETATION PROPERTIES ---------------------------------------------------------------------------
    GUITableState vegetationTable;
    SerializedProperty vegetationTypes;
    SerializedProperty vegMaxTreeNumber;
    SerializedProperty vegTreeSpacing;

    // DETAIL PROPERTIES -------------------------------------------------------------------------------
    GUITableState detailTable;
    SerializedProperty detailTypes;
    SerializedProperty detailMaxNumber;
    SerializedProperty detailSpacing;

    // WATER PROPERTIES --------------------------------------------------------------------------------
    SerializedProperty waterHeight;
    SerializedProperty waterGO;
    SerializedProperty waterShoreMaterial;
    SerializedProperty waterShoreThickness;
    #endregion

    private void OnEnable()
    {
        // TERRAIN HEIGHT PROPERTIES -----------------------------------------------------------------------
        // height reser checkbox
        resetHeightMapOnChange = serializedObject.FindProperty("resetHeightMapOnChange");

        // height randomiser -------
        randomHeightRange = serializedObject.FindProperty("randomHeightRange");
        
        // heigtmap image loader ---
        heightMapScale = serializedObject.FindProperty("heightMapScale");
        heightMapImage = serializedObject.FindProperty("heightMapImage");

        // sine wave height --------
        sinAmplitude = serializedObject.FindProperty("sinAmplitude");
        sineOffsetX = serializedObject.FindProperty("sineOffsetX");
        sinWavelength = serializedObject.FindProperty("sinWavelength");
        sineOffsetY = serializedObject.FindProperty("sineOffsetY");
        xValueForSine = serializedObject.FindProperty("xValueForSine");

        // perlin noise height -----
        perlinXScale = serializedObject.FindProperty("perlinXScale");
        perlinZScale = serializedObject.FindProperty("perlinZScale");
        perlinYScale = serializedObject.FindProperty("perlinYScale");
        perlinOffsetX = serializedObject.FindProperty("perlinOffsetX");
        perlinOffsetZ = serializedObject.FindProperty("perlinOffsetZ");

        // Fractal Brownian Motion Height-------------------------------------------
        fBMOctaves = serializedObject.FindProperty("fBMOctaves");
        fBMPersistence = serializedObject.FindProperty("fBMPersistence");
        fBMXScale = serializedObject.FindProperty("fBMXScale");
        fBMZScale = serializedObject.FindProperty("fBMZScale");
        fBMOffsetX = serializedObject.FindProperty("fBMOffsetX");
        fBMOffsetZ = serializedObject.FindProperty("fBMOffsetZ");
        fBMFrequencyMultiplier = serializedObject.FindProperty("fBMFrequencyMultiplier");
        fBMHeighScaler = serializedObject.FindProperty("fBMHeighScaler");

        // Multiple Fractal Brownian Motion Height----------------------------------
        multiplefBMTable = new GUITableState("multiplefBMTable");
        multiplefBMParameters = serializedObject.FindProperty("multiplefBMParameters");

        // Voronoi --------------------------------------------------------------------------
        voronoiHeightRange = serializedObject.FindProperty("voronoiHeightRange");
        voronoiSteepness = serializedObject.FindProperty("VoronoiSteepness");
        voronoiCurvature = serializedObject.FindProperty("voronoiCurvature");
        voronoiPeakCount = serializedObject.FindProperty("voronoiPeakCount");
        voronoiType = serializedObject.FindProperty("voronoiType");

        // Mid Point Displacement -----------------------------------------------------------
        mpdSmoothness = serializedObject.FindProperty("mpdSmoothness");
        hmpdHeightRandomFactor = serializedObject.FindProperty("hmpdHeightRandomFactor");
        mpdDampenerBase = serializedObject.FindProperty("mpdDampenerBase");
        mpdMinTerrainHeight = serializedObject.FindProperty("mpdMinTerrainHeight");
        mpdMaxTerrainHeight = serializedObject.FindProperty("mpdMaxTerrainHeight");

        // Smoothing ------------------------------------------------------------------------
        smothingFactor = serializedObject.FindProperty("smoothingFactor");

        // Extract Heightmap ----------------------------------------------------------------
        windowWidth = (int)(EditorGUIUtility.currentViewWidth);
        currentHeightMapTexture = new Texture2D(513, 513, TextureFormat.ARGB32, false);

        // EROSION PROPERTIES ------------------------------------------------------------------------------
        erosionType = serializedObject.FindProperty("erosionType");
        erosionSmoothing = serializedObject.FindProperty("erosionSmoothing");
        // Rain -----------------------------------------------------------------------------
        erosionRainDroplets = serializedObject.FindProperty("erosionRainDroplets");
        erosionRainStrength = serializedObject.FindProperty("erosionRainStrength");
        // river
        erosionRiverDroplets = serializedObject.FindProperty("erosionRiverDroplets");
        erosionRiverStrength = serializedObject.FindProperty("erosionRiverStrength");
        erosionRiverPasses = serializedObject.FindProperty("erosionRiverPasses");
        erosionRiverSolubility = serializedObject.FindProperty("erosionRiverSolubility");
        // slides
        erosionSlidesStrength = serializedObject.FindProperty("erosionSlidesStrength");
        erosionSlidesMinimum = serializedObject.FindProperty("erosionSlidesMinimum");
        erosionSlidesPasses = serializedObject.FindProperty("erosionSlidesPasses");
        // wind
        erosionWindStrength = serializedObject.FindProperty("erosionWindStrength");
        erosionWindStepWidth = serializedObject.FindProperty("erosionWindStepWidth");
        erosionWindNoiseX = serializedObject.FindProperty("erosionWindNoiseX");
        erosionWindNoiseZ = serializedObject.FindProperty("erosionWindNoiseZ");
        erosionWindNoiseAmplitude = serializedObject.FindProperty("erosionWindNoiseAmplitude");
        erosionWindDirection = serializedObject.FindProperty("erosionWindDirection");
        // canyon
        erosionCanyonBottomHeight = serializedObject.FindProperty("erosionCanyonBottomHeight");
        erosionCanyonMeandering = serializedObject.FindProperty("erosionCanyonMeandering");
        erosionCanyonBottomSteepness = serializedObject.FindProperty("erosionCanyonBottomSteepness");
        erosionCanyonBottomWidth = serializedObject.FindProperty("erosionCanyonBottomWidth");
        erosionCanyonTopStepHeight = serializedObject.FindProperty("erosionCanyonTopStepHeight");
        erosionCanyonStepNumber = serializedObject.FindProperty("erosionCanyonStepNumber");
        erosionCanyonMaxStepWidth = serializedObject.FindProperty("erosionCanyonMaxStepWidth");


        // TEXTURE PROPERTIES ------------------------------------------------------------------------------
        textureTable = new GUITableState("textureTable");
        textureParametersList = serializedObject.FindProperty("textureParametersList");
        terrainLayersBlending = serializedObject.FindProperty("terrainLayersBlending");
        terrainLayersNoiseFrequency = serializedObject.FindProperty("terrainLayersNoiseFrequency");
        terrainLayersNoiseFactor = serializedObject.FindProperty("terrainLayersNoiseFactor");

        // VEGETATION PROPERTIES ---------------------------------------------------------------------------
        vegetationTable = new GUITableState("vegetationTable");
        vegetationTypes = serializedObject.FindProperty("vegetationTypes");
        vegMaxTreeNumber = serializedObject.FindProperty("vegMaxTreeNumber");
        vegTreeSpacing = serializedObject.FindProperty("vegTreeSpacing");

        // DETAIL PROPERTIES -------------------------------------------------------------------------------
        detailTable = new GUITableState("detailTable");
        detailTypes = serializedObject.FindProperty("detailTypes");
        detailMaxNumber = serializedObject.FindProperty("detailMaxNumber");
        detailSpacing = serializedObject.FindProperty("detailSpacing");

        // WATER PROPERTIES --------------------------------------------------------------------------------
        waterHeight = serializedObject.FindProperty("waterHeight");
        waterGO = serializedObject.FindProperty("waterGO");
        waterShoreMaterial = serializedObject.FindProperty("waterShoreMaterial");
        waterShoreThickness = serializedObject.FindProperty("waterShoreThickness");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        CustomTerrain myCustomTerrain = (CustomTerrain)target;

        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
        
        GUILayout.Space(10);
        TerrainHeightOptions(myCustomTerrain);
        GUILayout.Space(10);

        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

        GUILayout.Space(10);
        ErosionOptions(myCustomTerrain);
        GUILayout.Space(10);

        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

        GUILayout.Space(10);
        TextureOptions(myCustomTerrain);
        GUILayout.Space(10);

        GUILayout.Space(10);
        VegetationOptions(myCustomTerrain);
        GUILayout.Space(10);

        GUILayout.Space(10);
        DetailsOptions(myCustomTerrain);
        GUILayout.Space(10);

        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

        GUILayout.Space(10);
        WaterOptions(myCustomTerrain);
        GUILayout.Space(10);

        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

        serializedObject.ApplyModifiedProperties();
    }

    #region Terrain Height section
    private void TerrainHeightOptions(CustomTerrain myCustomTerrain)
    {
        showTerrainHeight = EditorGUILayout.Foldout(showTerrainHeight, "Terrain Height Options");
        GUILayout.Label("Options to shape the terrain mesh via height map", EditorStyles.label);

        if (showTerrainHeight)
        {
            EditorGUI.indentLevel++;
            GUILayout.Space(10);
            FlattenTerrain(myCustomTerrain);
            ResetTerrainCheckbox();
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            LoadHeightmapImage(myCustomTerrain);
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            HeightRandomiser(myCustomTerrain);
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            SineWaveHeight(myCustomTerrain);
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            PerlinHeight(myCustomTerrain);
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            fBMHeight(myCustomTerrain);
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            MultiplefBM(myCustomTerrain);
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            Voronoi(myCustomTerrain);
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            MidPointDisplacement(myCustomTerrain);
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            Smoothing(myCustomTerrain);
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            ShowAndSaveTexture(myCustomTerrain);
            EditorGUI.indentLevel--;
        }
    }
    private void ResetTerrainCheckbox()
    {
        GUILayout.Space(10);
        EditorGUILayout.PropertyField(resetHeightMapOnChange, new GUIContent("Reset on change"));
    }
    private void FlattenTerrain(CustomTerrain myCustomTerrain)
    {
        GUILayout.Space(10);
        if (GUILayout.Button("Flatten Terrain"))
        {
            myCustomTerrain.FlattenTerrain();
        }
    }
    private void HeightRandomiser(CustomTerrain myCustomTerrain)
    {
        showRandom = EditorGUILayout.Foldout(showRandom, "Random");
        GUILayout.Label("Add heights between random values", EditorStyles.label);

        if (showRandom)
        {
            GUILayout.Space(10);
            //EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            EditorGUILayout.PropertyField(randomHeightRange);
            GUILayout.Space(10);
            if (GUILayout.Button("Add Randomised Height Values"))
            {
                myCustomTerrain.RandomiseTerrain();
            }
        }
    }
    private void LoadHeightmapImage(CustomTerrain myCustomTerrain)
    {
        showLoadHeightMapImage = EditorGUILayout.Foldout(showLoadHeightMapImage, "Load Image");
        GUILayout.Label("Load heights from 2D texture", EditorStyles.label);

        if (showLoadHeightMapImage)
        {
            GUILayout.Space(10);
            //EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            EditorGUILayout.PropertyField(heightMapImage);
            GUILayout.Label("!!! Remember to enable read/write for the texture !!!", EditorStyles.largeLabel);
            EditorGUILayout.PropertyField(heightMapScale);
            GUILayout.Space(10);
            if (GUILayout.Button("Load Texture"))
            {
                myCustomTerrain.LoadTextureHeightMap();
            }
        }
    }
    private void SineWaveHeight(CustomTerrain myCustomTerrain)
    {
        showAddSineWave = EditorGUILayout.Foldout(showAddSineWave, "Sine Wave Height");
        GUILayout.Label("Add or remove value of selected sine wave", EditorStyles.label);

        if (showAddSineWave)
        {
            GUILayout.Space(10);
            //EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            EditorGUILayout.PropertyField(xValueForSine);
            EditorGUILayout.Slider(sinAmplitude, 0, 1, new GUIContent("Amplitude"));
            EditorGUILayout.PropertyField(sinWavelength);
            EditorGUILayout.PropertyField(sineOffsetX);
            EditorGUILayout.Slider(sineOffsetY, -1, 1, new GUIContent("Vertical Offset"));
            GUILayout.Space(10);
            if (GUILayout.Button("Add Sine Wave"))
            {
                myCustomTerrain.AddSineWaveHeight(CustomTerrain.applyMode.add);
            }
            if (GUILayout.Button("Remove Sine Wave"))
            {
                myCustomTerrain.AddSineWaveHeight(CustomTerrain.applyMode.remove);
            }
        }
    }
    private void PerlinHeight(CustomTerrain myCustomTerrain)
    {
        showPerlinHeight = EditorGUILayout.Foldout(showPerlinHeight, "Perlin noise Height");
        GUILayout.Label("Add or remove specified perlin noise value", EditorStyles.label);

        if (showPerlinHeight)
        {
            GUILayout.Space(10);
            //EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            EditorGUILayout.Slider(perlinXScale, 0, 0.4f, new GUIContent("X Scale"));
            EditorGUILayout.Slider(perlinZScale, 0, 0.4f, new GUIContent("Z Scale"));
            EditorGUILayout.Slider(perlinYScale, 0, 1f, new GUIContent("Y Scale"));
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            EditorGUILayout.IntSlider(perlinOffsetX, 0, 1000, new GUIContent("X Offset"));
            EditorGUILayout.IntSlider(perlinOffsetZ, 0, 1000, new GUIContent("Z Offset"));
            GUILayout.Space(10);
            if (GUILayout.Button("Add Perlin Noise"))
            {
                myCustomTerrain.AddPerlinHeight(CustomTerrain.applyMode.add);
            }
            if (GUILayout.Button("Remove Perlin Noise"))
            {
                myCustomTerrain.AddPerlinHeight(CustomTerrain.applyMode.remove);
            }
        }
    }
    private void fBMHeight(CustomTerrain myCustomTerrain)
    {
        showfBMHeight = EditorGUILayout.Foldout(showfBMHeight, "Fractal Brownian Motion Height");
        GUILayout.Label("Add or remove specified fBM noise value", EditorStyles.label);

        if (showfBMHeight)
        {
            //EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            GUILayout.Space(10);
            EditorGUILayout.IntSlider(fBMOctaves, 1, 10, new GUIContent("Number of Octaves"));
            EditorGUILayout.Slider(fBMPersistence, 0.01f, 2f, new GUIContent("Persistence",
                                                                               "Of following octaves"));
            EditorGUILayout.Slider(fBMFrequencyMultiplier, 0.1f, 8f, new GUIContent("Frequency multiplier",
                                                                                      "For each following octave"));
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            EditorGUILayout.Slider(fBMXScale, 0, 0.4f, new GUIContent("X Scale"));
            EditorGUILayout.Slider(fBMZScale, 0, 0.4f, new GUIContent("Z Scale"));
            EditorGUILayout.Slider(fBMHeighScaler, 0, 1f, new GUIContent("Y Scale", 
                                                                         "For total height"));
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            EditorGUILayout.IntSlider(fBMOffsetX, 0, 10000, new GUIContent("X Offset"));
            EditorGUILayout.IntSlider(fBMOffsetZ, 0, 10000, new GUIContent("Z Offset"));
            GUILayout.Space(10);
            if (GUILayout.Button("Add fBM"))
            {
                myCustomTerrain.AddfBMHeight(CustomTerrain.applyMode.add);
            }
            if (GUILayout.Button("Remove fBM"))
            {
                myCustomTerrain.AddfBMHeight(CustomTerrain.applyMode.remove);
            }
        }
    }
    private void MultiplefBM(CustomTerrain myCustomTerrain)
    {
        showMultiplefBM = EditorGUILayout.Foldout(showMultiplefBM, "Multiple fBM additions");
        GUILayout.Label("Add multiple lines of Fractal Brownian Motion", EditorStyles.label);

        if (showMultiplefBM)
        {
            GUILayout.Space(10);
            //EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            multiplefBMTable = GUITableLayout.DrawTable(multiplefBMTable,
                                                        multiplefBMParameters);

            GUILayout.Space(40);
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("+"))
            { myCustomTerrain.AddfBMRow(); }
            if (GUILayout.Button("-"))
            { myCustomTerrain.RemovefBMRow(); }

            EditorGUILayout.EndHorizontal();
            if (GUILayout.Button("Apply multiple fBM"))
            { myCustomTerrain.MultiplefBMTerrain(); }
            GUILayout.Space(10);
        }
    }
    private void Voronoi(CustomTerrain myCustomTerrain)
    {
        showVoronoi = EditorGUILayout.Foldout(showVoronoi, "Voronoi");
        GUILayout.Label("Model random voronoi peaks", EditorStyles.label);

        if (showVoronoi)
        {
            GUILayout.Space(10);
            //EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            EditorGUILayout.PropertyField(voronoiType);
            EditorGUILayout.PropertyField(voronoiHeightRange);
            EditorGUILayout.IntSlider(voronoiPeakCount, 0, 20, new GUIContent("Number of peaks"));
            EditorGUILayout.Slider(voronoiSteepness, 0.0001f, 10f, new GUIContent("Steepness",
                                                                                "Lower values make for steeper sides"));
            EditorGUILayout.Slider(voronoiCurvature, 0.01f, 10f, new GUIContent("Curvature",
                                                                                "Above 0 is convex, below is concave"));
            GUILayout.Space(10);
            if (GUILayout.Button("Add voronoi peaks"))
            {
                myCustomTerrain.Voronoi();
            }
        }

    }
    private void MidPointDisplacement(CustomTerrain myCustomTerrain)
    {
        showMPD = EditorGUILayout.Foldout(showMPD, "Mid Point Displacement");
        GUILayout.Label("Model through Mid Point Displacement", EditorStyles.label);

        if (showMPD)
        {
            GUILayout.Space(10);
            //EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            EditorGUILayout.Slider(mpdMinTerrainHeight, 0f, 1f, new GUIContent("Min Terrain Height"));
            EditorGUILayout.Slider(mpdMaxTerrainHeight, 0f, 1f, new GUIContent("Max Terrain Height"));
            EditorGUILayout.Slider(mpdSmoothness, 0.8f, 5f, new GUIContent("Smoothness"));
            EditorGUILayout.Slider(hmpdHeightRandomFactor, 0f, 0.1f, new GUIContent("Height Random Factor"));
            EditorGUILayout.Slider(mpdDampenerBase, 1f, 5f, new GUIContent("Dampener"));

            if (GUILayout.Button("Apply MPD"))
            {
                myCustomTerrain.MidPointDisplacement();
            }
        }

    }
    private void Smoothing(CustomTerrain myCustomTerrain)
    {
        showSmoothing = EditorGUILayout.Foldout(showSmoothing, "Smoothing");
        GUILayout.Label("Smoothe existing terrain", EditorStyles.label);

        if (showSmoothing)
        {
            GUILayout.Space(10);
            //EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            EditorGUILayout.Slider(smothingFactor, 0.1f, 2f, new GUIContent("Smoothing Factor"));

            if (GUILayout.Button("Smoothe"))
            {
                myCustomTerrain.Smoothing(smothingFactor.floatValue);
            }
        }

    }
    private void ShowAndSaveTexture(CustomTerrain myCustomTerrain)
    {
        showExtractHeightMap = EditorGUILayout.Foldout(showExtractHeightMap, "Extract Heightmap");
        GUILayout.Label("Draw and save current heightmap to .png", EditorStyles.label);

        if (showExtractHeightMap)
        {
            currentHeightMapTexture = ExtractHeightMap(myCustomTerrain);
            
            GUILayout.Space(10);
            //EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

            if (GUILayout.Button("Refresh"))
            {
                currentHeightMapTexture = ExtractHeightMap(myCustomTerrain);
            }
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUILayout.Label(currentHeightMapTexture);
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            if (GUILayout.Button("Save As..."))
            {
                string savingPath = System.IO.Path.Combine(Application.dataPath, saveMapDirectoryPath);
                string path = EditorUtility.SaveFilePanel(
                "Save heightmap as PNG",
                savingPath,
                saveMapFileName,
                "png");

                if (path.Length != 0)
                {
                    byte[] bytes = currentHeightMapTexture.EncodeToPNG();
                    if (bytes != null)
                        System.IO.File.WriteAllBytes(path, bytes);
                }
            }
        }
    }
    public Texture2D ExtractHeightMap(CustomTerrain myCustomTerrain)
    {
        TerrainData myTerrainData = myCustomTerrain.GetComponent<Terrain>().terrainData;

        float[,] heightMap = myTerrainData.GetHeights(0, 0,
                                           myTerrainData.heightmapResolution,
                                           myTerrainData.heightmapResolution);
        int width = myTerrainData.heightmapResolution;
        Texture2D newTexture = new Texture2D(width, width,
                                             TextureFormat.ARGB32,
                                             false);

        for (int x = 0; x < width; x++)
        {
            for (int z = 0; z < width; z++)
            {
                float height = heightMap[x, z];
                Color colorValue = new Color(height, height, height);
                newTexture.SetPixel(x, z, colorValue);
            }
        }
        newTexture.Apply();
        return newTexture;
    }

    #endregion

    #region Erosion Section
    private void ErosionOptions(CustomTerrain myCustomTerrain)
    {
        showErosion = EditorGUILayout.Foldout(showErosion, "Erosion");
        GUILayout.Label("Simulate erosion on terrain", EditorStyles.label);

        if (showErosion)
        {
            GUILayout.Space(10);
            //EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            EditorGUILayout.PropertyField(erosionType);

            switch (myCustomTerrain.erosionType)
            {
                case CustomTerrain.ErosionType.Rain:
                    RainErosion(myCustomTerrain);
                    break;
                case CustomTerrain.ErosionType.River:
                    RiverErosion(myCustomTerrain);
                    break;
                case CustomTerrain.ErosionType.Slides:
                    SlidesErosion(myCustomTerrain);
                    break;
                case CustomTerrain.ErosionType.Tidal:
                    TidalErosion(myCustomTerrain);
                    break;
                case CustomTerrain.ErosionType.Wind:
                    WindErosion(myCustomTerrain);
                    break;
                case CustomTerrain.ErosionType.Canyon:
                    CanyonErosion(myCustomTerrain);
                    break;

            }
        }
    }
    private void RainErosion(CustomTerrain myCustomTerrain)
    {
        GUILayout.Space(10);
        //EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
        EditorGUILayout.IntSlider(erosionRainDroplets, 0, 1000, new GUIContent("Droplets"));
        EditorGUILayout.Slider(erosionRainStrength, 0.001f, 0.1f, new GUIContent("Strength"));

        GUILayout.Space(10);
        EditorGUILayout.IntSlider(erosionSmoothing, 0, 5, new GUIContent("Smoothing"));

        GUILayout.Space(10);
        if (GUILayout.Button("Erode"))
        {
            myCustomTerrain.Erode();
        }
    }
    private void RiverErosion(CustomTerrain myCustomTerrain)
    {
        GUILayout.Space(10);
        //EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
        EditorGUILayout.IntSlider(erosionRiverDroplets, 0, 1000, new GUIContent("Droplets"));
        EditorGUILayout.Slider(erosionRiverStrength, 0.001f, 1f, new GUIContent("Strength"));

        EditorGUILayout.IntSlider(erosionRiverPasses, 1, 20, new GUIContent("Passes",
                                                                            "Times the river function is applied " +
                                                                            "on each spring"));
        EditorGUILayout.Slider(erosionRiverSolubility, 0.001f, 1f, new GUIContent("Terrain solubility"));

        GUILayout.Space(10);
        EditorGUILayout.IntSlider(erosionSmoothing, 0, 5, new GUIContent("Smoothing"));

        GUILayout.Space(10);
        if (GUILayout.Button("Erode"))
        {
            myCustomTerrain.Erode();
        }
    }
    private void SlidesErosion(CustomTerrain myCustomTerrain)
    {
        GUILayout.Space(10);
        //EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
        EditorGUILayout.Slider(erosionSlidesStrength, 0.001f, 0.02f, new GUIContent("Strength"));
        EditorGUILayout.Slider(erosionSlidesMinimum, 0.001f, 0.05f, new GUIContent("Slide Minimum",
                                                                   "Higher values will have slides" +
                                                                    " affect only steeper terrains"));
        EditorGUILayout.IntSlider(erosionSlidesPasses, 1, 20, new GUIContent("Passes",
                                                                    "Number of times the slide" +
                                                                    "function is applied."));

        GUILayout.Space(10);
        EditorGUILayout.IntSlider(erosionSmoothing, 0, 5, new GUIContent("Smoothing"));

        GUILayout.Space(10);
        if (GUILayout.Button("Erode"))
        {
            myCustomTerrain.Erode();
        }
    }
    private void TidalErosion(CustomTerrain myCustomTerrain)
    {
        GUILayout.Space(10);
        EditorGUILayout.IntSlider(erosionSmoothing, 0, 5, new GUIContent("Smoothing"));

        GUILayout.Space(10);
        if (GUILayout.Button("Erode"))
        {
            myCustomTerrain.Erode();
        }
    }
    private void WindErosion(CustomTerrain myCustomTerrain)
    {
        GUILayout.Space(10);
        //EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
        EditorGUILayout.Slider(erosionWindStrength, 0.001f, 0.05f, new GUIContent("Strength"));
        EditorGUILayout.IntSlider(erosionWindStepWidth, 10, 100, new GUIContent("Distance between ripples"));

        EditorGUILayout.Slider(erosionWindNoiseX, 0.01f, 0.1f, new GUIContent("Noise X"));
        EditorGUILayout.Slider(erosionWindNoiseZ, 0.01f, 0.1f, new GUIContent("Noise Z"));
        EditorGUILayout.IntSlider(erosionWindNoiseAmplitude, 1, 100, new GUIContent("Scaler applied to perlin noise"));

        EditorGUILayout.Slider(erosionWindDirection, -180f, 180f, new GUIContent("Wind direction", "In degrees"));


        GUILayout.Space(10);
        EditorGUILayout.IntSlider(erosionSmoothing, 0, 5, new GUIContent("Smoothing"));

        GUILayout.Space(10);
        if (GUILayout.Button("Erode"))
        {
            myCustomTerrain.Erode();
        }
    }
    private void CanyonErosion(CustomTerrain myCustomTerrain)
    {
        GUILayout.Space(10);
        //EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
        EditorGUILayout.IntSlider(erosionCanyonMeandering, 0, 30, new GUIContent("Meandering",
                                                                                 "Higher values give less straight canyons"));
        EditorGUILayout.Slider(erosionCanyonBottomHeight, 0f, 0.9f, new GUIContent("Bottom height", 
                                                                                   "Height level of canyon bottom"));
        EditorGUILayout.Slider(erosionCanyonBottomSteepness, 0f, 0.1f, new GUIContent("Bottom steepness", 
                                                                                    "How steep is the bottom of the canyon"));
        EditorGUILayout.IntSlider(erosionCanyonBottomWidth, 1, 60, new GUIContent("Bottom width", 
                                                                                  "How wide the bottom o the canyon is"));

        EditorGUILayout.Slider(erosionCanyonTopStepHeight, 0.1f, 1f, new GUIContent("Top height",
                                                                            "The top step of the canyon" +
                                                                            "(it should be lower than flat level to see results)"));
        EditorGUILayout.IntSlider(erosionCanyonStepNumber, 1, 60, new GUIContent("Step number",
                                                                                  "How many vertical 'steps' the canyon will have"));
        EditorGUILayout.IntSlider(erosionCanyonMaxStepWidth, 1, 60, new GUIContent("Max step width"));

        GUILayout.Space(10);
        EditorGUILayout.IntSlider(erosionSmoothing, 0, 5, new GUIContent("Smoothing"));

        GUILayout.Space(10);
        if (GUILayout.Button("Erode"))
        {
            myCustomTerrain.Erode();
        }
    }

    #endregion

    #region Textures section
    private void TextureOptions(CustomTerrain myCustomTerrain)
    {
        SetTerrainTextures(myCustomTerrain);
    }

    private void SetTerrainTextures(CustomTerrain myCustomTerrain)
    {
        showTerrainTextures = EditorGUILayout.Foldout(showTerrainTextures, "Terrain Textures");
        GUILayout.Label("Set Terrain Layers based on heightmap", EditorStyles.label);

        if (showTerrainTextures)
        {
            GUILayout.Space(10);
            //EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            textureTable = GUITableLayout.DrawTable(textureTable,
                                                    textureParametersList);
            GUILayout.Space(40);
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("+"))
            { myCustomTerrain.AddTexture(); }
            if (GUILayout.Button("-"))
            { myCustomTerrain.RemoveTexture(); }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Slider(terrainLayersBlending, 0f, 0.1f, "Blending between layers");
            GUILayout.Label("Will add itself to any overlapping set in the table", EditorStyles.miniLabel);
            GUILayout.Space(5);
            EditorGUILayout.Slider(terrainLayersNoiseFrequency, 0f, 0.1f, "Layer noise frequency");
            EditorGUILayout.Slider(terrainLayersNoiseFactor, 0f, 0.5f, "Layer noise amplitude");

            GUILayout.Space(10);
            if (GUILayout.Button("Apply Terrain Layers"))
            { myCustomTerrain.ApplyTerrainLayers(); }
            GUILayout.Space(10);
        }
    }

    #endregion

    #region Vegetation Section
    private void VegetationOptions(CustomTerrain myCustomTerrain)
    {
        ApplyVegetation(myCustomTerrain);
    }

    private void ApplyVegetation(CustomTerrain myCustomTerrain)
    {
        showVegetation = EditorGUILayout.Foldout(showVegetation, "Vegetation");
        GUILayout.Label("Add trees or other meshes using Unity tree population settings", EditorStyles.label);

        if (showVegetation)
        {
            EditorGUILayout.IntSlider(vegMaxTreeNumber, 10, 10000, new GUIContent("Maximum number of trees"));
            GUILayout.Space(5);
            EditorGUILayout.IntSlider(vegTreeSpacing, 1, 50, new GUIContent("Space between trees"));

            GUILayout.Space(10);
            //EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            vegetationTable = GUITableLayout.DrawTable(vegetationTable,
                                                       vegetationTypes);

            GUILayout.Space(40);
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("+"))
            { myCustomTerrain.AddVegRow(); }
            if (GUILayout.Button("-"))
            { myCustomTerrain.RemoveVegRow(); }
            EditorGUILayout.EndHorizontal();
            GUILayout.Space(10);

            if (GUILayout.Button("Populate vegetation"))
            { myCustomTerrain.PopulateVegetation(); }
            GUILayout.Space(10);
        }

    }

    #endregion

    #region Details Section
    private void DetailsOptions(CustomTerrain myCustomTerrain)
    {
        ApplyDetails(myCustomTerrain);
    }

    private void ApplyDetails(CustomTerrain myCustomTerrain)
    {
        showDetails = EditorGUILayout.Foldout(showDetails, "Details");
        GUILayout.Label("Add details using Unity detail population", EditorStyles.label);

        if (showDetails)
        {
            EditorGUILayout.IntSlider(detailMaxNumber, 10, 10000, new GUIContent("Maximum number of detail instances"));
            GUILayout.Space(5);
            EditorGUILayout.IntSlider(detailSpacing, 1, 50, new GUIContent("Space between details"));

            GUILayout.Space(10);
            //EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            detailTable = GUITableLayout.DrawTable(detailTable,
                                                   detailTypes);

            GUILayout.Space(40);
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("+"))
            { myCustomTerrain.AddDetailRow(); }
            if (GUILayout.Button("-"))
            { myCustomTerrain.RemoveDetailRow(); }
            EditorGUILayout.EndHorizontal();
            GUILayout.Space(10);

            if (GUILayout.Button("Populate details"))
            { myCustomTerrain.PopulateDetails(); }
            GUILayout.Space(10);
        }

    }

    #endregion

    #region Water Section
    private void WaterOptions(CustomTerrain myCustomTerrain)
    {
        AddWater(myCustomTerrain);
    }

    private void AddWater(CustomTerrain myCustomTerrain)
    {
        showWater = EditorGUILayout.Foldout(showWater, "Water");
        GUILayout.Label("Add water to terrain", EditorStyles.label);

        if (showWater)
        {
            GUILayout.Space(10);
            //EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            EditorGUILayout.PropertyField(waterGO);
            EditorGUILayout.Slider(waterHeight, 0f, 1f);
            GUILayout.Space(10);
            if (GUILayout.Button("Add Wader"))
            {
                myCustomTerrain.AddWater();
            }
            GUILayout.Space(10);
            EditorGUILayout.PropertyField(waterShoreMaterial);
            EditorGUILayout.Slider(waterShoreThickness, 1f, 100f);
            GUILayout.Space(10);
            if (GUILayout.Button("Add Shoreline"))
            {
                myCustomTerrain.DrawShoreLine();
            }
            GUILayout.Space(10);
            if (GUILayout.Button("Add Shore Collider"))
            {
                myCustomTerrain.CreateShoreCollider();
            }

        }
    }


    #endregion

}
