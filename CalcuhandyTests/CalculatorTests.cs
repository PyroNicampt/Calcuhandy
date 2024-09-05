using Calcuhandy;
namespace CalcuhandyTests {
    public class CalculatorTests {
        [Theory]
        [MemberData(nameof(Equations))]
        public void EquationTheoryTester(string equation, double expected) {
            double epsilon = 0.00000001;
            Assert.Equal(expected, EquationParser.ParseToDouble(equation), epsilon);
        }
        public static IEnumerable<object[]> Equations =>
            new List<object[]> {
                new object[] { "1+1", 1+1 },
                new object[] { "5413-38", 5413-38 },
                new object[] { "319^49.5", Math.Pow(319.0, 49.5) },
                new object[] { "319^-49.2", Math.Pow(319.0, -49.2) },
                new object[] { "-38.3114", -38.3114 },
                new object[] { "2*3+4-5/6^7%8", 2*3+4-5/Math.Pow(6,7)%8 },
                new object[] { "2*(3+4-5/6)^7%8", 2.0* Math.Pow(3.0+4.0-5.0/6.0,7.0)%8.0 },
                new object[] { "1/0", double.PositiveInfinity },

                new object[] { "abs(-1.424)", Math.Abs(-1.424) },
                new object[] { "abs(319.31)", Math.Abs(319.31) },
                new object[] { "sin(pi)", 0 },
                new object[] { "mod(55.3, 2)", 1.3 },
            };
    }
}