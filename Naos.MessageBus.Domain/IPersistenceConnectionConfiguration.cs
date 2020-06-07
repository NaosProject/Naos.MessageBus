// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IPersistenceConnectionConfiguration.cs" company="Naos Project">
//    Copyright (c) Naos Project 2019. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Naos.MessageBus.Domain
{
    using System.Security;
    using System.Text;
    using Spritely.Recipes;
    using static System.FormattableString;

    /// <summary>
    /// Interface to support connecting to persistence of some type.
    /// </summary>
    public interface IPersistenceConnectionConfiguration
    {
        /// <summary>
        /// Gets or sets the credentials.
        /// </summary>
        /// <value>The credentials.</value>
        Credentials Credentials { get; set; }

        /// <summary>
        /// Gets or sets the database.
        /// </summary>
        /// <value>The database.</value>
        string Database { get; set; }

        /// <summary>
        /// Gets or sets the server port.
        /// </summary>
        /// <value>The server port.</value>
        int? Port { get; set; }

        /// <summary>
        /// Gets or sets the server name.
        /// </summary>
        /// <value>The server name.</value>
        string Server { get; set; }

        /// <summary>
        /// Gets or sets the connection timeout in seconds.
        /// </summary>
        /// <value>The connection timeout in seconds.</value>
        int? ConnectionTimeoutInSeconds { get; set; }

        /// <summary>
        /// Gets or sets the default command timeout in seconds.
        /// </summary>
        /// <value>The default command timeout in seconds.</value>
        int? DefaultCommandTimeoutInSeconds { get; set; }

        /// <summary>
        /// Gets or sets more options which is a string that will be appended to the connection string (an extensibility point
        ///     if the supplied options are insufficient).
        /// </summary>
        /// <value>The more options.</value>
        string MoreOptions { get; set; }
    }

    /// <summary>
    ///     A set of credentials.
    /// </summary>
    public sealed class Credentials
    {
        /// <summary>
        ///     Gets or sets the password.
        /// </summary>
        /// <value>
        ///     The password.
        /// </value>
        public SecureString Password { get; set; }

        /// <summary>
        ///     Gets or sets the user.
        /// </summary>
        /// <value>
        ///     The user.
        /// </value>
        public string User { get; set; }
    }

    /// <summary>
    /// Abstract implementation of <see cref="IPersistenceConnectionConfiguration"/>.
    /// </summary>
    public abstract class PersistenceConnectionConfiguration : IPersistenceConnectionConfiguration
    {
        /// <inheritdoc />
        public Credentials Credentials { get; set; }

        /// <inheritdoc />
        public string Database { get; set; }

        /// <inheritdoc />
        public int? Port { get; set; }

        /// <inheritdoc />
        public string Server { get; set; }

        /// <inheritdoc />
        public int? ConnectionTimeoutInSeconds { get; set; }

        /// <inheritdoc />
        public int? DefaultCommandTimeoutInSeconds { get; set; }

        /// <inheritdoc />
        public string MoreOptions { get; set; }

        /// <summary>
        /// Creates a SQL Server connection string from the provided information.
        /// </summary>
        /// <returns>SQL Server connection string from the provided information.</returns>
        public string ToSqlServerConnectionString()
        {
            var connectionString = this.CreateCredentiallessConnectionString();

            if (this.Credentials != null)
            {
                if (!string.IsNullOrWhiteSpace(this.MoreOptions))
                {
                    connectionString.Append(this.MoreOptions);
                }

                var password = this.Credentials.Password == null ? string.Empty : this.Credentials.Password.ToInsecureString();
                connectionString.Append(Invariant($"User Id={this.Credentials.User};"));
                connectionString.Append(Invariant($"Password={password};"));

                return connectionString.ToString();
            }

            connectionString.Append("Integrated Security=True;");
            if (!string.IsNullOrWhiteSpace(this.MoreOptions))
            {
                connectionString.Append(this.MoreOptions);
            }

            return connectionString.ToString();
        }

        private StringBuilder CreateCredentiallessConnectionString()
        {
            var connectionString = new StringBuilder();
            if (!string.IsNullOrWhiteSpace(this.Server))
            {
                connectionString.Append(Invariant($"Server={this.Server};"));
            }

            if (!string.IsNullOrWhiteSpace(this.Database))
            {
                connectionString.Append(Invariant($"Database={this.Database};"));
            }

            if (this.ConnectionTimeoutInSeconds.HasValue)
            {
                connectionString.Append(Invariant($"Connection Timeout={this.ConnectionTimeoutInSeconds.Value};"));
            }

            return connectionString;
        }
    }

    /// <summary>
    /// Courier specific derivative of <see cref="PersistenceConnectionConfiguration"/> for DI to work.
    /// </summary>
    public class CourierPersistenceConnectionConfiguration : PersistenceConnectionConfiguration
    {
    }

    /// <summary>
    /// Event specific derivative of <see cref="PersistenceConnectionConfiguration"/> for DI to work.
    /// </summary>
    public class EventPersistenceConnectionConfiguration : PersistenceConnectionConfiguration
    {
    }

    /// <summary>
    /// Read model specific derivative of <see cref="PersistenceConnectionConfiguration"/> for DI to work.
    /// </summary>
    public class ReadModelPersistenceConnectionConfiguration : PersistenceConnectionConfiguration
    {
    }
}
