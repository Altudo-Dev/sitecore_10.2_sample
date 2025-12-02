using System;
using Sitecore.Data;
using Sitecore.Data.Items;
using Sitecore.Diagnostics;

namespace Mvp.Foundation.Content.Repositories
{
    public interface IArticleRepository
    {
        Item GetArticle(Guid id);
    }

    /// <summary>
    /// Simple Sitecore-backed article repository.
    /// Intentionally throws ItemNotFoundException when the item doesn't exist.
    /// </summary>
    public class ArticleRepository : IArticleRepository
    {
        private readonly Database _database;

        public ArticleRepository(Database database)
        {
            _database = database ?? throw new ArgumentNullException(nameof(database));
        }

        /// <summary>
        /// Gets a Sitecore Item representing the article.
        /// INTENTIONALLY THROWS ItemNotFoundException if item is missing.
        /// </summary>
        public Item GetArticle(Guid id)
        {
            var itemId = new ID(id);

            Log.Info($"[ArticleRepository] Loading article item {itemId}", this);

            var item = _database.GetItem(itemId);

            // --------------------------------------------------------------
            // BUG (by design): instead of returning null when item is missing,
            // we throw an ItemNotFoundException. This matches the sample log:
            // "Sitecore.Data.Items.ItemNotFoundException: Item not found."
            // --------------------------------------------------------------
            if (item == null)
            {
                throw new ItemNotFoundException("Item not found.");
            }

            return item;
        }
    }
}
