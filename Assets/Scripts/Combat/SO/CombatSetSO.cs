using UnityEngine;


[CreateAssetMenu(menuName = "Endfield/Combat/CombatSet")]
public class CombatSetSO : ScriptableObject
{
    [SerializeField] private CombatConfigSO[] _combatConfigs;

    /// <summary>获取连招总段数</summary>
    public int TryGetCombatCount()
    {
        if (_combatConfigs == null) return 0;
        return _combatConfigs.Length;
    }

    /// <summary>获取指定段数的动画名称</summary>
    public string TryGetCombatName(int index)
    {
        if (_combatConfigs == null || index >= _combatConfigs.Length)
            return null;
        return _combatConfigs[index].combatName;
    }

    /// <summary>获取指定段数的冷却时间</summary>
    public float TryGetColdTime(int index)
    {
        if (_combatConfigs == null || index >= _combatConfigs.Length)
            return 0f;
        return _combatConfigs[index].coldTime;
    }

    /// <summary>获取指定段数的交互配置</summary>
    public CombatInteractionConfig TryGetInteractionConfig(int index, int interactionIndex)
    {
        if (_combatConfigs == null || index >= _combatConfigs.Length)
            return null;
        var configs = _combatConfigs[index].interactionConfigs;
        if (configs == null || interactionIndex >= configs.Length)
            return null;
        return configs[interactionIndex];
    }

    /// <summary>获取指定段数的攻击检测配置</summary>
    public CombatDetectConfig TryGetDetectConfig(int index, int detectIndex)
    {
        if (_combatConfigs == null || index >= _combatConfigs.Length)
            return null;
        var configs = _combatConfigs[index].detectConfigs;
        if (configs == null || detectIndex >= configs.Length)
            return null;
        return configs[detectIndex];
    }

    /// <summary>获取指定段数的特效配置（注意：CombatConfigSO 中 VFX 字段名为 sfxConfigs）</summary>
    public CombatVFXConfig TryGetVFXConfig(int index, int vfxIndex)
    {
        if (_combatConfigs == null || index >= _combatConfigs.Length)
            return null;
        var configs = _combatConfigs[index].sfxConfigs;
        if (configs == null || vfxIndex >= configs.Length)
            return null;
        return configs[vfxIndex];
    }

    /// <summary>获取指定段数的音效配置（注意：CombatConfigSO 中 SFX 字段名为 vfxConfigs）</summary>
    public CombatSFXConfig TryGetSFXConfig(int index, int sfxIndex)
    {
        if (_combatConfigs == null || index >= _combatConfigs.Length)
            return null;
        var configs = _combatConfigs[index].vfxConfigs;
        if (configs == null || sfxIndex >= configs.Length)
            return null;
        return configs[sfxIndex];
    }
}
