﻿using System;
using System.Threading;
using Theraot.Collections.Specialized;
using Theraot.Core;
using Theraot.Threading.Needles;

namespace Theraot.Threading
{
    internal sealed class NeedleLock<T> : INeedle<T>
    {
        private readonly LockContext<T> _context;
        private readonly int _hashCode;
        private FlagArray _capture;
        private T _target;

        internal NeedleLock(LockContext<T> context)
        {
            if (ReferenceEquals(context, null))
            {
                throw new ArgumentNullException("context");
            }
            else
            {
                _context = context;
                _hashCode = GetHashCode();
                _capture = new FlagArray(_context.Capacity);
            }
        }

        internal NeedleLock(LockContext<T> context, T target)
        {
            if (ReferenceEquals(context, null))
            {
                throw new ArgumentNullException("context");
            }
            else
            {
                _context = context;
                _target = target;
                if (ReferenceEquals(target, null))
                {
                    _hashCode = GetHashCode();
                }
                else
                {
                    _hashCode = target.GetHashCode();
                }
                _capture = new FlagArray(_context.Capacity);
            }
        }

        bool IReadOnlyNeedle<T>.IsAlive
        {
            get
            {
                return !ReferenceEquals(_target, null);
            }
        }

        public T Value
        {
            get
            {
                T value;
                if (_context.Read(_capture, out value))
                {
                    _target = value;
                }
                Thread.MemoryBarrier();
                return _target;
            }
            set
            {
                _target = value;
                Thread.MemoryBarrier();
            }
        }

        public static explicit operator T(NeedleLock<T> needle)
        {
            return Check.NotNullArgument(needle, "needle").Value;
        }

        public static bool operator !=(NeedleLock<T> left, NeedleLock<T> right)
        {
            return NotEqualsExtracted(left, right);
        }

        public static bool operator ==(NeedleLock<T> left, NeedleLock<T> right)
        {
            return EqualsExtracted(left, right);
        }

        public override bool Equals(object obj)
        {
            var _obj = obj as NeedleLock<T>;
            if (ReferenceEquals(null, _obj))
            {
                return _target.Equals(obj);
            }
            else
            {
                return EqualsExtracted(this, _obj);
            }
        }

        public bool Equals(NeedleLock<T> other)
        {
            return EqualsExtracted(this, other);
        }

        public void Free()
        {
            if (System.Linq.Enumerable.Count(ThreadingHelper.VolatileRead(ref _capture).Flags) == 0)
            {
                _target = default(T);
            }
        }

        public override int GetHashCode()
        {
            return _hashCode;
        }

        public override string ToString()
        {
            var target = Value;
            if ((this as IReadOnlyNeedle<T>).IsAlive)
            {
                return target.ToString();
            }
            else
            {
                return "<Dead Needle>";
            }
        }

        internal void Capture(int id)
        {
            _capture[id] = true;
        }

        internal void Uncapture(int id)
        {
            _capture[id] = false;
        }

        private static bool EqualsExtracted(NeedleLock<T> left, NeedleLock<T> right)
        {
            if (ReferenceEquals(left, null))
            {
                return ReferenceEquals(right, null);
            }
            else
            {
                return left._target.Equals(right._target);
            }
        }

        private static bool NotEqualsExtracted(NeedleLock<T> left, NeedleLock<T> right)
        {
            if (ReferenceEquals(left, null))
            {
                if (ReferenceEquals(right, null))
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
            else
            {
                return !left._target.Equals(right._target);
            }
        }
    }
}