// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SharedPropertyApplicatorTest.cs" company="Naos">
//   Copyright 2015 Naos
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Naos.MessageBus.Test
{
    using System;
    using System.IO;

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
                SharedPropertyApplicator.ApplySharedProperties(null, new CopyFileMessage());
            };
            var ex = Assert.Throws<SharePropertyException>(testCode);
            Assert.Equal("Neither source nor target can be null", ex.Message);
        }

        [Fact]
        public static void ApplySharedProperties_Target_Throws()
        {
            Action testCode = () =>
            {
                SharedPropertyApplicator.ApplySharedProperties(null, new CopyFileMessage());
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
            SharedPropertyApplicator.ApplySharedProperties(testHandler, testMessage);
            Assert.Equal(testHandler.FilePath, testMessage.FilePath);
        }
    }

    public class CopyFileHandler : IHandleMessages<CopyFileMessage>, IShareFilePath
    {
        public string FilePath { get; set; }

        public void Handle(CopyFileMessage message)
        {
            File.Copy(message.FilePath, "new file path");
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
