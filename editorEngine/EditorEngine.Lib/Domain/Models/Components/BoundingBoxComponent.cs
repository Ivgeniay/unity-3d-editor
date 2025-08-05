using System.Reactive.Subjects;
using System.Reactive.Linq;
using System.Numerics;
using System;

namespace EditorEngine.Lib.Domain
{
    public class BoundingBoxComponent : Component
    {
        private readonly BehaviorSubject<Vector3> _min;
        private readonly BehaviorSubject<Vector3> _max;

        public IObservable<Vector3> MinChanged => _min;
        public IObservable<Vector3> MaxChanged => _max;
        public IObservable<Vector3> CenterChanged { get; }
        public IObservable<Vector3> SizeChanged { get; }

        public Vector3 Min
        {
            get => _min.Value;
            set => _min.OnNext(value);
        }

        public Vector3 Max
        {
            get => _max.Value;
            set => _max.OnNext(value);
        }

        public Vector3 Center => (Min + Max) * 0.5f;
        public Vector3 Size => Max - Min;
        public Vector3 Extents => Size * 0.5f;

        public BoundingBoxComponent() : base(nameof(BoundingBoxComponent))
        {
            _min = new BehaviorSubject<Vector3>(Vector3.Zero);
            _max = new BehaviorSubject<Vector3>(Vector3.Zero);

            CenterChanged = Observable.CombineLatest(_min, _max, (min, max) => (min + max) * 0.5f);
            SizeChanged = Observable.CombineLatest(_min, _max, (min, max) => max - min);
        }

        public static BoundingBoxComponent FromMinMax(Vector3 min, Vector3 max)
        {
            var box = new BoundingBoxComponent();
            box._min.OnNext(min);
            box._max.OnNext(max);
            return box;
        }

        public static BoundingBoxComponent FromCenterAndSize(Vector3 center, Vector3 size)
        {
            var box = new BoundingBoxComponent();
            var extents = size * 0.5f;
            box._min.OnNext(center - extents);
            box._max.OnNext(center + extents);
            return box;
        }

        public void SetMinMax(Vector3 min, Vector3 max)
        {
            _min.OnNext(min);
            _max.OnNext(max);
        }

        public void SetCenterAndSize(Vector3 center, Vector3 size)
        {
            var extents = size * 0.5f;
            _min.OnNext(center - extents);
            _max.OnNext(center + extents);
        }

        public void Encapsulate(Vector3 point)
        {
            _min.OnNext(Vector3.Min(Min, point));
            _max.OnNext(Vector3.Max(Max, point));
        }

        public void Encapsulate(BoundingBoxComponent other)
        {
            _min.OnNext(Vector3.Min(Min, other.Min));
            _max.OnNext(Vector3.Max(Max, other.Max));
        }

        public void Expand(float amount)
        {
            var expansion = Vector3.One * amount;
            _min.OnNext(Min - expansion);
            _max.OnNext(Max + expansion);
        }

        public void Expand(Vector3 amount)
        {
            _min.OnNext(Min - amount);
            _max.OnNext(Max + amount);
        }

        public bool Contains(Vector3 point)
        {
            return point.X >= Min.X && point.X <= Max.X &&
                   point.Y >= Min.Y && point.Y <= Max.Y &&
                   point.Z >= Min.Z && point.Z <= Max.Z;
        }

        public bool Contains(BoundingBoxComponent other)
        {
            return Contains(other.Min) && Contains(other.Max);
        }

        public bool Intersects(BoundingBoxComponent other)
        {
            return Min.X <= other.Max.X && Max.X >= other.Min.X &&
                   Min.Y <= other.Max.Y && Max.Y >= other.Min.Y &&
                   Min.Z <= other.Max.Z && Max.Z >= other.Min.Z;
        }

        public float DistanceToPoint(Vector3 point)
        {
            var closestPoint = Vector3.Clamp(point, Min, Max);
            return Vector3.Distance(point, closestPoint);
        }

        public Vector3 ClosestPoint(Vector3 point)
        {
            return Vector3.Clamp(point, Min, Max);
        }

        public BoundingBoxComponent Transform(Matrix4x4 matrix)
        {
            var corners = new Vector3[]
            {
                new Vector3(Min.X, Min.Y, Min.Z),
                new Vector3(Min.X, Min.Y, Max.Z),
                new Vector3(Min.X, Max.Y, Min.Z),
                new Vector3(Min.X, Max.Y, Max.Z),
                new Vector3(Max.X, Min.Y, Min.Z),
                new Vector3(Max.X, Min.Y, Max.Z),
                new Vector3(Max.X, Max.Y, Min.Z),
                new Vector3(Max.X, Max.Y, Max.Z)
            };

            var transformedMin = Vector3.Transform(corners[0], matrix);
            var transformedMax = transformedMin;

            for (int i = 1; i < corners.Length; i++)
            {
                var transformed = Vector3.Transform(corners[i], matrix);
                transformedMin = Vector3.Min(transformedMin, transformed);
                transformedMax = Vector3.Max(transformedMax, transformed);
            }

            return BoundingBoxComponent.FromMinMax(transformedMin, transformedMax);
        }

        public bool Equals(BoundingBoxComponent other)
        {
            return other != null && Min.Equals(other.Min) && Max.Equals(other.Max);
        }

        public override bool Equals(object obj)
        {
            return obj is BoundingBoxComponent boundingBox && Equals(boundingBox);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Min, Max);
        }

        public static bool operator ==(BoundingBoxComponent left, BoundingBoxComponent right)
        {
            return left?.Equals(right) ?? right is null;
        }

        public static bool operator !=(BoundingBoxComponent left, BoundingBoxComponent right)
        {
            return !(left == right);
        }

        public override string ToString()
        {
            return $"BoundingBox[Min: {Min}, Max: {Max}, Size: {Size}]";
        }

        public override void Dispose()
        {
            base.Dispose();
            _min?.Dispose();
            _max?.Dispose();
        }
    }
}