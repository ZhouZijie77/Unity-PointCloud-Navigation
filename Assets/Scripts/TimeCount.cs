using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class TimeCount
{
    public static DateTime t1,t2;
    public static double GetSubSeconds(DateTime startTimer, DateTime endTimer)
    {
        TimeSpan startSpan = new TimeSpan(startTimer.Ticks);

        TimeSpan nowSpan = new TimeSpan(endTimer.Ticks);

        TimeSpan subTimer = nowSpan.Subtract(startSpan).Duration();

       // 返回间隔秒数（不算差的分钟和小时等，仅返回秒与秒之间的差）
        //return subTimer.Seconds;

        //返回相差时长
        return subTimer.TotalSeconds;
    }

}
