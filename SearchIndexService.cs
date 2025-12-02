using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Sitecore.Diagnostics;

namespace Mvp.Foundation.Indexing.Services
{// this is test
    public interface ISearchIndexService
    {
        Task<string> SearchArticles(string query, CancellationToken cancellationToken = default);
    }

    /// <summary>
    /// Very simple Solr-backed search service.
    /// Contains an intentional timeout bug to trigger HttpRequestException.
    /// </summary>
    public class SearchIndexService : ISearchIndexService
    {
        private readonly HttpClient _httpClient;

        public SearchIndexService(HttpClient httpClient)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));

            // Very aggressive timeout to more easily trigger timeouts in tests
            _httpClient.Timeout = TimeSpan.FromMilliseconds(5000);
        }

        /// <summary>
        /// Executes a Solr search query.
        /// INTENTIONALLY wraps timeout as HttpRequestException with a message
        /// similar to the sample log ("Solr request timed out after 5000ms").
        /// </summary>
        public async Task<string> SearchArticles(string query, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                return string.Empty;
            }

            var requestUri = $"/solr/articles/select?q={Uri.EscapeDataString(query)}";

            try
            {
                Log.Info($"[SearchIndexService] Executing Solr query: {requestUri}", this);

                // BUG-ish behaviour: no retry, tight timeout, no circuit breaking.
                using (var response = await _httpClient.GetAsync(requestUri, cancellationToken))
                {
                    response.EnsureSuccessStatusCode();
                    return await response.Content.ReadAsStringAsync();
                }
            }
            catch (TaskCanceledException ex)
            {
                // ----------------------------------------------------------
                // When HttpClient times out, it typically throws TaskCanceledException.
                // We rethrow as HttpRequestException with a specific message to match
                // the mock App Insights log: "Solr request timed out after 5000ms".
                // ----------------------------------------------------------
                var message = "Solr request timed out after 5000ms";
                Log.Error("[SearchIndexService] " + message, ex, this);
                throw new HttpRequestException(message, ex);
            }
            catch (HttpRequestException ex)
            {
                Log.Error("[SearchIndexService] HTTP error during Solr query", ex, this);
                throw;
            }
        }
    }
}
