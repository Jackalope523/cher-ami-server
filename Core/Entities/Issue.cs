using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Core.Boundaries;

using static Core.Entities.Psijic;

namespace Core.Entities
{

    public class Issue
    {
		#region Variables

		//////
		// Constants
		//////////////

		public const int MaximumTitleLength = 30;

		///////
		// Properties
		///////////////

        // Core
		public long Id { get; init; }
        public long CircleId { get; init; }
        public string Title { get; set; }
        public IssueType Type { get; set; }
        public DateTimeOffset StartDate { get; set; }
        public DateTimeOffset EndDate { get; set; }

        public bool IsUpcoming
            => HasYet(StartDate);
        public bool IsOpen
            => HasAlready(StartDate) && HasYet(EndDate);

        public bool Exists { get; set; } = true;

        ////////
        // Synced Properties
        //////////////////////
        
        public Circle Circle { get; }

        public List<PostShard> Posts { get; }

        public List<CoreOrder> Orders { get; }

        #endregion

        #region Initialisation & Extraction

  

        public Issue(Circle circle, List<PostShard> posts, List<CoreOrder> orders)
        {
            Circle = circle;
            Posts = posts;
            Orders = orders;
        }

        public Issue(CoreIssue fromIssue)
        {
            Id = fromIssue.Id;
            CircleId = fromIssue.CircleId;
            Title = fromIssue.Title;
            Type = fromIssue.Type;
            StartDate = fromIssue.StartDate;
            EndDate = fromIssue.EndDate;
        }

        public CoreIssue ToCoreIssue()
        {
            return new(Id, CircleId, Type,
                Title, StartDate, EndDate);
        }

        public IssueShard ToIssueShard()
        {
            return new(Id, CircleId, Type,
                Title, StartDate, EndDate);
        }

        public ProfileIssueShard ToProfileIssueShard()
        {
            return new(Id, EndDate);
        }

		#endregion

		#region Composition

		public bool ValidateAndNormalise(out string issues)
        {
            issues = "";

            // Sanitise User content
            Title = ContentValidation.NormaliseText(Title, MaximumTitleLength);
            if (string.IsNullOrEmpty(Title)) { issues += "Title cannot be empty. "; }

            // If in the past, make it now
            if (HappenedBefore(StartDate, Time)) { StartDate = Time; }

            // Verify end date after start date
            if (HappenedBefore(EndDate, StartDate)) { issues += "End date before start date. "; }

            return issues.Equals("");
        }

		#endregion

		#region Dissimilation

		public override bool Equals(object obj)
		{
			return obj is Issue other &&
                Exists == other.Exists &&
                Id.Equals(other.Id);
		}

		public override int GetHashCode()
		{
			return Id.GetHashCode();
		}

		#endregion
	}
}
