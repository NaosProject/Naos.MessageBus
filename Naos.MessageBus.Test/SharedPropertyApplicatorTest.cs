// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SharedPropertyApplicatorTest.cs" company="Naos">
//   Copyright 2015 Naos
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Naos.MessageBus.Test
{
    using System;
    using System.IO;
    using System.Threading.Tasks;

    using Naos.MessageBus.Core;
    using Naos.MessageBus.DataContract;
    using Naos.MessageBus.DataContract.Exceptions;
    using Naos.MessageBus.HandlingContract;
    using Naos.MessageBus.Hangfire.Harness;

    using Xunit;

    public class SharedPropertyApplicatorTest
    {
        [Fact]
        public static void ApplySharedProperties_Source_Throws()
        {
            Action testCode = () =>
            {
                SharedPropertyApplicator.ApplySharedProperties(TypeMatchStrategy.NamespaceAndName, null, new CopyFileMessage());
            };
            var ex = Assert.Throws<SharePropertyException>(testCode);
            Assert.Equal("Neither source nor target can be null", ex.Message);
        }

        [Fact]
        public static void ApplySharedProperties_Target_Throws()
        {
            Action testCode = () =>
            {
                SharedPropertyApplicator.ApplySharedProperties(TypeMatchStrategy.NamespaceAndName, null, new CopyFileMessage());
            };
            var ex = Assert.Throws<SharePropertyException>(testCode);
            Assert.Equal("Neither source nor target can be null", ex.Message);
        }

        [Fact]
        public static void ApplySharedProperties_ValidMatch_PropertiesSet()
        {
            var testHandler = new CopyFileHandler() { FilePath = "This should be set on the message" };
            var testMessage = new DeleteFileMessage();

            Assert.NotEqual(testHandler.FilePath, testMessage.FilePath);
            SharedPropertyApplicator.ApplySharedProperties(TypeMatchStrategy.NamespaceAndName, testHandler, testMessage);
            Assert.Equal(testHandler.FilePath, testMessage.FilePath);
        }

        [Fact]
        public static void ApplySharedProperties_ValidMatchOnEnum_PropertiesSet()
        {
            var testHandler = new FirstEnumHandler { EnumValueToTest = MyEnum.OtherValue };
            var testMessage = new SecondEnumMessage();

            Assert.NotEqual(testHandler.EnumValueToTest, testMessage.EnumValueToTest);
            SharedPropertyApplicator.ApplySharedProperties(TypeMatchStrategy.NamespaceAndName, testHandler, testMessage);
            Assert.Equal(testHandler.EnumValueToTest, testMessage.EnumValueToTest);
        }
    }

    public class FirstEnumHandler : IHandleMessages<FirstEnumMessage>, IShareEnum
    {
        public async Task HandleAsync(FirstEnumMessage message)
        {
            this.EnumValueToTest = await Task.FromResult(message.SeedValue);
        }

        public MyEnum EnumValueToTest { get; set; }
    }

    public class FirstEnumMessage : IMessage
    {
        public string Description { get; set; }

        public MyEnum EnumValueToTest { get; set; }

        public MyEnum SeedValue { get; set; }
    }

    public class SecondEnumMessage : IMessage, IShareEnum
    {
        public string Description { get; set; }

        public MyEnum EnumValueToTest { get; set; }
    }

    public enum MyEnum
    {
        /// <summary>
        /// Test value.
        /// </summary>
        ShouldNotGet,

        /// <summary>
        /// Test value.
        /// </summary>
        OtherValue,

        /// <summary>
        /// Test value.
        /// </summary>
        OtherOtherValue
    }

    public interface IShareEnum : IShare
    {
        MyEnum EnumValueToTest { get; set; }
    }

    public class CopyFileHandler : IHandleMessages<CopyFileMessage>, IShareFilePath
    {
        public string FilePath { get; set; }

        public async Task HandleAsync(CopyFileMessage message)
        {
            await Task.Run(() => File.Copy(message.FilePath, "new file path"));
            this.FilePath = message.FilePath;
        }
    }

    public class CopyFileMessage : IMessage, IShareFilePath
    {
        public string FilePath { get; set; }

        public string Description { get; set; }
    }

    public class DeleteFileMessage : IMessage, IShareFilePath
    {
        public string FilePath { get; set; }

        public string Description { get; set; }
    }

    public interface IShareFilePath : IShare
    {
        string FilePath { get; set; }
    }
}
