using System;
using Server.Mobiles;

namespace Server.Engines.BuffIcons;

public class BuffInfo
{
    private TimerExecutionToken _timerToken;

    public BuffInfo(
        BuffIcon iconID, int titleCliloc, TimeSpan duration = default, TextDefinition args = null,
        bool retainThroughDeath = false, uint maxStackSize = 1
    ) : this(iconID, titleCliloc, titleCliloc + 1, duration, args, retainThroughDeath, maxStackSize, 1)
    {
    }

    public BuffInfo(
        BuffIcon iconID, int titleCliloc, int secondaryCliloc, TimeSpan duration = default, TextDefinition args = null,
        bool retainThroughDeath = false, uint maxStackSize = 1, uint stack = 1
    )
    {
        ID = iconID;
        TitleCliloc = titleCliloc;
        SecondaryCliloc = secondaryCliloc;
        Duration = duration;
        Args = args;
        RetainThroughDeath = retainThroughDeath;
        Stack = stack;
        MaxStackSize = maxStackSize;
    }

    public static bool Enabled { get; private set; }

    public BuffIcon ID { get; }

    public int TitleCliloc { get; }

    public int SecondaryCliloc { get; }

    public DateTime StartTime { get; private set; }

    public TimeSpan Duration { get; }

    public bool RetainThroughDeath { get; }

    public TextDefinition Args { get; }

    public uint Stack { get; set; }
    public uint MaxStackSize { get; }

    public static void Configure()
    {
        Enabled = ServerConfiguration.GetOrUpdateSetting("buffIcons.enable", Core.ML);
    }

    public void StartTimer(PlayerMobile m)
    {
        if (Duration != TimeSpan.Zero)
        {
            StartTime = Core.Now;
            var id = ID;
            Timer.StartTimer(Duration, () => m.RemoveBuff(id), out _timerToken);
        }
    }

    public void StopTimer()
    {
        _timerToken.Cancel();
    }
}
