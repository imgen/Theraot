// Needed for Workaround

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Security.Permissions;
using System.Threading;
using Theraot.Core;

namespace Theraot.Threading.Needles
{
    [System.Diagnostics.DebuggerNonUserCode]
    public partial class WeakNeedle<T> : IEquatable<WeakNeedle<T>>, IRecyclableNeedle<T>, ICacheNeedle<T>
        where T : class
    {
        private readonly int _hashCode;
        private readonly bool _trackResurrection;
        private GCHandle _handle;
        private volatile bool _faultExpected;
        private int _managedDisposal;

        public WeakNeedle()
            : this(false)
        {
            // Empty
        }

        public WeakNeedle(bool trackResurrection)
        {
            _trackResurrection = trackResurrection;
            _hashCode = NeedleHelper.GetNextHashCode();
        }

        [SecurityPermission(SecurityAction.Demand, UnmanagedCode = true)]
        public WeakNeedle(T target)
            : this(target, false)
        {
            // Empty
        }

        [SecurityPermission(SecurityAction.Demand, UnmanagedCode = true)]
        public WeakNeedle(T target, bool trackResurrection)
        {
            _trackResurrection = trackResurrection;
            SetTargetValue(target);
            _hashCode = NeedleHelper.GetNextHashCode();
        }

        public Exception Exception
        {
            get
            {
                object target;
                if (ReadTarget(out target))
                {
                    var exception = target as Exception;
                    if (exception != null && _faultExpected)
                    {
                        return exception;
                    }
                }
                return null;
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes", Justification = "Returns false")]
        bool IPromise.IsCanceled
        {
            get
            {
                return false;
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes", Justification = "Returns true")]
        bool IPromise.IsCompleted
        {
            get
            {
                return true;
            }
        }

        public bool IsAlive
        {
            get
            {
                object target;
                if (ReadTarget(out target))
                {
                    if (target is T && !_faultExpected)
                    {
                        return true;
                    }
                }
                return false;
            }
        }

        public bool IsFaulted
        {
            get
            {
                object target;
                if (ReadTarget(out target))
                {
                    if (target is Exception && _faultExpected)
                    {
                        return true;
                    }
                }
                return false;
            }
        }

        public virtual bool TrackResurrection
        {
            get
            {
                return _trackResurrection;
            }
        }

        public virtual T Value
        {
            [SecurityPermission(SecurityAction.Demand, UnmanagedCode = true)]
            get
            {
                object target;
                if (ReadTarget(out target))
                {
                    var inner = target as T;
                    if (inner != null && !_faultExpected)
                    {
                        return inner;
                    }
                }
                return null;
            }
            [SecurityPermission(SecurityAction.Demand, UnmanagedCode = true)]
            set
            {
                SetTargetValue(value);
            }
        }

        public static explicit operator T(WeakNeedle<T> needle)
        {
            return Check.NotNullArgument(needle, "needle").Value;
        }

        public static implicit operator WeakNeedle<T>(T field)
        {
            return new WeakNeedle<T>(field);
        }

        public static bool operator !=(WeakNeedle<T> left, WeakNeedle<T> right)
        {
            return NotEqualsExtracted(left, right);
        }

        public static bool operator ==(WeakNeedle<T> left, WeakNeedle<T> right)
        {
            return EqualsExtracted(left, right);
        }

        public sealed override bool Equals(object obj)
        {
            var needle = obj as WeakNeedle<T>;
            if (needle != null)
            {
                return EqualsExtractedExtracted(this, needle);
            }
            var value = obj as T;
            if (value != null)
            {
                var target = Value;
                return IsAlive && EqualityComparer<T>.Default.Equals(target, value);
            }
            return false;
        }

        public bool Equals(WeakNeedle<T> other)
        {
            return !ReferenceEquals(other, null) && EqualsExtractedExtracted(this, other);
        }

        public void Free()
        {
            Dispose();
        }

        public sealed override int GetHashCode()
        {
            return _hashCode;
        }

        public override string ToString()
        {
            var target = Value;
            if (IsAlive)
            {
                return target.ToString();
            }
            else
            {
                return "<Dead Needle>";
            }
        }

        [SecurityPermission(SecurityAction.Demand, UnmanagedCode = true)]
        public virtual bool TryGetValue(out T value)
        {
            value = null;
            object target;
            if (ReadTarget(out target))
            {
                var inner = target as T;
                if (inner != null)
                {
                    value = inner;
                    return true;
                }
            }
            return false;
        }

        [SecurityPermission(SecurityAction.Demand, UnmanagedCode = true)]
        protected void SetTargetError(Exception error)
        {
            _faultExpected = true;
            WriteTarget(error);
        }

        [SecurityPermission(SecurityAction.Demand, UnmanagedCode = true)]
        protected void SetTargetValue(T value)
        {
            _faultExpected = false;
            WriteTarget(value);
        }

        private static bool EqualsExtracted(WeakNeedle<T> left, WeakNeedle<T> right)
        {
            if (ReferenceEquals(left, null))
            {
                return ReferenceEquals(right, null);
            }
            else
            {
                return !ReferenceEquals(right, null) && EqualsExtractedExtracted(left, right);
            }
        }

        private static bool EqualsExtractedExtracted(WeakNeedle<T> left, WeakNeedle<T> right)
        {
            var leftValue = left.Value;
            if (left.IsAlive)
            {
                var rightValue = right.Value;
                return right.IsAlive && EqualityComparer<T>.Default.Equals(leftValue, rightValue);
            }
            return !right.IsAlive;
        }

        private static bool NotEqualsExtracted(WeakNeedle<T> left, WeakNeedle<T> right)
        {
            if (ReferenceEquals(left, null))
            {
                return !ReferenceEquals(right, null);
            }
            else
            {
                return ReferenceEquals(right, null) || NotEqualsExtractedExtracted(left, right);
            }
        }

        private static bool NotEqualsExtractedExtracted(WeakNeedle<T> left, WeakNeedle<T> right)
        {
            var leftValue = left.Value;
            if (left.IsAlive)
            {
                var rightValue = right.Value;
                return !right.IsAlive || !EqualityComparer<T>.Default.Equals(leftValue, rightValue);
            }
            return right.IsAlive;
        }

        [SecurityPermission(SecurityAction.Demand, UnmanagedCode = true)]
        private void ReleaseExtracted()
        {
            if (_handle.IsAllocated)
            {
                try
                {
                    _handle.Free(); // Throws InvalidOperationException
                }
                catch (InvalidOperationException)
                {
                    // Empty
                }
            }
        }

        private bool ReadTarget(out object target)
        {
            target = null;
            if (_handle.IsAllocated)
            {
                try
                {
                    target = _handle.Target; // Throws InvalidOperationException
                }
                catch (InvalidOperationException)
                {
                    return false;
                }
                return true;
            }
            return false;
        }

        private void WriteTarget(object target)
        {
            if (_status == -1 || !ThreadingHelper.SpinWaitRelativeSet(ref _status, 1, -1))
            {
                ReleaseExtracted();
                _handle = GCHandle.Alloc(target, _trackResurrection ? GCHandleType.Weak : GCHandleType.WeakTrackResurrection);
                if (Interlocked.CompareExchange(ref _managedDisposal, 0, 1) == 1)
                {
                    GC.ReRegisterForFinalize(this);
                }
                UnDispose();
            }
            else
            {
                try
                {
                    var oldHandle = _handle;
                    if (oldHandle.IsAllocated)
                    {
                        try
                        {
                            oldHandle.Target = target;
                            return;
                        }
                        catch (InvalidOperationException)
                        {
                            _handle = GCHandle.Alloc(target, _trackResurrection ? GCHandleType.Weak : GCHandleType.WeakTrackResurrection);
                        }
                    }
                    else
                    {
                        _handle = GCHandle.Alloc(target, _trackResurrection ? GCHandleType.Weak : GCHandleType.WeakTrackResurrection);
                    }
                    if (oldHandle.IsAllocated)
                    {
                        oldHandle.Free();
                        try
                        {
                            oldHandle.Free();
                        }
                        catch (InvalidOperationException)
                        {
                            // Empty
                        }
                    }
                }
                finally
                {
                    Interlocked.Decrement(ref _status);
                }
            }
        }

        private void ReportManagedDisposal()
        {
            Thread.VolatileWrite(ref _managedDisposal, 1);
        }
    }
}