using System.Collections.Generic;
using System.Reactive.Subjects;
using System;

namespace EditorEngine.Lib.Domain
{
    public abstract class EditorObject : IEquatable<EditorObject>, IDisposable
    {
        private readonly BehaviorSubject<Guid> _id;
        private readonly BehaviorSubject<string> _name;
        private readonly BehaviorSubject<bool> _isEnabled;
        private readonly BehaviorSubject<Dictionary<string, object>> _userData;
        private readonly BehaviorSubject<HashSet<string>> _tags;

        public IObservable<Guid> IdChanged => _id;
        public IObservable<string> NameChanged => _name;
        public IObservable<bool> IsEnabledChanged => _isEnabled;
        public IObservable<Dictionary<string, object>> UserDataChanged => _userData;
        public IObservable<HashSet<string>> TagsChanged => _tags;

        public Guid Id => _id.Value;

        public string Name
        {
            get => _name.Value;
            set => _name.OnNext(value);
        }

        public bool IsEnabled
        {
            get => _isEnabled.Value;
            set => _isEnabled.OnNext(value);
        }

        public Dictionary<string, object> UserData => _userData.Value;
        public HashSet<string> Tags => _tags.Value;

        protected EditorObject(string name = null)
        {
            _id = new BehaviorSubject<Guid>(Guid.NewGuid());
            _name = new BehaviorSubject<string>(name ?? GetType().Name);
            _isEnabled = new BehaviorSubject<bool>(true);
            _userData = new BehaviorSubject<Dictionary<string, object>>(new Dictionary<string, object>());
            _tags = new BehaviorSubject<HashSet<string>>(new HashSet<string>());
        }

        public void SetUserData(string key, object value)
        {
            UserData[key] = value;
            _userData.OnNext(UserData);
        }

        public T GetUserData<T>(string key, T defaultValue = default)
        {
            return UserData.TryGetValue(key, out var value) && value is T typed ? typed : defaultValue;
        }

        public void AddTag(string tag)
        {
            if (Tags.Add(tag))
            {
                _tags.OnNext(Tags);
            }
        }

        public void RemoveTag(string tag)
        {
            if (Tags.Remove(tag))
            {
                _tags.OnNext(Tags);
            }
        }

        public bool HasTag(string tag)
        {
            return Tags.Contains(tag);
        }

        public bool Equals(EditorObject other)
        {
            return other != null && Id.Equals(other.Id);
        }

        public override bool Equals(object obj)
        {
            return obj is EditorObject editorObject && Equals(editorObject);
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

        public static bool operator ==(EditorObject left, EditorObject right)
        {
            return left?.Equals(right) ?? right is null;
        }

        public static bool operator !=(EditorObject left, EditorObject right)
        {
            return !(left == right);
        }

        public override string ToString()
        {
            return $"{GetType().Name}[{Name}, ID: {Id:N}]";
        }

        public virtual void Dispose()
        {
            _id?.Dispose();
            _name?.Dispose();
            _isEnabled?.Dispose();
            _userData?.Dispose();
            _tags?.Dispose();
        }
    }
}