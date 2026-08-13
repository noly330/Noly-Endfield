using System;
using System.Collections.Generic;
using UnityEngine;

namespace Endfield.Module.Audio
{
    [CreateAssetMenu(menuName = "Endfield/Audio/SoundData")]
    public class SoundData : ScriptableObject
    {
        [Serializable]
        public class SoundInfo
        {
            public SoundStyle style;
            public AudioClip[] clips;
        }
        [SerializeField] private List<SoundInfo> _infos = new();

        public AudioClip GetClip(SoundStyle style)
        {
            foreach (var info in _infos)
            {
                if (info.style == style && info.clips != null && info.clips.Length > 0)
                    return info.clips[UnityEngine.Random.Range(0, info.clips.Length)];
            }
            return null;
        }
        public AudioClip GetClip(SoundStyle style, int index)
        {
            foreach (var info in _infos)
            {
                if (info.style == style && info.clips != null && info.clips.Length > 0)
                    return info.clips[Mathf.Clamp(index, 0, info.clips.Length - 1)];
            }
            return null;
        }
    }
    public enum SoundStyle
    {
        Attack,   // 普攻
        AttackVoice,    // 普攻语音台词
        Skill,    // 技能
        SkillVoice,    // 技能语音台词
    }
}

