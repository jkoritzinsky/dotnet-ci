// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

/*============================================================
**
**
** Purpose: A wrapper for establishing a WeakReference to an Object.
**
===========================================================*/

using System;
using System.Runtime.Serialization;
using System.Security;
using System.Runtime.CompilerServices;
using System.Runtime.Versioning;
using System.Diagnostics;
using System.Diagnostics.Contracts;

namespace System
{
    [Serializable]
    public class WeakReference : ISerializable
    {
        // If you fix bugs here, please fix them in WeakReference<T> at the same time.

        // This field is not a regular GC handle. It can have a special values that are used to prevent a race condition between setting the target and finalization.
        internal IntPtr m_handle;

        // Migrating InheritanceDemands requires this default ctor, so we can mark it SafeCritical
        protected WeakReference()
        {
            Debug.Assert(false, "WeakReference's protected default ctor should never be used!");
            throw new NotImplementedException();
        }

        // Creates a new WeakReference that keeps track of target.
        // Assumes a Short Weak Reference (ie TrackResurrection is false.)
        //
        public WeakReference(Object target)
            : this(target, false)
        {
        }

        //Creates a new WeakReference that keeps track of target.
        //
        public WeakReference(Object target, bool trackResurrection)
        {
            Create(target, trackResurrection);
        }

        protected WeakReference(SerializationInfo info, StreamingContext context)
        {
            if (info == null)
            {
                throw new ArgumentNullException(nameof(info));
            }
            Contract.EndContractBlock();

            Object target = info.GetValue("TrackedObject", typeof(Object));
            bool trackResurrection = info.GetBoolean("TrackResurrection");

            Create(target, trackResurrection);
        }

        //Determines whether or not this instance of WeakReference still refers to an object
        //that has not been collected.
        //
        public extern virtual bool IsAlive
        {
            [MethodImplAttribute(MethodImplOptions.InternalCall)]
            get;
        }

        //Returns a boolean indicating whether or not we're tracking objects until they're collected (true)
        //or just until they're finalized (false).
        //
        public virtual bool TrackResurrection
        {
            // We need to call IsTrackResurrection non-virtually in GetObjectData, and so the virtual property cannot be FCall directly
            get { return IsTrackResurrection(); }
        }

        //Gets the Object stored in the handle if it's accessible.
        // Or sets it.
        //
        public extern virtual Object Target
        {
            [MethodImplAttribute(MethodImplOptions.InternalCall)]
            get;
            [MethodImplAttribute(MethodImplOptions.InternalCall)]
            set;
        }

        // Free all system resources associated with this reference.
        //
        // Note: The WeakReference finalizer is not actually run, but
        // treated specially in gc.cpp's ScanForFinalization
        // This is needed for subclasses deriving from WeakReference, however.
        // Additionally, there may be some cases during shutdown when we run this finalizer.
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        extern ~WeakReference();

        public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if (info == null)
            {
                throw new ArgumentNullException(nameof(info));
            }
            Contract.EndContractBlock();
            info.AddValue("TrackedObject", Target, typeof(Object));
            info.AddValue("TrackResurrection", IsTrackResurrection());
        }

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        private extern void Create(Object target, bool trackResurrection);

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        private extern bool IsTrackResurrection();
    }
}