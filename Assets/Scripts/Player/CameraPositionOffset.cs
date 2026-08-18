using Cinemachine;
using UnityEngine;

namespace Endfield
{
    /// <summary>
    /// 给虚拟相机加世界空间位移偏移（连携镜头"平移"用）。
    /// 挂在 VirtualCamera 上；ThirdPersonCamera 每帧设置/清空 Offset。
    /// Body 阶段给最终相机位置加一个偏移，不改 Follow target。
    /// </summary>
    public class CameraPositionOffset : CinemachineExtension
    {
        public Vector3 Offset;

        protected override void PostPipelineStageCallback(
            CinemachineVirtualCameraBase vcam, CinemachineCore.Stage stage, ref CameraState state, float deltaTime)
        {
            if (stage == CinemachineCore.Stage.Body)
            {
                state.PositionCorrection += Offset;   // 2.10 用 PositionCorrection（最终位置 = RawPosition + 修正）
            }
        }
    }
}
