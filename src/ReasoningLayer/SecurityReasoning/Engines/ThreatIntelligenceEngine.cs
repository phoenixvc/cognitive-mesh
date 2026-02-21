using CognitiveMesh.ReasoningLayer.SecurityReasoning.Ports;

namespace CognitiveMesh.ReasoningLayer.SecurityReasoning.Engines
{
    /// <summary>
    /// Implements the core reasoning logic for threat intelligence. This engine analyzes security data
    /// to detect threats, identify Indicators of Compromise (IOCs), and calculate dynamic risk scores,
    /// forming a critical part of the proactive defense strategy in the Zero-Trust framework.
    /// </summary>
    public class ThreatIntelligenceEngine : IThreatIntelligencePort
    {
        private readonly ILogger<ThreatIntelligenceEngine> _logger;
        private readonly IThreatFeedAdapter _threatFeedAdapter; // In a real system, this would be an external port.

        // Simulated adapter for demo purposes
        private interface IThreatFeedAdapter
        {
            Task<Dictionary<string, string>> GetKnownIOCsAsync();
        }

        private class SimulatedThreatFeedAdapter : IThreatFeedAdapter
        {
            private readonly Dictionary<string, string> _knownIOCs = new()
            {
                // IP Addresses associated with botnets or command-and-control servers
                { "198.51.100.14", "Known C2 Server (APT28)" },
                { "203.0.113.88", "Mirai Botnet Controller" },
                // File hashes of known malware
                { "e8845c47155a442b3c08f4326164d2f1", "WannaCry Ransomware Dropper" },
                { "d41d8cd98f00b204e9800998ecf8427e", "Empty file hash, often used in tests but can be suspicious" },
                // Malicious domains
                { "malicious-domain-example.com", "Phishing Campaign Domain (Group-IB)" },
                { "bad-actor-updates.net", "Malware Distribution Point" }
            };

            public Task<Dictionary<string, string>> GetKnownIOCsAsync()
            {
                // Simulate a network call to a threat intelligence provider
                return Task.FromResult(_knownIOCs);
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ThreatIntelligenceEngine"/> class.
        /// </summary>
        /// <param name="logger">The logger instance for structured logging.</param>
        public ThreatIntelligenceEngine(ILogger<ThreatIntelligenceEngine> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _threatFeedAdapter = new SimulatedThreatFeedAdapter(); // Injected for demo
        }

        /// <inheritdoc />
        public Task<ThreatAnalysisResponse> AnalyzeThreatPatternsAsync(ThreatAnalysisRequest request)
        {
            _logger.LogInformation("Analyzing {EventCount} security events for threat patterns.", request.Events.Count());

            // Rule 1: Impossible Travel Detection
            var loginEvents = request.Events
                .Where(e => e.EventType == "LoginAttempt" && (bool)e.Data.GetValueOrDefault("isSuccess", false))
                .OrderBy(e => e.Timestamp)
                .ToList();

            for (int i = 0; i < loginEvents.Count - 1; i++)
            {
                var event1 = loginEvents[i];
                var event2 = loginEvents[i + 1];
                var timeDiff = (event2.Timestamp - event1.Timestamp).TotalHours;
                
                // Simplified distance check. A real system would use geolocation and calculate speed.
                var location1 = event1.Data.GetValueOrDefault("location")?.ToString();
                var location2 = event2.Data.GetValueOrDefault("location")?.ToString();

                if (timeDiff < 1 && location1 != location2)
                {
                    _logger.LogWarning("Impossible Travel detected for subject '{SubjectId}'. Logins from '{Location1}' and '{Location2}' within {TimeDiff} hours.",
                        event1.Data["subjectId"], location1, location2, timeDiff);
                    return Task.FromResult(new ThreatAnalysisResponse
                    {
                        IsThreatDetected = true,
                        Severity = "High",
                        ThreatDescription = "Impossible Travel Detected: Successful logins from geographically distinct locations in an impossibly short time frame.",
                        RecommendedActions = new List<string> { "Lock user account.", "Invalidate active sessions.", "Require immediate password reset." }
                    });
                }
            }

            // Rule 2: Brute-Force Attack Detection
            var failedLogins = request.Events
                .Where(e => e.EventType == "LoginAttempt" && !(bool)e.Data.GetValueOrDefault("isSuccess", false))
                .ToList();
            
            if (failedLogins.Count > 5 && loginEvents.Any())
            {
                _logger.LogWarning("Potential Brute-Force Attack detected for subject '{SubjectId}'. {FailedCount} failed logins followed by a success.",
                    loginEvents.First().Data["subjectId"], failedLogins.Count);
                return Task.FromResult(new ThreatAnalysisResponse
                {
                    IsThreatDetected = true,
                    Severity = "Medium",
                    ThreatDescription = "Potential Brute-Force Attack: A high number of failed login attempts was followed by a successful login.",
                    RecommendedActions = new List<string> { "Monitor user activity for suspicious behavior.", "Enable multi-factor authentication if not already active." }
                });
            }

            // Rule 3: Data Exfiltration Detection
            var privilegeEscalation = request.Events.FirstOrDefault(e => e.EventType == "PrivilegeEscalation");
            if (privilegeEscalation != null)
            {
                var largeDataAccess = request.Events
                    .Where(e => e.Timestamp > privilegeEscalation.Timestamp && e.EventType == "DataAccess" && (long)e.Data.GetValueOrDefault("bytesTransferred", 0L) > 100_000_000) // >100MB
                    .ToList();
                
                if (largeDataAccess.Any())
                {
                    _logger.LogWarning("Potential Data Exfiltration detected for subject '{SubjectId}'. Large data access event occurred after privilege escalation.",
                        privilegeEscalation.Data["subjectId"]);
                    return Task.FromResult(new ThreatAnalysisResponse
                    {
                        IsThreatDetected = true,
                        Severity = "Critical",
                        ThreatDescription = "Potential Data Exfiltration: A user account accessed an unusually large amount of data shortly after a privilege escalation event.",
                        RecommendedActions = new List<string> { "Immediately suspend user account.", "Isolate affected systems from the network.", "Begin forensic analysis of data access logs." }
                    });
                }
            }
            
            _logger.LogInformation("No specific threat patterns detected in the event stream.");
            return Task.FromResult(new ThreatAnalysisResponse { IsThreatDetected = false });
        }

        /// <inheritdoc />
        public async Task<IOCDetectionResponse> DetectIndicatorsOfCompromiseAsync(IOCDetectionRequest request)
        {
            _logger.LogInformation("Scanning {ArtifactCount} artifacts for known Indicators of Compromise.", request.Artifacts.Sum(a => a.Value.Count));
            var knownIOCs = await _threatFeedAdapter.GetKnownIOCsAsync();
            var response = new IOCDetectionResponse();

            foreach (var artifactType in request.Artifacts.Keys)
            {
                foreach (var artifactValue in request.Artifacts[artifactType])
                {
                    if (knownIOCs.TryGetValue(artifactValue, out var threatInfo))
                    {
                        _logger.LogWarning("Detected known IOC: Type='{ArtifactType}', Value='{ArtifactValue}', Threat='{ThreatInfo}'",
                            artifactType, artifactValue, threatInfo);
                        response.DetectedIOCs.Add(new DetectedIOC
                        {
                            ArtifactType = artifactType,
                            ArtifactValue = artifactValue,
                            ThreatInfo = threatInfo
                        });
                    }
                }
            }

            if (response.DetectedIOCs.Any())
            {
                _logger.LogInformation("Found {IOCFoundCount} known IOCs.", response.DetectedIOCs.Count);
            }
            else
            {
                _logger.LogInformation("No known IOCs found in the provided artifacts.");
            }

            return response;
        }

        /// <inheritdoc />
        public Task<RiskScoringResponse> CalculateRiskScoreAsync(RiskScoringRequest request)
        {
            _logger.LogInformation("Calculating dynamic risk score for subject '{SubjectId}' performing action '{Action}'.", request.SubjectId, request.Action);
            var score = 20; // Baseline score for an authenticated user
            var contributingFactors = new List<string> { "Baseline score for authenticated user (+20)" };

            // Heuristic 1: Time of day
            var requestTime = request.Context.GetValueOrDefault("timestamp", DateTime.UtcNow) is DateTime time ? time : DateTime.UtcNow;
            if (requestTime.Hour < 6 || requestTime.Hour > 22)
            {
                score += 15;
                contributingFactors.Add("Off-hours activity (+15)");
            }

            // Heuristic 2: Location
            var location = request.Context.GetValueOrDefault("location")?.ToString();
            if (location == "Unknown" || location == "HighRiskCountry")
            {
                score += 25;
                contributingFactors.Add("High-risk or unknown location (+25)");
            }

            // Heuristic 3: Resource Sensitivity
            var resourceSensitivity = request.Context.GetValueOrDefault("resourceSensitivity")?.ToString();
            if (resourceSensitivity == "High")
            {
                score += 30;
                contributingFactors.Add("Accessing high-sensitivity resource (+30)");
            }

            // Heuristic 4: Recent Behavior
            var recentFailures = request.Context.GetValueOrDefault("recentLoginFailures", 0) is int fails ? fails : 0;
            if (recentFailures > 3)
            {
                score += (recentFailures * 5);
                contributingFactors.Add($"Multiple recent login failures (+{recentFailures * 5})");
            }
            
            score = Math.Min(score, 100); // Cap score at 100

            string riskLevel;
            if (score > 80) riskLevel = "Critical";
            else if (score > 60) riskLevel = "High";
            else if (score > 40) riskLevel = "Medium";
            else riskLevel = "Low";

            _logger.LogInformation("Calculated risk score for subject '{SubjectId}' is {RiskScore} ({RiskLevel}).", request.SubjectId, score, riskLevel);

            return Task.FromResult(new RiskScoringResponse
            {
                RiskScore = score,
                RiskLevel = riskLevel,
                ContributingFactors = contributingFactors
            });
        }
    }
}
