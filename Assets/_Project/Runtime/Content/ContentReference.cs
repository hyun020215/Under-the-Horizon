using System;
using UnityEngine;
[Serializable] public sealed class ContentReference<T> where T:UnityEngine.Object { [SerializeField] private string id; [SerializeField] private T asset; public string Id=>id; public T Asset=>asset; }
