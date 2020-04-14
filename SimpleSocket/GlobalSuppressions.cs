﻿// This file is used by Code Analysis to maintain SuppressMessage
// attributes that are applied to this project.
// Project-level suppressions either have no target or are given
// a specific target and scoped to a namespace, type, member, etc.

using System.Diagnostics.CodeAnalysis;

[assembly: SuppressMessage("Performance", "CA1819:Properties should not return arrays", Justification = "Usage justifies overriding.", Scope = "member", Target = "~P:SimpleSocket.Datagram.Data")]
[assembly: SuppressMessage("Usage", "CA2213:Disposable fields should be disposed", Justification = "Field is not instantiated by the IDisposable.", Scope = "member", Target = "SimpleSocket.Connection._stream")]