using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Utils
{
    public static float FractalBrownianMotion(float x, float z,
                                              int octaves,
                                              float persistence,
                                              float frequencyMultiplier=2)
    {
        float total = 0;
        float frequency = 1;
        float amplitude = 1;
        float maxValue = 0;

        for (int i = 0; i<octaves; i++)
        {
            total += Mathf.PerlinNoise((x) * frequency,
                                       (z) * frequency) 
                                       * amplitude;
            maxValue += amplitude;
            amplitude *= persistence;
            frequency *= frequencyMultiplier;
        }

        return total / maxValue;
    }

    public static float Map(float value,
                            float originalMin, float originalMax,
                            float targetMin, float targetMax)
    {
        float originalRange = (originalMax - originalMin);
        float targetRange = (targetMax - targetMin);
        float ratio = targetRange / originalRange;
        float targetValue = (value - originalMin)
                            * ratio
                            + targetMin;
        return targetValue;
    }

    public static System.Random r = new System.Random();
    public static void Shuffle<T>(this IList<T> list)
    {
        int n = list.Count;
        while (n > 1)
        {
            n--;
            int k = r.Next(n + 1);
            T value = list[k];
            list[k] = list[n];
            list[n] = value;
        }
    }
}
