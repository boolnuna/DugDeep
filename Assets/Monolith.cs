using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using NaughtyAttributes;
using Random = UnityEngine.Random;
using ExtensionMethods;
using UnityEngine.XR;

public class Monolith : MonoBehaviour
{
  public Voxel[] voxels = new Voxel[48];
  public int vIndex = 0;

  public Worm leftWorm, rightWorm;

  public VoxelObject player;
  public VoxelObject pump;
  VoxelObject pumpObject = null;

  public VoxelObject[] bats = new VoxelObject[3];

  public Rig rig = new Rig();
  public Simulate simulate = new Simulate();
  public Vhysics vhysics = new Vhysics();
  public Render render;

  [Header("References")]
  public Camera headsetCam;

  void OnEnable()
  {
    render.Enable(this);
  }

  void OnDisable()
  {
    render.Disable();
  }

  [Button]
  public void LightPass()
  {
    Lighting.CreateTexture3D();
  }

  [Button]
  public void Reset()
  {
    voxels = new Voxel[voxels.Length];
    voxels[0] = new Voxel(Vector3Int.zero);

    leftWorm.pos = rightWorm.pos = Vector3Int.zero;
    leftWorm.dirIndex = rightWorm.dirIndex = 0;
  }

  [Button]
  public void Step()
  {
    simulate.Step(this);
  }

  void Start()
  {
    rig.Start(this);
    vhysics.Start(this);
  }

  public Vector3Int VoxelPos(Vector3 pos)
  {
    return new Vector3Int(Mathf.RoundToInt(pos.x), Mathf.RoundToInt(pos.y), Mathf.RoundToInt(pos.z));
  }

  public bool InVoxel(Vector3Int pos)
  {
    for (int i = 0; i < voxels.Length; i++)
    {
      if (voxels[i].pos == pos) { return true; }
    }
    return false;
  }

  [ReadOnly]
  public Vector3 voxelCenter;
  float stepTime = 0;
  public float stepInterval = 3;
  void Update()
  {
    rig.Update();
    
    if (stepTime < Time.time)
    {
      simulate.Step(this);

      stepTime = Time.time + stepInterval;
    }

    // throwing
    if (pumpObject == null)
    {
      if (rig.mainConOne.onPress)
      {
        pump.pos = player.pos;
        pump.voxelBody.velocity = rig.mainConRot * Vector3.forward * 10;
      }
    }
    else
    {
      pump.voxelBody.velocity = pumpObject.voxelBody.velocity = Vector3.zero;

      if (rig.mainConOne.onPress)
      {
        pumpObject.scale *= 1.333f;

        if (pumpObject.scale > 3)
        {
          pumpObject.scale = 1;
          pumpObject.active = false;
          pumpObject = null;
        }
      }
    }

    // mining
    bool mainTrigger = false;
    rig.mainCon.TryGetFeatureValue(CommonUsages.triggerButton, out mainTrigger);
    if (mainTrigger)
    {
      if (!InVoxel(rig.cvPos))
      {
        voxels[vIndex].pos = rig.cvPos;
        vIndex++;
        if (vIndex == voxels.Length) { vIndex = 0; }

        // simulate.Step(this);
      }
    }

    Graphics.DrawMesh(render.meshPieceDebug,
      rig.mainCursor, Quaternion.identity,
      render.matObject, 0
    );

    if (!InVoxel(rig.cvPos))
      Graphics.DrawMesh(render.meshPieceDebug,
        rig.cvPos, Quaternion.identity,
        render.matObject, 0
      );

    // Movement
    Quaternion offRot = Quaternion.identity;
    rig.offCon.TryGetFeatureValue(CommonUsages.deviceRotation, out offRot);

    Vector2 offStick = Vector2.zero;
    rig.offCon.TryGetFeatureValue(CommonUsages.primary2DAxis, out offStick);
    offStick += new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
    Vector3 offDir = new Vector3(offStick.x, 0, offStick.y);
    if (offDir.sqrMagnitude > 0)
    {
      offRot *= Quaternion.LookRotation(offDir);
    }

    Vector3 vel = player.voxelBody.velocity;
    vel += offRot * Vector3.forward * offStick.magnitude * 6 * Time.deltaTime;

    if (offStick.sqrMagnitude == 0)
    {
      vel.x *= 1 - (60 * Time.deltaTime);
      vel.z *= 1 - (60 * Time.deltaTime);
    }

    // jumping ?
    bool jumpBtn = false;
    rig.offCon.TryGetFeatureValue(CommonUsages.triggerButton, out jumpBtn);
    if (Input.GetKeyDown(KeyCode.Space) || (jumpBtn && Mathf.Abs(vel.y) < 0.1f))
    {
      vel.y = 8;
    }
    float gravStr = 1;
    if ((!Input.GetKey(KeyCode.Space) && !jumpBtn) || vel.y < 0)
    {
      gravStr = 3;
    }

    vel.y += -9.81f * gravStr * Time.deltaTime;
    player.voxelBody.velocity = Vector3.ClampMagnitude(vel, 60);

    // the refactoring needs to be reconsidered
    // I don't want an arbitrary list of VoxelObjects
    // I'd like a Player
    // an array of each enemy type (I want them to be deeply unique)
    // an array of any/each physics object

    // just deliberate what needs to physicsed in the vphysics Update method

    // goal a physics object the player can throw in the direction of the cursor (auto retrieve)

    // Code motionless Enemy core, where the pump sticks to it, and then the player can pump
    for (int i = 0; i < bats.Length; i++)
    {
      VoxelObject bat = bats[i];

      if (!bat.active)
      {
        bat.pos = voxels[Random.Range(0, voxels.Length)].pos;
        bat.rot = Quaternion.identity;
        bat.mesh = render.meshPieceDebug;
        bat.scale = 1;

        bat.voxelBody.boundRadius = 0.4f;

        bat.active = true;
      }
      else
      {
        if (Random.value < Time.deltaTime)
        {
          bat.voxelBody.velocity += Random.rotation * Vector3.forward;
        }

        if (pumpObject == null && Vector3.Distance(pump.pos, bat.pos) < bat.voxelBody.boundRadius)
        {
          pumpObject = bat;
        }
      }
    }

    vhysics.Update();
    render.Update();
  }

  [HideInInspector]
  public Vector3Int[] dirs = new Vector3Int[] {
    new Vector3Int(-1, 0, 0), new Vector3Int(0, -1, 0), new Vector3Int(0, 0, -1),
    new Vector3Int(1, 0, 0), new Vector3Int(0, 1, 0), new Vector3Int(0, 0, 1)
  };

  [HideInInspector]
  public Vector3Int[] allDirs = new Vector3Int[] {
    new Vector3Int(-1, 0, 0),
    new Vector3Int(0, -1, 0),
    new Vector3Int(0, 0, -1),
    new Vector3Int(-1, -1, 0),
    new Vector3Int(0, -1, -1),
    new Vector3Int(-1, 0, -1),

    new Vector3Int(1, 0, 0),
    new Vector3Int(0, 1, 0),
    new Vector3Int(0, 0, 1),
    new Vector3Int(1, 1, 0),
    new Vector3Int(0, 1, 1),
    new Vector3Int(1, 0, 1),

    new Vector3Int(-1, 1, 0),
    new Vector3Int(0, -1, 1),
    new Vector3Int(1, 0, -1),
    new Vector3Int(1, -1, 0),
    new Vector3Int(0, 1, -1),
    new Vector3Int(-1, 0, 1)
  };

  public bool Outside(Vector3Int pos)
  {
    for (int v = 0; v < voxels.Length; v++)
    {
      if (pos == voxels[v].pos) { return false; }
    }
    return true;
  }
}

[Serializable]
public class Voxel
{
  public Vector3Int pos;

  public Voxel(Vector3Int pos)
  {
    this.pos = pos;
  }
}

[Serializable]
public class Worm
{
  public Vector3Int pos;
  public int dirIndex;

  public Worm(Vector3Int pos)
  {
    this.pos = pos;
    this.dirIndex = 0;
  }
}

[Serializable]
public class VoxelObject
{
  public bool active;

  public Vector3 pos;
  public Quaternion rot;
  public float scale;

  public VoxelBody voxelBody;

  public Mesh mesh;
  [HideInInspector] public Matrix4x4 m4;

  public void Draw(Material mat)
  {
    m4.SetTRS(pos, rot, Vector3.one * scale);
    Graphics.DrawMesh(mesh, m4, mat, 0);
  }
}

[Serializable]
public class VoxelBody
{
  public float boundRadius;
  public Vector3 velocity;
  // public float mass;
}

namespace ExtensionMethods
{
  public static class MyExtensions
  {
    public static int Rollover(this int index, int by, int length)
    {
      int rollover = index + by;
      int delta = rollover - length;
      if (delta >= 0)
      {
        rollover = delta;
      }

      return rollover;
    }
  }
}