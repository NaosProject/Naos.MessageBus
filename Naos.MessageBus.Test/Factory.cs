// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Factory.cs" company="Naos">
//   Copyright 2015 Naos
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Naos.MessageBus.Test
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    using FakeItEasy;

    using Naos.MessageBus.Domain;

    internal class Factory
    {
        public static Func<IPostOffice> GetInMemorySender(List<Parcel> trackingSends)
        {
            Func<Parcel, TrackingCode> send = parcel =>
            {
                trackingSends.Add(parcel);
                return null;
            };

            var ret = A.Fake<IPostOffice>();

            A.CallTo(ret)
                .Where(call => call.Method.Name == nameof(IPostOffice.Send))
                .WithReturnType<TrackingCode>()
                .Invokes(call => send(call.Arguments.FirstOrDefault() as Parcel));

            return () => ret;
        }

        public static Func<ICourier> GetInMemoryCourier(List<Crate> trackingSends)
        {
            Action<Crate> send = trackingSends.Add;

            var ret = A.Fake<ICourier>();
            A.CallTo(ret)
                .Where(call => call.Method.Name == nameof(ICourier.Send))
                .Invokes(call => send(call.Arguments.FirstOrDefault() as Crate));
            return () => ret;
        }

        public static Func<IParcelTrackingSystem> GetInMemoryParcelTrackingSystem(List<string> trackingCalls, List<Parcel> trackingParcelsFromSent)
        {
            Action<string> track = trackingCalls.Add;

            var ret = A.Fake<IParcelTrackingSystem>();
            A.CallTo(ret).WithReturnType<Task>().Invokes(
                call =>
                    {
                        if (call.Method.Name == nameof(IParcelTrackingSystem.UpdateSentAsync))
                        {
                            trackingParcelsFromSent.Add(call.Arguments.Skip(1).First() as Parcel);
                        }

                        track(call.Method.Name);
                    }).Returns(Task.Run(() => { }));

            return () => ret;
        }

        public static IGetTrackingReports GetSeededTrackerForGetLatestNoticeAsync(Dictionary<ITopic, NoticeThatTopicWasAffected> data)
        {
            var tracker = A.Fake<IGetTrackingReports>();

            foreach (var item in data)
            {
                A.CallTo(() => tracker.GetLatestNoticeThatTopicWasAffectedAsync(item.Key, TopicStatus.None)).Returns(Task.FromResult(item.Value));
                A.CallTo(() => tracker.GetLatestNoticeThatTopicWasAffectedAsync(item.Key, TopicStatus.WasAffected)).Returns(Task.FromResult(item.Value));
            }

            return tracker;
        }
    }
}