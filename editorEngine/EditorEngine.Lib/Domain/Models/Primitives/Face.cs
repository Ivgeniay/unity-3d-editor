using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Reactive.Subjects;
using System.Reactive.Linq;
using System.Numerics;
using System.Linq;
using System;

namespace EditorEngine.Lib.Domain
{
    public class Face : EditorObject, IEquatable<Face>, IDisposable
    {
        private readonly BehaviorSubject<ObservableCollection<Vertex>> _vertices;
        private readonly List<IDisposable> _subscriptions;

        public IObservable<ObservableCollection<Vertex>> VerticesChanged => _vertices;
        public IObservable<Vector3> NormalChanged { get; }
        public IObservable<float> AreaChanged { get; }

        public ObservableCollection<Vertex> Vertices => _vertices.Value;
        public Vector3 Normal => CalculateNormal();
        public float Area => CalculateArea();

        public Face(params Vertex[] vertices) : base(nameof(Face))
        {
            if (vertices.Length < 3)
                throw new ArgumentException("Face must have at least 3 vertices");

            _vertices = new BehaviorSubject<ObservableCollection<Vertex>>(new ObservableCollection<Vertex>(vertices));
            _subscriptions = new List<IDisposable>();

            NormalChanged = _vertices
                .SelectMany(verts => Observable.Merge(verts.Select(v => v.PositionChanged)))
                .Select(_ => CalculateNormal());

            AreaChanged = _vertices
                .SelectMany(verts => Observable.Merge(verts.Select(v => v.PositionChanged)))
                .Select(_ => CalculateArea());

            _subscriptions.Add(NormalChanged.Subscribe());
            _subscriptions.Add(AreaChanged.Subscribe());
        }

        private Vector3 CalculateNormal()
        {
            if (Vertices.Count < 3) return Vector3.Zero;

            var v1 = Vertices[1].Position - Vertices[0].Position;
            var v2 = Vertices[2].Position - Vertices[0].Position;
            return Vector3.Normalize(Vector3.Cross(v1, v2));
        }

        private float CalculateArea()
        {
            if (Vertices.Count < 3) return 0f;

            if (Vertices.Count == 3)
            {
                var v1 = Vertices[1].Position - Vertices[0].Position;
                var v2 = Vertices[2].Position - Vertices[0].Position;
                return Vector3.Cross(v1, v2).Length() * 0.5f;
            }

            float area = 0f;
            var center = Vertices.Aggregate(Vector3.Zero, (sum, v) => sum + v.Position) / Vertices.Count;

            for (int i = 0; i < Vertices.Count; i++)
            {
                var current = Vertices[i].Position - center;
                var next = Vertices[(i + 1) % Vertices.Count].Position - center;
                area += Vector3.Cross(current, next).Length() * 0.5f;
            }

            return area;
        }

        public void AddVertex(Vertex vertex)
        {
            Vertices.Add(vertex);
        }

        public void RemoveVertex(Vertex vertex)
        {
            Vertices.Remove(vertex);
        }

        public bool Equals(Face other)
        {
            if (other == null || Vertices.Count != other.Vertices.Count) return false;

            var thisVertices = Vertices.ToHashSet();
            var otherVertices = other.Vertices.ToHashSet();
            return thisVertices.SetEquals(otherVertices);
        }

        public override bool Equals(object obj)
        {
            return obj is Face face && Equals(face);
        }

        public override int GetHashCode()
        {
            return Vertices.Aggregate(0, (hash, vertex) => hash ^ vertex.GetHashCode());
        }

        public static bool operator ==(Face left, Face right)
        {
            return left?.Equals(right) ?? right is null;
        }

        public static bool operator !=(Face left, Face right)
        {
            return !(left == right);
        }

        public override string ToString()
        {
            return $"Face[{Vertices.Count} vertices, Area: {Area:F3}]";
        }

        public override void Dispose()
        {
            base.Dispose();
            foreach (var subscription in _subscriptions)
            {
                subscription?.Dispose();
            }
            _subscriptions.Clear();
            _vertices?.Dispose();
        }
    }
}