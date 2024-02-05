using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "AudioScriptableObject", menuName = "ScriptableObjects/Audio")]
public class AudioScriptableObject : ScriptableObject
{
    public AudioClip carEngine;
    public AudioClip boostStart;
    public AudioClip boostOn;
    public AudioClip crash;
}