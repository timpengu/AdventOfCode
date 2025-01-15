using FluentAssertions;

namespace IntCode.Tests
{
    [TestClass]
    public sealed class ComputerTests
    {
        [TestMethod]
        [DataRow(new[] { 1, 9, 10, 3, 2, 3, 11, 0, 99, 30, 40, 50 }, new[] { 3500, 9, 10, 70, 2, 3, 11, 0, 99, 30, 40, 50 })]
        [DataRow(new[] { 1, 0, 0, 0, 99 }, new[] { 2, 0, 0, 0, 99 })]
        [DataRow(new[] { 2, 3, 0, 3, 99 }, new[] { 2, 3, 0, 6, 99 })]
        [DataRow(new[] { 2, 4, 4, 5, 99, 0 }, new[] { 2, 4, 4, 5, 99, 9801 })]
        [DataRow(new[] { 1, 1, 1, 4, 99, 5, 6, 0, 99 }, new[] { 30, 1, 1, 4, 2, 5, 6, 0, 99 })]
        public void AoC2019_Day02_Examples(int[] program, int[] expected)
        {
            var sut = new Computer<int>(program);
            sut.ExecuteAll();
            
            Enumerable.Range(0, expected.Length)
                .Select(i => sut[i])
                .Should()
                .BeEquivalentTo(expected, opt => opt.WithStrictOrdering());
        }

        [TestMethod]
        [DataRow(-1000)]
        [DataRow(0)]
        [DataRow(1729)]
        [DataRow(int.MinValue)]
        [DataRow(int.MaxValue)]
        public void AoC2019_Day05_Example1_OutputsInput(int input)
        {
            int[] program = [3, 0, 4, 0, 99];

            var sut = new Computer<int>(program, input);
            var output = sut.ExecuteOutputs().ToList();

            output.Should().HaveCount(1);
            output[0].Should().Be(input);
        }

        [TestMethod]
        public void AoC2019_Day05_Example2_MultipliesWithCorrectParameterModes()
        {
            int[] program = [1002, 4, 3, 4, 33];

            var sut = new Computer<int>(program);
            var output = sut.ExecuteOutputs().ToList();

            output.Should().BeEmpty();
            sut[4].Should().Be(99);
        }

        [TestMethod]
        public void AoC2019_Day05_Example3_AddsNegativeInteger()
        {
            int[] program = [1101, 100, -1, 4, 0];

            var sut = new Computer<int>(program);
            var output = sut.ExecuteOutputs().ToList();

            output.Should().BeEmpty();
            sut[4].Should().Be(99);
        }

        [TestMethod]
        [DataRow(1)]
        [DataRow(8)]
        [DataRow(-8)]
        [DataRow(108)]
        [DataRow(int.MinValue)]
        [DataRow(int.MaxValue)]
        public void AoC2019_Day05_Example4_PositionMode_OutputsEqualTo8(int input)
        {
            int[] program = [3, 9, 8, 9, 10, 9, 4, 9, 99, -1, 8];

            var sut = new Computer<int>(program, input);
            var output = sut.ExecuteOutputs().ToList();

            output.Should().HaveCount(1);
            output[0].Should().Be(input == 8 ? 1 : 0);
        }

        [TestMethod]
        [DataRow(1)]
        [DataRow(8)]
        [DataRow(-8)]
        [DataRow(108)]
        [DataRow(int.MinValue)]
        [DataRow(int.MaxValue)]
        public void AoC2019_Day05_Example5_PositionMode_OutputsLessThan8(int input)
        {
            int[] program = [3, 9, 7, 9, 10, 9, 4, 9, 99, -1, 8];

            var sut = new Computer<int>(program, input);
            var output = sut.ExecuteOutputs().ToList();

            output.Should().HaveCount(1);
            output[0].Should().Be(input < 8 ? 1 : 0);
        }

        [TestMethod]
        [DataRow(1)]
        [DataRow(8)]
        [DataRow(-8)]
        [DataRow(108)]
        [DataRow(int.MinValue)]
        [DataRow(int.MaxValue)]
        public void AoC2019_Day05_Example6_ImmediateMode_OutputsEqualTo8(int input)
        {
            int[] program = [3, 3, 1108, -1, 8, 3, 4, 3, 99];

            var sut = new Computer<int>(program, input);
            var output = sut.ExecuteOutputs().ToList();

            output.Should().HaveCount(1);
            output[0].Should().Be(input == 8 ? 1 : 0);
        }

        [TestMethod]
        [DataRow(1)]
        [DataRow(8)]
        [DataRow(-8)]
        [DataRow(108)]
        [DataRow(int.MinValue)]
        [DataRow(int.MaxValue)]
        public void AoC2019_Day05_Example7_ImmediateMode_OutputsLessThan8(int input)
        {
            int[] program = [3, 3, 1107, -1, 8, 3, 4, 3, 99];

            var sut = new Computer<int>(program, input);
            var output = sut.ExecuteOutputs().ToList();

            output.Should().HaveCount(1);
            output[0].Should().Be(input < 8 ? 1 : 0);
        }

        [TestMethod]
        [DataRow(-1)]
        [DataRow(0)]
        [DataRow(1)]
        [DataRow(int.MinValue)]
        [DataRow(int.MaxValue)]
        public void AoC2019_Day05_Example8_PositionModeJumpTest_OutputsEqualToZero(int input)
        {
            int[] program = [3, 12, 6, 12, 15, 1, 13, 14, 13, 4, 13, 99, -1, 0, 1, 9];

            var sut = new Computer<int>(program, input);
            var output = sut.ExecuteOutputs().ToList();

            output.Should().HaveCount(1);
            output[0].Should().Be(input == 0 ? 0 : 1);
        }

        [TestMethod]
        [DataRow(-1)]
        [DataRow(0)]
        [DataRow(1)]
        [DataRow(int.MinValue)]
        [DataRow(int.MaxValue)]
        public void AoC2019_Day05_Example9_ImmediateModeJumpTest_OutputsEqualToZero(int input)
        {
            int[] program = [3, 3, 1105, -1, 9, 1101, 0, 0, 12, 4, 12, 99, 1];

            var sut = new Computer<int>(program, input);
            var output = sut.ExecuteOutputs().ToList();

            output.Should().HaveCount(1);
            output[0].Should().Be(input == 0 ? 0 : 1);
        }

        [TestMethod]
        [DataRow(1)]
        [DataRow(7)]
        [DataRow(8)]
        [DataRow(9)]
        [DataRow(-8)]
        [DataRow(108)]
        [DataRow(int.MinValue)]
        [DataRow(int.MaxValue)]
        public void AoC2019_Day05_Example10_OutputsCompareWith8(int input)
        {
            int[] program = [3, 21, 1008, 21, 8, 20, 1005, 20, 22, 107, 8, 21, 20, 1006, 20, 31, 1106, 0, 36, 98, 0, 0, 1002, 21, 125, 20, 4, 20, 1105, 1, 46, 104, 999, 1105, 1, 46, 1101, 1000, 1, 20, 4, 20, 1105, 1, 46, 98, 99];

            var sut = new Computer<int>(program, input);
            var output = sut.ExecuteOutputs().ToList();

            output.Should().HaveCount(1);
            output[0].Should().Be(1000 + input.CompareTo(8));
        }

        [TestMethod]
        public void AoC2019_Day07_Example1_OutputsMaxSignal()
        {
            int[] program = [3, 15, 3, 16, 1002, 16, 10, 16, 1, 16, 15, 15, 4, 15, 99, 0, 0];

            int[] phases = [4, 3, 2, 1, 0];
            int output = phases.Aggregate(0, (signal, phase) =>
            {
                var sut = new Computer<int>(program, phase, signal);
                return sut.ExecuteOutputs().First();
            });

            output.Should().Be(43210);
        }

        [TestMethod]
        public void AoC2019_Day07_Example2_OutputsMaxSignal()
        {
            int[] program = [3, 23, 3, 24, 1002, 24, 10, 24, 1002, 23, -1, 23, 101, 5, 23, 23, 1, 24, 23, 23, 4, 23, 99, 0, 0];

            int[] phases = [0, 1, 2, 3, 4];
            int output = phases.Aggregate(0, (signal, phase) =>
            {
                var sut = new Computer<int>(program, phase, signal);
                return sut.ExecuteOutputs().First();
            });

            output.Should().Be(54321);
        }

        [TestMethod]
        public void AoC2019_Day07_Example3_OutputsMaxSignal()
        {
            int[] program = [3, 31, 3, 32, 1002, 32, 10, 32, 1001, 31, -2, 31, 1007, 31, 0, 33, 1002, 33, 7, 33, 1, 33, 31, 31, 1, 32, 31, 31, 4, 31, 99, 0, 0, 0];

            int[] phases = [1, 0, 4, 3, 2];
            int output = phases.Aggregate(0, (signal, phase) =>
            {
                var sut = new Computer<int>(program, phase, signal);
                return sut.ExecuteOutputs().First();
            });

            output.Should().Be(65210);
        }

        [TestMethod]
        public void AoC2019_Day09_Example1_OutputsQuine()
        {
            long[] program = [109, 1, 204, -1, 1001, 100, 1, 100, 1008, 100, 16, 101, 1006, 101, 0, 99];

            var sut = new Computer<long>(program);
            var output = sut.ExecuteOutputs().ToList();

            output.Should().BeEquivalentTo(program, opt => opt.WithStrictOrdering());
        }

        [TestMethod]
        public void AoC2019_Day09_Example2_Outputs16DigitNumber()
        {
            long[] program = [1102, 34915192, 34915192, 7, 4, 7, 99, 0];

            var sut = new Computer<long>(program);
            var output = sut.ExecuteOutputs().ToList();

            output.Should().HaveCount(1);
            output[0].Should().BeGreaterThanOrEqualTo(1_000_000_000_000_000L);
            output[0].Should().BeLessThan(10_000_000_000_000_000L);
        }

        [TestMethod]
        public void AoC2019_Day09_Example3_OutputsMiddleNumber()
        {
            long[] program = [104, 1125899906842624, 99];

            var sut = new Computer<long>(program);
            var output = sut.ExecuteOutputs().ToList();

            output.Should().HaveCount(1);
            output[0].Should().Be(program[1]);
        }
    }
}
