﻿namespace MarginTrading.Services.Events
{
    public interface IEventChannel<TEventArgs>
    {
        void SendEvent(object sender, TEventArgs ea);
    }
}