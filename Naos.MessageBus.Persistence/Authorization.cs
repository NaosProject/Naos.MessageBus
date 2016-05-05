namespace Naos.MessageBus.Persistence
{
    using System.Security.Principal;
    using System.Threading;

    using Microsoft.Its.Domain.Authorization;

    public static class Authorization
    {
        static Authorization()
        {
            AuthorizationFor<AlwaysIsInRolePrincipal>.ToApplyAnyCommand.ToA<Shipment>.Requires((principal, acct) => true);
        }

        public static void AuthorizeAllCommands()
        {
            Thread.CurrentPrincipal = new AlwaysIsInRolePrincipal();
        }

        public class AlwaysIsInRolePrincipal : IPrincipal
        {
            public bool IsInRole(string role)
            {
                return true;
            }

            public IIdentity Identity { get; private set; }
        }
    }
}