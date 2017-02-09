// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

//

namespace System.Runtime.InteropServices.WindowsRuntime
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Diagnostics.Contracts;
    using System.Reflection;
    using System.Reflection.Emit;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Security;
    
    internal static class WindowsRuntimeMetadata
    {
        private static EventHandler<DesignerNamespaceResolveEventArgs> DesignerNamespaceResolve;
        
        internal static string[] OnDesignerNamespaceResolveEvent(AppDomain appDomain, string namespaceName)
        {
            EventHandler<DesignerNamespaceResolveEventArgs> eventHandler = DesignerNamespaceResolve;
            if (eventHandler != null)
            {
                Delegate[] ds = eventHandler.GetInvocationList();
                int len = ds.Length;
                for (int i = 0; i < len; i++)
                {
                    DesignerNamespaceResolveEventArgs eventArgs = new DesignerNamespaceResolveEventArgs(namespaceName);

                    ((EventHandler<DesignerNamespaceResolveEventArgs>)ds[i])(appDomain, eventArgs);

                    Collection<string> assemblyFilesCollection = eventArgs.ResolvedAssemblyFiles;
                    if (assemblyFilesCollection.Count > 0)
                    {
                        string[] retAssemblyFiles = new string[assemblyFilesCollection.Count];
                        int retIndex = 0;
                        foreach (string assemblyFile in assemblyFilesCollection)
                        {
                            if (String.IsNullOrEmpty(assemblyFile))
                            {   // DesignerNamespaceResolve event returned null or empty file name - that is not allowed
                                throw new ArgumentException(Environment.GetResourceString("Arg_EmptyOrNullString"), "DesignerNamespaceResolveEventArgs.ResolvedAssemblyFiles");
                            }
                            retAssemblyFiles[retIndex] = assemblyFile;
                            retIndex++;
                        }

                        return retAssemblyFiles;
                    }
                }
            }
            
            return null;
        }
    }
    
#if FEATURE_REFLECTION_ONLY_LOAD
    public class NamespaceResolveEventArgs : EventArgs
    {
        private string _NamespaceName;
        private Assembly _RequestingAssembly;
        private Collection<Assembly> _ResolvedAssemblies;

        public string NamespaceName
        {
            get
            {
                return _NamespaceName;
            }
        }

        public Assembly RequestingAssembly
        {
            get
            {
                return _RequestingAssembly;
            }
        }

        public Collection<Assembly> ResolvedAssemblies
        {
            get
            {
                return _ResolvedAssemblies;
            }
        }
        
        public NamespaceResolveEventArgs(string namespaceName, Assembly requestingAssembly)
        {
            _NamespaceName = namespaceName;
            _RequestingAssembly = requestingAssembly;
            _ResolvedAssemblies = new Collection<Assembly>();
        }
    }
#endif //FEATURE_REFLECTION_ONLY

    internal class DesignerNamespaceResolveEventArgs : EventArgs
    {
        private string _NamespaceName;
        private Collection<string> _ResolvedAssemblyFiles;

        public Collection<string> ResolvedAssemblyFiles
        {
            get
            {
                return _ResolvedAssemblyFiles;
            }
        }

        public DesignerNamespaceResolveEventArgs(string namespaceName)
        {
            _NamespaceName = namespaceName;
            _ResolvedAssemblyFiles = new Collection<string>();
        }
    }
}