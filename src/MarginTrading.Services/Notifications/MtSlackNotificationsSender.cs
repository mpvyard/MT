﻿using System;
using System.Text;
using System.Threading.Tasks;
using Lykke.SlackNotifications;
using MarginTrading.Core.Settings;

namespace MarginTrading.Services.Notifications
{
    public class MtSlackNotificationsSender : ISlackNotificationsSender
    {
        private readonly ISlackNotificationsSender _sender;
        private readonly string _appName;
        private readonly string _env;

        public MtSlackNotificationsSender(ISlackNotificationsSender sender, string appName, string env)
        {
            _sender = sender;
            _appName = appName;
            _env = env;
        }

        public async Task SendAsync(string type, string sender, string message)
        {
            await _sender.SendAsync(ChannelTypes.MarginTrading, sender, GetSlackMsg(message));
        }

        private string GetSlackMsg(string message)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(_appName);
            sb.Append(":");
            sb.AppendLine(_env);
            sb.AppendLine("\n====================================");
            sb.AppendLine(message);
            sb.AppendLine("====================================\n");

            return sb.ToString();
        }
    }
}
