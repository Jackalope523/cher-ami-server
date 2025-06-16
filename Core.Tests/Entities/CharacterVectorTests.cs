using System;
using Core.Entities;
using Xunit;

namespace Core.Tests.Entities
{
	public class CharacterVectorTests
	{
		[Fact]
		public void AngleBetween_SameVector_ZeroAngle()
		{
			// Arrange
			CharacterVector vector = new() { Extraversion = 30, Athleticism = 40, Chaoticness = 20, Competitiveness = 10, Industriousness = 50, NightOwl = 60 };

			// Act
			float angle = CharacterVector.AngleBetween(vector, vector);

			// Assert
			Assert.Equal(0, angle);
		}

		[Fact]
		public void AngleBetween_DifferentVectors_CorrectAngle()
		{
			// Arrange
			CharacterVector vector1 = new() { Extraversion = 30, Athleticism = 40, Chaoticness = 20, Competitiveness = 10, Industriousness = 50, NightOwl = 60 };
			CharacterVector vector2 = new() { Extraversion = 10, Athleticism = 20, Chaoticness = 30, Competitiveness = 40, Industriousness = 50, NightOwl = 60 };

			// Act
			float angle = CharacterVector.AngleBetween(vector1, vector2);

			// Assert
			Assert.True(angle > 0 && angle < MathF.PI);
		}

		[Fact]
		public void AngleBetweenAffected_DifferentVectors_CorrectAngleWithOpennessModifier()
		{
			// Arrange
			CharacterVector vector1 = new() { Extraversion = 30, Athleticism = 40, Chaoticness = 20, Competitiveness = 10, Industriousness = 50, NightOwl = 60, Openness = 70 };
			CharacterVector vector2 = new() { Extraversion = 10, Athleticism = 20, Chaoticness = 30, Competitiveness = 40, Industriousness = 50, NightOwl = 60, Openness = 80 };

			// Act
			float angle = CharacterVector.AngleBetweenAffected(vector1, vector2);

			// Assert
			float expectedAngle = ((vector1.Openness + vector2.Openness) / 100f) * CharacterVector.AngleBetween(vector1, vector2);
			Assert.Equal(expectedAngle, angle);
		}

		[Fact]
		public void MoveTowards_TargetVector_CorrectResult()
		{
			// Arrange
			CharacterVector startVector = new() { Extraversion = 30, Athleticism = 40, Chaoticness = 20, Competitiveness = 10, Industriousness = 50, NightOwl = 60 };
			CharacterVector targetVector = new() { Extraversion = 50, Athleticism = 60, Chaoticness = 40, Competitiveness = 30, Industriousness = 70, NightOwl = 80 };

			// Act
			CharacterVector result = startVector.MoveTowards(targetVector, 0.5f);

			// Assert
			Assert.Equal(40, result.Extraversion);
			Assert.Equal(50, result.Athleticism);
			Assert.Equal(30, result.Chaoticness);
			Assert.Equal(20, result.Competitiveness);
			Assert.Equal(60, result.Industriousness);
			Assert.Equal(70, result.NightOwl);
		}
	}
}
