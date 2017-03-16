using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace RedditSharp.Things
{
    /// <summary>
    /// A reddit user.
    /// </summary>
    public class RedditUser : Thing
    {
        #pragma warning disable 1591
        public RedditUser(Reddit reddit, JToken json) : base(reddit, json) {
        }
        #pragma warning restore 1591

        private string OverviewUrl => $"/user/{Name}.json";
        private string CommentsUrl => $"/user/{Name}/comments.json";
        private string LinksUrl => $"/user/{Name}/submitted.json";
        private const string SubscribedSubredditsUrl = "/subreddits/mine.json";
        private string LikedUrl => $"/user/{Name}/liked.json";
        private string DislikedUrl => $"/user/{Name}/disliked.json";
        private string SavedUrl => $"/user/{Name}/saved.json";

        private const int MAX_LIMIT = 100;

        /// <inheritdoc/>
        protected override JToken GetJsonData(JToken json) => json["name"] == null ? json["data"] : json;

        /// <summary>
        /// Reddit username.
        /// </summary>
        [JsonProperty("name")]
        public string Name { get; internal set; }

        /// <summary>
        /// Returns true if the user has reddit gold.
        /// </summary>
        [JsonProperty("is_gold")]
        public bool HasGold { get; private set; }

        /// <summary>
        /// Returns true if the user is a moderator of any subreddit.
        /// </summary>
        [JsonProperty("is_mod")]
        public bool IsModerator { get; private set; }

        /// <summary>
        /// Total link karma of the user.
        /// </summary>
        [JsonProperty("link_karma")]
        public int LinkKarma { get; private set; }

        /// <summary>
        /// Total comment karma of the user.
        /// </summary>
        [JsonProperty("comment_karma")]
        public int CommentKarma { get; private set; }

        /// <summary>
        /// Date the user was created.
        /// </summary>
        [JsonProperty("created")]
        [JsonConverter(typeof(UnixTimestampConverter))]
        public DateTime Created { get; private set; }

        /// <summary>
        /// Return the users overview.
        /// </summary>
        public Listing<VotableThing> GetOverview(int max = -1) => Listing<VotableThing>.Create(Reddit, OverviewUrl, max, 100);

        /// <summary>
        /// Return a <see cref="Listing{T}"/> of posts liked by the logged in user.
        /// </summary>
        public Listing<Post> GetLikedPosts(int max = -1) => Listing<Post>.Create(Reddit,LikedUrl, max, 100);

        /// <summary>
        /// Return a <see cref="Listing{T}"/> of posts disliked by the logged in user.
        /// </summary>
        public Listing<Post> GetDislikedPosts(int max = -1) => Listing<Post>.Create(Reddit, DislikedUrl, max, 100);

        /// <summary>
        /// Return a <see cref="Listing{T}"/> of comments made by the user.
        /// </summary>
        public Listing<Comment> GetComments(int max = -1) => Listing<Comment>.Create(Reddit, CommentsUrl, max, 100);

        /// <summary>
        /// Return a <see cref="Listing{T}"/> of posts made by the user.
        /// </summary>
        public Listing<Post> GetPosts(int max = -1) => Listing<Post>.Create(Reddit, LinksUrl, max, 100);

        /// <summary>
        /// Return a list of subscribed subreddits for the logged in user.
        /// </summary>
        public Listing<Subreddit> GetSubscribedSubreddits(int max = -1) => Listing<Subreddit>.Create(Reddit, SubscribedSubredditsUrl, max, 100);

        static string QueryString(Sort sort, int limit, FromTime time) =>
          $"?sort={sort.ToString("g")}&limit={limit}&t={time.ToString("g")}";

        static void CheckRange(int limit, int max_limit) {
            if ((limit < 1) || (limit > max_limit))
                throw new ArgumentOutOfRangeException(nameof(limit), $"Valid range: [1, {max_limit}]");
        }

        /// <summary>
        /// Get a listing of comments and posts from the user sorted by <paramref name="sorting"/>, from time <paramref name="fromTime"/>
        /// and limited to <paramref name="limit"/>.
        /// </summary>
        /// <param name="sorting">How to sort the comments (hot, new, top, controversial).</param>
        /// <param name="limit">How many comments to fetch per request. Max is 100.</param>
        /// <param name="fromTime">What time frame of comments to show (hour, day, week, month, year, all).</param>
        /// <returns>The listing of comments requested.</returns>
        public Listing<VotableThing> GetOverview(Sort sorting = Sort.New, int limit = 25, FromTime fromTime = FromTime.All)
        {
            CheckRange(limit, MAX_LIMIT);
            string overviewUrl = OverviewUrl + QueryString(sorting, limit, fromTime);
            return new Listing<VotableThing>(Reddit, overviewUrl);
        }

        /// <summary>
        /// Get a listing of comments from the user sorted by <paramref name="sorting"/>, from time <paramref name="fromTime"/>
        /// and limited to <paramref name="limit"/>.
        /// </summary>
        /// <param name="sorting">How to sort the comments (hot, new, top, controversial).</param>
        /// <param name="limit">How many comments to fetch per request. Max is 100.</param>
        /// <param name="fromTime">What time frame of comments to show (hour, day, week, month, year, all).</param>
        /// <returns>The listing of comments requested.</returns>
        public Listing<Comment> GetComments(Sort sorting = Sort.New, int limit = 25, FromTime fromTime = FromTime.All)
        {
            CheckRange(limit, MAX_LIMIT);
            string commentsUrl = CommentsUrl + QueryString(sorting, limit, fromTime);
            return new Listing<Comment>(Reddit, commentsUrl);
        }

        /// <summary>
        /// Get a listing of posts from the user sorted by <paramref name="sorting"/>, from time <paramref name="fromTime"/>
        /// and limited to <paramref name="limit"/>.
        /// </summary>
        /// <param name="sorting">How to sort the posts (hot, new, top, controversial).</param>
        /// <param name="limit">How many posts to fetch per request. Max is 100.</param>
        /// <param name="fromTime">What time frame of posts to show (hour, day, week, month, year, all).</param>
        /// <returns>The listing of posts requested.</returns>
        public Listing<Post> GetPosts(Sort sorting = Sort.New, int limit = 25, FromTime fromTime = FromTime.All)
        {
            CheckRange(limit, 100);
            string linksUrl = LinksUrl + QueryString(sorting, limit, fromTime);
            return new Listing<Post>(Reddit, linksUrl);
        }

        /// <summary>
        /// Get a listing of comments and posts saved by the user sorted by <paramref name="sorting"/>, from time <paramref name="fromTime"/>
        /// and limited to <paramref name="limit"/>.
        /// </summary>
        /// <param name="sorting">How to sort the comments (hot, new, top, controversial).</param>
        /// <param name="limit">How many comments to fetch per request. Max is 100.</param>
        /// <param name="fromTime">What time frame of comments to show (hour, day, week, month, year, all).</param>
        /// <returns>The listing of posts and/or comments requested that the user saved.</returns>
        public Listing<VotableThing> GetSaved(Sort sorting = Sort.New, int limit = 25, FromTime fromTime = FromTime.All)
        {
            CheckRange(limit, 100);
            string savedUrl = SavedUrl + QueryString(sorting, limit, fromTime);
            return new Listing<VotableThing>(Reddit, savedUrl);
        }

        /// <inheritdoc/>
        public override string ToString() => Name;

    }
#pragma warning disable 1591
    public enum Sort
    {
        New,
        Hot,
        Top,
        Controversial
    }

    public enum FromTime
    {
        All,
        Year,
        Month,
        Week,
        Day,
        Hour
    }
#pragma warning restore 1591
}
