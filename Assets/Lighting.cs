using UnityEngine;
using UnityEditor;

public class Lighting
{
  public static void CreateTexture3D()
  {
    // Configure the texture
    int size = 32;
    TextureFormat format = TextureFormat.RGBA32;
    TextureWrapMode wrapMode = TextureWrapMode.Clamp;

    // Create the texture and apply the configuration
    Texture3D texture = new Texture3D(size, size, size, format, false);
    texture.wrapMode = wrapMode;

    // Create a 3-dimensional array to store color data
    Color[] colors = new Color[size * size * size];

    // Populate the array so that the x, y, and z values of the texture will map to red, blue, and green colors
    float inverseResolution = 1.0f / (size - 1.0f);
    for (int z = 0; z < size; z++)
    {
      int zOffset = z * size * size;
      for (int y = 0; y < size; y++)
      {
        int yOffset = y * size;
        for (int x = 0; x < size; x++)
        {
          colors[x + yOffset + zOffset] = new Color(x * inverseResolution,
              y * inverseResolution, z * inverseResolution, 1.0f);
        }
      }
    }

    // 3d texture?? yup
    // just use brush fire algorithm to light the cave
    // that will make nicely for the base ambiance
    // then you can handle light paths or other nice depth indicating things

    // Copy the color values to the texture
    texture.SetPixels(colors);

    // Apply the changes to the texture and upload the updated texture to the GPU
    texture.Apply();

#if UNITY_EDITOR
    // Save the texture to your Unity Project
    AssetDatabase.CreateAsset(texture, "Assets/Example3DTexture.asset");
#endif
  }
}