// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IShare.cs" company="Naos">
//    Copyright (c) Naos 2017. All Rights Reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Naos.MessageBus.Domain
{
    /// <summary>
    /// Interface for derivative implementations to allow passing properties from one object to another.
    /// Used to share properties from a handler to a downstream message in a message sequence.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1040:AvoidEmptyInterfaces", Justification = "Keeping for extension and reflection.")]
    public interface IShare
    {
    }
}
