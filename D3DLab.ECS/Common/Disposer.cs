using System;
using System.Collections.Generic;
using System.Text;

namespace D3DLab.ECS.Common { 

    [Obsolete("extra abstraction, remove")]
    public static class Disposer {
        public static void DisposeAll(params IDisposable[] source) {
            DisposeAll((IEnumerable<IDisposable>)source);
        }
        public static void DisposeAll(IEnumerable<IDisposable> source) {
            if (source == null) return;
            foreach (var d in source) {
                d?.Dispose();
            }
        }
        public static void RemoveAndDispose<T>(ref T resource) where T : class, IDisposable {
            if (resource == null)
                return;

            try {
                resource.Dispose();
            } catch {
            }

            resource = null;
        }
    }
}
