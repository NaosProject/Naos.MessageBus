// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IShareUnitOfWorkResults.cs" company="Naos Project">
//    Copyright (c) Naos Project 2019. All rights reserved.
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
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "Keeping this way for now.")]
        UnitOfWorkResult[] UnitOfWorkResults { get; set; }
    }

    /// <summary>
    /// A message to share some <see cref="UnitOfWorkResult"/>.
    /// </summary>
    public class ShareUnitOfWorkResultsMessage : IMessage
    {
        /// <inheritdoc />
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="UnitOfWorkResult"/>'s to share.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "Keeping this way for now.")]
        public UnitOfWorkResult[] UnitOfWorkResultsToShare { get; set; }
    }

    /// <summary>
    /// Handles a <see cref="ShareUnitOfWorkResultsMessage"/>.
    /// </summary>
    public class ShareUnitOfWorkResultsMessageHandler : MessageHandlerBase<ShareUnitOfWorkResultsMessage>, IShareUnitOfWorkResults
    {
        /// <inheritdoc cref="MessageHandlerBase{T}" />
        public override async Task HandleAsync(ShareUnitOfWorkResultsMessage message)
        {
            this.UnitOfWorkResults = await Task.FromResult(message.UnitOfWorkResultsToShare);
        }

        /// <inheritdoc />
        public UnitOfWorkResult[] UnitOfWorkResults { get; set; }
    }
}
