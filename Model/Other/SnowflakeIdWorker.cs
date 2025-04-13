using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Model.Other
{
    public class SnowflakeIdWorker
    {
        private const long Twepoch = 1640995200000L; // 定义起始时间戳，这里以2022年为起点
        private const int WorkerIdBits = 10;       // 机器ID所占位数
        private const int DatacenterIdBits = 10;   // 数据中心ID所占位数
        private const long MaxWorkerId = -1L ^ (-1L << WorkerIdBits);  // 最大机器ID
        private const int SequenceBits = 12;      // 序列号所占位数
        private const long WorkerIdShift = SequenceBits; // 机器ID左移位数
        private const long DatacenterIdShift = SequenceBits + WorkerIdBits; // 数据中心ID左移位数
        private const long TimestampLeftShift = SequenceBits + WorkerIdBits + DatacenterIdBits; // 时间戳左移位数
        private const long SequenceMask = -1L ^ (-1L << SequenceBits); // 序列号掩码

        private readonly long _workerId; // 机器ID
        private readonly long _datacenterId; // 数据中心ID
        private long _sequence; // 序列号
        private long _lastTimestamp = -1L; // 上次生成ID的时间戳
        private readonly object _lockHelper = new object(); // 锁定对象

        private SnowflakeIdWorker(long workerId, long datacenterId)
        {
            if (workerId > MaxWorkerId || workerId < 0)
            {
                throw new ArgumentException($"worker Id can't be greater than {MaxWorkerId} or less than 0");
            }
            if (datacenterId > MaxWorkerId || datacenterId < 0)
            {
                throw new ArgumentException($"datacenter Id can't be greater than {MaxWorkerId} or less than 0");
            }
            this._workerId = workerId;
            this._datacenterId = datacenterId;
        }

        public ulong NextId()
        {
            lock (_lockHelper)
            {
                long timestamp = TimeGen();
                if (timestamp < _lastTimestamp)
                {
                    throw new Exception("Clock moved backwards. Refusing to generate id for " + (_lastTimestamp - timestamp) + " milliseconds");
                }

                if (_lastTimestamp == timestamp)
                {
                    _sequence = (_sequence + 1) & SequenceMask;
                    if (_sequence == 0)
                    {
                        timestamp = TilNextMillis(_lastTimestamp);
                    }
                }
                else
                {
                    _sequence = 0;
                }

                _lastTimestamp = timestamp;
                return ((ulong)((timestamp - Twepoch) << (int)TimestampLeftShift) |
                        ((ulong)_datacenterId << (int)DatacenterIdShift) |
                        ((ulong)_workerId << (int)WorkerIdShift) |
                        (ulong)_sequence);
            }
        }

        private static long TilNextMillis(long lastTimestamp)
        {
            while (true)
            {
                var timestamp = TimeGen();
                if (timestamp > lastTimestamp)
                {
                    return timestamp;
                }
            }
        }

        private static long TimeGen()
        {
            return DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        }

        private static readonly Lazy<SnowflakeIdWorker> Lazy =
            new(() => new SnowflakeIdWorker(1, 1), LazyThreadSafetyMode.ExecutionAndPublication);

        public static SnowflakeIdWorker Instance => Lazy.Value;
    }
}