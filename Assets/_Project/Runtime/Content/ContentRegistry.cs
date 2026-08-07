using System;
using System.Collections.Generic;
public sealed class ContentRegistry<T> where T : class { private readonly Dictionary<string,T> values = new(StringComparer.Ordinal); public IEnumerable<T> Values => values.Values; public void Register(string id,T value){if(string.IsNullOrWhiteSpace(id))throw new ArgumentException("Content ID is required.");values.Add(id,value??throw new ArgumentNullException(nameof(value)));} public bool TryGet(string id,out T value)=>values.TryGetValue(id??string.Empty,out value); }
