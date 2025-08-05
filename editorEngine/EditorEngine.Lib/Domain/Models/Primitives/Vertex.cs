using System.Reactive.Subjects;
using System.Numerics;
using System;

namespace EditorEngine.Lib.Domain
{
    public class Vertex : EditorObject, IEquatable<Vertex>, IDisposable
    {
        private readonly BehaviorSubject<Vector3> _position;

        public IObservable<Vector3> PositionChanged => _position;

        public Vector3 Position
        {
            get => _position.Value;
            set => _position.OnNext(value);
        }

        public float X => Position.X;
        public float Y => Position.Y;
        public float Z => Position.Z;

        public Vertex(Vector3 position) : base(nameof(Vertex))
        {
            _position = new BehaviorSubject<Vector3>(position);
        }

        public Vertex(float x, float y, float z) : this(new Vector3(x, y, z))
        {
        }

        public void WithX(float x) => Position = new Vector3(x, Position.Y, Position.Z);
        public void WithY(float y) => Position = new Vector3(Position.X, y, Position.Z);
        public void WithZ(float z) => Position = new Vector3(Position.X, Position.Y, z);
        

        public bool Equals(Vertex other)
        {
            return other != null && Vector3.Distance(Position, other.Position) < float.Epsilon;
        }

        public override bool Equals(object obj)
        {
            return obj is Vertex vertex && Equals(vertex);
        }

        public override int GetHashCode()
        {
            return Position.GetHashCode();
        }

        public static bool operator ==(Vertex left, Vertex right)
        {
            return left?.Equals(right) ?? right is null;
        }

        public static bool operator !=(Vertex left, Vertex right)
        {
            return !(left == right);
        }

        public override string ToString()
        {
            return $"({Position.X:F3}, {Position.Y:F3}, {Position.Z:F3})";
        }

        public override void Dispose()
        {
            base.Dispose();
            _position?.Dispose();
        }
    }

}