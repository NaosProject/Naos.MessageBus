// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IShareUnitOfWorkResults.cs" company="Naos">
//   Copyright 2015 Naos
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Naos.MessageBus.Domain
{
    using System.Threading.Tasks;

    /// <summary>
    /// Shares some <see cref="UnitOfWorkResult"/>.
    /// </summary>
    public interface IShareUnitOfWorkResults : IShare
    {
        /// <summary>
        /// Gets or sets the results of performing some unit-of-work.
        /// </summary>
        UnitOfWorkResult[] UnitOfWorkResults { get; set; }
    }

    /// <summary>
    /// A message to share some <see cref="UnitOfWorkResult"/>.
    /// </summary>
    public class ShareUnitOfWorkResultsMessage : IMessage
    {
        /// <inheritdoc />
        public string Description { get; set; }

        /// <inheritdoc />
        public UnitOfWorkResult[] UnitOfWorkResultsToShare { get; set; }
    }

    /// <summary>
    /// Handles a <see cref="ShareUnitOfWorkResultsMessage"/>.
    /// </summary>
    public class ShareUnitOfWorkResultsMessageHandler : IHandleMessages<ShareUnitOfWorkResultsMessage>, IShareUnitOfWorkResults
    {
        /// <inheritdoc />
        public async Task HandleAsync(ShareUnitOfWorkResultsMessage message)
        {
            this.UnitOfWorkResults = await Task.FromResult(message.UnitOfWorkResultsToShare);
        }

        /// <inheritdoc />
        public UnitOfWorkResult[] UnitOfWorkResults { get; set; }
    }
}