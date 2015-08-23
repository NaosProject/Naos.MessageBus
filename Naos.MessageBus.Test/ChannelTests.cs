// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ChannelTests.cs" company="Naos">
//   Copyright 2015 Naos
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Naos.MessageBus.Test
{
    using System.Linq;

    using Naos.MessageBus.DataContract;

    using Xunit;

    public class ChannelTests
    {
        [Fact]
        public void DisinctOnChannelCollection_Duplicates_ReturnsActualDistinct()
        {
            var duplicateName = "HelloDolly";
            var input = new[] { new Channel { Name = duplicateName }, new Channel { Name = duplicateName } };
            var actual = input.Distinct().ToList();
            Assert.Equal(1, actual.Count);
            Assert.Equal(duplicateName, actual.Single().Name);
        }
    }
}
