using PhoneNumbers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using static Core.Entities.Arbiter;

namespace Core.Entities
{
	internal static class ContentValidation
	{
		#region Variables

		private static TextFilter filter;
		private static List<string> DisallowedPhrases = new()
		#region Disallowed Phrases List
		{ "crack", "cocaine" };
		#endregion

		#endregion

		#region Initialiation

		static ContentValidation()
		{
			filter = new TextFilter(DisallowedPhrases);
		}

		#endregion

		#region Operations

		public static bool IsEmailValid(string email)
		{
			if (!MailAddress.TryCreate(email, out _)) { return false; }

			return true;
		}

		public static string NormaliseText(string content, int maximumLength = int.MaxValue)
		{
			maximumLength = Math.Min(content.Length, maximumLength);

			content = content[..maximumLength];

			// Sanitise text
			content = filter.Sanitise(content);

            // Check if text contains inappropriate phrases
            content = filter.CensorText(content);

            // Check if text contains links, phone numbers, or emails
            content = filter.HideInformation(content);

            return content;
        }

        public static bool TryNormalisePhoneNumber(string phoneNumber, out string normalisedPhoneNumber)
		{
			normalisedPhoneNumber = PhoneNumberUtil.ExtractPossibleNumber(phoneNumber);

			// Check if phone number is valid
			if (string.IsNullOrEmpty(normalisedPhoneNumber) ||
				!PhoneNumberUtil.IsViablePhoneNumber(normalisedPhoneNumber) ||
				normalisedPhoneNumber.Length < 6)
			{ normalisedPhoneNumber = null; return false; }

			// Normalise number
			normalisedPhoneNumber = PhoneNumberUtil.Normalize(normalisedPhoneNumber);
			return true;
		}

		#endregion
	}

	internal class TextFilter
	{
		#region Variables

		public IList<string> CensoredWords { get; private set; }

		#endregion

		#region Initialisation

		public TextFilter(IEnumerable<string> censoredWords)
		{
			CensoredWords = new List<string>(censoredWords);
		}

		#endregion

		#region Operations

		public string Sanitise(string text)
		{
			// First, remove anything above a double line break
            string result = Regex.Replace(text, @"(\r?\n){3,}", "\n\n");

            // Second, remove line breaks between single letters
            result = Regex.Replace(result, @"(?<=\w)\r?\n(?=\w)", "");

			return result;
        }

		public string CensorText(string text)
		{
			if (string.IsNullOrEmpty(text))
			{
				return "";
			}

			string censoredText = text;

			foreach (string censoredWord in CensoredWords)
			{
				string regularExpression = ToRegexPattern(censoredWord);

				censoredText = Regex.Replace(censoredText, regularExpression, StarCensoredMatch,
					RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
			}

			return censoredText;
		}

		public string HideInformation(string text)
		{
			if (string.IsNullOrEmpty(text))
			{
				return "";
			}

            // Regular expression to find links
            string linkPattern = @"http[s]?://(?:[a-zA-Z]|[0-9]|[$-_@.&+]|[!*\\(\\),]|(?:%[0-9a-fA-F][0-9a-fA-F]))+";

            // Regular expression to find phone numbers (10 digit)
            string phonePattern = @"\b\d{3}[-.]?\d{3}[-.]?\d{4}\b";
			
            // Regular expression to find emails
            string emailPattern = @"[\w\.-]+@[\w\.-]+";

            text = Regex.Replace(text, linkPattern, "[hidden]");
            text = Regex.Replace(text, phonePattern, "[hidden]");
            text = Regex.Replace(text, emailPattern, "[hidden]");

			return text;
        }

		#endregion

		#region Tools

		private string StarCensoredMatch(Match m)
		{
			string word = m.Captures[0].Value;

			return new string('*', word.Length);
		}

		private string ToRegexPattern(string wildcardSearch)
		{
			string regexPattern = Regex.Escape(wildcardSearch);

			regexPattern = regexPattern.Replace(@"\*", ".*?");
			regexPattern = regexPattern.Replace(@"\?", ".");

			if (regexPattern.StartsWith(".*?"))
			{
				regexPattern = regexPattern.Substring(3);

				regexPattern = @"(^\b)*?" + regexPattern;
			}

			regexPattern = @"\b" + regexPattern + @"\b";

			return regexPattern;
		}

		#endregion
	}
}
