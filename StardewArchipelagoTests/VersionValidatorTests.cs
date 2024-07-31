namespace StardewArchipelagoTests
{
    public class VersionValidatorTests
    {
        [SetUp]
        public void Setup()
        {
        }

        [TestCase("1.0.0", "1.0.0")]
        [TestCase("5.24.651", "5.24.651")]
        [TestCase("2.6.2", "2.6.x")]
        [TestCase("8.8.52", "x.8.x")]
        [TestCase("1.0.0", "x.x.x")]
        public void TestVersionIsCorrect(string version, string expectedVersion)
        {
            // Arrange
            var versionValidator = new VersionValidator();

            // Act
            var isCorrect = versionValidator.IsVersionCorrect(version, expectedVersion);

            // Assert
            isCorrect.Should().BeTrue();
        }

        [TestCase("x.x.x", "1.0.0")]
        [TestCase("1.0.0", "1.0.1")]
        [TestCase("1.0.0", "1.1.0")]
        [TestCase("1.0.0", "2.0.0")]
        [TestCase("5.24.651", "2.6.2")]
        [TestCase("5.23.x", "5.24.x")]
        [TestCase("5.24.x", "5.24.52")]
        [TestCase("8.8.52", "x.24.x")]
        [TestCase("1.0.0.0", "x.x.x")]
        public void TestVersionIsIncorrect(string version, string expectedVersion)
        {
            // Arrange
            var versionValidator = new VersionValidator();

            // Act
            var isCorrect = versionValidator.IsVersionCorrect(version, expectedVersion);

            // Assert
            isCorrect.Should().BeFalse();
        }
    }
}
