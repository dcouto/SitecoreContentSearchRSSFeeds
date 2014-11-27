using Sitecore.Configuration;
using Sitecore.ContentSearch;
using Sitecore.ContentSearch.Linq;
using Sitecore.ContentSearch.Linq.Utilities;
using Sitecore.Data;
using Sitecore.Data.Fields;
using Sitecore.Data.Items;
using Sitecore.Syndication;
using SitecoreContentSearchRSSFeeds.Web.SearchTypes;
using System.Collections.Generic;
using System.Linq;

namespace SitecoreContentSearchRSSFeeds.Web.Syndication
{
    public class ContentSearchRssFeed : PublicFeed
    {
        private ID _rootPath = ID.Null;
        private ID RootPath
        {
            get
            {
                if (_rootPath.IsNull)
                    _rootPath = ((LookupField)this.FeedItem.Fields["Root Path"]).TargetID;

                return _rootPath;
            }
        }

        private IEnumerable<ID> _includedContentTypes;
        private IEnumerable<ID> IncludedContentTypes
        {
            get
            {
                if (_includedContentTypes == null)
                    _includedContentTypes = ((MultilistField)this.FeedItem.Fields["Included Content Types"]).Items.Select(ID.Parse);

                return _includedContentTypes;
            }
        }

        private IEnumerable<ID> _tags;
        private IEnumerable<ID> Tags
        {
            get
            {
                if (_tags == null)
                    _tags = ((MultilistField)this.FeedItem.Fields["Tags"]).Items.Select(ID.Parse);

                return _tags;
            }
        }

        public override IEnumerable<Item> GetSourceItems()
        {
            // RootPath and IncludedContentTypes are required
            // If these are not set, return an empty Enumerable
            if (ID.IsNullOrEmpty(RootPath) || IncludedContentTypes.Any() == false)
                return Enumerable.Empty<Item>();

            using (
                var searcher =
                    ContentSearchManager.GetIndex(string.Format("sitecore_{0}_index", Sitecore.Context.Database.Name))
                        .CreateSearchContext())
            {
                var query = PredicateBuilder.True<CustomSearchResultItem>();

                // Root Path
                query = query.And(i => i.Paths.Contains(RootPath));


                // Included Content Types
                var contentTypesQuery = PredicateBuilder.False<CustomSearchResultItem>();

                foreach (var ct in IncludedContentTypes)
                {
                    var id = ct;

                    contentTypesQuery = contentTypesQuery.Or(i => i.TemplateId == id);
                }

                query = query.And(contentTypesQuery);


                // Tags
                var tagsQuery = PredicateBuilder.True<CustomSearchResultItem>();

                foreach (var tag in Tags)
                {
                    var id = tag;

                    tagsQuery = tagsQuery.And(i => i.Links.Contains(id));
                }

                query = query.And(tagsQuery);


                // results
                var maximumItemsInFeed = int.Parse(Settings.GetSetting("Feeds.MaximumItemsInFeed", "50"));

                var results = searcher.GetQueryable<CustomSearchResultItem>()
                    .Where(query)
                    .Take(maximumItemsInFeed)
                    .GetResults()
                    .Hits
                    .Select(i => i.Document.GetItem())
                    .ToList();

                return results;
            }
        }
    }
}