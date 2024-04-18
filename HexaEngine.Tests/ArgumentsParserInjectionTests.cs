namespace HexaEngine.Tests
{
    using NUnit.Framework;
    using System.Collections.Generic;
    using HexaEngine.Editor.External;

    [TestFixture]
    public class ArgumentsParserInjectionTests
    {
        [TestCase("-o output.txt -v", "-o output.txt -v")]
        [TestCase("-o $outputFile -v", "-o output.txt -v")]
        [TestCase("-o $outputFile -v", "-o output.txt -v")]
        public void Parse_NoInjection_ReturnsExpectedResult(string args, string expected)
        {
            // Arrange
            var parser = new ArgumentsParser();

            // Act
            var result = parser.Parse(args, new Dictionary<string, string> { { "outputFile", "output.txt" } });

            // Assert
            Assert.That(result, Is.EqualTo(expected));
        }

        [Test]
        public void Parse_WithPipeInjection_RemovesInjectedPart()
        {
            // Arrange
            var parser = new ArgumentsParser();
            var args = "-o output.txt | rm -rf /";
            var expected = "-o output.txt ";

            // Act
            var result = parser.Parse(args, new Dictionary<string, string>());

            // Assert
            Assert.That(result, Is.EqualTo(expected));
        }

        [Test]
        public void Parse_WithAngleBracketsInjection_RemovesInjectedPart()
        {
            // Arrange
            var parser = new ArgumentsParser();
            var args = "-o output.txt && echo 'Hacked' > hacked.txt";
            var expected = "-o output.txt ";

            // Act
            var result = parser.Parse(args, new Dictionary<string, string>());

            // Assert
            Assert.That(result, Is.EqualTo(expected));
        }

        [Test]
        public void Parse_WithSemicolonInjection_RemovesInjectedPart()
        {
            // Arrange
            var parser = new ArgumentsParser();
            var args = "-o output.txt; rm -rf /";
            var expected = "-o output.txt";

            // Act
            var result = parser.Parse(args, new Dictionary<string, string>());

            // Assert
            Assert.That(result, Is.EqualTo(expected));
        }

        [Test]
        public void Parse_WithBackticksInjection_RemovesInjectedPart()
        {
            // Arrange
            var parser = new ArgumentsParser();
            var args = "-o `rm -rf /`";
            var expected = "-o ";

            // Act
            var result = parser.Parse(args, new Dictionary<string, string>());

            // Assert
            Assert.That(result, Is.EqualTo(expected));
        }

        [Test]
        public void Parse_WithRedirectionInjection_RemovesInjectedPart()
        {
            // Arrange
            var parser = new ArgumentsParser();
            var args = "echo 'Hacked' > /etc/passwd";
            var expected = "echo 'Hacked' ";

            // Act
            var result = parser.Parse(args, new Dictionary<string, string>());

            // Assert
            Assert.That(result, Is.EqualTo(expected));
        }

        [Test]
        public void Parse_WithWildcardInjection_RemovesInjectedPart()
        {
            // Arrange
            var parser = new ArgumentsParser();
            var args = "rm *.txt";
            var expected = "rm ";

            // Act
            var result = parser.Parse(args, new Dictionary<string, string>());

            // Assert
            Assert.That(result, Is.EqualTo(expected));
        }

        [Test]
        public void Parse_WithShellVariableInjection_RemovesInjectedPart()
        {
            // Arrange
            var parser = new ArgumentsParser();
            var args = "export FILE=hacked.txt; echo $FILE";
            var expected = "";

            // Act
            var result = parser.Parse(args, new Dictionary<string, string>());

            // Assert
            Assert.That(result, Is.EqualTo(expected));
        }
    }
}