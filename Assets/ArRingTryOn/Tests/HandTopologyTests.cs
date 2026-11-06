using System;
using System.Collections.Generic;
using System.Linq;
using ArRingTryOn.Core;
using NUnit.Framework;

namespace ArRingTryOn.Tests
{
    public class HandTopologyTests
    {
        [Test]
        public void LandmarkIds_CoverAllIndicesInOrder()
        {
            var values = Enum.GetValues(typeof(HandLandmarkId)).Cast<int>().OrderBy(v => v);

            Assert.That(values, Is.EqualTo(Enumerable.Range(0, HandLandmarks.Count)));
        }

        [Test]
        public void Connections_FormOneSkeletonReachingEveryLandmark()
        {
            var neighbours = new Dictionary<HandLandmarkId, List<HandLandmarkId>>();
            foreach (var (from, to) in HandTopology.Connections)
            {
                Neighbours(neighbours, from).Add(to);
                Neighbours(neighbours, to).Add(from);
            }

            var reached = new HashSet<HandLandmarkId> { HandLandmarkId.Wrist };
            var pending = new Stack<HandLandmarkId>(reached);
            while (pending.Count > 0)
            {
                foreach (var next in neighbours[pending.Pop()].Where(reached.Add))
                {
                    pending.Push(next);
                }
            }

            Assert.That(reached.Count, Is.EqualTo(HandLandmarks.Count));
        }

        [Test]
        public void Connections_HasNoDuplicates()
        {
            var unique = HandTopology.Connections
                .Select(c => (Math.Min((int)c.From, (int)c.To), Math.Max((int)c.From, (int)c.To)))
                .Distinct();

            Assert.That(unique.Count(), Is.EqualTo(HandTopology.Connections.Count));
        }

        [TestCase(Finger.Thumb, HandLandmarkId.ThumbMcp, HandLandmarkId.ThumbIp)]
        [TestCase(Finger.Index, HandLandmarkId.IndexMcp, HandLandmarkId.IndexPip)]
        [TestCase(Finger.Middle, HandLandmarkId.MiddleMcp, HandLandmarkId.MiddlePip)]
        [TestCase(Finger.Ring, HandLandmarkId.RingMcp, HandLandmarkId.RingPip)]
        [TestCase(Finger.Pinky, HandLandmarkId.PinkyMcp, HandLandmarkId.PinkyPip)]
        public void RingSegment_ReturnsBaseAndNextJoint(Finger finger, HandLandmarkId expectedBase, HandLandmarkId expectedNext)
        {
            var (baseId, nextId) = HandTopology.RingSegment(finger);

            Assert.That(baseId, Is.EqualTo(expectedBase));
            Assert.That(nextId, Is.EqualTo(expectedNext));
        }

        [Test]
        public void RingSegment_RejectsUnknownFinger()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => HandTopology.RingSegment((Finger)99));
        }

        private static List<HandLandmarkId> Neighbours(Dictionary<HandLandmarkId, List<HandLandmarkId>> map, HandLandmarkId id)
        {
            if (!map.TryGetValue(id, out var list))
            {
                list = new List<HandLandmarkId>();
                map[id] = list;
            }

            return list;
        }
    }
}
