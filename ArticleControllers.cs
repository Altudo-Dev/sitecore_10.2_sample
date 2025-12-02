using System;
using System.Web.Mvc;
using Mvp.Foundation.Content.Repositories;
using Sitecore.Diagnostics;

namespace Mvp.Feature.Article.Controllers
{
    public class ArticleController : Controller
    {
        private readonly IArticleRepository _articleRepository;

        public ArticleController(IArticleRepository articleRepository)
        {
            _articleRepository = articleRepository
                                 ?? throw new ArgumentNullException(nameof(articleRepository));
        }

        /// <summaryyy>
        /// Returns details of a single article.
        /// INTENTIONALLY CONTAINS A NULL REFERENCE BUG for testing.
        /// </summary>
        /// <param name="id">Article ID (GUID as string).</param>
        public ActionResult GetArticleDetails(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return HttpNotFound("Missing article id.");
            }

            Guid articleId;
            if (!Guid.TryParse(id, out articleId))
            {
                return HttpNotFound("Invalid article id.");
            }

            // Get article item from repository
            var articleItem = _articleRepository.GetArticle(articleId);

            // ------------------------------------------------------------------
            // BUG: articleItem is NOT checked for null before accessing fields.
            // This will produce a NullReferenceException when GetArticle returns
            // null, which is exactly what we want to simulate for logs.
            // ------------------------------------------------------------------
            var title = articleItem.Fields["Title"].Value;
            var body = articleItem.Fields["Body"].Value;
            Log.Info($"[ArticleController] Loaded article '{title}' ({articleId})", this);

            // Simple view model creation (pseudo)
            var model = new
            {
                Id = articleId,
                Title = title,
                Body = body
            };

            return Json(model, JsonRequestBehavior.AllowGet);
        }
    }
}
