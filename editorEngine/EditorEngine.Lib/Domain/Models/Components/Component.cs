using System.Reactive.Subjects;
using System;

namespace EditorEngine.Lib.Domain
{
    public abstract class Component : EditorObject
    {
        private readonly BehaviorSubject<SceneObject> _owner;

        public IObservable<SceneObject> OwnerChanged => _owner;

        public SceneObject Owner
        {
            get => _owner.Value;
            internal set => _owner.OnNext(value);
        }

        protected Component() : base(nameof(Component))
        {
            _owner = new BehaviorSubject<SceneObject>(null);
        }

        protected Component(string name) : base(name)
        {
            _owner = new BehaviorSubject<SceneObject>(null);
        }

        public override void Dispose()
        {
            base.Dispose();
            _owner?.Dispose();
        }
    }
}