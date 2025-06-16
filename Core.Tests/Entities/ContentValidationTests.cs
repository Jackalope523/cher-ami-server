using System;
using Core.Entities;
using Xunit;

namespace Core.Tests.Entities
{
	public class ContentValidationTests
	{
		[Fact]
		public void IsEmailValid_ValidEmail_ReturnsTrue()
		{
			// Arrange
			string validEmail = "test@example.com";

			// Act
			bool result = ContentValidation.IsEmailValid(validEmail);

			// Assert
			Assert.True(result);
		}

		[Fact]
		public void IsEmailValid_InvalidEmail_ReturnsFalse()
		{
			// Arrange
			string invalidEmail = "invalidemail";

			// Act
			bool result = ContentValidation.IsEmailValid(invalidEmail);

			// Assert
			Assert.False(result);
		}

		[Fact]
		public void NormaliseText_CensorsDisallowedPhrases()
		{
			// Arrange
			string content = "This is a crack test.";

			// Act
			string result = ContentValidation.NormaliseText(content);

			// Assert
			Assert.DoesNotContain("crack", result); // Assuming the censoring logic replaces the disallowed phrases with '*'
		}

		[Fact]
		public void NormaliseText_DoesNotCensorClbutticPhrases()
		{
			// Arrange
			string content = "This is a cracked test.";

			// Act
			string result = ContentValidation.NormaliseText(content);

			// Assert
			Assert.Contains("cracked", result); // Assuming the censoring logic replaces the disallowed phrases with '*'
		}

		[Fact]
		public void NormaliseText_HidesLinks()
		{
			// Arrange
			string contentWithLinks = "Visit our website at https://example.com";

			// Act
			string result = ContentValidation.NormaliseText(contentWithLinks);

			// Assert
			Assert.DoesNotContain("https://example.com", result);
			Assert.Contains("[hidden]", result);
		}

		[Fact]
		public void NormaliseText_HidesPhoneNumbers()
		{
			// Arrange
			string contentWithPhone = "Contact us at 123-456-7890 for more information";

			// Act
			string result = ContentValidation.NormaliseText(contentWithPhone);

			// Assert
			Assert.DoesNotContain("123-456-7890", result);
			Assert.Contains("[hidden]", result);
		}

		[Fact]
		public void NormaliseText_HidesEmails()
		{
			// Arrange
			string contentWithEmail = "Send us an email at test@example.com";

			// Act
			string result = ContentValidation.NormaliseText(contentWithEmail);

			// Assert
			Assert.DoesNotContain("test@example.com", result);
			Assert.Contains("[hidden]", result);
		}

		[Fact]
		public void NormaliseText_HidesMultipleOccurrences()
		{
			// Arrange
			string contentWithMultiple = "Call 555-123-4567 or visit https://example.com for support. Email us at contact@example.com";

			// Act
			string result = ContentValidation.NormaliseText(contentWithMultiple);

			// Assert
			Assert.DoesNotContain("555-123-4567", result);
			Assert.DoesNotContain("https://example.com", result);
			Assert.DoesNotContain("contact@example.com", result);
			Assert.Contains("[hidden]", result);
		}

		[Fact]
		public void TryNormalisePhoneNumber_ValidPhoneNumber_ReturnsTrueAndNormalises()
		{
			// Arrange
			string validPhoneNumber = "123-456-7890";
			string normalisedPhoneNumber;

			// Act
			bool result = ContentValidation.TryNormalisePhoneNumber(validPhoneNumber, out normalisedPhoneNumber);

			// Assert
			Assert.True(result);
			Assert.Equal("1234567890", normalisedPhoneNumber);
		}

		[Fact]
		public void TryNormalisePhoneNumber_InvalidPhoneNumber_ReturnsFalse()
		{
			// Arrange
			string invalidPhoneNumber = "123";
			string normalisedPhoneNumber;

			// Act
			bool result = ContentValidation.TryNormalisePhoneNumber(invalidPhoneNumber, out normalisedPhoneNumber);

			// Assert
			Assert.False(result);
			Assert.Null(normalisedPhoneNumber);
		}
	}
}
