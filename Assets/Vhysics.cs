using System;
using UnityEngine;

[Serializable]
public class Vhysics
{
  Monolith mono;

  public void Start(Monolith mono)
  {
    this.mono = mono;
  }

  public void Update()
  {
    // bounds
    VoxelCollision(mono.player);
    VoxelCollision(mono.pump);

    for (int i = 0; i < mono.bats.Length; i++)
    {
      VoxelObject bat = mono.bats[i];
      if (bat.active)
      {
        VoxelCollision(bat);
      }
    }

    // FAT
    // hit callbacks
    // rectangular bounds
    // larger than one voxel
  }

  public void VoxelCollision(VoxelObject vobj)
  {
    Vector3 toPos = vobj.pos + vobj.voxelBody.velocity * Time.deltaTime;

    int w = 0;
    while (w < 3)
    {
      Vector3 clampPos = new Vector3(
        Mathf.Clamp(toPos.x, Bound(vobj, 0, -1), Bound(vobj, 0, 1)),
        Mathf.Clamp(toPos.y, Bound(vobj, 1, -1), Bound(vobj, 1, 1)),
        Mathf.Clamp(toPos.z, Bound(vobj, 2, -1), Bound(vobj, 2, 1))
      );

      float largest = 0;
      int largeIndex = -1;
      for (int j = 0; j < 3; j++)
      {
        float dist = Mathf.Abs(toPos[j] - clampPos[j]);
        if (dist > largest)
        {
          largeIndex = j;
          largest = dist;
        }
      }

      if (largeIndex > -1)
      {
        toPos[largeIndex] = clampPos[largeIndex];
        vobj.voxelBody.velocity[largeIndex] *= -0.25f; // Bounce
      }
      else
      {
        break;
      }

      w++;
    }

    vobj.pos = toPos;
  }

  public float Bound(VoxelObject vobj, int axis, int dir)
  {
    Vector3Int step = Vector3Int.zero;
    step[axis] = dir;

    float bound = Mathf.Infinity * dir;
    float closest = Mathf.Infinity;
    for (int i = 0; i < mono.allDirs.Length; i++)
    {
      Vector3 d = (Vector3)mono.allDirs[i] * (vobj.voxelBody.boundRadius - 0.001f);
      d[axis] = 0;
      Vector3 vPos = Voxelcast(mono.VoxelPos(vobj.pos + d), step);
      float dist = Mathf.Abs(vPos[axis] - vobj.pos[axis]);
      if (dist < closest)
      {
        bound = vPos[axis];
        closest = dist;
      }
    }
    // when hit ?
    return bound + (((1 - vobj.voxelBody.boundRadius * 2) / 2) * dir);
  }

  public Vector3Int Voxelcast(Vector3Int from, Vector3Int step)
  {
    Vector3Int vPos = from;
    int i = 0;
    while (i < 15)
    {
      vPos += step;
      if (!mono.InVoxel(vPos))
      {
        vPos -= step;
        break;
      }

      i++;
    }

    return vPos;
  }
}