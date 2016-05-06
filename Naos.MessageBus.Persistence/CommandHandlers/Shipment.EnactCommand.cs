﻿namespace Naos.MessageBus.Persistence
{
    public partial class Shipment
    {
        public void EnactCommand(CreateShipment command)
        {
            this.RecordEvent(new Created { Parcel = command.Parcel, MetaData = command.MetaData });
        }
    }
}