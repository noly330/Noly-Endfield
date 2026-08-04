using UnityEngine;

[CreateAssetMenu(menuName = "Endfield/Combat/CombatConfig")]

public class CombatConfigSO : ScriptableObject
{
    [Header("基础配置")]
    public string combatName;
    public float coldTime;

    [Header("战斗交互配置")]
    public CombatInteractionConfig[] interactionConfigs;

    [Header("战斗检测配置")]
    public CombatDetectConfig[] detectConfigs;

    [Header("战斗攻击特效配置")]
    public CombatVFXConfig[] sfxConfigs;

    [Header("战斗音效配置")]
    public CombatSFXConfig[] vfxConfigs;
    
    [Header("战斗能量恢复配置")]
    public CombatRecoverEnergyConfig[] recoverEnergyConfigs;
}
public enum CombatDetectType
{
    None,Area,Single
}


[System.Serializable]

public class CombatInteractionConfig
{
    public string hitName;
    public float damageMul;
}

[System.Serializable]
public class CombatDetectConfig
{
    public float startTime;
    public Vector3 position;
    public Vector3 rotation;
    public Vector3 scale;
    public CombatDetectType detectType;
}

[System.Serializable]
public class CombatVFXConfig
{
    public float startTime;
    public GameObject VFXObject;
    public Vector3 position;
    public Vector3 rotation;
    public Vector3 scale;
}

[System.Serializable]
public class CombatSFXConfig
{
    public float startTime;
    public AudioClip audioClip;
    public float volume;
}

[System.Serializable]
public class CombatRecoverEnergyConfig
{
    public int attackIndex;  //在第attackIndex段攻击的时候触发
    public float energyRecover;
}

