using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Endfield.Tools
{
    public static class TransformUtility
    {
        /// <summary>
        /// 获取增量角
        /// </summary>
        /// <param name="currentDirection">当前移动方向</param>
        /// <param name="targetDirection">目标移动方向</param>
        /// <returns></returns>
        public static float GetDeltaAngle(Transform currentDirection, Vector3 targetDirection)
        {
            
            //不完全等同于欧拉角的y，因为单纯的欧拉角在斜坡并不是我们想要的
            //计算当前角色与目标方向的角度差，atan2返回的是弧度，使用Rad2Deg转换为角度
            float angleCurrent = Mathf.Atan2(currentDirection.forward.x, currentDirection.forward.z) * Mathf.Rad2Deg;
            float targetAngle = Mathf.Atan2(targetDirection.x, targetDirection.z) * Mathf.Rad2Deg;

            return Mathf.DeltaAngle(angleCurrent, targetAngle);
        
        }
    }

    
}