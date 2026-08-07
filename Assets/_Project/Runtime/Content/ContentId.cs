using System;
[Serializable] public struct ContentId : IEquatable<ContentId> { public string value; public bool Equals(ContentId other)=>string.Equals(value,other.value,StringComparison.Ordinal); public override string ToString()=>value??string.Empty; }
