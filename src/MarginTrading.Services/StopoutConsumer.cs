﻿using System.Linq;
using Common;
using Lykke.Common;
using MarginTrading.Core;
using MarginTrading.Core.Clients;
using MarginTrading.Core.Messages;
using MarginTrading.Services.Events;
using MarginTrading.Services.Notifications;

namespace MarginTrading.Services
{
    // TODO: Rename by role
    public class StopOutConsumer : SendNotificationBase, IEventConsumer<StopOutEventArgs>
    {
        private readonly IThreadSwitcher _threadSwitcher;
        private readonly IClientAccountService _clientAccountService;
        private readonly IClientNotifyService _notifyService;
        private readonly IEmailService _emailService;
        private readonly IMarginTradingOperationsLogService _operationsLogService;
        private readonly IRabbitMqNotifyService _rabbitMqNotifyService;
        private readonly IDateService _dateService;

        public StopOutConsumer(IThreadSwitcher threadSwitcher,
            IClientSettingsRepository clientSettingsRepository,
            IAppNotifications appNotifications,
            IClientAccountService clientAccountService,
            IClientNotifyService notifyService,
            IEmailService emailService,
            IMarginTradingOperationsLogService operationsLogService,
            IRabbitMqNotifyService rabbitMqNotifyService,
            IDateService dateService) : base(clientSettingsRepository,
            appNotifications,
            clientAccountService)
        {
            _threadSwitcher = threadSwitcher;
            _clientAccountService = clientAccountService;
            _notifyService = notifyService;
            _emailService = emailService;
            _operationsLogService = operationsLogService;
            _rabbitMqNotifyService = rabbitMqNotifyService;
            _dateService = dateService;
        }

        int IEventConsumer.ConsumerRank => 100;

        void IEventConsumer<StopOutEventArgs>.ConsumeEvent(object sender, StopOutEventArgs ea)
        {
            var account = ea.Account;
            var orders = ea.Orders;
            var eventTime = _dateService.Now();
                _threadSwitcher.SwitchThread(async () =>
                {
                    _operationsLogService.AddLog("stopout", account.ClientId, account.Id, "", ea.ToJson());

                    var totalPnl = orders.Sum(x => x.GetFpl());

                _notifyService.NotifyAccountStopout(account.ClientId, account.Id, orders.Length, totalPnl);
                _notifyService.NotifyAccountUpdated(account);

                    await SendNotification(account.ClientId,
                        string.Format(MtMessages.Notifications_StopOutNotification, orders.Length, totalPnl,
                            account.BaseAssetId), null);

                    var clientAcc = await _clientAccountService.GetAsync(account.ClientId);

                    if (clientAcc != null)
                    {
                        await _emailService.SendStopOutEmailAsync(clientAcc.Email, account.BaseAssetId, account.Id);
                    }

                    await _rabbitMqNotifyService.AccountMarginEvent(account, true, eventTime);
                });
        }
    }
}