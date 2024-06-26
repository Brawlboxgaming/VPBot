﻿using DSharpPlus;
using DSharpPlus.Commands;
using DSharpPlus.Commands.Processors.SlashCommands;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using System.ComponentModel;

namespace VPBot.Commands.Wiimmfi
{
    public class Rooms
    {
        [Command("rooms")]
        [Description("Checks for rooms on the Variety Pack region.")]
        public async Task RoomCommand(CommandContext ctx)
        {
            if (ctx is SlashCommandContext sCtx)
            {
                await sCtx.DeferResponseAsync(Util.CheckEphemeral(ctx));
            }
            else
            {
                await ctx.DeferResponseAsync();
            }
        }
    }
}
