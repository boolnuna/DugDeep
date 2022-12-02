using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using ExtensionMethods;

[Serializable]
public class Render
{
  Monolith mono;

  public void Enable(Monolith mono)
  {
    this.mono = mono;
  }

  public void Disable()
  {
  }

  public Mesh meshVoxelDebug, meshPieceDebug, meshCube;
  public Material matVoxelDebug, matObject, matBounds, matEnemy, matPath;

  Matrix4x4 tempM4 = new Matrix4x4();
  public void Update()
  {
    Voxels();

    mono.player.Draw(matObject);
    mono.pump.Draw(matObject);

    // Draw Enemy
    for (int i = 0; i < mono.bats.Length; i++)
    {
      VoxelObject bat = mono.bats[i];
      if (bat.active)
      {
        bat.Draw(matEnemy);
      }
    }

    // Render vhysics bounds
    // tempM4.SetTRS(
    //   voxelObject.pos, 
    //   Quaternion.identity, 
    //   Vector3.one * voxelObject.voxelBody.boundRadius * 2
    // );
    // Graphics.DrawMesh(meshCube, tempM4, matBounds, 0);
  }

  List<Matrix4x4> voxelM4 = new List<Matrix4x4>();
  void Voxels()
  {
    voxelM4.Clear();
    for (int i = 0; i < mono.voxels.Length; i++)
    {
      if (mono.voxels[i] != null)
      {
        for (int d = 0; d < mono.dirs.Length; d++)
        {
          if (mono.Outside(mono.voxels[i].pos + mono.dirs[d]))
          {
            Vector3 renderPos = mono.voxels[i].pos + (Vector3)mono.dirs[d] / 2;
            Matrix4x4 m4 = new Matrix4x4();
            m4.SetTRS(renderPos,
              Quaternion.LookRotation(renderPos - mono.voxels[i].pos),
              Vector3.one
            );
            voxelM4.Add(m4);
          }
        }
      }
    }

    if (voxelM4.Count > 0)
    {
      Graphics.DrawMeshInstanced(meshVoxelDebug, 0, matVoxelDebug, voxelM4);
    }

  }
}