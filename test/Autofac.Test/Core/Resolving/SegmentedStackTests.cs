using System;
using System.Linq;
using Autofac.Core.Resolving;
using Xunit;

namespace Autofac.Test.Core.Resolving
{
    public class SegmentedStackTests
    {
        [Fact]
        public void CanPushAndPopWithoutSegment()
        {
            var stack = new SegmentedStack<string>();

            stack.Push("1");
            stack.Push("2");
            stack.Push("3");
            Assert.Equal("3", stack.Pop());
            Assert.Equal("2", stack.Pop());
            Assert.Equal("1", stack.Pop());
        }

        [Fact]
        public void CannotPopEmptyStack()
        {
            var stack = new SegmentedStack<string>();

            stack.Push("1");
            stack.Push("2");
            Assert.Equal("2", stack.Pop());
            Assert.Equal("1", stack.Pop());
            Assert.Throws<InvalidOperationException>(() => stack.Pop());
        }

        [Fact]
        public void CanEnumerateStackItems()
        {
            var stack = new SegmentedStack<string>();

            stack.Push("1");
            stack.Push("2");
            stack.Push("3");

            Assert.Equal(stack, new[] { "3", "2", "1" });
        }

        [Fact]
        public void CanCreateStackSegment()
        {
            var stack = new SegmentedStack<string>();

            stack.Push("1");
            stack.Push("2");
            stack.Push("3");

            using (stack.EnterSegment())
            {
                Assert.Equal(stack, Enumerable.Empty<string>());

                stack.Push("4");
                stack.Push("5");
                stack.Push("6");

                Assert.Equal(stack, new[] { "6", "5", "4" });

                stack.Pop();
                stack.Pop();
                stack.Pop();

                Assert.Equal(stack, Enumerable.Empty<string>());
            }

            Assert.Equal(stack, new[] { "3", "2", "1" });
        }

        [Fact]
        public void StackTracksCountForSegmentsCorrectly()
        {
            var stack = new SegmentedStack<string>();

            stack.Push("1");
            stack.Push("2");
            stack.Push("3");

            Assert.Equal(3, stack.Count);

            using (stack.EnterSegment())
            {
                Assert.Equal(0, stack.Count);

                stack.Push("4");
                stack.Push("5");
                stack.Push("6");

                Assert.Equal(3, stack.Count);

                stack.Pop();
                stack.Pop();
                stack.Pop();

                Assert.Equal(0, stack.Count);
            }

            Assert.Equal(3, stack.Count);
        }

        [Fact]
        public void CannotPopEmptySegment()
        {
            var stack = new SegmentedStack<string>();

            stack.Push("1");
            stack.Push("2");
            stack.Push("3");

            using (stack.EnterSegment())
            {
                Assert.Throws<InvalidOperationException>(() => stack.Pop());
            }
        }

        [Fact]
        public void CannotExitSegmentWithUnpoppedItems()
        {
            var stack = new SegmentedStack<string>();

            stack.Push("1");
            stack.Push("2");
            stack.Push("3");

            Assert.Equal(3, stack.Count);

            var segment = stack.EnterSegment();

            Assert.Equal(0, stack.Count);

            stack.Push("4");
            stack.Push("5");
            stack.Push("6");

            Assert.Equal(3, stack.Count);

            stack.Pop();

            Assert.Throws<InvalidOperationException>(() => segment.Dispose());
        }
    }
}
