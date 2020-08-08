using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Amazon.XRay.Recorder.Core;
using Amazon.XRay.Recorder.Core.Internal.Context;
using Amazon.XRay.Recorder.Core.Internal.Emitters;
using Amazon.XRay.Recorder.Core.Internal.Entities;
using Amazon.XRay.Recorder.Core.Sampling;
using Amazon.XRay.Recorder.Core.Strategies;

namespace Irradiate.Tests
{
    public class BenchmarkContext : ITraceContext
    {
        bool _isEntityPresent;

        public BenchmarkContext(bool isEntityPresent)
        {
            _isEntityPresent = isEntityPresent;
        }

        public bool IsEntityPresent() => _isEntityPresent;

        public void ClearEntity() => throw new NotImplementedException();
        public Entity GetEntity() => throw new NotImplementedException();
        public void HandleEntityMissing(IAWSXRayRecorder recorder, Exception e, string message) => throw new NotImplementedException();
        public void SetEntity(Entity entity) => throw new NotImplementedException();

    }

    public class BenchmarkRecorder : IAWSXRayRecorder
    {
        public ITraceContext TraceContext { get; set; }

        public BenchmarkRecorder(bool isEntityPresent)
        {
            TraceContext = new BenchmarkContext(isEntityPresent);
        }

        public void AddAnnotation(string key, object value) { }
        public void AddException(Exception ex) { }
        public void AddMetadata(string key, object value) { }
        public void AddMetadata(string nameSpace, string key, object value) { }
        public void BeginSubsegment(string name, DateTime? timestamp = null) { }
        public void EndSubsegment(DateTime? timestamp = null) { }

        #region Unused functionality

        public string Origin { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public ISamplingStrategy SamplingStrategy { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public IStreamingStrategy StreamingStrategy { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public ContextMissingStrategy ContextMissingStrategy { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public IDictionary<string, object> RuntimeContext => throw new NotImplementedException();
        public ExceptionSerializationStrategy ExceptionSerializationStrategy { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public ISegmentEmitter Emitter { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public void AddHttpInformation(string key, object value) => throw new NotImplementedException();
        public void AddPrecursorId(string precursorId) => throw new NotImplementedException();
        public void AddSqlInformation(string key, string value) => throw new NotImplementedException();
        public void BeginSegment(string name, string traceId = null, string parentId = null, SamplingResponse samplingResponse = null, DateTime? timestamp = null) => throw new NotImplementedException();
        public void Dispose() { }
        public void EndSegment(DateTime? timestamp = null) => throw new NotImplementedException();
        public void MarkError() => throw new NotImplementedException();
        public void MarkFault() => throw new NotImplementedException();
        public void MarkThrottle() => throw new NotImplementedException();
        public void SetDaemonAddress(string daemonAddress) => throw new NotImplementedException();
        public void SetNamespace(string value) => throw new NotImplementedException();
        public TResult TraceMethod<TResult>(string name, Func<TResult> method) => throw new NotImplementedException();
        public void TraceMethod(string name, Action method) => throw new NotImplementedException();
        public Task<TResult> TraceMethodAsync<TResult>(string name, Func<Task<TResult>> method) => throw new NotImplementedException();
        public Task TraceMethodAsync(string name, Func<Task> method) => throw new NotImplementedException();

        #endregion
    }
}
