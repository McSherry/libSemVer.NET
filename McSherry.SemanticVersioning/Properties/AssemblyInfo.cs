// Copyright (c) 2015-16 Liam McSherry
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

using CLSCompliantAttribute = System.CLSCompliantAttribute;

// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
//[assembly: AssemblyTitle("Semantic Versioning for .NET")]
//[assembly: AssemblyProduct("Semantic Versioning for .NET")]
//[assembly: AssemblyCopyright("Copyright 2015-16 © Liam McSherry")]
[assembly: AssemblyCulture("")]
[assembly: InternalsVisibleTo("McSherry.SemanticVersioning.Testing")]
[assembly: CLSCompliant(true)]

// We're using Semantic Versioning for the library, but this doesn't translate
// exactly to .NET versioning. To get around this, we're going to specify two
// version attributes:
//
//  [AssemblyInformationalVersion]: We're using this for the "actual" (semantic)
//                                  version number.
//
//               [AssemblyVersion]: This will track the "actual" version number's 
//                                  major and minor versions. This should allow any 
//                                  patched versions to be swapped out without 
//                                  needing a recompile (e.g. you built against 
//                                  v1.0.0, but you should be able to use v1.0.1 or
//                                  v1.0.2 without needing to recompile).
//
//[assembly: AssemblyVersion("1.2.0.0")]
//[assembly: AssemblyInformationalVersion("1.2.0")]
