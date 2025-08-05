using System.Reactive.Subjects;
using System.Reactive.Linq;
using System.Numerics;
using System.Linq;
using System;

namespace EditorEngine.Lib.Domain
{
    public class Edge : EditorObject, IEquatable<Edge>, IDisposable
    {
        private readonly BehaviorSubject<Vertex> _startVertex;
        private readonly BehaviorSubject<Vertex> _endVertex;
        private readonly IDisposable _lengthSubscription;

        public IObservable<Vertex> StartVertexChanged => _startVertex;
        public IObservable<Vertex> EndVertexChanged => _endVertex;
        public IObservable<float> LengthChanged { get; }

        public Vertex StartVertex
        {
            get => _startVertex.Value;
            set => _startVertex.OnNext(value);
        }

        public Vertex EndVertex
        {
            get => _endVertex.Value;
            set => _endVertex.OnNext(value);
        }

        public float Length => Vector3.Distance(StartVertex.Position, EndVertex.Position);

        public Edge(Vertex startVertex, Vertex endVertex) : base(nameof(Edge))
        {
            _startVertex = new BehaviorSubject<Vertex>(startVertex);
            _endVertex = new BehaviorSubject<Vertex>(endVertex);

            LengthChanged = Observable.CombineLatest(
                _startVertex.SelectMany(v => v.PositionChanged),
                _endVertex.SelectMany(v => v.PositionChanged),
                (start, end) => Vector3.Distance(start, end));

            _lengthSubscription = LengthChanged.Subscribe();
        }

        public bool Equals(Edge other)
        {
            if (other == null) return false;
            
            return (StartVertex.Equals(other.StartVertex) && EndVertex.Equals(other.EndVertex)) ||
                   (StartVertex.Equals(other.EndVertex) && EndVertex.Equals(other.StartVertex));
        }

        public override bool Equals(object obj)
        {
            return obj is Edge edge && Equals(edge);
        }

        public override int GetHashCode()
        {
            var hash1 = StartVertex.GetHashCode();
            var hash2 = EndVertex.GetHashCode();
            return hash1 < hash2 ? HashCode.Combine(hash1, hash2) : HashCode.Combine(hash2, hash1);
        }

        public static bool operator ==(Edge left, Edge right)
        {
            return left?.Equals(right) ?? right is null;
        }

        public static bool operator !=(Edge left, Edge right)
        {
            return !(left == right);
        }

        public override string ToString()
        {
            return $"Edge[{StartVertex} -> {EndVertex}, Length: {Length:F3}]";
        }

        public override void Dispose()
        {
            base.Dispose();
            _lengthSubscription?.Dispose();
            _startVertex?.Dispose();
            _endVertex?.Dispose();
        }
    }
}