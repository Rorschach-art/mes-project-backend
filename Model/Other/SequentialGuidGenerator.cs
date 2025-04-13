namespace Model.Other;

/// <summary>
/// 生成连续的 GUID 的类
/// </summary>
public class SequentialGuidGenerator
{
    /// <summary>
    /// 使用 Lazy<T> 实现延迟初始化和线程安全的单例模式
    /// 这里定义了一个静态只读字段 Instances，用于存储 SequentialGuidGenerator 的实例
    /// LazyThreadSafetyMode.ExecutionAndPublication 确保在并行执行时只有一个实例被创建
    /// </summary>
    private static readonly Lazy<SequentialGuidGenerator> Instances =
        new(() => new SequentialGuidGenerator(), LazyThreadSafetyMode.ExecutionAndPublication);

    /// <summary>
    /// 私有构造函数，防止外部实例化
    /// </summary>
    private SequentialGuidGenerator() { }

    /// <summary>
    /// 提供全局访问点
    /// </summary>
    public static SequentialGuidGenerator Instance => Instances.Value;

    /// <summary>
    /// 状态变量，记录上一次的时间戳
    /// </summary>
    private long _lastTimestamp = -1;

    /// <summary>
    /// 状态变量，记录当前的计数器值
    /// </summary>
    private int _counter;

    /// <summary>
    /// 生成连续的 GUID
    /// </summary>
    /// <returns>连续的 GUID</returns>
    public Guid GenerateSequentialGuid()
    {
        // 获取当前时间戳
        long timestamp = DateTime.UtcNow.Ticks;
        if (timestamp <= _lastTimestamp)
        {
            // 如果时间戳没有变化，则递增计数器
            _counter++;
        }
        else
        {
            // 如果时间戳更新，则重置计数器
            _counter = 0;
        }
        _lastTimestamp = timestamp;

        // 将时间戳转换为字节数组
        var timestampBytes = BitConverter.GetBytes(timestamp);
        if (BitConverter.IsLittleEndian)
        {
            Array.Reverse(timestampBytes);
        }

        // 生成一个随机的 GUID
        var guidBytes = Guid.NewGuid().ToByteArray();

        // 将时间戳的前 6 个字节复制到 GUID 的前 6 个字节
        Array.Copy(timestampBytes, 2, guidBytes, 0, 6);

        // 设置 GUID 的版本号为 1（表示基于时间的 GUID）
        guidBytes[7] = (byte)(guidBytes[7] & 0x0F | 0x10);

        // 将计数器的值写入 GUID 的最后 2 个字节（可选）
        byte[] counterBytes = BitConverter.GetBytes(_counter);
        if (BitConverter.IsLittleEndian)
        {
            Array.Reverse(counterBytes);
        }
        Array.Copy(counterBytes, 0, guidBytes, guidBytes.Length - 2, 2);

        // 返回新的 GUID
        return new Guid(guidBytes);
    }
}
