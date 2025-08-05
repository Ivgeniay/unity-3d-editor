using System.Reactive.Subjects;
using System.Reactive.Linq;
using System.Numerics;
using System;

namespace EditorEngine.Lib.Domain
{
    public class TransformComponent : Component
    {
        private readonly BehaviorSubject<Vector3> _position;
        private readonly BehaviorSubject<Quaternion> _rotation;
        private readonly BehaviorSubject<Vector3> _scale;

        public IObservable<Vector3> PositionChanged => _position;
        public IObservable<Quaternion> RotationChanged => _rotation;
        public IObservable<Vector3> ScaleChanged => _scale;
        public IObservable<Matrix4x4> MatrixChanged { get; }

        public Vector3 Position
        {
            get => _position.Value;
            set => _position.OnNext(value);
        }

        public Quaternion Rotation
        {
            get => _rotation.Value;
            set => _rotation.OnNext(value);
        }

        public Vector3 Scale
        {
            get => _scale.Value;
            set => _scale.OnNext(value);
        }

        public Matrix4x4 Matrix => CalculateMatrix();

        public Vector3 Forward => Vector3.Transform(Vector3.UnitZ, Rotation);
        public Vector3 Right => Vector3.Transform(Vector3.UnitX, Rotation);
        public Vector3 Up => Vector3.Transform(Vector3.UnitY, Rotation);

        public TransformComponent() : base(nameof(TransformComponent))
        {
            _position = new BehaviorSubject<Vector3>(Vector3.Zero);
            _rotation = new BehaviorSubject<Quaternion>(Quaternion.Identity);
            _scale = new BehaviorSubject<Vector3>(Vector3.One);

            MatrixChanged = Observable.CombineLatest(
                _position,
                _rotation,
                _scale,
                (pos, rot, scale) => CalculateMatrix(pos, rot, scale));
        }

        public TransformComponent(Vector3 position, Quaternion rotation, Vector3 scale) : base(nameof(TransformComponent))
        {
            _position = new BehaviorSubject<Vector3>(position);
            _rotation = new BehaviorSubject<Quaternion>(rotation);
            _scale = new BehaviorSubject<Vector3>(scale);

            MatrixChanged = Observable.CombineLatest(
                _position,
                _rotation,
                _scale,
                (pos, rot, scale) => CalculateMatrix(pos, rot, scale));
        }

        private Matrix4x4 CalculateMatrix()
        {
            return CalculateMatrix(Position, Rotation, Scale);
        }

        private static Matrix4x4 CalculateMatrix(Vector3 position, Quaternion rotation, Vector3 scale)
        {
            return Matrix4x4.CreateScale(scale) *
                   Matrix4x4.CreateFromQuaternion(rotation) *
                   Matrix4x4.CreateTranslation(position);
        }

        public Vector3 TransformPoint(Vector3 point)
        {
            return Vector3.Transform(point, Matrix);
        }

        public Vector3 TransformDirection(Vector3 direction)
        {
            return Vector3.Transform(direction, Rotation);
        }

        public Vector3 InverseTransformPoint(Vector3 point)
        {
            Matrix4x4.Invert(Matrix, out var inverted);
            return Vector3.Transform(point, inverted);
        }

        public Vector3 InverseTransformDirection(Vector3 direction)
        {
            return Vector3.Transform(direction, Quaternion.Inverse(Rotation));
        }

        public void LookAt(Vector3 target, Vector3 up)
        {
            var forward = Vector3.Normalize(target - Position);
            var right = Vector3.Normalize(Vector3.Cross(up, forward));
            var actualUp = Vector3.Cross(forward, right);
            
            var matrix = new Matrix4x4(
                right.X, right.Y, right.Z, 0,
                actualUp.X, actualUp.Y, actualUp.Z, 0,
                forward.X, forward.Y, forward.Z, 0,
                0, 0, 0, 1);
            
            Rotation = Quaternion.CreateFromRotationMatrix(matrix);
        }

        public void Translate(Vector3 translation)
        {
            Position += translation;
        }

        public void Rotate(Quaternion rotation)
        {
            Rotation = Quaternion.Normalize(rotation * Rotation);
        }

        public void RotateAround(Vector3 point, Vector3 axis, float angle)
        {
            var rotation = Quaternion.CreateFromAxisAngle(axis, angle);
            var direction = Position - point;
            direction = Vector3.Transform(direction, rotation);
            Position = point + direction;
            Rotate(rotation);
        }

        public bool Equals(TransformComponent other)
        {
            if (other == null) return false;

            return Position.Equals(other.Position) &&
                   Rotation.Equals(other.Rotation) &&
                   Scale.Equals(other.Scale);
        }

        public override bool Equals(object obj)
        {
            return obj is TransformComponent transform && Equals(transform);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Position, Rotation, Scale);
        }

        public static bool operator ==(TransformComponent left, TransformComponent right)
        {
            return left?.Equals(right) ?? right is null;
        }

        public static bool operator !=(TransformComponent left, TransformComponent right)
        {
            return !(left == right);
        }

        public override string ToString()
        {
            return $"Transform[Pos: {Position}, Rot: {Rotation}, Scale: {Scale}]";
        }

        public override void Dispose()
        {
            base.Dispose();
            _position?.Dispose();
            _rotation?.Dispose();
            _scale?.Dispose();
        }
    }

}