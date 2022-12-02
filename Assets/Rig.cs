using UnityEngine;
using UnityEngine.XR;
using System;

[Serializable]
public class Rig
{
  Monolith mono;

  public InputDevice headset, offCon, mainCon;

  // Tracking
  public Vector3 offset = Vector3.down;
  public Vector3 headsetPos, offConPos, mainConPos;
  [HideInInspector]
  public Quaternion headsetRot, offConRot, mainConRot;

  // Input
  public Btn mainConTrigger = new Btn();
  public Btn mainConOne = new Btn();
  public Vector2 mainConJoystick;

  // Player
  public Vector3 mainCursor;
  public Vector3Int cvPos;

  // Stretch Cursor
  public Vector3 cursor;
  public float str = 6;

  public void Start(Monolith mono)
  {
    this.mono = mono;

    headset = InputDevices.GetDeviceAtXRNode(XRNode.Head);
    offCon = InputDevices.GetDeviceAtXRNode(XRNode.LeftHand);
    mainCon = InputDevices.GetDeviceAtXRNode(XRNode.RightHand);
  }

  public void Update()
  {
    // Tracking
    headset.TryGetFeatureValue(CommonUsages.devicePosition, out headsetPos);
    headset.TryGetFeatureValue(CommonUsages.deviceRotation, out headsetRot);

    offCon.TryGetFeatureValue(CommonUsages.devicePosition, out offConPos);
    offCon.TryGetFeatureValue(CommonUsages.deviceRotation, out offConRot);

    mainCon.TryGetFeatureValue(CommonUsages.devicePosition, out mainConPos);
    mainCon.TryGetFeatureValue(CommonUsages.deviceRotation, out mainConRot);

    headsetRot.Normalize();
    offConRot.Normalize();
    mainConRot.Normalize();

    headsetPos += offset;
    offConPos += offset;
    mainConPos += offset;

    mono.headsetCam.transform.position = headsetPos;
    mono.headsetCam.transform.rotation = headsetRot;

    // Input
    bool state;
    mainCon.TryGetFeatureValue(CommonUsages.triggerButton, out state);
    mainConTrigger.On(state);

    mainCon.TryGetFeatureValue(CommonUsages.primaryButton, out state);
    mainConOne.On(state);

    mainCon.TryGetFeatureValue(CommonUsages.primary2DAxis, out mainConJoystick);

    // cursor
    Quaternion rot = mainConRot;
    Vector3 mainDir = new Vector3(mainConJoystick.x, 0, mainConJoystick.y);
    if (mainDir.sqrMagnitude > 0)
    {
      rot *= Quaternion.LookRotation(mainDir);
    }

    mainCursor = mono.player.pos + (rot * Vector3.forward);
    cvPos = mono.VoxelPos(mainCursor);

    // orbitcam
    Transform camForm = mono.headsetCam.transform;
    camForm.rotation = headsetRot;
    camForm.position = mono.voxelCenter + (headsetRot * Vector3.back * 10);

    // Stretch Cursor
    float stretch = Vector3.Distance(mainConPos, offConPos);
    cursor = mainConPos + mainConRot * Vector3.forward * stretch * str;
  }
}

public class Btn
{
  public bool onPress, held, onUp;

  public void On(bool state)
  {
    onPress = onUp = false;
    if (state)
    {
      if (!held) { onPress = true; }
      held = true;
    }
    else
    {
      if (held) { onUp = true; }
      held = false;
    }
  }
}