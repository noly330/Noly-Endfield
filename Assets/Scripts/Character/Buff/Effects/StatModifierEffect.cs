namespace Endfield
{
    /// <summary>
    /// 数值效果（增伤/免伤/脆弱）：挂上按层数写入，叠层按 delta 调整，移除全额回退。
    /// 无状态：只读 instance.stackCount 计算，一份代码管所有同类数值 buff。
    /// </summary>
    public class StatModifierEffect : IBuffEffect
    {
        public enum StatType { ATK, DamageTaken }   // 先这两个，够物理队用
        public StatType stat;
        public float valuePerLayer;                 // 每层数值（脆弱 = 受伤加深 +0.04/层）

        public void OnApply(BuffInstance instance, IBuffTarget target) => Add(target, instance.stackCount);
        public void OnStackChanged(BuffInstance instance, IBuffTarget target, int deltaStack) => Add(target, deltaStack);
        public void OnTick(BuffInstance instance, IBuffTarget target, float deltaTime) { }
        public void OnRemove(BuffInstance instance, IBuffTarget target) => Add(target, -instance.stackCount);   // 全额回退

        private void Add(IBuffTarget target, int layers)
        {
            float v = valuePerLayer * layers;
            if (stat == StatType.ATK) target.Attribute.AddAtkPercent(v);
            else target.Attribute.AddDamageTakenPercent(v);
        }
    }
}
