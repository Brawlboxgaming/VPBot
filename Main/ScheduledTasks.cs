﻿using FluentScheduler;
using VPBot.Commands;
using VPBot.Commands.Scheduling;

namespace VPBot.Main
{
    public class ScheduledTasks
    {
        public static async Task StartTimers()
        {

            JobManager.Stop();
            JobManager.RemoveAllJobs();

            Util.ScheduleRegister = new Registry();
            Util.ScheduleRegister.Schedule(async () => await ExecuteTimer()).ToRunEvery(1).Days().At(13, 0);

            JobManager.Initialize(Util.ScheduleRegister);
        }
        public static async Task ExecuteTimer()
        {
            try
            {
                await Update.UpdateSheet(null);
                await CheckTrackSubmissions.CheckTrackPolls(null);
            }
            catch (Exception ex)
            {
                await Util.ThrowInteractionlessError(ex);
            }
        }
    }
}
