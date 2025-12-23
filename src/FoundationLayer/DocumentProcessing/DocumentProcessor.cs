namespace FoundationLayer.DocumentProcessing
{
    /// <summary>
    /// Handles ingestion, chunking, and indexing of documents for RAG and knowledge services.
    /// </summary>
    public class DocumentProcessor : IDisposable
    {
        private readonly IBlobStorageAdapter _blobStorage;
        private readonly IVectorDatabaseAdapter _vectorDb;
        private bool _disposed = false;

        public DocumentProcessor(IBlobStorageAdapter blobStorage, IVectorDatabaseAdapter vectorDb)
        {
            _blobStorage = blobStorage ?? throw new ArgumentNullException(nameof(blobStorage));
            _vectorDb = vectorDb ?? throw new ArgumentNullException(nameof(vectorDb));
        }

        /// <summary>
        /// Ingests a document from blob storage, processes it, and stores embeddings.
        /// </summary>
        /// <param name="documentId">The identifier of the document to ingest.</param>
        public async Task IngestDocumentAsync(string documentId)
        {
            if (string.IsNullOrWhiteSpace(documentId))
                throw new ArgumentException("Document ID cannot be null or whitespace.", nameof(documentId));

            // Download document content from blob storage
            var content = await _blobStorage.DownloadAsync(documentId);
            
            // Split content into chunks
            var chunks = ChunkText(content);
            
            // Index chunks in vector database
            await IndexChunksAsync(documentId, chunks);
        }

        /// <summary>
        /// Splits raw text into smaller chunks for indexing.
        /// </summary>
        /// <param name="text">Raw document text.</param>
        /// <returns>List of text chunks.</returns>
        public IEnumerable<string> ChunkText(string text)
        {
            if (string.IsNullOrEmpty(text))
                return Array.Empty<string>();

            // Simple chunking by paragraphs first
            var paragraphs = text.Split(new[] { "\n\n" }, StringSplitOptions.RemoveEmptyEntries);
            var chunks = new List<string>();
            
            // Further split long paragraphs
            foreach (var paragraph in paragraphs)
            {
                if (paragraph.Length <= 1000) // Reasonable chunk size
                {
                    chunks.Add(paragraph);
                }
                else
                {
                    // Split into sentences or fixed-size chunks
                    var sentences = paragraph.Split(new[] { '.', '!', '?' }, StringSplitOptions.RemoveEmptyEntries);
                    var currentChunk = new System.Text.StringBuilder();
                    
                    foreach (var sentence in sentences)
                    {
                        if (currentChunk.Length + sentence.Length > 1000)
                        {
                            chunks.Add(currentChunk.ToString());
                            currentChunk.Clear();
                        }
                        currentChunk.Append(sentence).Append('.');
                    }
                    
                    if (currentChunk.Length > 0)
                    {
                        chunks.Add(currentChunk.ToString());
                    }
                }
            }
            
            return chunks;
        }

        /// <summary>
        /// Indexes text chunks into the vector database for semantic retrieval.
        /// </summary>
        /// <param name="documentId">Original document identifier.</param>
        /// <param name="chunks">List of text chunks to index.</param>
        private async Task IndexChunksAsync(string documentId, IEnumerable<string> chunks)
        {
            if (string.IsNullOrWhiteSpace(documentId))
                throw new ArgumentException("Document ID cannot be null or whitespace.", nameof(documentId));
                
            if (chunks == null)
                throw new ArgumentNullException(nameof(chunks));

            var chunkList = new List<VectorRecord>();
            int chunkIndex = 0;
            
            foreach (var chunk in chunks)
            {
                chunkList.Add(new VectorRecord
                {
                    Id = $"{documentId}_chunk{chunkIndex++}",
                    Text = chunk,
                    Metadata = new Dictionary<string, string>
                    {
                        { "documentId", documentId },
                        { "chunkIndex", chunkIndex.ToString() },
                        { "timestamp", DateTime.UtcNow.ToString("o") }
                    }
                });
            }
            
            await _vectorDb.UpsertVectorsAsync(chunkList);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    // Dispose managed resources
                    if (_blobStorage is IDisposable disposableBlobStorage)
                        disposableBlobStorage.Dispose();
                        
                    if (_vectorDb is IDisposable disposableVectorDb)
                        disposableVectorDb.Dispose();
                }
                _disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
