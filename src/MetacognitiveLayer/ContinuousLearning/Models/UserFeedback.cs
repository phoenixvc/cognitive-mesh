namespace CognitiveMesh.MetacognitiveLayer.ContinuousLearning.Models
{
    /// <summary>
    /// Represents feedback provided by a user for a specific query or interaction.
    /// </summary>
    public class UserFeedback
    {
        /// <summary>
        /// The user's satisfaction rating, typically on a 1-5 scale.
        /// </summary>
        public int Rating { get; set; }

        /// <summary>
        /// Optional textual comments from the user providing more context.
        /// </summary>
        public string Comments { get; set; }
    }
}
