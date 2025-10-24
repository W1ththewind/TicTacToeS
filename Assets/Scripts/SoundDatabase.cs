using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SoundDB", menuName = "TicTacToeS/SoundDB")]
public class SoundDatabase : ScriptableObject, ISerializationCallbackReceiver
{

    [Serializable]
    public struct AudioClipContainer
    {
        public string ClipName;
        public AudioClip Clip;
    }

    [SerializeField]
    protected List<AudioClipContainer> _clipsInDB = new();
    public Dictionary<string, AudioClip> AudioClipDict = new Dictionary<string, AudioClip>();

    public void OnBeforeSerialize()
    {
        // _clipsInDB.Clear();
        // foreach (var ac in AudioClipDict)
        // {
        //     _clipsInDB.Add(new() { ClipName = ac.Key, Clip = ac.Value });
        // }
    }

    public void OnAfterDeserialize()
    {
        AudioClipDict.Clear();
        for (int i = 0; i < _clipsInDB.Count; i++)
        {
            if (!AudioClipDict.ContainsKey(_clipsInDB[i].ClipName))
            {
                AudioClipDict.Add(_clipsInDB[i].ClipName, _clipsInDB[i].Clip);
            }

        }

    }

    public AudioClip Get(string clipName)
    {
        if (AudioClipDict.ContainsKey(clipName))
        {
            return AudioClipDict[clipName];
        }

        return null;
    }

}
