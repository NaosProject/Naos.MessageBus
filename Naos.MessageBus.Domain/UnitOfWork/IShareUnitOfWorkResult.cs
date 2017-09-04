// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IShareUnitOfWorkResult.cs" company="Naos">
//    Copyright (c) Naos 2017. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Naos.MessageBus.Domain
{
    using System.Threading.Tasks;

    /// <summary>
    /// Shares a <see cref="UnitOfWorkResult"/>.
    /// </summary>
    public interface IShareUnitOfWorkResult : IShare
    {
        /// <summary>
        /// Gets or sets the result of performing some unit-of-work.
        /// </summary>
        UnitOfWorkResult UnitOfWorkResult { get; set; }
    }

    /// <summary>
    /// A message to share a <see cref="UnitOfWorkResult"/>.
    /// </summary>
    public class ShareUnitOfWorkResultMessage : IMessage
    {
        /// <inheritdoc />
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="UnitOfWorkResult"/> to share.
        /// </summary>
        public UnitOfWorkResult UnitOfWorkResultToShare { get; set; }
    }

    /// <summary>
    /// Handles a <see cref="ShareUnitOfWorkResultMessage"/>.
    /// </summary>
    public class ShareUnitOfWorkResultMessageHandler : IHandleMessages<ShareUnitOfWorkResultMessage>, IShareUnitOfWorkResult
    {
        /// <inheritdoc />
        public async Task HandleAsync(ShareUnitOfWorkResultMessage message)
        {
            this.UnitOfWorkResult = await Task.FromResult(message.UnitOfWorkResultToShare);
        }

        /// <inheritdoc />
        public UnitOfWorkResult UnitOfWorkResult { get; set; }
    }
}