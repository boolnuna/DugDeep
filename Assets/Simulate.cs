using UnityEngine;
using System;
using Random = UnityEngine.Random;

public class Simulate
{
  public Generate generate = new Generate();

  public void Step(Monolith mono)
  {
    generate.Step(mono);

    Vector3 center = Vector3.zero;
    for (int i = 0; i < mono.voxels.Length; i++)
    {
      center += mono.voxels[i].pos;
    }

    mono.voxelCenter = center / mono.voxels.Length;
  }
}

public class Generate
{
  public void Step(Monolith mono)
  {
    WormStep(mono, mono.leftWorm, mono.rightWorm, 1);
    WormStep(mono, mono.rightWorm, mono.leftWorm, -1);
  }

  void WormStep(Monolith mono, Worm worm, Worm otherWorm, int stepDir)
  {
    worm.pos += mono.dirs[worm.dirIndex];

    if (Random.value > 0.666f) { worm.dirIndex += stepDir; }
    else
    {
      // we want to move closer to the other worm
      // cycle through the directions and pick the closest
      worm.dirIndex = otherWorm.dirIndex;
      Vector3Int delta = otherWorm.pos - worm.pos;
      if (delta.sqrMagnitude > 0)
      {
        // pick the greatest axis
        int greatest = 0;
        for (int i = 0; i < 3; i++)
        {
          if (Mathf.Abs(delta[i]) > Mathf.Abs(delta[greatest])) { greatest = i; }
        }

        Vector3Int newDir = Vector3Int.zero;
        newDir[greatest] = delta[greatest] / Mathf.Abs(delta[greatest]);
        for (int i = 0; i < mono.dirs.Length; i++)
        {
          if (newDir == mono.dirs[i]) { worm.dirIndex = i; }
        }
      }
    }
    if (worm.dirIndex == mono.dirs.Length) { worm.dirIndex = 0; }
    if (worm.dirIndex < 0) { worm.dirIndex = mono.dirs.Length - 1; }


    for (int i = 0; i < mono.voxels.Length; i++)
    {
      if (worm.pos == mono.voxels[i].pos)
      {
        return;
      }
    }

    mono.voxels[mono.vIndex].pos = worm.pos;

    mono.vIndex++;
    if (mono.vIndex == mono.voxels.Length) { mono.vIndex = 0; }
  }
}