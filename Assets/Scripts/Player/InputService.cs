/// <summary>
/// 输入源静态网关：当前生效的 IInputSource。
/// 默认由 PlayerInputSystem 在 Awake 时注册本机实现；
/// 将来联机/输入回放时替换 Current 即可，消费者零改动。
/// </summary>
public static class InputService
{
    public static IInputSource Current { get; set; }
}
