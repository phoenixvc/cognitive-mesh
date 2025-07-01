using System;
using System.Collections.Generic;

namespace CognitiveMesh.UILayer.Models.Marketplace
{
    /// <summary>
    /// Represents a single user review for a marketplace plugin.
    /// </summary>
    public class UserReview
    {
        /// <summary>
        /// A unique identifier for the review.
        /// </summary>
        public string ReviewId { get; set; } = Guid.NewGuid().ToString();

        /// <summary>
        /// The ID of the user who submitted the review.
        /// </summary>
        public string UserId { get; set; }

        /// <summary>
        /// The display name of the user who submitted the review.
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        /// The star rating provided by the user (e.g., on a scale of 1 to 5).
        /// </summary>
        public int Rating { get; set; }

        /// <summary>
        /// An optional title for the review.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// The main text content of the user's review.
        /// </summary>
        public string Comment { get; set; }

        /// <summary>
        /// The date and time when the review was submitted.
        /// </summary>
        public DateTimeOffset Date { get; set; }
    }

    /// <summary>
    /// Represents an approved and published plugin as it appears in the user-facing marketplace.
    /// This model contains all the information needed for discovery, evaluation, and installation by users.
    /// </summary>
    public class MarketplaceEntry
    {
        /// <summary>
        /// A unique identifier for this marketplace entry.
        /// </summary>
        public string EntryId { get; set; }

        /// <summary>
        /// The core definition of the widget, containing its technical details,
        /// permissions, and metadata.
        /// </summary>
        public WidgetDefinition WidgetDefinition { get; set; }

        /// <summary>
        /// The date and time when this plugin was first published to the marketplace.
        /// </summary>
        public DateTimeOffset PublishedDate { get; set; }

        /// <summary>
        /// The date and time of the most recent update to this plugin.
        /// </summary>
        public DateTimeOffset LastUpdated { get; set; }

        /// <summary>
        /// A counter tracking the total number of times this plugin has been installed or enabled by users.
        /// </summary>
        public int DownloadCount { get; set; }

        /// <summary>
        /// The average user rating for this plugin, typically on a scale of 1 to 5.
        /// </summary>
        public double Rating { get; set; }

        /// <summary>
        /// A collection of user-submitted reviews for the plugin.
        /// </summary>
        public List<UserReview> Reviews { get; set; } = new List<UserReview>();

        /// <summary>
        /// A list of URLs pointing to screenshots or promotional images for the plugin.
        /// </summary>
        public List<string> Screenshots { get; set; } = new List<string>();

        /// <summary>
        /// A dictionary of links to external documentation resources, such as user guides or API references.
        /// The key is the link title (e.g., "User Guide") and the value is the URL.
        /// </summary>
        public Dictionary<string, string> DocumentationLinks { get; set; } = new Dictionary<string, string>();

        /// <summary>
        /// A flag indicating whether the plugin is currently active and available for installation.
        /// Admins can use this to temporarily delist a plugin without deleting it.
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// A flag indicating whether this plugin should be highlighted or promoted on the marketplace's main page.
        /// </summary>
        public bool IsFeatured { get; set; }

        /// <summary>
        /// A list of categories this plugin belongs to, used for filtering and discoverability in the marketplace UI.
        /// This can be curated by an admin and may differ from the widget definition's single category.
        /// </summary>
        public List<string> Categories { get; set; } = new List<string>();
    }
}
