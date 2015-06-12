// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IShare.cs" company="Naos">
//   Copyright 2015 Naos
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Naos.MessageBus.DataContract
{
    /// <summary>
    /// Interface for derivative implementations to allow passing properties from one object to another.
    /// Used to share properties from a handler to a downstream message in a message sequence.
    /// </summary>
    public interface IShare
    {
    }
}
