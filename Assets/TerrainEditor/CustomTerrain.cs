using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;

[ExecuteInEditMode]


public class CustomTerrain : MonoBehaviour
{
    #region Class properties
    public enum PropertyType { Tag, Layer }
    [SerializeField] int terrainLayer = 0;
    #endregion

    #region Height Map Properties
    // height map reset checkbox
    public bool resetHeightMapOnChange = false;
    // add or remove height modes
    public enum applyMode { add, remove }
    // heigtmap image loader ---------------------------------------------------
    public Texture2D heightMapImage;
    public Vector3 heightMapScale = new Vector3(1, 1, 1);
    // height randomiser -------------------------------------------------------
    public Vector2 randomHeightRange = new Vector2(0f, 0.1f);
    // sine wave height --------------------------------------------------------
    public float sinAmplitude = 0.1f;
    public float sinWavelength = 130;
    public float sineOffsetX = 0;
    public float sineOffsetY = 0;
    public enum XValueForSine { xOnly, zOnly, addition, XminusZ, ZminusX }
    public XValueForSine xValueForSine;
    // perlin noise height------------------------------------------------------
    public float perlinXScale = 0.01f;
    public float perlinZScale = 0.01f;
    public float perlinYScale = 0.1f;
    public int perlinOffsetX = 500;
    public int perlinOffsetZ = 500;
    // Fractal Brownian Motion Height-------------------------------------------
    public int fBMOctaves = 5;
    public float fBMPersistence = 0.5f;
    public float fBMXScale = 0.01f;
    public float fBMZScale = 0.01f;
    public int fBMOffsetX = 5000;
    public int fBMOffsetZ = 5000;
    public float fBMFrequencyMultiplier = 2;
    public float fBMHeighScaler = 0.1f;
    // Multiple Fractal Brownian Motion Height-------------------------------------------
    [System.Serializable]
    public class fBMParameters
    {
        public int octaves = 5;
        public float persistence = 0.5f;
        public float xScale = 0.01f;
        public float zScale = 0.01f;
        public int xOffset = 0;
        public int zOffset = 0;
        public float frequencyMultiplier = 2;
        public float heighScaler = 0.1f;

        public bool remove = false;    //for removing table lines
    }
    public List<fBMParameters> multiplefBMParameters = new List<fBMParameters>()
                                                               { new fBMParameters() };
    // Voronoi --------------------------------------------------------------------------
    public Vector2 voronoiHeightRange = new Vector2(0f, 0.5f);
    public float VoronoiSteepness = 8f;
    public float voronoiCurvature = 7f;
    public int voronoiPeakCount = 10;
    public enum VoronoiType { Linear, Power, CombinedPower, SquareTopPlateau }
    public VoronoiType voronoiType = VoronoiType.SquareTopPlateau;
    // Mid Point Displacement -----------------------------------------------------------
    public float mpdMinTerrainHeight = 0.2f;
    public float mpdMaxTerrainHeight = 0.8f;
    public float mpdSmoothness = 2.5f;
    public float mpdDampenerBase = 1.5f;
    public float hmpdHeightRandomFactor = 0.02f;
    // Smmothing ------------------------------------------------------------------------
    public float smoothingFactor = 1f;

    #endregion

    #region Texture Properties

    // Splat Heights -----------------------------------------------------------
    [System.Serializable]
    public class textureParameters
    {
        public Texture2D texture = null;
        public Texture2D normal = null;
        public float minHeight = 0f;
        public float maxHeight = 1f;
        public float minSlope = 0f;
        public float maxSlope = 90f;
        public Vector2 tileOffset = Vector2.zero;
        public Vector2 tileSize = new Vector2(4, 4);
        public bool remove = false;
    }
    public List<textureParameters> textureParametersList = new List<textureParameters>()
                                                    { new textureParameters() };
    public float terrainLayersBlending = 0.01f;
    public float terrainLayersNoiseFrequency = 0.01f;
    public float terrainLayersNoiseFactor = 0.01f;

    #endregion

    #region Vegetation properties
    public int vegMaxTreeNumber = 5000;
    public int vegTreeSpacing = 5;

    [System.Serializable]
    public class Vegetation
    {
        public GameObject mesh;
        public float minHeightRange = 0f;
        public float maxHeightRange = 1f;
        public float minSlope = 0f;
        public float maxSlope = 90f;
        public Color colour1 = Color.white;
        public Color colour2 = Color.white;
        public Color lightmapColor1 = Color.white;
        public Color lightmapColor2 = Color.white;
        public float density = 0.5f;
        public float minHeightScale = 0.8f;
        public float maxHeightScale = 0.95f;
        public float minWidthScale = 0.8f;
        public float maxWidthScale = 0.95f;
        public bool randomRotation = true;
        public bool remove = false;
    }
    public List<Vegetation> vegetationTypes = new List<Vegetation>() { new Vegetation() };

    #endregion

    #region Details properties

    public int detailMaxNumber = 5000;
    public int detailSpacing = 5;

    [System.Serializable]
    public class Detail
    {
        public GameObject prototype = null;
        public Texture2D prototypeTexture = null;
        public float minHeightRange = 0f;
        public float maxHeightRange = 1f;
        public float minSlope = 0f;
        public float maxSlope = 90f;
        public float overlap = 0.01f;
        public float feather = 0.05f;
        public float density = 0.5f;
        public Color dryColour = Color.white;
        public Color healthyColour = Color.white;
        public float minHeightScale = 0.8f;
        public float maxHeightScale = 0.95f;
        public float minWidthScale = 0.8f;
        public float maxWidthScale = 0.95f;
        public float noiseSpread = 0.5f;
        public bool remove = false;
    }
    public List<Detail> detailTypes = new List<Detail>() { new Detail() };

    #endregion

    #region Water properties
    public float waterHeight = 0.2f;
    public GameObject waterGO;
    public Material waterShoreMaterial;
    public float waterShoreThickness = 10f;
    #endregion

    #region Erosion properties
    public enum ErosionType { Rain, Slides, Tidal, River, Wind, Canyon}
    public ErosionType erosionType = ErosionType.Rain;
    public int erosionSmoothing = 5;

    // rain
    public int erosionRainDroplets = 500;
    public float erosionRainStrength = 0.1f;
    // river
    public int erosionRiverDroplets = 500;
    public float erosionRiverStrength = 0.1f;
    public int erosionRiverPasses = 5;
    public float erosionRiverSolubility = 0.01f;
    // slides
    public float erosionSlidesStrength = 0.1f;
    public float erosionSlidesMinimum = 0.1f;
    public int erosionSlidesPasses = 5;
    // wind
    public float erosionWindStrength = 0.001f;
    public int erosionWindStepWidth = 10;
    public float erosionWindNoiseX = 0.06f;
    public float erosionWindNoiseZ = 0.06f;
    public int erosionWindNoiseAmplitude = 30;
    public float erosionWindDirection = 0f;
    // canyon
    public float erosionCanyonBottomHeight = 0.5f;
    public float erosionCanyonTopStepHeight = 1f;
    public int erosionCanyonStepNumber = 6;
    public int erosionCanyonMeandering = 8;
    public float erosionCanyonBottomSteepness = 0.0005f;
    public int erosionCanyonBottomWidth = 12;
    public int erosionCanyonMaxStepWidth = 5;

    #endregion

    #region member variables
    Terrain myTerrain;
    TerrainData myTerrainData;
    #endregion

    // --------------------------------------------------------------------------------

    #region Height Map functions
    // terrain shaping functions --------------------------------------------------------------------------
    public void FlattenTerrain()
    {
        float[,] heightMap = new float[myTerrainData.heightmapResolution, myTerrainData.heightmapResolution];

        myTerrainData.SetHeights(0, 0, heightMap);
    }
    public void LoadTextureHeightMap()
    {
        float[,] heightMap = GetHeightMap();

        for (int x = 0; x < myTerrainData.heightmapResolution; x++)
        {
            for (int z = 0; z < myTerrainData.heightmapResolution; z++)
            {
                heightMap[x, z] += heightMapImage.GetPixel((int)(z * heightMapScale.z), (int)(x * heightMapScale.x))
                                                          .grayscale
                                                          * heightMapScale.y;
            }
        }
        myTerrainData.SetHeights(0, 0, heightMap);
    }
    public void RandomiseTerrain()
    {
        float[,] heightMap = GetHeightMap();

        for (int x = 0; x < myTerrainData.heightmapResolution; x++)
        {
            for (int z = 0; z < myTerrainData.heightmapResolution; z++)
            {
                // because we represented the range with a Vector2.
                // not actually related to x and y coordinates.
                float minHeight = randomHeightRange.x;
                float maxHeight = randomHeightRange.y;

                float randValue = UnityEngine.Random.Range(minHeight, maxHeight);
                heightMap[x, z] += randValue;
            }
        }
        myTerrainData.SetHeights(0, 0, heightMap);
    }
    public void AddSineWaveHeight(applyMode mode)
    {
        float[,] heightMap = GetHeightMap();

        for (int x = 0; x < myTerrainData.heightmapResolution; x++)
        {
            for (int z = 0; z < myTerrainData.heightmapResolution; z++)
            {
                float xValue;
                switch (xValueForSine)
                {
                    case XValueForSine.xOnly:
                        xValue = x;
                        break;
                    case XValueForSine.zOnly:
                        xValue = z;
                        break;
                    case XValueForSine.addition:
                        xValue = x + z;
                        break;
                    case XValueForSine.XminusZ:
                        xValue = x - z;
                        break;
                    case XValueForSine.ZminusX:
                        xValue = z - x;
                        break;
                    default:
                        Debug.Log("Error with selection of terrain sine wave x value");
                        xValue = 0;
                        break;
                }

                float sineValue = CalculateSinWave(xValue, sinAmplitude, sineOffsetX, sinWavelength, sineOffsetY);

                if (mode == applyMode.add)
                {
                    heightMap[x, z] += sineValue;
                }
                else if (mode == applyMode.remove)
                {
                    heightMap[x, z] -= sineValue;
                }
                else
                {
                    Debug.Log("Wrong apply mode selected for Add sine Wave Height Function");
                }
            }
        }
        myTerrainData.SetHeights(0, 0, heightMap);
    }
    public void AddPerlinHeight(applyMode mode)
    {
        float[,] heightMap = GetHeightMap();

        for (int x = 0; x < myTerrainData.heightmapResolution; x++)
        {
            for (int z = 0; z < myTerrainData.heightmapResolution; z++)
            {
                float perlinValue = Mathf.PerlinNoise((x + perlinOffsetX) * perlinXScale,
                                                      (z + perlinOffsetZ) * perlinZScale)
                                    * perlinYScale;

                if (mode == applyMode.add)
                {
                    heightMap[x, z] += perlinValue;
                }
                else if (mode == applyMode.remove)
                {
                    heightMap[x, z] -= perlinValue;
                }
                else
                {
                    Debug.Log("Wrong apply mode selected for Add Perlin Height Function");
                }
            }
        }
        myTerrainData.SetHeights(0, 0, heightMap);
    }
    public void AddfBMHeight(applyMode mode)
    {
        float[,] heightMap = GetHeightMap();

        for (int x = 0; x < myTerrainData.heightmapResolution; x++)
        {
            for (int z = 0; z < myTerrainData.heightmapResolution; z++)
            {
                float fBMValue = Utils.FractalBrownianMotion((x + fBMOffsetX) * fBMXScale,
                                                             (z + fBMOffsetZ) * fBMZScale,
                                                             fBMOctaves,
                                                             fBMPersistence,
                                                             fBMFrequencyMultiplier)
                                                             * fBMHeighScaler;

                if (mode == applyMode.add)
                {
                    heightMap[x, z] += fBMValue;
                }
                else if (mode == applyMode.remove)
                {
                    heightMap[x, z] -= fBMValue;
                }
                else
                {
                    Debug.Log("Wrong apply mode selected for Add fBM Function");
                }
            }
        }
        myTerrainData.SetHeights(0, 0, heightMap);

    }
    // Multiple Fractal Brownian Motion Height ------------------------------------------
    public void MultiplefBMTerrain()
    {
        float[,] heightMap = GetHeightMap();
        for (int x = 0; x < myTerrainData.heightmapResolution; x++)
        {
            for (int z = 0; z < myTerrainData.heightmapResolution; z++)
            {
                foreach (fBMParameters row in multiplefBMParameters)
                {
                    float fBMValue = Utils.FractalBrownianMotion((x + row.xOffset) * row.xScale,
                                                                 (z + row.zOffset) * row.zScale,
                                                                 row.octaves,
                                                                 row.persistence,
                                                                 row.frequencyMultiplier)
                                                                 * row.heighScaler;
                    heightMap[x, z] += fBMValue;
                }
            }
        }
        myTerrainData.SetHeights(0, 0, heightMap);
    }
    public void AddfBMRow()
    { multiplefBMParameters.Add(new fBMParameters()); }
    public void RemovefBMRow()
    {
        List<fBMParameters> rowsToKeep = new List<fBMParameters>();
        foreach(fBMParameters row in multiplefBMParameters)
        {
            if (!row.remove)
            {
                rowsToKeep.Add(row);
            }
        }
        if (rowsToKeep.Count == 0)    // we cannot have an empty list or table will protest
        {
            rowsToKeep.Add(new fBMParameters());
        }

        multiplefBMParameters = rowsToKeep;
    }
    // Voronoi --------------------------------------------------------------------------
    public void Voronoi()
    {
        float[,] heightMap = GetHeightMap();
        float maxDistance = Vector2.Distance(Vector2.zero, new Vector2(myTerrainData.heightmapResolution,
                                                               myTerrainData.heightmapResolution));

        // for each peak
        for (int i = 0; i < voronoiPeakCount; i++)
        {
            Vector3 peak = new Vector3(UnityEngine.Random.Range(0, myTerrainData.heightmapResolution),
                                       UnityEngine.Random.Range(voronoiHeightRange.x, voronoiHeightRange.y),
                                       UnityEngine.Random.Range(0, myTerrainData.heightmapResolution));

            // iterate through each vertex
            for (int x = 0; x < myTerrainData.heightmapResolution; x++)
            {
                for (int z = 0; z < myTerrainData.heightmapResolution; z++)
                {
                    float distanceToPeak = DistanceFromPeak(x, z, peak) / maxDistance;    // to keep it between 0 and 1
                    float height;
                    
                    switch (voronoiType)
                    {
                        case VoronoiType.Linear:
                            height = peak.y
                                     - distanceToPeak * VoronoiSteepness;
                            break;

                        case VoronoiType.Power:
                            height = peak.y
                                     - Mathf.Pow(distanceToPeak, voronoiCurvature)
                                     *VoronoiSteepness;
                            break;
                        
                        case VoronoiType.CombinedPower:
                            height = peak.y
                                     - distanceToPeak * VoronoiSteepness
                                     - Mathf.Pow(distanceToPeak, voronoiCurvature);
                            break;

                        case VoronoiType.SquareTopPlateau:
                            height = peak.y
                                     - Mathf.Sin(distanceToPeak)
                                     * Mathf.Pow(distanceToPeak, (10 - voronoiCurvature))
                                     / ((10-VoronoiSteepness)/1000);    // modified to fit editor slider values
                            break;


                        default:
                            height = 0f;
                            Debug.Log("Did not assign a voronoi type while generating terrain");
                            break;
                    }

                    if (heightMap[x, z] < height) { heightMap[x, z] = height; }
                }
            }
        }
        myTerrainData.SetHeights(0, 0, heightMap);
    }
    private float DistanceFromPeak(int x, int z, Vector3 peak)
    {
        return Vector2.Distance(new Vector2(peak.x, peak.z),
                                new Vector2(x, z));
    }
    // Mid Point Displacement -----------------------------------------------------------
    public void MidPointDisplacement()
    {
        float[,] heightMap = GetHeightMap();
        int width = myTerrainData.heightmapResolution - 1;    //hm is 1 pixel larger as it maps vertexes,
                                                              //and we need a power of 2
        int currentSquareSize = width;

        // variables to tweak
        float heightRandomness = ((float)(currentSquareSize) / 2.0f) * hmpdHeightRandomFactor;
        float heightRandomFactorDampener = (float)Mathf.Pow(mpdDampenerBase, -1 * mpdSmoothness);
        
        while (currentSquareSize > 0)
        {
            // loop over x and z by steps of the currentSquareSize
            DiamondStep(heightMap, width, currentSquareSize, heightRandomness);
            SquareStep(heightMap, width, currentSquareSize, heightRandomness);

            currentSquareSize = (int)(currentSquareSize / 2.0f);
            heightRandomness *= heightRandomFactorDampener;
        }
        myTerrainData.SetHeights(0, 0, heightMap);
    }

    private void DiamondStep(float[,] heightMap,
                                    int width, int currentSquareSize, 
                                    float heightRandomFactor)
    {
        int farCornerX, farCornerZ;    // the coordinates of the opposite edge of the square to x and z
        int midX, midZ;

        for (int x = 0; x < width; x += currentSquareSize)
        {
            for (int z = 0; z < width; z += currentSquareSize)
            {
                farCornerX = x + currentSquareSize;
                farCornerZ = z + currentSquareSize;

                midX = (int)(x + currentSquareSize / 2f);
                midZ = (int)(z + currentSquareSize / 2f);

                heightMap[midX, midZ] = (float)((heightMap[x, z]
                                                + heightMap[x, farCornerZ]
                                                + heightMap[farCornerX, z]
                                                + heightMap[farCornerX, farCornerZ])
                                                / 4f
                                                + UnityEngine.Random.Range(-heightRandomFactor, heightRandomFactor));

                heightMap[midX, midZ] = Mathf.Clamp(heightMap[midX, midZ], mpdMinTerrainHeight, mpdMaxTerrainHeight);
            }
        }
    }

    private void SquareStep(float[,] heightMap,
                                int width, int currentSquareSize,
                                float heightRandomFactor)
    {
        int farCornerX, farCornerZ;    // the coordinates of the opposite edge of the square to x and z
        int midX, midZ;
        int neighbourLeftX, neighbourRightX, neighbourUpZ, neighbourDownZ;

        for (int x = 0; x < width; x += currentSquareSize)
        {
            for (int z = 0; z < width; z += currentSquareSize)
            {
                farCornerX = x + currentSquareSize;
                farCornerZ = z + currentSquareSize;

                midX = (int)(x + currentSquareSize / 2f);
                midZ = (int)(z + currentSquareSize / 2f);

                neighbourLeftX = (int)midX - currentSquareSize;
                neighbourRightX = (int)midX + currentSquareSize;
                neighbourDownZ = (int)midZ - currentSquareSize;
                neighbourUpZ = (int)midZ + currentSquareSize;

                bool isOnEdgeOfMap = neighbourLeftX < 0 ||
                                     neighbourDownZ < 0 ||
                                     neighbourRightX > width - 1 ||
                                     neighbourUpZ > width - 1;

                if (isOnEdgeOfMap) { continue; }

                // bottom mid point of square
                heightMap[midX, z] = (float)((heightMap[midX, neighbourDownZ]
                                              + heightMap[farCornerX, z]
                                              + heightMap[midX, midZ]
                                              + heightMap[x, z])
                                              / 4f
                                              + UnityEngine.Random.Range(-heightRandomFactor, heightRandomFactor));
                heightMap[midX, z] = Mathf.Clamp(heightMap[midX, z], mpdMinTerrainHeight, mpdMaxTerrainHeight);


                // upper mid point of square
                heightMap[midX, farCornerZ] = (float)((heightMap[x, farCornerZ]
                                                       + heightMap[midX, neighbourUpZ]
                                                       + heightMap[farCornerX, farCornerZ]
                                                       + heightMap[midX, midZ])
                                                       / 4f
                                                       + UnityEngine.Random.Range(-heightRandomFactor, heightRandomFactor));
                heightMap[midX, farCornerZ] = Mathf.Clamp(heightMap[midX, farCornerZ], mpdMinTerrainHeight, mpdMaxTerrainHeight);


                // left mid point of square
                heightMap[x, midZ] = (float)((heightMap[neighbourLeftX, midZ]
                                              + heightMap[x, farCornerZ]
                                              + heightMap[midX, midZ]
                                              + heightMap[x, z])
                                              / 4f
                                              + UnityEngine.Random.Range(-heightRandomFactor, heightRandomFactor));
                heightMap[x, midZ] = Mathf.Clamp(heightMap[x, midZ], mpdMinTerrainHeight, mpdMaxTerrainHeight);


                // right mid point of square
                heightMap[farCornerX, midZ] = (float)((heightMap[midX, midZ]
                                                       + heightMap[farCornerX, farCornerZ]
                                                       + heightMap[neighbourRightX, midZ]
                                                       + heightMap[farCornerX, z])
                                                       / 4f
                                                       + UnityEngine.Random.Range(-heightRandomFactor, heightRandomFactor));
                heightMap[farCornerX, midZ] = Mathf.Clamp(heightMap[farCornerX, midZ], mpdMinTerrainHeight, mpdMaxTerrainHeight);


            }
        }
    }
    // Smoothing ------------------------------------------------------------------------
    public void Smoothing(float smoothingFactor)
    {
        float[,] heightMap = myTerrainData.GetHeights(0, 0,
                                                      myTerrainData.heightmapResolution,
                                                      myTerrainData.heightmapResolution);
        int width = myTerrainData.heightmapResolution;

        // iterate through each vertex
        for (int x = 0; x < width; x++)
        {
            for (int z = 0; z < width; z++)
            {
                List<float> borderValues = FindValidNeighbours(heightMap, width, x, z);

                float smoothedValue = (borderValues.Sum()
                                      + heightMap[x, z] / smoothingFactor)    // smoothing factor cannot be 0
                                      / (borderValues.Count + 1 + 1/smoothingFactor);    //
                heightMap[x, z] = smoothedValue;
            }
        }
        myTerrainData.SetHeights(0, 0, heightMap);
    }

    // helpers --------------------------------------------------------------------------------------------
    private float[,] GetHeightMap()
        {
            if (resetHeightMapOnChange)
            { return new float[myTerrainData.heightmapResolution, myTerrainData.heightmapResolution]; }
            else
            {
                return myTerrainData.GetHeights(0, 0,
                                                myTerrainData.heightmapResolution,
                                                myTerrainData.heightmapResolution);
            }
        }
    private float CalculateSinWave(float x, float sinAmplitude = 0, float xOffset = 0, float sinWavelength = 1, float yOffset = 0)
    {
        float y = sinAmplitude * Mathf.Sin((x - xOffset) / sinWavelength) + yOffset;
        return y;
    }

    #endregion

    #region Texture functions
    public void AddTexture()
    { textureParametersList.Add(new textureParameters()); }
    public void RemoveTexture()
    {
        List<textureParameters> rowsToKeep = new List<textureParameters>();
        foreach (textureParameters row in textureParametersList)
        {
            if (!row.remove)
            {
                rowsToKeep.Add(row);
            }
        }
        if (rowsToKeep.Count == 0)    // we cannot have an empty list or table will protest
        {
            rowsToKeep.Add(new textureParameters());
        }

        textureParametersList = rowsToKeep;
    }
    public void ApplyTerrainLayers()
    {
        CreateLayersFromEditorTable();

        float[,] heightMap = myTerrainData.GetHeights(0, 0,
                                                      myTerrainData.heightmapResolution,
                                                      myTerrainData.heightmapResolution);
        float[,,] terrainLayerData = new float[myTerrainData.alphamapWidth,
                                               myTerrainData.alphamapHeight,
                                               myTerrainData.alphamapLayers];

        // iterate through x and z locations of map
        int progressBarValue = 0;
        for (int x = 0; x < myTerrainData.alphamapWidth; x++)
        {
            EditorUtility.DisplayProgressBar("Applying Terrain Layer Textures",
                                             "Progress",
                                             progressBarValue / myTerrainData.alphamapWidth);

            for (int z = 0; z < myTerrainData.alphamapHeight; z++)
            {
                CreateLayerDataForXZPosition(heightMap, terrainLayerData, x, z);

            }
            myTerrainData.SetAlphamaps(0, 0, terrainLayerData);

            progressBarValue++;
        }
        EditorUtility.ClearProgressBar();
    }
    private void CreateLayersFromEditorTable()
    {
        TerrainLayer[] newTerrainLayers = new TerrainLayer[textureParametersList.Count];

        int currentIndex = 0;
        foreach (textureParameters sh in textureParametersList)
        {
            // set texture properties based on editor values
            newTerrainLayers[currentIndex] = new TerrainLayer();
            newTerrainLayers[currentIndex].diffuseTexture = sh.texture;
            newTerrainLayers[currentIndex].tileOffset = sh.tileOffset;
            newTerrainLayers[currentIndex].tileSize = sh.tileSize;
            if (sh.normal)
            {
                newTerrainLayers[currentIndex].normalMapTexture = sh.normal;
            }
            newTerrainLayers[currentIndex].diffuseTexture.Apply(true);    // and apply them

            // create asset at given path
            string path = "Assets/New Terrain Layer " + currentIndex + ".terrainlayer";
            AssetDatabase.CreateAsset(newTerrainLayers[currentIndex], path);

            currentIndex++;

            Selection.activeObject = this.gameObject;
        }

        // assign all these layers to the terrain
        myTerrainData.terrainLayers = newTerrainLayers;
    }
    private void CreateLayerDataForXZPosition(float[,] heightMap, float[,,] terrainLayerData, int x, int z)
    {
        // one float per terrain layer
        float[] layersValues = new float[myTerrainData.alphamapLayers];

        // iterate through all textures present in the table,
        // and decide if they have any alpha in this location
        for (int i = 0; i < textureParametersList.Count; i++)
        {
            textureParameters thisTexture = textureParametersList[i];

            // define height range for this texture in this location,
            // based on editor settings, and perlin noise
            float noise = Mathf.PerlinNoise(x * terrainLayersNoiseFrequency,
                                            z * terrainLayersNoiseFrequency)
                                            * terrainLayersNoiseFactor;

            float minHeight = thisTexture.minHeight;
            if (minHeight > 0)
            {
                minHeight -= terrainLayersBlending + noise;
            }

            float maxHeight = thisTexture.maxHeight;
            if (maxHeight < 1)
            {
                maxHeight += terrainLayersBlending + noise;
            }

            // check if height is in texture range
            bool isRightHeight = heightMap[x, z] >= minHeight &&
                                 heightMap[x, z] <= maxHeight;

            // define slope of location and check it fits in texture slope range
            float steepness = myTerrainData.GetSteepness(z / (float)myTerrainData.alphamapWidth,
                                                         x / (float)myTerrainData.alphamapHeight);    // swapped because alphamap is
                                                                                                      // at 90° from heightmap

            steepness += terrainLayersBlending * 90 * 2;    // steepness is a value betwwn 0 and 90
                                                            // *2 added because it was not affecting things enough

            bool isRightSlope = steepness >= thisTexture.minSlope
                                && steepness <= thisTexture.maxSlope;

            // turn layer on if it fits given parameters (height and slope)
            if (isRightHeight && isRightSlope)
            {
                layersValues[i] = 1;
            }
        }
        NormaliseVector(layersValues);    // layer data wants the total alpha of all layers at a given coordinate to be 1

        for (int j = 0; j < textureParametersList.Count; j++)
        {
            terrainLayerData[x, z, j] = layersValues[j];
        }
    }
    private void NormaliseVector(float[] vector)
    {
        float totalValue = vector.Sum();
        if (totalValue < Mathf.Epsilon)
        {
            //Debug.Log("Incorrect values applied to terrain textures min/max heights");
            return;
        }
        for (int i = 0; i < vector.Length; i++)
        {
            vector[i] /= totalValue;
        }
    }
    #endregion

    #region Vegetation functions
    public void AddVegRow()
    { vegetationTypes.Add(new Vegetation()); }
    public void RemoveVegRow()
    {
        List<Vegetation> rowsToKeep = new List<Vegetation>();
        foreach (Vegetation row in vegetationTypes)
        {
            if (!row.remove)
            {
                rowsToKeep.Add(row);
            }
        }
        if (rowsToKeep.Count == 0)    // we cannot have an empty list or table will protest
        {
            rowsToKeep.Add(new Vegetation());
        }

        vegetationTypes = rowsToKeep;
    }
    private void CreateTreesFromEditorTable()
    {
        TreePrototype[] newVegetation = new TreePrototype[vegetationTypes.Count];

        int currentIndex = 0;
        foreach (Vegetation vegType in vegetationTypes)
        {
            // set veg properties based on editor values
            newVegetation[currentIndex] = new TreePrototype();
            newVegetation[currentIndex].prefab = vegType.mesh;

            currentIndex++;
        }
        myTerrainData.treePrototypes = newVegetation;
    }
    public void PopulateVegetation()
    {
        CreateTreesFromEditorTable();

        List<TreeInstance> allVegetation = new List<TreeInstance>();
        allVegetation = CreateVegetationList();
        myTerrainData.treeInstances = allVegetation.ToArray();
    }
    private List<TreeInstance> CreateVegetationList()
    {
        List<TreeInstance> allVegetation = new List<TreeInstance>();

        // iterate through x and z locations of map
        for (int x = 0; x < myTerrainData.size.x; x += vegTreeSpacing)
        {
            for (int z = 0; z < myTerrainData.size.z; z += vegTreeSpacing)
            {
                // iterate over all vegetation types
                for (int prototypeIndex = 0; prototypeIndex < myTerrainData.treePrototypes.Length; prototypeIndex++)
                {
                    Vegetation prototypeVegetation = vegetationTypes[prototypeIndex];

                    // density check
                    if (UnityEngine.Random.Range(0f, 1f) > prototypeVegetation.density) { continue; }

                    // this is the height of the corresponding vertex, expressed as a number from 0 to 1
                    int mappedX = (int)Utils.Map(x, 0, myTerrainData.size.x, 0, myTerrainData.heightmapResolution);
                    int mappedZ = (int)Utils.Map(z, 0, myTerrainData.size.z, 0, myTerrainData.heightmapResolution);

                    float normalisedHeightAtPosition = myTerrainData.GetHeight(mappedX, mappedZ)
                                                                               / (float)myTerrainData.size.y;

                    //float normalisedHeightAtPosition = myTerrain.SampleHeight(new Vector2(x, z) + new Vector2(this.transform.position.x, this.transform.position.z)) / (float)myTerrainData.size.y;

                    // check if in correct height range for this vegetation prototype
                    bool inHeightRange = (normalisedHeightAtPosition >= prototypeVegetation.minHeightRange
                                          && normalisedHeightAtPosition <= prototypeVegetation.maxHeightRange);
                    if (!inHeightRange) { continue; }


                    // define slope of location and check it fits in texture slope range
                    float steepness = myTerrainData.GetSteepness((float)x / (float)myTerrainData.size.x,
                                                                 (float)z / (float)myTerrainData.size.z);    
                    bool isRightSlope = steepness >= prototypeVegetation.minSlope
                                        && steepness <= prototypeVegetation.maxSlope;

                    if (!isRightSlope) { continue; }

                    // check if instance would be inside terrain
                    Vector3 instancePosition = FindInstancePosition(x, z, normalisedHeightAtPosition);
                    if (instancePosition == Vector3.zero) { continue; }

                    // create instance and set its properties
                    TreeInstance instance = new TreeInstance();
                    instance.prototypeIndex = prototypeIndex;
                    instance.position = instancePosition;

                    // colour
                    instance.color = Color.Lerp(prototypeVegetation.colour1,
                                                prototypeVegetation.colour2,
                                                UnityEngine.Random.Range(0f, 1f));
                    instance.lightmapColor = Color.Lerp(prototypeVegetation.lightmapColor1,
                                                        prototypeVegetation.lightmapColor2,
                                                        UnityEngine.Random.Range(0f, 1f));

                    // scale
                    instance.heightScale = UnityEngine.Random.Range(prototypeVegetation.minHeightScale,
                                                                    prototypeVegetation.maxHeightScale);
                    instance.widthScale = UnityEngine.Random.Range(prototypeVegetation.minWidthScale,
                                                                   prototypeVegetation.maxWidthScale);

                    instance.rotation = UnityEngine.Random.Range(0, 2 * Mathf.PI);

                    // add tree to vegetation list
                    allVegetation.Add(instance);
                    if (allVegetation.Count >= vegMaxTreeNumber) 
                    {
                        Debug.Log("Could not generate all trees ase maximum number was reached");
                        return allVegetation;
                    }
                }
            }
        }

        return allVegetation;
    }
    private Vector3 FindInstancePosition(int xWorldUnits, int zWorldUnits, 
                                         float normalisedHeightAtPosition)
    {
        float instanceWorldUnitsX = UnityEngine.Random.Range(xWorldUnits, xWorldUnits + vegTreeSpacing);
        float instanceWorldUnitsZ = UnityEngine.Random.Range(zWorldUnits, zWorldUnits + vegTreeSpacing);

        float instanceWorldUnitsY = normalisedHeightAtPosition * myTerrainData.size.y;

        Vector3 tempInstancePosWorldUnits = new Vector3(instanceWorldUnitsX,
                                                        instanceWorldUnitsY,
                                                        instanceWorldUnitsZ);

        Vector3 tempInstanceWorldPos = tempInstancePosWorldUnits + this.transform.position;    // to make relative to world

        if (! IsPositionOnTerrain(tempInstancePosWorldUnits)) { return Vector3.zero; }

        RaycastHit raycastHit;
        int terrainLayerMask = 1 << terrainLayer;

        Vector3 rayCastOrigin = tempInstanceWorldPos + new Vector3(0, myTerrainData.size.y + 1, 0);

        if (Physics.Raycast(origin: rayCastOrigin,
                            direction: Vector3.down,
                            out raycastHit,
                            maxDistance: myTerrainData.size.y * 2,
                            layerMask: terrainLayerMask)) 
        {
            instanceWorldUnitsY = raycastHit.point.y;
        }
        else
        {
            Debug.Log("Vegetation raycasting not hitting terrain at " + instanceWorldUnitsX + " " + instanceWorldUnitsZ);
            Debug.DrawRay(rayCastOrigin, Vector3.down * (myTerrainData.size.y * 2), Color.red, 10);
        }

        Vector3 instancePostion = new Vector3(instanceWorldUnitsX / myTerrainData.size.x,
                                              instanceWorldUnitsY / myTerrainData.size.y,
                                              instanceWorldUnitsZ / myTerrainData.size.z);
        return instancePostion;
    }
    private bool IsPositionOnTerrain(Vector3 instancePosition)
    {
        return (instancePosition.x > 0.1
                && instancePosition.x < (myTerrainData.size.x - 0.1)
                && instancePosition.z > 0.1
                && instancePosition.z < (myTerrainData.size.z - 0.1));
    }

    #endregion

    #region Detail Functions
    public void AddDetailRow()
    { detailTypes.Add(new Detail()); }
    public void RemoveDetailRow()
    {
        List<Detail> rowsToKeep = new List<Detail>();
        foreach (Detail row in detailTypes)
        {
            if (!row.remove)
            {
                rowsToKeep.Add(row);
            }
        }
        if (rowsToKeep.Count == 0)    // we cannot have an empty list or table will protest
        {
            rowsToKeep.Add(new Detail());
        }

        detailTypes = rowsToKeep;
    }
    public void PopulateDetails()
    {
        CreateDetailsFromEditorTable();
        
        for (int i = 0; i < myTerrainData.detailPrototypes.Length; i++)
        {
            int[,] layerDetailMap = CreateLayerDetailMap(i);

            myTerrainData.SetDetailLayer(0, 0, i, layerDetailMap);
        }

    }

    private void CreateDetailsFromEditorTable()
    {
        DetailPrototype[] newDetails = new DetailPrototype[detailTypes.Count];

        int currentIndex = 0;
        foreach (Detail detailType in detailTypes)
        {
            // set detail properties based on editor values
            newDetails[currentIndex] = new DetailPrototype();

            newDetails[currentIndex].healthyColor = Color.white;
            newDetails[currentIndex].prototype = detailType.prototype;
            newDetails[currentIndex].prototypeTexture = detailType.prototypeTexture;
            if (newDetails[currentIndex].prototype)
            {
                newDetails[currentIndex].usePrototypeMesh = true;
                newDetails[currentIndex].renderMode = DetailRenderMode.VertexLit;
            }
            else
            {
                newDetails[currentIndex].usePrototypeMesh = false;
                newDetails[currentIndex].renderMode = DetailRenderMode.GrassBillboard;
            }

            newDetails[currentIndex].healthyColor = detailType.healthyColour;
            newDetails[currentIndex].dryColor = detailType.dryColour;

            newDetails[currentIndex].minHeight = detailType.minHeightScale;
            newDetails[currentIndex].maxHeight = detailType.maxHeightScale;

            newDetails[currentIndex].minWidth = detailType.minWidthScale;
            newDetails[currentIndex].maxWidth = detailType.maxWidthScale;

            newDetails[currentIndex].noiseSpread = detailType.noiseSpread;

            currentIndex++;
        }
        myTerrainData.detailPrototypes = newDetails;
    }
    private int[,] CreateLayerDetailMap(int detailLayerIndex)
    {
        int[,] detailMap = new int[myTerrainData.detailWidth, myTerrainData.detailHeight];

        for (int x = 0; x < myTerrainData.detailWidth; x += detailSpacing)
        {
            for (int z = 0; z < myTerrainData.detailHeight; z += detailSpacing)
            {
                Detail thisDetail = detailTypes[detailLayerIndex];

                // check for density
                if (UnityEngine.Random.Range(0f, 1f) > thisDetail.density) { continue; }

                // create noise value
                float noise = Mathf.PerlinNoise(x * thisDetail.feather,
                                                z * thisDetail.feather);
                noise = Utils.Map(noise,
                                  0, 1,
                                  0.5f, 1f);    // pushes a value 0-1 to be 0.5-1

                // get heightmap at position
                int heightmapX = (int)(x / (float)myTerrainData.detailWidth * myTerrainData.heightmapResolution);
                int heightmapZ = (int)(z / (float)myTerrainData.detailHeight * myTerrainData.heightmapResolution);
                float positionHeight = myTerrainData.GetHeight(heightmapX, heightmapZ) / myTerrainData.size.y;

                // check for height range
                float minHeight = thisDetail.minHeightRange 
                                  * noise
                                  - thisDetail.overlap * noise;
                float maxHeight = thisDetail.maxHeightRange
                                  * noise
                                  + thisDetail.overlap * noise;

                bool inHeightRange = (positionHeight >= minHeight
                                   && positionHeight <= maxHeight);
                if (!inHeightRange) { continue; }

                // check for steepness
                float steepness = myTerrainData.GetSteepness(heightmapX / (float)myTerrainData.alphamapHeight,
                                                             heightmapZ / (float)myTerrainData.alphamapWidth);
                bool isRightSlope = steepness >= thisDetail.minSlope
                                    && steepness <= thisDetail.maxSlope;
                if (!isRightSlope) { continue; }

                detailMap[z, x] = 1;    // inverted coordinates, like for alphaMap.
            }
        }

        return detailMap;
    }

    #endregion

    #region Water functions
    public void AddWater()
    {
        GameObject water = GameObject.Find("water");
        if (water)
        {
            DestroyImmediate(water);
        }

        water = Instantiate(waterGO, this.transform.position, this.transform.rotation);
        water.name = "water";


        water.transform.position = this.transform.position + new Vector3(myTerrainData.size.x / 2,
                                                                         waterHeight * myTerrainData.size.y,
                                                                         myTerrainData.size.z / 2);
        water.transform.localScale = new Vector3(myTerrainData.size.x,
                                                 1,
                                                 myTerrainData.size.z);
    }
    
    public void DrawShoreLine()
    {
        float[,] heightmap = myTerrainData.GetHeights(0, 0,
                                                      myTerrainData.heightmapResolution,
                                                      myTerrainData.heightmapResolution);

        // iterate over heigthmap
        for (int x = 0; x < myTerrainData.heightmapResolution; x++)
        {
            for (int z = 0; z < myTerrainData.heightmapResolution; z++)
            {
                List<Vector2> neighbours = GetNeighbours(myTerrainData.heightmapResolution,
                                         x, z);
                bool isOnShore = false;
                Vector2 shoreNeighbour = new Vector2();

                // create a quad at each shore location
                isOnShore = IsOnShore(heightmap, x, z, neighbours, out shoreNeighbour);

                if (isOnShore)
                {
                    GameObject shoreGO = GameObject.CreatePrimitive(PrimitiveType.Quad);
                    shoreGO.transform.localScale *= waterShoreThickness;

                    shoreGO.transform.position = new Vector3(z / (float)myTerrainData.heightmapResolution * myTerrainData.size.z,
                                                             waterHeight * myTerrainData.size.y,
                                                             x / (float)myTerrainData.heightmapResolution * myTerrainData.size.x);


                    shoreGO.transform.LookAt(new Vector3(shoreNeighbour.y / (float)myTerrainData.heightmapResolution * myTerrainData.size.z,
                                                         waterHeight * myTerrainData.size.y,
                                                         shoreNeighbour.x / (float)myTerrainData.heightmapResolution * myTerrainData.size.x));

                    shoreGO.transform.Rotate(90, 0, 0);
                    shoreGO.tag = "Shore";
                }
            }
        }

        // Store all quad meshes in an array
        GameObject[] shoreQuads = GameObject.FindGameObjectsWithTag("Shore");
        MeshFilter[] shoreQuadsMeshFilters = new MeshFilter[shoreQuads.Length];
        for (int i = 0; i < shoreQuads.Length; i++)
        {
            shoreQuadsMeshFilters[i] = shoreQuads[i].GetComponent<MeshFilter>();
        }

        // combine all shore meshes
        CombineInstance[] combine = new CombineInstance[shoreQuadsMeshFilters.Length];
        for (int i = 0; i < shoreQuadsMeshFilters.Length; i++)
        {
            combine[i].mesh = shoreQuadsMeshFilters[i].sharedMesh;
            combine[i].transform = shoreQuadsMeshFilters[i].transform.localToWorldMatrix;
            shoreQuadsMeshFilters[i].gameObject.SetActive(false);
        }

        GameObject currentShoreLine = GameObject.Find("ShoreLine");
        if (currentShoreLine)
        {
            DestroyImmediate(currentShoreLine);
        }
        GameObject shoreLine = new GameObject();
        shoreLine.name = "ShoreLine";
        shoreLine.AddComponent<WaveAnimation>();
        shoreLine.transform.position = this.transform.position;
        shoreLine.transform.rotation = this.transform.rotation;
        MeshFilter shoreMeshFilter = shoreLine.AddComponent<MeshFilter>();
        shoreMeshFilter.mesh = new Mesh();
        shoreLine.GetComponent<MeshFilter>().sharedMesh.CombineMeshes(combine);
        MeshRenderer meshRenderer = shoreLine.AddComponent<MeshRenderer>();
        meshRenderer.sharedMaterial = waterShoreMaterial;

        for (int iQuad = 0; iQuad < shoreQuads.Length; iQuad++)
        {
            DestroyImmediate(shoreQuads[iQuad]);
        }


    }

    public void CreateShoreCollider()
    {
        float[,] heightmap = myTerrainData.GetHeights(0, 0,
                                                      myTerrainData.heightmapResolution,
                                                      myTerrainData.heightmapResolution);

        // iterate over heigthmap
        for (int x = 0; x < myTerrainData.heightmapResolution; x++)
        {
            for (int z = 0; z < myTerrainData.heightmapResolution; z++)
            {
                List<Vector2> neighbours = GetNeighbours(myTerrainData.heightmapResolution,
                                         x, z);
                bool isOnShore = false;
                Vector2 shoreNeighbour = new Vector2();

                // create a quad at each shore location
                isOnShore = IsOnShore(heightmap, x, z, neighbours, out shoreNeighbour);

                if (isOnShore)
                {
                    GameObject shoreGO = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    //shoreGO.transform.localScale *= waterShoreThickness;

                    shoreGO.transform.position = new Vector3(z / (float)myTerrainData.heightmapResolution * myTerrainData.size.z,
                                                             waterHeight * myTerrainData.size.y,
                                                             x / (float)myTerrainData.heightmapResolution * myTerrainData.size.x);


                    shoreGO.transform.LookAt(new Vector3(shoreNeighbour.y / (float)myTerrainData.heightmapResolution * myTerrainData.size.z,
                                                         waterHeight * myTerrainData.size.y,
                                                         shoreNeighbour.x / (float)myTerrainData.heightmapResolution * myTerrainData.size.x));

                    shoreGO.transform.Rotate(90, 0, 0);
                    shoreGO.tag = "ShoreCollider";
                }
            }
        }

        // Store all quad meshes in an array
        GameObject[] shoreCubes = GameObject.FindGameObjectsWithTag("ShoreCollider");
        MeshFilter[] shoreCubesMeshFilters = new MeshFilter[shoreCubes.Length];
        for (int i = 0; i < shoreCubes.Length; i++)
        {
            shoreCubesMeshFilters[i] = shoreCubes[i].GetComponent<MeshFilter>();
        }

        // combine all shore meshes
        CombineInstance[] combine = new CombineInstance[shoreCubesMeshFilters.Length];
        for (int i = 0; i < shoreCubesMeshFilters.Length; i++)
        {
            combine[i].mesh = shoreCubesMeshFilters[i].sharedMesh;
            combine[i].transform = shoreCubesMeshFilters[i].transform.localToWorldMatrix;
            shoreCubesMeshFilters[i].gameObject.SetActive(false);
        }

        GameObject currentShoreCollider = GameObject.Find("ShoreCollider");
        if (currentShoreCollider)
        {
            DestroyImmediate(currentShoreCollider);
        }
        GameObject shoreCollider = new GameObject();
        shoreCollider.name = "ShoreCollider";
        shoreCollider.transform.position = this.transform.position;
        shoreCollider.transform.rotation = this.transform.rotation;
        MeshFilter shoreMeshFilter = shoreCollider.AddComponent<MeshFilter>();
        shoreMeshFilter.mesh = new Mesh();
        shoreCollider.GetComponent<MeshFilter>().sharedMesh.CombineMeshes(combine);
        MeshRenderer meshRenderer = shoreCollider.AddComponent<MeshRenderer>();

        for (int iQuad = 0; iQuad < shoreCubes.Length; iQuad++)
        {
            DestroyImmediate(shoreCubes[iQuad]);
        }

    }

    #endregion

    #region Erosion functions
    public void Erode()
    {
        switch (erosionType)
        {
            case ErosionType.Rain:
                ErosionRain();
                break;
            case ErosionType.River:
                ErosionRiver();
                break;
            case ErosionType.Slides:
                ErosionSlides();
                break;
            case ErosionType.Tidal:
                ErosionTidal();
                break;
            case ErosionType.Wind:
                ErosionWind();
                break;
            case ErosionType.Canyon:
                ErosionCanyon();
                break;
        }

        for (int i = 0; i < erosionSmoothing; i++)
        {
            Smoothing(0.01f);
        }
    }
    private void ErosionRain()
    {
        float[,] heightmap = myTerrainData.GetHeights(0, 0, 
                                                      myTerrainData.heightmapResolution,
                                                      myTerrainData.heightmapResolution);

        for (int i = 0; i < erosionRainDroplets; i++)
        {
            int dropletX = (int)(UnityEngine.Random.Range(0, myTerrainData.heightmapResolution));
            int dropletZ = (int)(UnityEngine.Random.Range(0, myTerrainData.heightmapResolution));

            heightmap[dropletX, dropletZ] -= erosionRainStrength;
        }
        myTerrainData.SetHeights(0, 0, heightmap);
    }
    private void ErosionRiver()
    {
        int mapWidth = myTerrainData.heightmapResolution;
        float[,] heightmap = myTerrainData.GetHeights(0, 0, mapWidth, mapWidth);
        float[,] erosionMap = new float[mapWidth, mapWidth];

        for (int i = 0; i < erosionRiverDroplets; i++)
        {
            // place droplet at random place on map
            int dropletX = (int)(UnityEngine.Random.Range(0, myTerrainData.heightmapResolution));
            int dropletZ = (int)(UnityEngine.Random.Range(0, myTerrainData.heightmapResolution));

            erosionMap[dropletX, dropletZ] = erosionRiverStrength;

            // create erosion map for all given water passes
            for (int j = 0; j < erosionRiverPasses; j++)
            {
                erosionMap = CreateRiverErosionMap(dropletX,
                                                   dropletZ,
                                                   erosionMap,
                                                   heightmap,
                                                   mapWidth);

                // iterate heightmap and subtract erosion value
                for (int x = 0; x < mapWidth; x++)
                {
                    for (int z = 0; z < mapWidth; z++)
                    {
                        float erosionValueAtPosition = erosionMap[x, z];
                        if (erosionValueAtPosition > 0)
                        {
                            
                            float newHeight = heightmap[x, z] - erosionValueAtPosition;
                            if (newHeight > (waterHeight - 0.01f))
                            {
                                heightmap[x, z] = newHeight;
                            }
                        }
                    }
                }
            }
        }
        myTerrainData.SetHeights(0, 0, heightmap);

    }
    private float[,] CreateRiverErosionMap(int dropletX,
                                           int dropletZ,
                                           float[,] erosionMap,
                                           float[,] heightmap,
                                           int mapWidth)
    {
        while (erosionMap[dropletX,dropletZ] > 0)    // while water still has some force
        {
            List<Vector2> neighbours = GetNeighbours(mapWidth, dropletX, dropletZ);
            //neighbours = neighbours.OrderBy(item =>(heightmap[(int)item.x, (int)item.y])).ToList();
            neighbours.Shuffle();

            bool foundLowerNeighbour = false;
            float sediment = erosionRiverSolubility;
            foreach (Vector2 neighbour in neighbours)
            {
                float neighbourHeight = heightmap[(int)neighbour.x, (int)neighbour.y];
                float currentDropletHeight = heightmap[dropletX, dropletZ];
                if (neighbourHeight < currentDropletHeight)
                {
                    erosionMap[(int)neighbour.x, (int)neighbour.y] = erosionMap[dropletX, dropletZ]
                                                                     - erosionRiverSolubility;
                    sediment += erosionRiverSolubility;
                    dropletX = (int)neighbour.x;
                    dropletZ = (int)neighbour.y;
                    foundLowerNeighbour = true;
                    break;
                }
            }
            if (!foundLowerNeighbour)
            {
                erosionMap[dropletX, dropletZ] -= erosionRiverSolubility;
            }
        }
        return erosionMap;
    }
    private void ErosionTidal()
    {
        int mapWidth = myTerrainData.heightmapResolution;
        float[,] heightmap = myTerrainData.GetHeights(0, 0, mapWidth, mapWidth);

        for (int x = 0; x < mapWidth; x++)
        {
            for (int z = 0; z < mapWidth; z++)
            {
                float positionHeight = heightmap[x, z];
                List<Vector2> neighbours = GetNeighbours(mapWidth, x, z);

                foreach (Vector2 neighbour in neighbours)
                {
                    if (positionHeight < waterHeight)
                    {
                        float neighbourHeight = heightmap[(int)neighbour.x, (int)neighbour.y];

                        if (neighbourHeight > waterHeight)
                        {
                            float heightDifference = neighbourHeight - positionHeight;

                            //neighbourHeight -= heightDifference * erosionStrength;
                            //positionHeight += heightDifference * erosionStrength;

                            neighbourHeight = waterHeight;
                            positionHeight = waterHeight;

                            heightmap[(int)neighbour.x, (int)neighbour.y] = neighbourHeight;
                            heightmap[x, z] = positionHeight;

                        }
                    }
                }
            }
        }
        myTerrainData.SetHeights(0, 0, heightmap);
    }
    private void ErosionSlides()
    {
        int mapWidth = myTerrainData.heightmapResolution;
        float[,] heightmap = myTerrainData.GetHeights(0, 0, mapWidth, mapWidth);

        int progressBarValue = 0;
        for (int i = 0; i < erosionSlidesPasses; i++)
        {
            EditorUtility.DisplayProgressBar("Applying Slide Erosion",
                                 "Progress",
                                 ((float)progressBarValue / (float)erosionSlidesPasses));

            ErosionSingleSlide(mapWidth, heightmap);
            progressBarValue++;
        }
        EditorUtility.ClearProgressBar();
    }
    private void ErosionSingleSlide(int mapWidth, float[,] heightmap)
    {
        List<Vector2> heightmapPositions = new List<Vector2>();
        for (int x = 0; x < mapWidth; x++)
        {
            for (int z = 0; z < mapWidth; z++)
            {
                heightmapPositions.Add(new Vector2(x, z));
            }
        }

        // order by height
        heightmapPositions = heightmapPositions.OrderBy(item => (heightmap[(int)item.x, (int)item.y])).ToList();
        //heightmapPositions = heightmapPositions.OrderBy(item => Random.value).ToList();    // so that they are in a random list

        foreach (Vector2 position in heightmapPositions)
        {
            int x = (int)position.x;
            int z = (int)position.y;

            List<Vector2> neighbours = GetNeighbours(mapWidth, x, z);
            //neighbours = neighbours.OrderBy(item => Random.value).ToList();    // so that neighbours are in a random list
            //neighbours = neighbours.OrderBy(item => (heightmap[(int)item.x, (int)item.y])).ToList();    // so that neighbours are height order
            //neighbours.Reverse();

            foreach (Vector2 neighbour in neighbours)
            {
                float positionHeight = heightmap[x, z];
                float neighbourHeight = heightmap[(int)neighbour.x, (int)neighbour.y];

                if (positionHeight > (neighbourHeight + erosionSlidesMinimum))
                {
                    positionHeight -= positionHeight * erosionSlidesStrength;
                    neighbourHeight += positionHeight * erosionSlidesStrength;

                    heightmap[x, z] = positionHeight;
                    heightmap[(int)neighbour.x, (int)neighbour.y] = neighbourHeight;
                }
            }
        }
        myTerrainData.SetHeights(0, 0, heightmap);
    }
    private void ErosionWind()
    {
        int mapWidth = myTerrainData.heightmapResolution;
        float[,] heightmap = myTerrainData.GetHeights(0, 0, mapWidth, mapWidth);

        float sinAngle = -Mathf.Sin(erosionWindDirection * Mathf.Deg2Rad);
        float cosAngle = Mathf.Cos(erosionWindDirection * Mathf.Deg2Rad);

        for (int x = -mapWidth; x < mapWidth*2; x++)
        {
            for (int z = -mapWidth; z < mapWidth*2; z+= erosionWindStepWidth)
            {
                float noise = Mathf.PerlinNoise(x * erosionWindNoiseX, z * erosionWindNoiseZ) * erosionWindNoiseAmplitude;
                int unrotatedDiggingZ = z + (int)noise;
                int nextX = x;
                int nextZ = z + (erosionWindStepWidth/2) + (int)noise;

                int digX = (int)(x * cosAngle - unrotatedDiggingZ * sinAngle);
                int digZ = (int)(unrotatedDiggingZ * cosAngle + x * sinAngle);
                int pileX = (int)(nextX * cosAngle - nextZ * sinAngle);
                int pileZ = (int)(nextZ * cosAngle + nextX * sinAngle);

                bool isNextPosOnMap = pileX >= 0 && pileX < mapWidth
                                      && pileZ >= 0 && pileZ < mapWidth;
                bool isDigRotatedOnMap = digX >= 0 && digX < mapWidth
                                          && digZ >= 0 && digZ < mapWidth;

                if (isNextPosOnMap && isDigRotatedOnMap)
                {
                    heightmap[digX, digZ] -= erosionWindStrength;
                    heightmap[pileX, pileZ] += erosionWindStrength;
                }
            }
        }
        myTerrainData.SetHeights(0, 0, heightmap);
    }
    private void ErosionCanyon()
    {
        EditorUtility.DisplayProgressBar("Digging canyon",
                     "Finding canyion path...", 0);

        int mapWidth = myTerrainData.heightmapResolution;
        float[,] heightmap = myTerrainData.GetHeights(0, 0, mapWidth, mapWidth);

        // find the line at the very bottom of the canyon
        var bottomPath = CanyonFindBottomPath(mapWidth);
        CanyonDigBottomLine(heightmap, bottomPath);

        List<Vector2> currentEdge = bottomPath.ToList();

        // Dig the flat bottom of the canyon
        for (int i = 0; i < erosionCanyonBottomWidth; i++)
        {
            EditorUtility.DisplayProgressBar("Digging canyon",
             "Digging bottom width " + i + " of " + erosionCanyonBottomWidth,
             ((float)i / (float)erosionCanyonBottomWidth));

            // dig around the edge, and set the new one as the current canyon edge
            currentEdge = DigAroundEdge(mapWidth, heightmap, currentEdge, erosionCanyonBottomSteepness, 1f);
        }

        // Dig the steps of the canyon
        for (int i = 0; i < erosionCanyonStepNumber; i++)
        {
            EditorUtility.DisplayProgressBar("Digging canyon",
             "Digging side steps " + i + " of " + erosionCanyonStepNumber,
             ((float)i / (float)erosionCanyonStepNumber));

            // dig around the edge, and set the new one as the current canyon edge
            // steepness is dictated by number of steps to accommodate, with a random factor so steps are not equal
            float randomScaler = UnityEngine.Random.Range(0.02f, 4f);
            float steepness = ((erosionCanyonTopStepHeight - erosionCanyonBottomHeight) / (float)erosionCanyonStepNumber)
                              * randomScaler;
            currentEdge = DigAroundEdge(mapWidth, heightmap, currentEdge, steepness, 1f);

            // dig the "step"
            int stepwidth = UnityEngine.Random.Range(1, erosionCanyonMaxStepWidth);
            for (int j = 0; j < stepwidth; j++)    // TODO make this sometimes only for irregular steps
            {
                float probability = 1;
                if (j>0) { probability = 1f / (float)j; }
                currentEdge = DigAroundEdge(mapWidth, heightmap, currentEdge, 0, probability);
            }

        }
        EditorUtility.ClearProgressBar();
        myTerrainData.SetHeights(0, 0, heightmap);
    }

    /// <summary>
    /// Finds a random path that crosses the heightmap on the X direction.
    /// Path is random, and dictated by erosionCanyonMeandering.
    /// </summary>
    /// <param name="mapWidth">Heightmap resolution</param>
    /// <returns>Array of Vector2 points that define the bottom line of the canyon</returns>
    private Vector2[] CanyonFindBottomPath(int mapWidth)
    {
        Vector2[] bottomPath = new Vector2[mapWidth];

        int currentZ = UnityEngine.Random.Range(0, mapWidth);
        for (int x = 0; x < mapWidth; x++)
        {
            currentZ = UnityEngine.Random.Range(currentZ - erosionCanyonMeandering, currentZ + erosionCanyonMeandering + 1);
            currentZ = Mathf.Clamp(currentZ, 0, mapWidth -1);
            bottomPath[x] = new Vector2(x, currentZ);
        }
        return bottomPath;
    }

    private void CanyonDigBottomLine(float[,] heightmap, Vector2[] bottomPath)
    {
        foreach (Vector2 point in bottomPath)
        {
            if (heightmap[(int)point.x, (int)point.y] <= erosionCanyonBottomHeight) { continue; }
            heightmap[(int)point.x, (int)point.y] = erosionCanyonBottomHeight;
        }
    }

    /// <summary>
    /// Digs one layer of neighbours around the list of given vectors (the edge).
    /// </summary>
    /// <param name="mapWidth">Heightmap resolution</param>
    /// <param name="heightmap">working heightmap</param>
    /// <param name="currentEdge">Current edgge of the canyon</param>
    /// <param name="steepness">Height value that each neigbour will add to current edge height</param>
    /// <returns>A new list of Vectors that detail the new edge of the canyon</returns>
    private List<Vector2> DigAroundEdge(int mapWidth, float[,] heightmap, List<Vector2> currentEdge, float steepness, float probability)
    {        
        List<Vector2> newEdge = new List<Vector2>();
        
        // for all points that are currently on the edge of the canyon
        foreach (Vector2 point in currentEdge)
        {
            // probability check
            if (Random.value > probability)
            {
                // add neigbour to new bottom edge if not already there
                bool isAlreadyInList = newEdge.Contains(point);
                if (!isAlreadyInList) { newEdge.Add(point); }

                continue;
            }

            List<Vector2> neighbours = GetNeighbours(mapWidth, (int)point.x, (int)point.y);
            foreach (Vector2 neighbour in neighbours)
            {
                // consider only neighbours that are not lower than current level
                float thisHeight = heightmap[(int)point.x, (int)point.y];
                float neighbourHeight = heightmap[(int)neighbour.x, (int)neighbour.y];
                if (neighbourHeight <= thisHeight) { continue; }

                // add neigbour to new bottom edge if not already there
                bool isAlreadyInList = newEdge.Contains(neighbour);
                if (!isAlreadyInList) { newEdge.Add(neighbour); }

                // set neighbour slightly higher than current (based on bottom steepness setting)
                float newNeighbourHeight = Mathf.Clamp(thisHeight + steepness,
                                                       thisHeight,
                                                       neighbourHeight);
                heightmap[(int)neighbour.x, (int)neighbour.y] = newNeighbourHeight;
            }
        }
        return newEdge;
    }
    #endregion

    #region Helpers
    private List<float> FindValidNeighbours(float[,] heightMap, int mapWidth, int x, int z)
    {
        List<float> borderValues = new List<float>();

        if (x != 0)    //neighbours to the left
        {
            borderValues.Add(heightMap[x - 1, z]);

            if (z != 0) { borderValues.Add(heightMap[x - 1, z - 1]); }
            if (z != mapWidth - 1) { borderValues.Add(heightMap[x - 1, z + 1]); }
        }

        if (x != mapWidth - 1)    //neighbours to the right
        {
            borderValues.Add(heightMap[x + 1, z]);

            if (z != 0) { borderValues.Add(heightMap[x + 1, z - 1]); }
            if (z != mapWidth - 1) { borderValues.Add(heightMap[x + 1, z + 1]); }
        }
        if (z != 0) { borderValues.Add(heightMap[x, z - 1]); }    // bottom centre
        if (z != mapWidth - 1) { borderValues.Add(heightMap[x, z + 1]); }    // top centre

        return borderValues;
    }
    private List<Vector2> GetNeighbours(int mapWidth, int x, int z)
    {
        List<Vector2>neighbours = new List<Vector2>();

        if (x > 0)    //neighbours to the left
        {
            neighbours.Add(new Vector2(x-1, z));    // left centre
            if (z > 0) { neighbours.Add(new Vector2(x-1, z-1)); }    // left down
            if (z < mapWidth - 1) { neighbours.Add(new Vector2(x-1, z+1)); }    // left up
        }

        if (x < mapWidth - 1)    //neighbours to the right
        {
            neighbours.Add(new Vector2(x+1, z));    // right centre

            if (z > 0) { neighbours.Add(new Vector2(x+1, z-1)); }    // right down
            if (z < mapWidth - 1) { neighbours.Add(new Vector2(x+1, z+1)); }    // right up
        }
        if (z > 0) { neighbours.Add(new Vector2(x, z-1)); }    // bottom centre
        if (z < mapWidth - 1) { neighbours.Add(new Vector2(x, z+1)); }    // top centre

        return neighbours;
    }
    private bool IsOnShore(float[,] heightmap, int x, int z, List<Vector2> neighbours, out Vector2 shoreNeightbour)
    {
        if (heightmap[x, z] < waterHeight)
        {
            foreach (Vector2 neighbour in neighbours)
            {
                float neighbourHeight = heightmap[(int)neighbour.x, (int)neighbour.y];
                if (neighbourHeight >= waterHeight)
                {
                    shoreNeightbour = neighbour;
                    return true;
                }
            }
        }
        shoreNeightbour = Vector2.zero;
        return false;
    }

    #endregion

    #region Callback functions
    private void OnEnable()
    {
        Debug.Log("Initialising Terrain Data");
        myTerrain = GetComponent<Terrain>();
        myTerrainData = myTerrain.terrainData;
    }

    private void Awake()
    {
        SerializedObject tagManager = new SerializedObject(
                                        AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);

        // add the required tags to terrain
        SerializedProperty tagsProp = tagManager.FindProperty("tags");
        AddOptionToProperty(tagsProp, "Terrain", PropertyType.Tag);
        AddOptionToProperty(tagsProp, "Cloud", PropertyType.Tag);
        AddOptionToProperty(tagsProp, "Shore", PropertyType.Tag);
        AddOptionToProperty(tagsProp, "ShoreCollider", PropertyType.Tag);

        // put terrain on Terrain layer
        SerializedProperty layerProp = tagManager.FindProperty("layers");
        terrainLayer = AddOptionToProperty(layerProp, "Terrain", PropertyType.Layer);

        tagManager.ApplyModifiedProperties();
        this.gameObject.tag = "Terrain";
        this.gameObject.layer = terrainLayer;
    }

    private int AddOptionToProperty(SerializedProperty property, string newOption, PropertyType propType)
    {
        for (int i = 0; i < property.arraySize; i++)
        {
            SerializedProperty propSettingAtIndex = property.GetArrayElementAtIndex(i);
            if (propSettingAtIndex.stringValue.Equals(newOption))
            { return i; }
        }

        // only runs if property was not found among existing ones
        switch (propType)
        {
            case PropertyType.Tag:
                property.InsertArrayElementAtIndex(0);
                SerializedProperty newTagProp = property.GetArrayElementAtIndex(0);
                newTagProp.stringValue = newOption;
                break;

            case PropertyType.Layer:
                for (int j = 8; j < property.arraySize; j++)
                {
                    SerializedProperty layerAtIndex = property.GetArrayElementAtIndex(j);
                    if (layerAtIndex.stringValue == "")
                    {
                        Debug.Log("Adding new layer to project: " + newOption);
                        layerAtIndex.stringValue = newOption;
                        return j;
                    }

                }
                break;

            default:
                break;
        }
        return -1;
    }

    #endregion
}
