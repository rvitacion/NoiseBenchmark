using System.Collections;
using System.Diagnostics;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Profiling;

public class NoiseGenerator : MonoBehaviour {

  public Dropdown noiseTypeField;
  public Dropdown fractalTypeField;
  public InputField resolutionField;
  public InputField seedField;
  public InputField frequencyField;
  public InputField octavesField;
  public InputField lacunarityField;
  public InputField gainField;
  public InputField iterationsField;

  public Text log;
  public InputField generationTimeField;
  public InputField memoryUsageField;
  public InputField heapSizeField;
  public RawImage mapRenderer;

  FastNoise noiseModule;
	FastNoise.NoiseType ntype = FastNoise.NoiseType.PerlinFractal;
  FastNoise.FractalType ftype = FastNoise.FractalType.FBM;
  int resolution = 1024;
  int seed = 5833;
  float frequency = 0.01f;
  int octaves = 1;
  float lacunarity = 2.0f;
  float gain = 0.5f;
  int iterations = 1;
  Texture2D heightMap;

  StreamWriter file;
  Stopwatch timer;

  public NoiseGenerator() {
    noiseModule = new FastNoise(seed);
    noiseModule.SetNoiseType(ntype);
    noiseModule.SetFractalType(ftype);
    noiseModule.SetFrequency(frequency);
    noiseModule.SetFractalOctaves(octaves);
    noiseModule.SetFractalLacunarity(lacunarity);
    noiseModule.SetFractalGain(gain);
  }

  public void SetNoiseType() {
    if (noiseTypeField.value == 0) ntype = FastNoise.NoiseType.PerlinFractal;
    if (noiseTypeField.value == 1) ntype = FastNoise.NoiseType.SimplexFractal;
    if (noiseTypeField.value == 2) ntype = FastNoise.NoiseType.ValueFractal;
    if (noiseTypeField.value == 3) ntype = FastNoise.NoiseType.CubicFractal;
    noiseModule.SetNoiseType(ntype);
  }

  public void SetFractalType() {
    if (fractalTypeField.value == 0) ftype = FastNoise.FractalType.FBM;
    if (fractalTypeField.value == 1) ftype = FastNoise.FractalType.Billow;
    if (fractalTypeField.value == 2) ftype = FastNoise.FractalType.RigidMulti;
    noiseModule.SetFractalType(ftype);
  }

  public void SetResolution() {
    resolution = int.Parse(resolutionField.text);
  }

  public void SetSeed() {
    seed = int.Parse(seedField.text);
    noiseModule.SetSeed(seed);
  }

  public void SetFrequency() {
    frequency = float.Parse(frequencyField.text);
    noiseModule.SetFrequency(frequency);
  }

  public void SetOctaves() {
    octaves = int.Parse(octavesField.text);
    noiseModule.SetFractalOctaves(octaves);
  }

  public void SetLacunarity() {
    lacunarity = float.Parse(lacunarityField.text);
    noiseModule.SetFractalLacunarity(lacunarity);
  }

  public void SetGain() {
    gain = float.Parse(gainField.text);
    noiseModule.SetFractalGain(gain);
  }

  public void Generate() {
    heightMap = new Texture2D(resolution,resolution);
    mapRenderer.texture = heightMap;
    timer = new Stopwatch();
    timer.Start();
    long before = Profiler.GetTotalAllocatedMemoryLong();
    for (int j=0; j<resolution; j++) {
      for (int k=0; k<resolution; k++) {
        float height = ((noiseModule.GetNoise(j,k) + 1.0f) * 0.5f);
        heightMap.SetPixel(j,k, new Color(height,height,height));
      }
    }
    timer.Stop();
    memoryUsageField.text = (Profiler.GetTotalAllocatedMemoryLong()-before).ToString();
    generationTimeField.text = timer.ElapsedMilliseconds.ToString();
    heapSizeField.text = Profiler.GetMonoHeapSizeLong().ToString();
    heightMap.Apply();
  }

  public void SetIterations() {
    iterations = int.Parse(iterationsField.text);
  }

  IEnumerator Benchmark() {
    
    System.Random rng = new System.Random();

    for(int i=0; i<noiseTypeField.options.Count; i++) {
      noiseTypeField.value = i;
      file = new StreamWriter(noiseTypeField.options[noiseTypeField.value].text + resolution + "_" + octaves + "oct_x" + iterations + ".csv");
      for(int j=0; j<iterations; j++) {
        log.text = noiseTypeField.options[noiseTypeField.value].text + " Noise - Iteration " + (j+1) + "/" + iterations;
        seedField.text = rng.Next().ToString();
        SetSeed();
        Generate();
        file.WriteLine(generationTimeField.text + "," + memoryUsageField.text + "," + heapSizeField.text);
        yield return null;
      }
      file.Close();
    }

    log.text = "Benchmark Complete!";
  }

  public void runBenchmark() {
    StartCoroutine("Benchmark");
  }

  void ClearHeightMap() {
    Color[] values = heightMap.GetPixels();
    for (int i=0; i<values.Length; i++) {
      values[i] = Color.black;
    }
    heightMap.SetPixels(values);
    heightMap.Apply();
  }

  void Start() {
    resolutionField.text = resolution.ToString();
    seedField.text = seed.ToString();
    frequencyField.text = frequency.ToString();
    octavesField.text = octaves.ToString();
    lacunarityField.text = lacunarity.ToString();
    gainField.text = gain.ToString();
    iterationsField.text = iterations.ToString();
    heightMap = new Texture2D(resolution,resolution);
    ClearHeightMap();
    mapRenderer.texture = heightMap;
  }
}
