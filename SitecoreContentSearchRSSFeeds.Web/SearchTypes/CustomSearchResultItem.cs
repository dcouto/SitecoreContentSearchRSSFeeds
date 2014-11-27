using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Sitecore.ContentSearch;
using Sitecore.ContentSearch.SearchTypes;
using Sitecore.Data;

namespace SitecoreContentSearchRSSFeeds.Web.SearchTypes
{
    public class CustomSearchResultItem : SearchResultItem
    {
        [IndexField("_links")]
        public IEnumerable<ID> Links { get; set; }
    }
}