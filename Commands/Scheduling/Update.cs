﻿using System.Net;
using System.Security.Cryptography.X509Certificates;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using DSharpPlus.SlashCommands.Attributes;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using HtmlAgilityPack;

namespace VPBot.Commands.Scheduling
{
    public class Update : ApplicationCommandModule
    {
        [SlashCommand("update", "Updates the track update information on the sheet")]
        [SlashRequireOwner]
        public static async Task UpdateSheetWrapper(InteractionContext ctx)
        {
            await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource, new DiscordInteractionResponseBuilder() { IsEphemeral = Util.CheckEphemeral(ctx) });
            await UpdateSheet(ctx);
            var embed = new DiscordEmbedBuilder
            {
                Color = new DiscordColor("#0070FF"),
                Description = "# Notice\n" +
                "Sheet has been updated.",
                Footer = new DiscordEmbedBuilder.EmbedFooter
                {
                    Text = $"Server Time: {DateTime.Now}"
                }
            };
            await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(embed));
        }
        private class SheetColumn
        {
            public static readonly int Ix = 0;
            public static readonly int Name = 1;
            public static readonly int Link = 2;
            public static readonly int Author = 3;
            public static readonly int PackVer = 4;
            public static readonly int WikiVer = 5;
            public static readonly int Date = 6;
            public static readonly int UpdateStatus = 7;
            public static readonly int Download = 8;
            public static readonly int TrackSlot = 9;
            public static readonly int MusicSlot = 10;
            public static readonly int GhostSetter = 11;
            public static readonly int GhostTime = 12;
        }

        public static async Task UpdateSheet(InteractionContext ctx)
        {
            try
            {
                string description = "__**Tracks Have Been Updated:**__\n";

                string serviceAccountEmail = "sheetbox@sonic-fiber-399810.iam.gserviceaccount.com";
                var certificate = new X509Certificate2(@"key.p12", "notasecret", X509KeyStorageFlags.Exportable);
                ServiceAccountCredential credential = new ServiceAccountCredential(
                   new ServiceAccountCredential.Initializer(serviceAccountEmail).FromCertificate(certificate));
                var service = new SheetsService(new BaseClientService.Initializer()
                {
                    HttpClientInitializer = credential,
                    ApplicationName = "Variety Pack Bot",
                });

                var request = service.Spreadsheets.Values.Get("19mtwtrQCgdrLEAb-z_sSd4_1tIGoJ12RjEaC7pzIyn8", "'Custom Tracks'");
                request.ValueRenderOption = SpreadsheetsResource.ValuesResource.GetRequest.ValueRenderOptionEnum.FORMULA;
                var response = await request.ExecuteAsync();

                var textRequest = service.Spreadsheets.Values.Get("19mtwtrQCgdrLEAb-z_sSd4_1tIGoJ12RjEaC7pzIyn8", "'Custom Tracks'");
                var textResponse = await textRequest.ExecuteAsync();

                HtmlWeb web = new HtmlWeb();
                web.UserAgent = Util.GetUserAgent();

                for (int i = 1; i < response.Values.Count; i++)
                {
                    try
                    {
                        HtmlDocument document = await web.LoadFromWebAsync(textResponse.Values[i][SheetColumn.Link].ToString());

                        var nodes = document.DocumentNode.SelectNodes("//tr");
                        for (int j = 0; j < nodes.Count; j++)
                        {
                            if (nodes[j].InnerText.Contains("Version:"))
                            {
                                if (response.Values[i][SheetColumn.WikiVer].ToString() != nodes[j].InnerText.Replace("\n", string.Empty).Replace("Version:", string.Empty))
                                {
                                    if (!response.Values[i][SheetColumn.WikiVer].ToString().Contains(".ctgp") &&
                                    !response.Values[i][SheetColumn.WikiVer].ToString().Contains(".vp") &&
                                    !response.Values[i][SheetColumn.WikiVer].ToString().Contains(".le"))
                                    {
                                        description += $"{response.Values[i][SheetColumn.Name]} updated to {nodes[j].InnerText.Replace("\n", string.Empty).Replace("Version:", string.Empty)}\n";
                                    }
                                    response.Values[i][SheetColumn.WikiVer] = nodes[j].InnerText.Replace("\n", string.Empty).Replace("\r", string.Empty).Replace("Version:", string.Empty);
                                    Console.WriteLine($"{response.Values[i][SheetColumn.Name]} updated to {response.Values[i][SheetColumn.PackVer]}.");
                                }
                            }
                            if (nodes[j].InnerText.Contains("Date of latest version:"))
                            {
                                if (response.Values[i][SheetColumn.Date].ToString() != nodes[j].InnerText.Replace("\n", string.Empty).Replace("Date of latest version:", string.Empty))
                                {
                                    response.Values[i][SheetColumn.Date] = nodes[j].InnerText.Replace("\n", string.Empty).Replace("Date of latest version:", string.Empty);
                                    Console.WriteLine($"{response.Values[i][SheetColumn.Name]} date updated to {response.Values[i][SheetColumn.Date]}.");
                                }
                            }
                            if (nodes[j].InnerText.Contains("ct.wiimm.de"))
                            {
                                WebClient webClient = new WebClient();
                                string html = webClient.DownloadString(nodes[j].InnerHtml.Split('"')[11]);
                                HtmlDocument wiimmDoc = new HtmlDocument();
                                wiimmDoc.LoadHtml(html);
                                var trackName = response.Values[i][SheetColumn.Name].ToString().Split('(')[0].Trim().Replace("Wii U", "WiiU");
                                var version = response.Values[i][SheetColumn.PackVer].ToString().Contains(".vp") || response.Values[i][SheetColumn.PackVer].ToString().Contains(".ctgp") || response.Values[i][SheetColumn.PackVer].ToString().Contains(".le") ? response.Values[i][SheetColumn.PackVer].ToString() : response.Values[i][SheetColumn.WikiVer].ToString();
                                version = version.Replace(" Nether", "").Replace("-", ".").Replace(" ", ".").Replace("hotfix", "fix");
                                var wiimmNodes = wiimmDoc.DocumentNode.SelectNodes("//tr/td");
                                for (int k = 8; k < wiimmNodes.Count; k += 10)
                                {
                                    if (wiimmNodes[k].InnerText.Contains(trackName) && (wiimmNodes[k].InnerText.Contains(version) || wiimmNodes[k].InnerText.Contains(version.Replace(".0", ""))))
                                    {
                                        var id = wiimmNodes[k - 7].InnerText;
                                        response.Values[i][SheetColumn.Download] = $"https://ct.wiimm.de/dl/@myLhAVA9/{id}";
                                        break;
                                    }
                                    response.Values[i][SheetColumn.Download] = "Not Found";
                                }
                            }
                        }
                    }
                    catch
                    {
                        response.Values[i][SheetColumn.WikiVer] = "N/A";
                        description += $"Could not download wiki page of {response.Values[i][SheetColumn.Name]}. Updated to N/A\n";
                        Console.WriteLine($"Could not download wiki page of {response.Values[i][SheetColumn.Name]}. Updated to N/A.");
                    }
                }
                var updateRequest = service.Spreadsheets.Values.Update(response, "19mtwtrQCgdrLEAb-z_sSd4_1tIGoJ12RjEaC7pzIyn8", $"'Custom Tracks'!A1:{Util.StrAlpha[response.Values[0].Count - 1]}{response.Values.Count}");
                updateRequest.ValueInputOption = SpreadsheetsResource.ValuesResource.UpdateRequest.ValueInputOptionEnum.USERENTERED;
                var update = await updateRequest.ExecuteAsync();

                if (description != "__**Tracks Have Been Updated:**__\n")
                {
                    DiscordChannel channel = ctx.Channel;

                    foreach (var c in ctx.Guild.Channels)
                    {
                        if (c.Value.Id == 1094364785950339112)
                        {
                            channel = c.Value;
                        }
                    }

                    var embed = new DiscordEmbedBuilder
                    {
                        Color = new DiscordColor("#0070FF"),
                        Description = "# Notice\n" + description,
                        Footer = new DiscordEmbedBuilder.EmbedFooter
                        {
                            Text = $"Server Time: {DateTime.Now}"
                        }
                    };
                    await channel.SendMessageAsync(embed);
                }

                var today = DateTime.Now;
                
                DiscordActivity activity = new DiscordActivity();
                activity.Name = $"Last Updated: {DateTime.Now.ToShortDateString()} {DateTime.Now.ToShortTimeString()}" +
                    $" | /help";
                await Bot.Client.UpdateStatusAsync(activity);
            }
            catch (Exception ex)
            {
                var embed = new DiscordEmbedBuilder
                {
                    Color = new DiscordColor("#0070FF"),
                    Description = "# Error" + ex.Message,
                    Footer = new DiscordEmbedBuilder.EmbedFooter
                    {
                        Text = $"Server Time: {DateTime.Now}"
                    }
                };
                if (ctx != null) await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(embed));

                Console.WriteLine(ex.ToString());
            }
        }
    }
}
