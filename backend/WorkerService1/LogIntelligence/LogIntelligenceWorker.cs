using System.Text.RegularExpressions;

namespace LogMin.Worker.LogIntelligence;

public static class PatternFamilies
{
    public const string ObjectAccessFailure = "ObjectAccessFailure";
    public const string TimeoutIssue = "TimeoutIssue";
    public const string MemoryExhaustion = "MemoryExhaustion";
    public const string RuntimeCrash = "RuntimeCrash";
    public const string NetworkFailure = "NetworkFailure";
    public const string DependencyUnavailable = "DependencyUnavailable";
    public const string IOFailure = "IOFailure";
    public const string ConcurrencyIssue = "ConcurrencyIssue";
    public const string SerializationFailure = "SerializationFailure";
    public const string ConfigurationError = "ConfigurationError";
    public const string CircuitBreakerOpen = "CircuitBreakerOpen";
    public const string DeploymentIssue = "DeploymentIssue";
    public const string Unknown = "Unknown";
}

public sealed class LogIntelligenceWorker
{
    private const double ClassificationThreshold = 0.7;

    private static readonly Regex TokenSplitter = new(
        @"[^a-zA-Z0-9_]+",
        RegexOptions.Compiled);

    private static readonly HashSet<string> StopTokens = new(StringComparer.OrdinalIgnoreCase)
    {
        "the", "a", "an", "to", "of", "in", "on", "at", "is", "was", "with",
        "for", "and", "or", "as", "by", "be", "it", "this", "that"
    };

    public LogAnalysisResult Analyze(LogEntry entry)
    {
        var rawText = BuildRawText(entry);
        var tokens = Tokenize(rawText);
        var tokenSet = new HashSet<string>(tokens, StringComparer.OrdinalIgnoreCase);

        var signals = new List<SignalHit>();
        signals.AddRange(DetectObjectAccessSignals(tokenSet, rawText));
        signals.AddRange(DetectTimeoutSignals(tokenSet, rawText));
        signals.AddRange(DetectMemorySignals(tokenSet, rawText));
        signals.AddRange(DetectRuntimeCrashSignals(tokenSet, rawText, entry));
        signals.AddRange(DetectNetworkSignals(tokenSet, rawText));
        signals.AddRange(DetectDependencySignals(tokenSet, rawText));
        signals.AddRange(DetectIOSignals(tokenSet, rawText));
        signals.AddRange(DetectConcurrencySignals(tokenSet, rawText));
        signals.AddRange(DetectSerializationSignals(tokenSet, rawText));
        signals.AddRange(DetectConfigurationSignals(tokenSet, rawText));
        signals.AddRange(DetectCircuitBreakerSignals(tokenSet, rawText));
        signals.AddRange(DetectDeploymentSignals(tokenSet, rawText));

        var features = BuildFeatures(signals);
        var (pattern, score) = Classify(signals);

        return new LogAnalysisResult
        {
            Tokens = tokens,
            Signals = signals,
            Features = features,
            Score = Math.Round(score, 3),
            Pattern = pattern
        };
    }

    private static string BuildRawText(LogEntry entry)
    {
        var parts = new List<string>(2);
        if (!string.IsNullOrWhiteSpace(entry.Message)) parts.Add(entry.Message);
        if (!string.IsNullOrWhiteSpace(entry.StackTrace)) parts.Add(entry.StackTrace!);
        return string.Join('\n', parts).ToLowerInvariant();
    }

    private static List<string> Tokenize(string normalizedText)
    {
        if (string.IsNullOrWhiteSpace(normalizedText)) return new List<string>();

        var seen = new HashSet<string>(StringComparer.Ordinal);
        var tokens = new List<string>();
        foreach (var raw in TokenSplitter.Split(normalizedText))
        {
            if (raw.Length < 2) continue;
            if (StopTokens.Contains(raw)) continue;
            if (raw.All(char.IsDigit)) continue;
            if (seen.Add(raw)) tokens.Add(raw);
        }
        return tokens;
    }

    private static IEnumerable<SignalHit> DetectObjectAccessSignals(HashSet<string> t, string text)
    {
        var hits = new List<SignalHit>();

        if (t.Contains("undefined") &&
            (t.Contains("read") || t.Contains("access") || t.Contains("property") || t.Contains("cannot")))
            hits.Add(Signal("undefined access", 0.5, PatternFamilies.ObjectAccessFailure));

        var nullLikeFragments = new[] { "null", "nil", "none", "nullptr" };
        if (AnyTokenContainsAny(t, nullLikeFragments))
            hits.Add(Signal("null-like behavior", 0.5, PatternFamilies.ObjectAccessFailure));

        var memberWords = new[] { "property", "attribute", "member", "field" };
        var missingWords = new[] { "undefined", "missing", "no", "not", "exist" };
        if (memberWords.Any(t.Contains) && missingWords.Any(t.Contains))
            hits.Add(Signal("property access failure", 0.3, PatternFamilies.ObjectAccessFailure));

        var refWords = new[] { "reference", "instance", "object" };
        var refMissing = new[] { "null", "missing", "initialized", "set", "nil", "none" };
        if (refWords.Any(t.Contains) && refMissing.Any(t.Contains))
            hits.Add(Signal("instance/reference missing", 0.4, PatternFamilies.ObjectAccessFailure));

        if (text.Contains("object reference") || text.Contains("not set to an instance"))
            hits.Add(Signal("instance/reference missing", 0.6, PatternFamilies.ObjectAccessFailure));

        return Dedupe(hits);
    }

    private static IEnumerable<SignalHit> DetectTimeoutSignals(HashSet<string> t, string text)
    {
        var hits = new List<SignalHit>();

        if (t.Contains("timeout") || t.Contains("timedout") ||
            (t.Contains("timed") && t.Contains("out")))
            hits.Add(Signal("timeout", 0.6, PatternFamilies.TimeoutIssue));

        if (t.Contains("deadline") && (t.Contains("exceeded") || t.Contains("expired")))
            hits.Add(Signal("expired execution", 0.5, PatternFamilies.TimeoutIssue));

        var latencyWords = new[] { "latency", "slow", "delay", "lag" };
        if (latencyWords.Any(t.Contains))
            hits.Add(Signal("latency increase", 0.3, PatternFamilies.TimeoutIssue));

        var queryWords = new[] { "query", "sql", "db", "database" };
        var queryDelay = new[] { "slow", "delay", "timeout", "expired", "longer" };
        if (queryWords.Any(t.Contains) && queryDelay.Any(t.Contains))
            hits.Add(Signal("query delay", 0.4, PatternFamilies.TimeoutIssue));

        if (text.Contains("took longer") || text.Contains("execution time exceeded"))
            hits.Add(Signal("expired execution", 0.4, PatternFamilies.TimeoutIssue));

        return Dedupe(hits);
    }

    private static IEnumerable<SignalHit> DetectMemorySignals(HashSet<string> t, string text)
    {
        var hits = new List<SignalHit>();

        if (t.Contains("oom") || t.Contains("outofmemory") || t.Contains("outofmemoryerror") ||
            t.Contains("outofmemoryexception") || (t.Contains("out") && t.Contains("memory")))
            hits.Add(Signal("out of memory", 0.6, PatternFamilies.MemoryExhaustion));

        var heapWords = new[] { "heap", "gc", "allocation", "allocator", "malloc" };
        if (heapWords.Any(t.Contains))
            hits.Add(Signal("heap error", 0.4, PatternFamilies.MemoryExhaustion));

        var resourceWords = new[] { "resource", "exhausted", "exhaustion", "quota", "capacity", "limit" };
        var hitResource = resourceWords.Count(t.Contains);
        if (hitResource >= 2)
            hits.Add(Signal("resource exhaustion", 0.4, PatternFamilies.MemoryExhaustion));
        else if (t.Contains("exhausted") || t.Contains("exhaustion"))
            hits.Add(Signal("resource exhaustion", 0.25, PatternFamilies.MemoryExhaustion));

        return Dedupe(hits);
    }

    private static IEnumerable<SignalHit> DetectRuntimeCrashSignals(
        HashSet<string> t, string text, LogEntry entry)
    {
        var hits = new List<SignalHit>();

        var exceptionWords = new[] { "exception", "error", "throw", "thrown", "fatal", "panic", "traceback" };
        if (exceptionWords.Any(t.Contains))
            hits.Add(Signal("exception presence", 0.2, PatternFamilies.RuntimeCrash));

        if (HasStackTraceShape(entry, text))
            hits.Add(Signal("stack trace presence", 0.3, PatternFamilies.RuntimeCrash));

        var crashWords = new[] { "crash", "crashed", "abort", "aborted", "segfault", "sigsegv" };
        if (crashWords.Any(t.Contains))
            hits.Add(Signal("runtime failure", 0.4, PatternFamilies.RuntimeCrash));

        return Dedupe(hits);
    }

    private static IEnumerable<SignalHit> DetectNetworkSignals(HashSet<string> t, string text)
    {
        var hits = new List<SignalHit>();

        var connWords = new[] { "connection", "socket", "tcp" };
        var connState = new[] { "refused", "reset", "closed", "aborted", "broken", "pipe" };
        if (connWords.Any(t.Contains) && connState.Any(t.Contains))
            hits.Add(Signal("connection refused/reset", 0.6, PatternFamilies.NetworkFailure));

        var dnsWords = new[] { "dns", "hostname", "getaddrinfo", "nxdomain", "eai_again", "resolve", "resolution" };
        if (dnsWords.Any(t.Contains))
            hits.Add(Signal("dns failure", 0.5, PatternFamilies.NetworkFailure));

        var errnos = new[] { "econnreset", "econnrefused", "epipe", "enetunreach", "ehostunreach" };
        if (errnos.Any(t.Contains))
            hits.Add(Signal("socket failure", 0.5, PatternFamilies.NetworkFailure));

        if (hits.Count > 0)
        {
            var ctx = new[] { "host", "port", "tcp", "http", "ip", "address" };
            if (ctx.Any(t.Contains))
                hits.Add(Signal("network context", 0.2, PatternFamilies.NetworkFailure));
        }

        return Dedupe(hits);
    }

    private static IEnumerable<SignalHit> DetectDependencySignals(HashSet<string> t, string text)
    {
        var hits = new List<SignalHit>();

        var fiveXx = new[] { "500", "502", "503", "504" };
        if (fiveXx.Any(t.Contains))
            hits.Add(Signal("upstream 5xx", 0.5, PatternFamilies.DependencyUnavailable));

        if (text.Contains("bad gateway") || text.Contains("service unavailable") ||
            text.Contains("gateway timeout"))
            hits.Add(Signal("upstream 5xx", 0.5, PatternFamilies.DependencyUnavailable));

        var depWords = new[] { "upstream", "downstream", "dependency", "service" };
        var depState = new[] { "down", "unreachable", "unavailable", "failing", "unhealthy" };
        if (depWords.Any(t.Contains) && depState.Any(t.Contains))
            hits.Add(Signal("service down", 0.4, PatternFamilies.DependencyUnavailable));

        if (hits.Count > 0)
        {
            var ctx = new[] { "api", "endpoint", "service", "client", "call", "request", "response" };
            if (ctx.Any(t.Contains))
                hits.Add(Signal("dependency context", 0.2, PatternFamilies.DependencyUnavailable));
        }

        return Dedupe(hits);
    }

    private static IEnumerable<SignalHit> DetectIOSignals(HashSet<string> t, string text)
    {
        var hits = new List<SignalHit>();

        if (t.Contains("enospc") ||
            (t.Contains("disk") && (t.Contains("full") || t.Contains("space"))) ||
            text.Contains("no space left"))
            hits.Add(Signal("disk full", 0.6, PatternFamilies.IOFailure));

        if (t.Contains("eacces") ||
            (t.Contains("permission") && t.Contains("denied")))
            hits.Add(Signal("file permission", 0.5, PatternFamilies.IOFailure));

        var fileWords = new[] { "file", "path", "directory" };
        if (fileWords.Any(t.Contains) &&
            (t.Contains("locked") || (t.Contains("lock") && t.Contains("held")) || t.Contains("etxtbsy")))
            hits.Add(Signal("file lock", 0.4, PatternFamilies.IOFailure));

        if (t.Contains("enoent") ||
            (fileWords.Any(t.Contains) && t.Contains("not") && (t.Contains("found") || t.Contains("exist"))))
            hits.Add(Signal("path missing", 0.4, PatternFamilies.IOFailure));

        if (hits.Count > 0)
        {
            var ctx = new[] { "device", "writing", "reading", "stream", "fs", "io" };
            if (ctx.Any(t.Contains))
                hits.Add(Signal("io context", 0.2, PatternFamilies.IOFailure));
        }

        return Dedupe(hits);
    }

    private static IEnumerable<SignalHit> DetectConcurrencySignals(HashSet<string> t, string text)
    {
        var hits = new List<SignalHit>();

        if (AnyTokenContainsAny(t, new[] { "deadlock" }))
            hits.Add(Signal("deadlock", 0.6, PatternFamilies.ConcurrencyIssue));

        if (t.Contains("lock") && (t.Contains("wait") || t.Contains("acquire") || t.Contains("timeout")))
            hits.Add(Signal("lock timeout", 0.4, PatternFamilies.ConcurrencyIssue));

        if (AnyTokenContainsAny(t, new[] { "concurrentmodification" }) ||
            (t.Contains("race") && t.Contains("condition")))
            hits.Add(Signal("race condition", 0.5, PatternFamilies.ConcurrencyIssue));

        if (hits.Count > 0)
        {
            var ctx = new[] { "thread", "transaction", "concurrent", "mutex", "semaphore", "lock" };
            if (ctx.Any(t.Contains))
                hits.Add(Signal("concurrency context", 0.2, PatternFamilies.ConcurrencyIssue));
        }

        return Dedupe(hits);
    }

    private static IEnumerable<SignalHit> DetectSerializationSignals(HashSet<string> t, string text)
    {
        var hits = new List<SignalHit>();

        var formatFragments = new[] { "json", "xml", "yaml", "protobuf", "avro" };
        var parseFragments = new[] { "parse", "deserial", "malformed", "syntax", "unexpected" };
        var hasFormat = AnyTokenContainsAny(t, formatFragments);
        var hasParse = AnyTokenContainsAny(t, parseFragments);
        if (hasFormat && hasParse)
            hits.Add(Signal("parse error", 0.6, PatternFamilies.SerializationFailure));
        else if (hasParse && (t.Contains("token") || t.Contains("character") || t.Contains("position")))
            hits.Add(Signal("parse error", 0.4, PatternFamilies.SerializationFailure));

        var encWords = new[] { "encoding", "charset", "decode", "decoder", "encoder", "utf" };
        var encFail = new[] { "mismatch", "invalid", "unsupported", "error", "failed" };
        if (encWords.Any(t.Contains) && encFail.Any(t.Contains))
            hits.Add(Signal("encoding mismatch", 0.4, PatternFamilies.SerializationFailure));

        if (hits.Count > 0)
        {
            var ctx = new[] { "payload", "body", "message", "stream", "field" };
            if (ctx.Any(t.Contains) || hasFormat)
                hits.Add(Signal("serialization context", 0.2, PatternFamilies.SerializationFailure));
        }

        return Dedupe(hits);
    }

    private static IEnumerable<SignalHit> DetectConfigurationSignals(HashSet<string> t, string text)
    {
        var hits = new List<SignalHit>();

        var envWords = new[] { "env", "environment", "variable", "configuration", "config", "settings" };
        var missingWords = new[] { "missing", "undefined", "required", "empty" };
        if (envWords.Any(t.Contains) && missingWords.Any(t.Contains))
            hits.Add(Signal("missing config", 0.5, PatternFamilies.ConfigurationError));

        var invalidWords = new[] { "invalid", "malformed", "unrecognized" };
        if (envWords.Any(t.Contains) && invalidWords.Any(t.Contains))
            hits.Add(Signal("invalid config", 0.4, PatternFamilies.ConfigurationError));

        var bootWords = new[] { "startup", "boot", "init", "initialize", "initialization", "bootstrap" };
        if (bootWords.Any(t.Contains) &&
            (t.Contains("missing") || t.Contains("required") || t.Contains("not")))
            hits.Add(Signal("missing file at boot", 0.4, PatternFamilies.ConfigurationError));

        if (hits.Count > 0)
        {
            var ctx = new[] { "property", "value", "key", "yaml", "json", "ini", "toml", "secret" };
            if (ctx.Any(t.Contains) || envWords.Any(t.Contains))
                hits.Add(Signal("configuration context", 0.2, PatternFamilies.ConfigurationError));
        }

        return Dedupe(hits);
    }

    private static IEnumerable<SignalHit> DetectCircuitBreakerSignals(HashSet<string> t, string text)
    {
        var hits = new List<SignalHit>();

        var breakerWords = new[] { "circuit", "breaker", "hystrix", "resilience4j", "polly" };
        var breakerStates = new[] { "open", "tripped", "opened" };
        if (breakerWords.Any(t.Contains) && (breakerStates.Any(t.Contains) || t.Contains("breaker")))
            hits.Add(Signal("breaker tripped", 0.6, PatternFamilies.CircuitBreakerOpen));

        if (t.Contains("fallback") && (t.Contains("triggered") || t.Contains("executed") || t.Contains("invoked")))
            hits.Add(Signal("fallback triggered", 0.4, PatternFamilies.CircuitBreakerOpen));

        if (hits.Count > 0)
        {
            var ctx = new[] { "transitioned", "failures", "threshold", "half_open", "halfopen", "state" };
            if (ctx.Any(t.Contains))
                hits.Add(Signal("breaker context", 0.2, PatternFamilies.CircuitBreakerOpen));
        }

        return Dedupe(hits);
    }

    private static IEnumerable<SignalHit> DetectDeploymentSignals(HashSet<string> t, string text)
    {
        var hits = new List<SignalHit>();

        if ((t.Contains("version") || t.Contains("schema")) &&
            (t.Contains("mismatch") || t.Contains("incompatible")))
            hits.Add(Signal("version mismatch", 0.5, PatternFamilies.DeploymentIssue));

        if (t.Contains("migration") &&
            (t.Contains("pending") || t.Contains("missing") || t.Contains("failed") || t.Contains("not")))
            hits.Add(Signal("missing migration", 0.5, PatternFamilies.DeploymentIssue));

        var bootWords = new[] { "startup", "boot", "bootstrap" };
        if (bootWords.Any(t.Contains) &&
            (t.Contains("failed") || t.Contains("error") || t.Contains("aborted")))
            hits.Add(Signal("startup failure", 0.4, PatternFamilies.DeploymentIssue));

        if (text.Contains("application failed to start") ||
            text.Contains("application context failed"))
            hits.Add(Signal("startup failure", 0.5, PatternFamilies.DeploymentIssue));

        return Dedupe(hits);
    }

    private static bool HasStackTraceShape(LogEntry entry, string text)
    {
        if (!string.IsNullOrWhiteSpace(entry.StackTrace)) return true;
        if (Regex.IsMatch(text, @"\bat\s+[\w\.\$<>]+", RegexOptions.IgnoreCase)) return true;
        if (text.Contains("traceback")) return true;
        if (Regex.IsMatch(text, @"file\s+""[^""]+"",\s*line\s+\d+")) return true;
        if (Regex.IsMatch(text, @"\([^()]+:\d+:\d+\)")) return true;
        return false;
    }

    private static Dictionary<string, bool> BuildFeatures(List<SignalHit> signals)
    {
        var families = signals.Select(s => s.Family).ToHashSet();
        return new Dictionary<string, bool>
        {
            ["isRuntimeError"] = families.Contains(PatternFamilies.RuntimeCrash),
            ["hasObjectAccessFailure"] = families.Contains(PatternFamilies.ObjectAccessFailure),
            ["hasTimeoutBehavior"] = families.Contains(PatternFamilies.TimeoutIssue),
            ["hasMemoryIssue"] = families.Contains(PatternFamilies.MemoryExhaustion),
            ["hasNetworkFailure"] = families.Contains(PatternFamilies.NetworkFailure),
            ["hasDependencyUnavailable"] = families.Contains(PatternFamilies.DependencyUnavailable),
            ["hasIOFailure"] = families.Contains(PatternFamilies.IOFailure),
            ["hasConcurrencyIssue"] = families.Contains(PatternFamilies.ConcurrencyIssue),
            ["hasSerializationFailure"] = families.Contains(PatternFamilies.SerializationFailure),
            ["hasConfigurationError"] = families.Contains(PatternFamilies.ConfigurationError),
            ["hasCircuitBreakerOpen"] = families.Contains(PatternFamilies.CircuitBreakerOpen),
            ["hasDeploymentIssue"] = families.Contains(PatternFamilies.DeploymentIssue)
        };
    }

    private static (string pattern, double score) Classify(List<SignalHit> signals)
    {
        if (signals.Count == 0) return (PatternFamilies.Unknown, 0.0);

        var perFamily = signals
            .GroupBy(s => s.Family)
            .Select(g => new { Family = g.Key, Total = Math.Min(1.0, g.Sum(x => x.Weight)) })
            .OrderByDescending(x => x.Total)
            .ToList();

        var top = perFamily[0];
        if (top.Total < ClassificationThreshold) return (PatternFamilies.Unknown, top.Total);
        return (top.Family, top.Total);
    }

    private static SignalHit Signal(string name, double weight, string family) =>
        new() { Name = name, Weight = weight, Family = family };

    private static bool AnyTokenContainsAny(HashSet<string> tokens, IEnumerable<string> fragments)
    {
        foreach (var token in tokens)
            foreach (var frag in fragments)
                if (token.IndexOf(frag, StringComparison.OrdinalIgnoreCase) >= 0)
                    return true;
        return false;
    }

    private static IEnumerable<SignalHit> Dedupe(IEnumerable<SignalHit> hits) =>
        hits.GroupBy(h => h.Name)
            .Select(g => g.OrderByDescending(x => x.Weight).First());
}
