// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UnitOfWorkTests.cs" company="Naos Project">
//    Copyright (c) Naos Project 2019. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Naos.MessageBus.Test
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using FakeItEasy;
    using FluentAssertions;

    using Naos.MessageBus.Domain;
    using OBeautifulCode.Representation.System;
    using OBeautifulCode.Serialization;
    using OBeautifulCode.Serialization.Json;
    using OBeautifulCode.Serialization.Recipes;
    using OBeautifulCode.Type;

    using Xunit;

    public static class UnitOfWorkTests
    {
        [Fact]
        public static void Serializes()
        {
            var configurationType = typeof(TestDetailsJsonConfiguration);
            var serializer = new ObcJsonSerializer(configurationType);
            var details = new TestDetailsImplementation { Property = A.Dummy<string>() };
            var description = new SerializationDescription(SerializationKind.Json, SerializationFormat.String, configurationType.ToRepresentation());
            var described = new DescribedSerialization(typeof(TestDetailsBase).ToRepresentation(), serializer.SerializeToString(details), description);

            var expected = new UnitOfWorkResult
            {
                Name = A.Dummy<string>(),
                Outcome = UnitOfWorkOutcome.Failed,
                Details = described,
            };

            var messageBusSerializer = new ObcJsonSerializer(typeof(MessageBusJsonConfiguration));
            var serializedUnitOfWork = messageBusSerializer.SerializeToString(expected);
            var actual = messageBusSerializer.Deserialize<UnitOfWorkResult>(serializedUnitOfWork);

            var actualDetails = actual.Details.DeserializePayload();
            actualDetails.Should().BeOfType<TestDetailsImplementation>();
            (actualDetails as TestDetailsImplementation).Property.Should().Be(details.Property);
        }
    }

    public abstract class TestDetailsBase
    {
        public string Property { get; set; }
    }

    public class TestDetailsImplementation : TestDetailsBase
    {
    }

    public class TestDetailsJsonConfiguration : JsonConfigurationBase
    {
        protected override IReadOnlyCollection<Type> TypesToAutoRegister => new[] { typeof(TestDetailsBase) };
    }
}
