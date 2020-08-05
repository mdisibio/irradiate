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
    public class TestContext : ITraceContext
    {
        public bool IsEntityPresent() => true;

        public void ClearEntity() => throw new NotImplementedException();
        public Entity GetEntity() => throw new NotImplementedException();
        public void HandleEntityMissing(IAWSXRayRecorder recorder, Exception e, string message) => throw new NotImplementedException();
        public void SetEntity(Entity entity) => throw new NotImplementedException();
    }

    public class TestSubsegment
    {
        public string Name;
        public List<Exception> Exceptions = new List<Exception>();
        public Dictionary<string, object> Annotations = new Dictionary<string, object>();
        public Dictionary<string, object> MetaData = new Dictionary<string, object>();
    }

    /// <summary>
    /// Stub IAWSXrayRecorder that records the information to local properties.
    /// </summary>
    public class TestRecorder : IAWSXRayRecorder
    {
        public List<TestSubsegment> Subsegments = new List<TestSubsegment>();
        public TestSubsegment CurrentSubsegment = null;

        public ITraceContext TraceContext { get; set; } = new TestContext();

        public void AddAnnotation(string key, object value)
        {
            CurrentSubsegment.Annotations[key] = value;
        }

        public void AddException(Exception ex)
        {
            CurrentSubsegment.Exceptions.Add(ex);
        }

        public void AddMetadata(string key, object value)
        {
            CurrentSubsegment.MetaData[key] = value;
        }

        public void AddMetadata(string nameSpace, string key, object value)
        {
            CurrentSubsegment.MetaData[nameSpace + "." + key] = value;
        }

        public void BeginSubsegment(string name, DateTime? timestamp = null)
        {
            if (CurrentSubsegment == null)
            {
                CurrentSubsegment = new TestSubsegment
                {
                    Name = name
                };
                Subsegments.Add(CurrentSubsegment);
            }
            else
            {
                throw new InvalidOperationException("New subsegment was began without ending previous one");
            }

        }

        public void EndSubsegment(DateTime? timestamp = null)
        {
            CurrentSubsegment = null;
        }

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
